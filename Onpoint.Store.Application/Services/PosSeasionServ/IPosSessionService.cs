using BuildingBlocks.Results;
using Onpoint.Store.Application.DTOs.PosSession;
using Onpoint.Store.Domin.Enums;

namespace Onpoint.Store.Application.Services.PosServ
{
    public interface IPosSessionService
    {
        Task<ServiceResult<PosSessionDto>> CreateSessionAsync(int cashierId);
        Task<ServiceResult<PosSessionDto>> GetSessionAsync(int sessionId);
        Task<ServiceResult<PosSessionDto>> GetActiveSessionForCashierAsync(int cashierId);
        Task<ServiceResult<PosSessionDto>> AddItemAsync(int sessionId, int productId, int quantity);
        Task<ServiceResult<PosSessionDto>> RemoveItemAsync(int sessionId, int itemId);
        Task<ServiceResult<PosSessionDto>> UpdateItemQuantityAsync(int sessionId, int itemId, int quantity);
        Task<ServiceResult<PosSessionDto>> ApplyCouponAsync(int sessionId, string couponCode);
        Task<ServiceResult<PosSessionDto>> RemoveCouponAsync(int sessionId);
        Task<ServiceResult<PosOrderDto>> CompleteSessionAsync(int sessionId, PaymentMethod paymentMethod);
        Task<ServiceResult<bool>> HoldSessionAsync(int sessionId);
        Task<ServiceResult<PosSessionDto>> ResumeSessionAsync(int sessionId, int cashierId);
        Task<ServiceResult<bool>> CancelSessionAsync(int sessionId);
    }
}