using Sky.Template.Backend.Core.Constants;
using Sky.Template.Backend.Contract.Requests.Suppliers;
using Sky.Template.Backend.Core.BaseResponse;
using Sky.Template.Backend.Contract.Responses.SupplierResponses;
using Sky.Template.Backend.Core.Aspects.Autofac.SecuredOperation;
using Sky.Template.Backend.Core.Exceptions;
using Sky.Template.Backend.Infrastructure.Entities.Supplier;
using Sky.Template.Backend.Infrastructure.Repositories;
using Sky.Template.Backend.Infrastructure.Repositories.UnitOfWork;
using Microsoft.AspNetCore.Http;
using Sky.Template.Backend.Core.Extensions;
using Sky.Template.Backend.Core.Aspects.Autofac.Authorization;

namespace Sky.Template.Backend.Application.Services.Admin;

public interface IAdminSupplierService
{
    #region CRUD Operations
    Task<BaseControllerResponse<PaginatedSupplierListResponse>> GetFilteredPaginatedSuppliersAsync(SupplierFilterRequest request);
    Task<BaseControllerResponse<SupplierListResponse>> GetAllSuppliersAsync();
    Task<BaseControllerResponse<SingleSupplierResponse>> GetSupplierByIdAsync(Guid id);
    Task<BaseControllerResponse<SingleSupplierResponse>> CreateSupplierAsync(CreateSupplierRequest request);
    Task<BaseControllerResponse<SingleSupplierResponse>> UpdateSupplierAsync(UpdateSupplierRequest request);
    Task<BaseControllerResponse> DeleteSupplierAsync(Guid id);
    Task<BaseControllerResponse> SoftDeleteSupplierAsync(Guid id, string reason = "");
    #endregion

    #region Business Operations
    Task<BaseControllerResponse<SingleSupplierResponse>> GetSupplierByEmailAsync(string email);
    Task<BaseControllerResponse<SingleSupplierResponse>> GetSupplierByTaxNumberAsync(string taxNumber);
    Task<bool> IsEmailUniqueAsync(string email, Guid? excludeId = null);
    Task<bool> IsTaxNumberUniqueAsync(string taxNumber, Guid? excludeId = null);
    #endregion
}
public class AdminSupplierService : IAdminSupplierService
{
    private readonly ISupplierRepository _supplierRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public AdminSupplierService(ISupplierRepository supplierRepository, IUnitOfWork unitOfWork, IHttpContextAccessor httpContextAccessor)
    {
        _supplierRepository = supplierRepository;
        _unitOfWork = unitOfWork;
        _httpContextAccessor = httpContextAccessor;
    }

    #region CRUD Operations
    [HasPermission(Permissions.Suppliers.View)]
    public async Task<BaseControllerResponse<PaginatedSupplierListResponse>> GetFilteredPaginatedSuppliersAsync(SupplierFilterRequest request)
    {
        var (suppliers, totalCount) = await _supplierRepository.GetFilteredPaginatedAsync(request);

        if (!suppliers.Any())
        {
            throw new NotFoundException("SupplierNotFound");
        }

        var supplierList = suppliers.Select(s => new SupplierDto()
        {
            Id = s.Id,
            Name = s.Name,
            ContactPerson = s.ContactPerson,
            Email = s.Email,
            Phone = s.Phone,
            Address = s.Address,
            TaxNumber = s.TaxNumber,
            TaxOffice = s.TaxOffice,
            SupplierType = s.SupplierType,
            Status = s.Status,
            PaymentTerms = s.PaymentTerms,
            CreditLimit = s.CreditLimit,
            Notes = s.Notes,
            CreatedAt = s.CreatedAt,
            CreatedBy = s.CreatedBy,
            UpdatedAt = s.UpdatedAt,
            UpdatedBy = s.UpdatedBy,
            IsDeleted = s.IsDeleted,
            DeletedAt = s.DeletedAt,
            DeletedBy = s.DeletedBy,
            DeleteReason = s.DeleteReason
        });

        return ControllerResponseBuilder.Success(new PaginatedSupplierListResponse()
        {
            Suppliers = new PaginatedData<SupplierDto>()
            {
                TotalCount = totalCount,
                Items = supplierList.ToList(),
                PageSize = suppliers.Count(),
                Page = request.Page,
                TotalPage = (int)Math.Ceiling((double)totalCount / request.PageSize)
            }
        });
    }

    public async Task<BaseControllerResponse<SupplierListResponse>> GetAllSuppliersAsync()
    {
        var data = await _supplierRepository.GetAllAsync();

        if (!data.Any())
        {
            throw new NotFoundException("SupplierNotFound");
        }

        return ControllerResponseBuilder.Success(new SupplierListResponse()
        {
            Suppliers = data.Select(s => new SupplierDto()
            {
                Id = s.Id,
                Name = s.Name,
                ContactPerson = s.ContactPerson,
                Email = s.Email,
                Phone = s.Phone,
                Address = s.Address,
                TaxNumber = s.TaxNumber,
                TaxOffice = s.TaxOffice,
                SupplierType = s.SupplierType,
                Status = s.Status,
                PaymentTerms = s.PaymentTerms,
                CreditLimit = s.CreditLimit,
                Notes = s.Notes,
                CreatedAt = s.CreatedAt,
                CreatedBy = s.CreatedBy,
                UpdatedAt = s.UpdatedAt,
                UpdatedBy = s.UpdatedBy,
                IsDeleted = s.IsDeleted,
                DeletedAt = s.DeletedAt,
                DeletedBy = s.DeletedBy,
                DeleteReason = s.DeleteReason
            }).ToList()
        });
    }

    [HasPermission(Permissions.Suppliers.View)]
    [EnsureUserIsValid(new[] { "id" })]
    public async Task<BaseControllerResponse<SingleSupplierResponse>> GetSupplierByIdAsync(Guid id)
    {
        var supplier = await _supplierRepository.GetByIdAsync(id);

        if (supplier == null)
        {
            throw new NotFoundException("SupplierNotFoundWithId", id);
        }

        return ControllerResponseBuilder.Success(new SingleSupplierResponse()
        {
            Supplier = new SupplierDto()
            {
                Id = supplier.Id,
                Name = supplier.Name,
                ContactPerson = supplier.ContactPerson,
                Email = supplier.Email,
                Phone = supplier.Phone,
                Address = supplier.Address,
                TaxNumber = supplier.TaxNumber,
                TaxOffice = supplier.TaxOffice,
                SupplierType = supplier.SupplierType,
                Status = supplier.Status,
                PaymentTerms = supplier.PaymentTerms,
                CreditLimit = supplier.CreditLimit,
                Notes = supplier.Notes,
                CreatedAt = supplier.CreatedAt,
                CreatedBy = supplier.CreatedBy,
                UpdatedAt = supplier.UpdatedAt,
                UpdatedBy = supplier.UpdatedBy,
                IsDeleted = supplier.IsDeleted,
                DeletedAt = supplier.DeletedAt,
                DeletedBy = supplier.DeletedBy,
                DeleteReason = supplier.DeleteReason
            }
        });
    }

    [HasPermission(Permissions.Suppliers.Edit)]
    public async Task<BaseControllerResponse<SingleSupplierResponse>> CreateSupplierAsync(CreateSupplierRequest request)
    {
        // Business Rules: Email/TaxNumber unique kontrolü
        if (!await IsEmailUniqueAsync(request.Email))
            throw new BusinessRulesException("Supplier.EmailAlreadyExists");
        if (!await IsTaxNumberUniqueAsync(request.TaxNumber))
            throw new BusinessRulesException("Supplier.TaxNumberAlreadyExists");

        var userId = _httpContextAccessor.HttpContext.GetUserId();
        var supplier = new SupplierEntity
        {
            Name = request.Name,
            ContactPerson = request.ContactPerson,
            Email = request.Email,
            Phone = request.Phone,
            Address = request.Address,
            TaxNumber = request.TaxNumber,
            TaxOffice = request.TaxOffice,
            SupplierType = request.SupplierType,
            Status = request.Status,
            PaymentTerms = request.PaymentTerms,
            CreditLimit = request.CreditLimit,
            Notes = request.Notes,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = userId,
            IsDeleted = false
        };

        // Tek insert işlemi - UnitOfWork gerekmez
        await _supplierRepository.CreateAsync(supplier);

        return ControllerResponseBuilder.Success(new SingleSupplierResponse()
        {
            Supplier = new SupplierDto()
            {
                Id = supplier.Id,
                Name = supplier.Name,
                ContactPerson = supplier.ContactPerson,
                Email = supplier.Email,
                Phone = supplier.Phone,
                Address = supplier.Address,
                TaxNumber = supplier.TaxNumber,
                TaxOffice = supplier.TaxOffice,
                SupplierType = supplier.SupplierType,
                Status = supplier.Status,
                PaymentTerms = supplier.PaymentTerms,
                CreditLimit = supplier.CreditLimit,
                Notes = supplier.Notes,
                CreatedAt = supplier.CreatedAt,
                CreatedBy = supplier.CreatedBy,
                UpdatedAt = supplier.UpdatedAt,
                UpdatedBy = supplier.UpdatedBy,
                IsDeleted = supplier.IsDeleted,
                DeletedAt = supplier.DeletedAt,
                DeletedBy = supplier.DeletedBy,
                DeleteReason = supplier.DeleteReason
            }
        }, "SupplierCreatedSuccessfully");
    }

    [HasPermission(Permissions.Suppliers.Edit)]
    public async Task<BaseControllerResponse<SingleSupplierResponse>> UpdateSupplierAsync(UpdateSupplierRequest request)
    {
        var supplier = await _supplierRepository.GetByIdAsync(request.Id);
        if (supplier == null)
            throw new NotFoundException("SupplierNotFound", request.Id);

        // Business Rules: Email/TaxNumber unique kontrolü (kendisi hariç)
        if (!await IsEmailUniqueAsync(request.Email, request.Id))
            throw new BusinessRulesException("Supplier.EmailAlreadyExists");
        if (!await IsTaxNumberUniqueAsync(request.TaxNumber, request.Id))
            throw new BusinessRulesException("Supplier.TaxNumberAlreadyExists");

        var userId = _httpContextAccessor.HttpContext.GetUserId();
        supplier.Name = request.Name;
        supplier.ContactPerson = request.ContactPerson;
        supplier.Email = request.Email;
        supplier.Phone = request.Phone;
        supplier.Address = request.Address;
        supplier.TaxNumber = request.TaxNumber;
        supplier.TaxOffice = request.TaxOffice;
        supplier.SupplierType = request.SupplierType;
        supplier.Status = request.Status;
        supplier.PaymentTerms = request.PaymentTerms;
        supplier.CreditLimit = request.CreditLimit;
        supplier.Notes = request.Notes;
        supplier.UpdatedAt = DateTime.UtcNow;
        supplier.UpdatedBy = userId;

        // Tek update işlemi - UnitOfWork gerekmez
        await _supplierRepository.UpdateAsync(supplier);

        var response = new SingleSupplierResponse
        {
            Supplier = new SupplierDto
            {
                Id = supplier.Id,
                Name = supplier.Name,
                ContactPerson = supplier.ContactPerson,
                Email = supplier.Email,
                Phone = supplier.Phone,
                Address = supplier.Address,
                TaxNumber = supplier.TaxNumber,
                TaxOffice = supplier.TaxOffice,
                SupplierType = supplier.SupplierType,
                Status = supplier.Status,
                PaymentTerms = supplier.PaymentTerms,
                CreditLimit = supplier.CreditLimit,
                Notes = supplier.Notes,
                CreatedAt = supplier.CreatedAt,
                CreatedBy = supplier.CreatedBy,
                UpdatedAt = supplier.UpdatedAt,
                UpdatedBy = supplier.UpdatedBy,
                IsDeleted = supplier.IsDeleted,
                DeletedAt = supplier.DeletedAt,
                DeletedBy = supplier.DeletedBy,
                DeleteReason = supplier.DeleteReason
            }
        };
        return ControllerResponseBuilder.Success(response, "SupplierUpdatedSuccessfully");
    }

    [HasPermission(Permissions.Common.HardDelete)]
    [EnsureUserIsValid(new[] { "id" })]
    public async Task<BaseControllerResponse> DeleteSupplierAsync(Guid id)
    {
        var supplier = await _supplierRepository.GetByIdAsync(id);
        if (supplier == null)
        {
            throw new NotFoundException("SupplierNotFoundWithId", id);
        }

        // Tek delete işlemi - UnitOfWork gerekmez
        await _supplierRepository.DeleteAsync(id);

        return ControllerResponseBuilder.Success();
    }

    [HasPermission(Permissions.Suppliers.Delete)]
    [EnsureUserIsValid(new[] { "id" })]
    public async Task<BaseControllerResponse> SoftDeleteSupplierAsync(Guid id, string reason = "")
    {
        var supplier = await _supplierRepository.GetByIdAsync(id);
        if (supplier == null)
        {
            throw new NotFoundException("SupplierNotFoundWithId", id);
        }

        // Tek soft delete işlemi - UnitOfWork gerekmez
        await _supplierRepository.SoftDeleteAsync(id, reason);

        return ControllerResponseBuilder.Success();
    }
    #endregion

    #region Business Operations
    [HasPermission(Permissions.Suppliers.View)]
    public async Task<BaseControllerResponse<SingleSupplierResponse>> GetSupplierByEmailAsync(string email)
    {
        var supplier = await _supplierRepository.GetSupplierByEmailAsync(email);

        if (supplier == null)
        {
            throw new NotFoundException("SupplierNotFoundWithEmail", email);
        }

        return ControllerResponseBuilder.Success(new SingleSupplierResponse()
        {
            Supplier = new SupplierDto()
            {
                Id = supplier.Id,
                Name = supplier.Name,
                ContactPerson = supplier.ContactPerson,
                Email = supplier.Email,
                Phone = supplier.Phone,
                Address = supplier.Address,
                TaxNumber = supplier.TaxNumber,
                TaxOffice = supplier.TaxOffice,
                SupplierType = supplier.SupplierType,
                Status = supplier.Status,
                PaymentTerms = supplier.PaymentTerms,
                CreditLimit = supplier.CreditLimit,
                Notes = supplier.Notes,
                CreatedAt = supplier.CreatedAt,
                CreatedBy = supplier.CreatedBy,
                UpdatedAt = supplier.UpdatedAt,
                UpdatedBy = supplier.UpdatedBy,
                IsDeleted = supplier.IsDeleted,
                DeletedAt = supplier.DeletedAt,
                DeletedBy = supplier.DeletedBy,
                DeleteReason = supplier.DeleteReason
            }
        });
    }

    [HasPermission(Permissions.Suppliers.View)]
    public async Task<BaseControllerResponse<SingleSupplierResponse>> GetSupplierByTaxNumberAsync(string taxNumber)
    {
        var supplier = await _supplierRepository.GetSupplierByTaxNumberAsync(taxNumber);

        if (supplier == null)
        {
            throw new NotFoundException("SupplierNotFoundWithTaxNumber", taxNumber);
        }

        return ControllerResponseBuilder.Success(new SingleSupplierResponse()
        {
            Supplier = new SupplierDto()
            {
                Id = supplier.Id,
                Name = supplier.Name,
                ContactPerson = supplier.ContactPerson,
                Email = supplier.Email,
                Phone = supplier.Phone,
                Address = supplier.Address,
                TaxNumber = supplier.TaxNumber,
                TaxOffice = supplier.TaxOffice,
                SupplierType = supplier.SupplierType,
                Status = supplier.Status,
                PaymentTerms = supplier.PaymentTerms,
                CreditLimit = supplier.CreditLimit,
                Notes = supplier.Notes,
                CreatedAt = supplier.CreatedAt,
                CreatedBy = supplier.CreatedBy,
                UpdatedAt = supplier.UpdatedAt,
                UpdatedBy = supplier.UpdatedBy,
                IsDeleted = supplier.IsDeleted,
                DeletedAt = supplier.DeletedAt,
                DeletedBy = supplier.DeletedBy,
                DeleteReason = supplier.DeleteReason
            }
        });
    }

    public async Task<bool> IsEmailUniqueAsync(string email, Guid? excludeId = null)
    {
        return await _supplierRepository.IsEmailUniqueAsync(email, excludeId);
    }

    public async Task<bool> IsTaxNumberUniqueAsync(string taxNumber, Guid? excludeId = null)
    {
        return await _supplierRepository.IsTaxNumberUniqueAsync(taxNumber, excludeId);
    }
    #endregion
}