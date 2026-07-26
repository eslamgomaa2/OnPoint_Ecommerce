using AutoMapper;
using BuildingBlocks.Results;
using FluentValidation;
using Microsoft.Extensions.Configuration;
using Onpoint.Store.Application.Common;
using Onpoint.Store.Application.DTOs.Cart;
using Onpoint.Store.Domin.Entities;
using Onpoint.Store.Domin.Enums;
using Onpoint.Store.Domin.Repositories;

namespace Onpoint.Store.Application.Services.CartServ
{
    public class CartService : ICartService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ServiceResultHandler _resultHandler;
        private readonly IMapper _mapper;
        private readonly IValidator<AddToCartDto> _addToCartValidator;
        private readonly IValidator<UpdateCartItemDto> _updateItemValidator;
        private readonly IValidator<ApplyCouponDto> _applyCouponValidator;
        private readonly IConfiguration _configuration;

        public CartService(
            IUnitOfWork unitOfWork,
            ServiceResultHandler resultHandler,
            IMapper mapper,
            IValidator<AddToCartDto> addToCartValidator,
            IValidator<UpdateCartItemDto> updateItemValidator,
            IValidator<ApplyCouponDto> applyCouponValidator,
            IConfiguration configuration)
        {
            _unitOfWork = unitOfWork;
            _resultHandler = resultHandler;
            _mapper = mapper;
            _addToCartValidator = addToCartValidator;
            _updateItemValidator = updateItemValidator;
            _applyCouponValidator = applyCouponValidator;
            _configuration = configuration;
        }

        public async Task<ServiceResult<CartDto>> GetUserCartAsync(int userId, CancellationToken ct = default)
        {
            var cart = await _unitOfWork.Carts.GetUserCartWithItemsAsync(userId, ct);
            if (cart == null)
                return _resultHandler.Success(new CartDto { CartId = 0 });

            return _resultHandler.Success(MapToCartDto(cart));
        }

        public async Task<ServiceResult<CartDto>> AddToCartAsync(int userId, int productId, AddToCartDto dto, CancellationToken ct = default)
        {
            await _addToCartValidator.ValidateAndThrowAsync(dto, cancellationToken: ct);


            var product = await _unitOfWork.Products.GetByIdWithVariantsAsync(productId, ct);
            if (product == null)
                throw new KeyNotFoundException("Product not found.");

            decimal unitPrice;

            if (dto.ProductVariantId.HasValue)
            {
                var variant = product.Variants.FirstOrDefault(v => v.Id == dto.ProductVariantId.Value);
                if (variant == null || !variant.IsActive)
                    return _resultHandler.BadRequest<CartDto>("Selected variant is not available.");

                unitPrice = PricingHelper.CalculateFinalPrice(product, variant);
            }
            else
            {
                if (product.Variants.Any(v => v.IsActive))
                    return _resultHandler.BadRequest<CartDto>("This product requires selecting a variant.");

                unitPrice = PricingHelper.CalculateFinalPrice(product);
            }

            var defaultBranch = await _unitOfWork.Branches.FirstOrDefaultAsync(b => b.IsDefault && b.IsActive, ct);
            if (defaultBranch == null)
                return _resultHandler.BadRequest<CartDto>("Default online branch is not configured.");

            var stock = await _unitOfWork.Stocks.GetByProductVariantAndBranchAsync(productId, dto.ProductVariantId, defaultBranch.Id, ct);


            var cart = await _unitOfWork.Carts.GetUserCartWithItemsAsync(userId, ct);
            if (cart == null)
            {
                cart = new Cart { UserId = userId };
                await _unitOfWork.Carts.AddAsync(cart, ct);

            }

            var existingItem = cart.Items.FirstOrDefault(i => i.ProductId == productId && i.ProductVariantId == dto.ProductVariantId);
            var requestedTotalQuantity = (existingItem?.Quantity ?? 0) + dto.Quantity;

            if (stock == null || stock.AvailableQuantity < requestedTotalQuantity)
                return _resultHandler.BadRequest<CartDto>("Not enough stock available.");

            if (existingItem != null)
            {
                existingItem.Quantity = requestedTotalQuantity;
                existingItem.UnitPrice = unitPrice;
            }
            else
            {
                cart.Items.Add(new CartItem
                {
                    ProductId = productId,
                    ProductVariantId = dto.ProductVariantId,
                    Quantity = dto.Quantity,
                    UnitPrice = unitPrice
                });
            }

            await RecalculateDiscountAsync(cart, ct);

            // FIX #1: explicitly mark the cart (and its item graph) as modified before saving.
            // Without this, if GetUserCartWithItemsAsync ever reads with AsNoTracking (or the
            // Items collection isn't tracked), added/updated items silently never persist.
            _unitOfWork.Carts.Update(cart);
            await _unitOfWork.SaveChangesAsync(ct);

            var updatedCart = await _unitOfWork.Carts.GetUserCartWithItemsAsync(userId, ct);
            return _resultHandler.Success(MapToCartDto(updatedCart!));
        }

        public async Task<ServiceResult<CartDto>> UpdateItemQuantityAsync(int userId, int cartItemId, UpdateCartItemDto dto, CancellationToken ct = default)
        {
            await _updateItemValidator.ValidateAndThrowAsync(dto, cancellationToken: ct);

            var cart = await _unitOfWork.Carts.GetUserCartWithItemsAsync(userId, ct);
            if (cart == null)
                return _resultHandler.NotFound<CartDto>("Cart not found.");

            var item = cart.Items.FirstOrDefault(i => i.Id == cartItemId);
            if (item == null)
                return _resultHandler.NotFound<CartDto>("Item not found in cart.");

            if (dto.Quantity == 0)
            {
                cart.Items.Remove(item);
            }
            else
            {
                var defaultBranch = await _unitOfWork.Branches.FirstOrDefaultAsync(b => b.IsDefault && b.IsActive, ct);
                if (defaultBranch == null)
                    return _resultHandler.BadRequest<CartDto>("Default online branch is not configured.");

                var stock = await _unitOfWork.Stocks.GetByProductVariantAndBranchAsync(item.ProductId, item.ProductVariantId, defaultBranch.Id, ct);

                if (stock == null || stock.AvailableQuantity < dto.Quantity)
                    return _resultHandler.BadRequest<CartDto>("Not enough stock available.");

                item.Quantity = dto.Quantity;
            }

            await RecalculateDiscountAsync(cart, ct);

            // FIX #1
            _unitOfWork.Carts.Update(cart);
            await _unitOfWork.SaveChangesAsync(ct);

            var updatedCart = await _unitOfWork.Carts.GetUserCartWithItemsAsync(userId, ct);
            return _resultHandler.Success(MapToCartDto(updatedCart!));
        }

        public async Task<ServiceResult<string>> RemoveItemAsync(int userId, int cartItemId, CancellationToken ct = default)
        {
            var cart = await _unitOfWork.Carts.GetUserCartWithItemsAsync(userId, ct);
            if (cart == null)
                return _resultHandler.NotFound<string>("Cart not found.");

            var item = cart.Items.FirstOrDefault(i => i.Id == cartItemId);
            if (item == null)
                return _resultHandler.NotFound<string>("Item not found in cart.");

            cart.Items.Remove(item);
            await RecalculateDiscountAsync(cart, ct);

            // FIX #1
            _unitOfWork.Carts.Update(cart);
            await _unitOfWork.SaveChangesAsync(ct);

            return _resultHandler.Deleted<string>("Item removed from cart.");
        }

        public async Task<ServiceResult<string>> ClearCartAsync(int userId, CancellationToken ct = default)
        {
            var cart = await _unitOfWork.Carts.GetUserCartWithItemsAsync(userId, ct);
            if (cart == null)
                return _resultHandler.Success<string>("Cart not found.");

            cart.Items.Clear();
            cart.AppliedCouponCode = null;
            cart.DiscountAmount = 0;

            // FIX #1
            _unitOfWork.Carts.Update(cart);
            await _unitOfWork.SaveChangesAsync(ct);
            return _resultHandler.Success<string>("Cart cleared.");
        }

        public async Task<ServiceResult<CartDto>> ApplyCouponAsync(int userId, ApplyCouponDto dto, CancellationToken ct = default)
        {
            await _applyCouponValidator.ValidateAndThrowAsync(dto, cancellationToken: ct);

            var cart = await _unitOfWork.Carts.GetUserCartWithItemsAsync(userId, ct);
            if (cart == null || !cart.Items.Any())
                return _resultHandler.BadRequest<CartDto>("Cart is empty. Add items before applying a coupon.");

            var coupon = await _unitOfWork.Coupons.FirstOrDefaultAsync(c => c.Code == dto.Code.ToUpper(), ct);
            if (coupon == null)
                return _resultHandler.BadRequest<CartDto>("Invalid coupon code");
            if (!coupon.IsActive)
                return _resultHandler.BadRequest<CartDto>("This coupon is no longer active");
            if (coupon.StartDate > DateTime.Now || coupon.EndDate < DateTime.Now)
                return _resultHandler.BadRequest<CartDto>("This coupon has expired");
            if (coupon.UsedCount >= coupon.MaxUses)
                return _resultHandler.BadRequest<CartDto>("This coupon has reached its maximum usage limit");

            decimal subTotal = cart.Items.Sum(i => i.UnitPrice * i.Quantity);
            if (subTotal < coupon.MinOrderAmount)
                return _resultHandler.BadRequest<CartDto>($"Minimum order amount for this coupon is {coupon.MinOrderAmount} KWD");

            decimal discountAmount = coupon.DiscountType == CouponType.Percentage
                ? (subTotal * coupon.Value) / 100
                : coupon.Value;

            if (discountAmount > subTotal)
                discountAmount = subTotal;

            cart.AppliedCouponCode = coupon.Code;
            cart.DiscountAmount = discountAmount;

            _unitOfWork.Carts.Update(cart);
            await _unitOfWork.SaveChangesAsync(ct);

            return _resultHandler.Success(MapToCartDto(cart));
        }

        public async Task<ServiceResult<CartDto>> RemoveCouponAsync(int userId, CancellationToken ct = default)
        {
            var cart = await _unitOfWork.Carts.GetUserCartWithItemsAsync(userId, ct);
            if (cart == null)
                return _resultHandler.NotFound<CartDto>("Cart not found");

            cart.AppliedCouponCode = null;
            cart.DiscountAmount = 0;

            _unitOfWork.Carts.Update(cart);
            await _unitOfWork.SaveChangesAsync(ct);

            return _resultHandler.Success(MapToCartDto(cart));
        }

        private async Task RecalculateDiscountAsync(Cart cart, CancellationToken ct = default)
        {
            if (string.IsNullOrEmpty(cart.AppliedCouponCode))
            {
                cart.DiscountAmount = 0;
                return;
            }

            if (!cart.Items.Any())
            {
                cart.AppliedCouponCode = null;
                cart.DiscountAmount = 0;
                return;
            }

            var coupon = await _unitOfWork.Coupons.FirstOrDefaultAsync(c => c.Code == cart.AppliedCouponCode, ct);
            bool couponStillValid = coupon != null && coupon.IsActive && coupon.StartDate <= DateTime.Now && coupon.EndDate >= DateTime.Now && coupon.UsedCount < coupon.MaxUses;

            if (!couponStillValid)
            {
                cart.AppliedCouponCode = null;
                cart.DiscountAmount = 0;
                return;
            }

            decimal subTotal = cart.Items.Sum(i => i.UnitPrice * i.Quantity);
            if (subTotal < coupon!.MinOrderAmount)
            {
                cart.AppliedCouponCode = null;
                cart.DiscountAmount = 0;
                return;
            }

            decimal discountAmount = coupon.DiscountType == CouponType.Percentage
                ? (subTotal * coupon.Value) / 100
                : coupon.Value;

            if (discountAmount > subTotal)
                discountAmount = subTotal;

            cart.DiscountAmount = discountAmount;
        }

        private CartDto MapToCartDto(Cart cart)
        {
            var dto = _mapper.Map<CartDto>(cart);
            dto.ShippingCost = cart.Items.Any() ? GetShippingCost() : 0;
            return dto;
        }

        private decimal GetShippingCost() => _configuration.GetValue<decimal>("CartSettings:FixedShippingCost", 10.0m);
    }
}