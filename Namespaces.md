# Onpoint.Store - Namespace Reference

## 1. Domain Layer (Entities, Enums, Common, Repositories)

### Entities
- `Onpoint.Store.Domin.Entities`
- `Onpoint.Store.Domin.Entities.Sales`

### Enums
- `Onpoint.Store.Domin.Enums`

### Common (BaseEntity)
- `Onpoint.Store.Domin.Common`

### Repository Interfaces
- `Onpoint.Store.Domin.Repositories`

---

## 2. Application Layer (DTOs, Validators, Services, Mappings)

### DTOs
- `Onpoint.Store.Application.DTOs.Cart`
- `Onpoint.Store.Application.DTOs.Order`
- `Onpoint.Store.Application.DTOs.Pos`
- `Onpoint.Store.Application.DTOs.PosSession`
- `Onpoint.Store.Application.DTOs.Branch`

### Validators
- `Onpoint.Store.Application.Validators.Cart`
- `Onpoint.Store.Application.Validators.Order`
- `Onpoint.Store.Application.Validators.Pos`
- `Onpoint.Store.Application.Validators.PosSession`
- `Onpoint.Store.Application.Validators.Branch`

### Services
- `Onpoint.Store.Application.Services.OrderServ`
- `Onpoint.Store.Application.Services.PosServ`
- `Onpoint.Store.Application.Services.BranchServ`

### AutoMapper Profiles
- `Onpoint.Store.Application.Mappings`

---

## 3. Infrastructure Layer (DbContext, Configurations, Repositories)

### DbContext
- `Onpoint.Store.Infrastructure.Data.Context`

### Entity Configurations
- `Onpoint.Store.Infrastructure.Data.Configurations`

### Repository Implementations
- `Onpoint.Store.Infrastructure.Repositories`

---

## 4. API Layer (Controllers)

### Controllers
- `Onpoint.Store.Api.Controllers`

---

## 5. Recommended New Namespaces (For Future Features)

### If you add POS Reports:
- `Onpoint.Store.Application.DTOs.PosReports`
- `Onpoint.Store.Application.Services.PosReportServ`

### If you add Branch Management:
- `Onpoint.Store.Application.DTOs.Branch`
- `Onpoint.Store.Application.Services.BranchServ`

### If you add Inventory/Stock Management:
- `Onpoint.Store.Application.DTOs.Stock`
- `Onpoint.Store.Application.Services.StockServ`


