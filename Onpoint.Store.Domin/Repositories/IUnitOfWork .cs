<<<<<<< HEAD
﻿using Onpoint.Store.Domin.Entities;
=======
﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
>>>>>>> a4229cd5541012e96d4a2a23d93425d84f1837e3

namespace Onpoint.Store.Domin.Repositories
{
    public interface IUnitOfWork
    {
<<<<<<< HEAD
        ICategoryRepository Categories { get; }
        IProductRepository Products { get; }
        ICartRepository Carts { get; }
        IEmailVerificationOtpRepo EmailVerificationOtpRepo { get; }
        IAddressRepository Addresses { get; }
        ICouponRepository Coupons { get; }
        IOrderRepo Orders { get; }
        IGenericRepository<OrderItem, int> OrderItems { get; }
        IPaymentTransactionRepository PaymentTransactions { get; }
        IInvoiceRepository Invoices { get; }
        IWishlistRepository Wishlists { get; }
        IReviewRepository Reviews { get; }

        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);

        Task BeginTransactionAsync(CancellationToken cancellationToken = default);
        Task CommitTransactionAsync(CancellationToken cancellationToken = default);
        Task RollbackTransactionAsync(CancellationToken cancellationToken = default);
    }
}
=======

        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    }
}
>>>>>>> a4229cd5541012e96d4a2a23d93425d84f1837e3
