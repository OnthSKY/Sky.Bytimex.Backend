using Sky.Template.Backend.Core.Constants;
using Sky.Template.Backend.Contract.Requests.Buyers;
using Sky.Template.Backend.Core.BaseResponse;
using Sky.Template.Backend.Contract.Responses.SaleResponses;
using Sky.Template.Backend.Core.Exceptions;
using Sky.Template.Backend.Infrastructure.Entities.Sales;
using Sky.Template.Backend.Infrastructure.Repositories;
using Sky.Template.Backend.Infrastructure.Repositories.UnitOfWork;
using Microsoft.AspNetCore.Http;
using Sky.Template.Backend.Core.Extensions;
using Sky.Template.Backend.Core.Aspects.Autofac.SecuredOperation;
using Sky.Template.Backend.Core.Aspects.Autofac.Caching;
using Sky.Template.Backend.Core.Aspects.Autofac.Authorization;
using Sky.Template.Backend.Core.CrossCuttingConcerns.Caching;

namespace Sky.Template.Backend.Application.Services.Admin;

public interface IAdminBuyerService
{
    Task<BaseControllerResponse<BuyerListResponse>> GetAllBuyersAsync();
    Task<BaseControllerResponse<SingleBuyerResponse>> GetBuyerByIdAsync(Guid id);
    Task<BaseControllerResponse<SingleBuyerResponse>> CreateBuyerAsync(CreateBuyerRequest request);
    Task<BaseControllerResponse<SingleBuyerResponse>> UpdateBuyerAsync(Guid id, UpdateBuyerRequest request);
    Task<BaseControllerResponse> SoftDeleteBuyerAsync(Guid id);
    Task<BaseControllerResponse> HardDeleteBuyerAsync(Guid id);
    Task<BaseControllerResponse<SingleBuyerResponse>> GetBuyerByEmailAsync(string email);
    Task<BaseControllerResponse<SingleBuyerResponse>> GetBuyerByPhoneAsync(string phone);
    Task<BaseControllerResponse<BuyerListPaginatedResponse>> GetFilteredPaginatedBuyersAsync(BuyerFilterRequest request);
    Task<bool> IsEmailUniqueAsync(string email, Guid? excludeId = null);
    Task<bool> IsPhoneUniqueAsync(string phone, Guid? excludeId = null);
}

public class AdminBuyerService : IAdminBuyerService
{
    private readonly IBuyerRepository _buyerRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public AdminBuyerService(IBuyerRepository buyerRepository, IUnitOfWork unitOfWork, IHttpContextAccessor httpContextAccessor)
    {
        _buyerRepository = buyerRepository;
        _unitOfWork = unitOfWork;
        _httpContextAccessor = httpContextAccessor;
    }

    [Cacheable(CacheKeyPrefix = nameof(CacheKeys.BuyersPrefix), ExpirationInMinutes = 60)]
    public async Task<BaseControllerResponse<BuyerListResponse>> GetAllBuyersAsync()
    {
        var buyers = await _buyerRepository.GetAllAsync();
        var response = new BuyerListResponse
        {
            Buyers = buyers.Select(b => new SingleBuyerResponse
            {
                Id = b.Id,
                BuyerType = b.BuyerType,
                Name = b.Name,
                Email = b.Email,
                Phone = b.Phone,
                CompanyName = b.CompanyName,
                TaxNumber = b.TaxNumber,
                TaxOffice = b.TaxOffice,
                Description = b.Description,
                LinkedUserId = b.LinkedUserId,
                IsDeleted = b.IsDeleted,
                CreatedAt = b.CreatedAt,
                CreatedBy = b.CreatedBy,
                UpdatedAt = b.UpdatedAt,
                UpdatedBy = b.UpdatedBy,
                DeletedAt = b.DeletedAt,
                DeletedBy = b.DeletedBy,
                DeleteReason = b.DeleteReason
            }).ToList(),
        };
        return ControllerResponseBuilder.Success(response);
    }

    [Cacheable(CacheKeyPrefix = nameof(CacheKeys.BuyersPrefix), ExpirationInMinutes = 60)]
    [EnsureUserIsValid(new[] { "id" })]
    public async Task<BaseControllerResponse<SingleBuyerResponse>> GetBuyerByIdAsync(Guid id)
    {
        var buyer = await _buyerRepository.GetByIdAsync(id);
        if (buyer == null)
            throw new NotFoundException("BuyerNotFound", id);
        var response = new SingleBuyerResponse
        {
            Id = buyer.Id,
            BuyerType = buyer.BuyerType,
            Name = buyer.Name,
            Email = buyer.Email,
            Phone = buyer.Phone,
            CompanyName = buyer.CompanyName,
            TaxNumber = buyer.TaxNumber,
            TaxOffice = buyer.TaxOffice,
            Description = buyer.Description,
            LinkedUserId = buyer.LinkedUserId,
            IsDeleted = buyer.IsDeleted,
            CreatedAt = buyer.CreatedAt,
            CreatedBy = buyer.CreatedBy,
            UpdatedAt = buyer.UpdatedAt,
            UpdatedBy = buyer.UpdatedBy,
            DeletedAt = buyer.DeletedAt,
            DeletedBy = buyer.DeletedBy,
            DeleteReason = buyer.DeleteReason
        };
        return ControllerResponseBuilder.Success(response);
    }

    [CacheRemove(nameof(CacheKeys.BuyersPattern))]
    public async Task<BaseControllerResponse<SingleBuyerResponse>> CreateBuyerAsync(CreateBuyerRequest request)
    {
        // Business Rules: Email/Phone unique kontrolü
        if (!await IsEmailUniqueAsync(request.Email))
            throw new BusinessRulesException("Buyer.EmailAlreadyExists");
        if (!await IsPhoneUniqueAsync(request.Phone))
            throw new BusinessRulesException("Buyer.PhoneAlreadyExists");

        var userId = _httpContextAccessor.HttpContext.GetUserId();
        var buyer = new BuyerEntity
        {
            Name = request.Name,
            //Surname = request.Surname,
            Email = request.Email,
            Phone = request.Phone,
            //Address = request.Address,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = userId,
            IsDeleted = false
        };
        
        // Tek insert işlemi - UnitOfWork gerekmez
        await _buyerRepository.CreateAsync(buyer);
        
        var response = new SingleBuyerResponse
        {
            Id = buyer.Id,
            BuyerType = buyer.BuyerType,
            Name = buyer.Name,
            Email = buyer.Email,
            Phone = buyer.Phone,
            CompanyName = buyer.CompanyName,
            TaxNumber = buyer.TaxNumber,
            TaxOffice = buyer.TaxOffice,
            Description = buyer.Description,
            LinkedUserId = buyer.LinkedUserId,
            IsDeleted = buyer.IsDeleted,
            CreatedAt = buyer.CreatedAt,
            CreatedBy = buyer.CreatedBy,
            UpdatedAt = buyer.UpdatedAt,
            UpdatedBy = buyer.UpdatedBy,
            DeletedAt = buyer.DeletedAt,
            DeletedBy = buyer.DeletedBy,
            DeleteReason = buyer.DeleteReason
        };
        return ControllerResponseBuilder.Success(response, "BuyerCreatedSuccessfully");
    }

    [CacheRemove(nameof(CacheKeys.BuyersPattern))]
    [EnsureUserIsValid(new[] { "id" })]
    public async Task<BaseControllerResponse<SingleBuyerResponse>> UpdateBuyerAsync(Guid id, UpdateBuyerRequest request)
    {
        var buyer = await _buyerRepository.GetByIdAsync(id);
        if (buyer == null)
            throw new NotFoundException("BuyerNotFound", id);

        // Business Rules: Email/Phone unique kontrolü (kendisi hariç)
        if (!await IsEmailUniqueAsync(request.Email, id))
            throw new BusinessRulesException("Buyer.EmailAlreadyExists");
        if (!await IsPhoneUniqueAsync(request.Phone, id))
            throw new BusinessRulesException("Buyer.PhoneAlreadyExists");

        var userId = _httpContextAccessor.HttpContext.GetUserId();
        buyer.Name = request.Name;
        //buyer.Surname = request.Surname;
        buyer.Email = request.Email;
        buyer.Phone = request.Phone;
        //buyer.Address = request.Address;
        buyer.UpdatedAt = DateTime.UtcNow;
        buyer.UpdatedBy = userId;

        // Tek update işlemi - UnitOfWork gerekmez
        await _buyerRepository.UpdateAsync(buyer);
        
        var response = new SingleBuyerResponse
        {
            Id = buyer.Id,
            BuyerType = buyer.BuyerType,
            Name = buyer.Name,
            Email = buyer.Email,
            Phone = buyer.Phone,
            CompanyName = buyer.CompanyName,
            TaxNumber = buyer.TaxNumber,
            TaxOffice = buyer.TaxOffice,
            Description = buyer.Description,
            LinkedUserId = buyer.LinkedUserId,
            IsDeleted = buyer.IsDeleted,
            CreatedAt = buyer.CreatedAt,
            CreatedBy = buyer.CreatedBy,
            UpdatedAt = buyer.UpdatedAt,
            UpdatedBy = buyer.UpdatedBy,
            DeletedAt = buyer.DeletedAt,
            DeletedBy = buyer.DeletedBy,
            DeleteReason = buyer.DeleteReason
        };
        return ControllerResponseBuilder.Success(response, "BuyerUpdatedSuccessfully");
    }

    [CacheRemove(nameof(CacheKeys.BuyersPattern))]
    [EnsureUserIsValid(new[] { "id" })]
    public async Task<BaseControllerResponse> SoftDeleteBuyerAsync(Guid id)
    {
        var buyer = await _buyerRepository.GetByIdAsync(id);
        if (buyer == null)
            throw new NotFoundException("BuyerNotFound", id);

        var userId = _httpContextAccessor.HttpContext.GetUserId();
        buyer.IsDeleted = true;
        buyer.DeletedAt = DateTime.UtcNow;
        buyer.DeletedBy = userId;

        // Tek update işlemi - UnitOfWork gerekmez
        await _buyerRepository.UpdateAsync(buyer);
        
        return ControllerResponseBuilder.Success();
    }

    [HasPermission(Permissions.Common.HardDelete)]
    [CacheRemove(nameof(CacheKeys.BuyersPattern))]
    [EnsureUserIsValid(new[] { "id" })]
    public async Task<BaseControllerResponse> HardDeleteBuyerAsync(Guid id)
    {
        var buyer = await _buyerRepository.GetByIdAsync(id);
        if (buyer == null)
            throw new NotFoundException("BuyerNotFound", id);
        
        // Tek delete işlemi - UnitOfWork gerekmez
        await _buyerRepository.DeleteAsync(id);
        
        return ControllerResponseBuilder.Success();
    }

    [Cacheable(CacheKeyPrefix = nameof(CacheKeys.BuyersPrefix), ExpirationInMinutes = 60)]
    public async Task<BaseControllerResponse<SingleBuyerResponse>> GetBuyerByEmailAsync(string email)
    {
        var buyer = await _buyerRepository.GetBuyerByEmailAsync(email);
        if (buyer == null)
            throw new NotFoundException("BuyerNotFoundWithEmail", email);
        var response = new SingleBuyerResponse
        {
            Id = buyer.Id,
            BuyerType = buyer.BuyerType,
            Name = buyer.Name,
            Email = buyer.Email,
            Phone = buyer.Phone,
            CompanyName = buyer.CompanyName,
            TaxNumber = buyer.TaxNumber,
            TaxOffice = buyer.TaxOffice,
            Description = buyer.Description,
            LinkedUserId = buyer.LinkedUserId,
            IsDeleted = buyer.IsDeleted,
            CreatedAt = buyer.CreatedAt,
            CreatedBy = buyer.CreatedBy,
            UpdatedAt = buyer.UpdatedAt,
            UpdatedBy = buyer.UpdatedBy,
            DeletedAt = buyer.DeletedAt,
            DeletedBy = buyer.DeletedBy,
            DeleteReason = buyer.DeleteReason
        };
        return ControllerResponseBuilder.Success(response);
    }

    [Cacheable(CacheKeyPrefix = nameof(CacheKeys.BuyersPrefix), ExpirationInMinutes = 60)]
    public async Task<BaseControllerResponse<SingleBuyerResponse>> GetBuyerByPhoneAsync(string phone)
    {
        var buyer = await _buyerRepository.GetBuyerByPhoneAsync(phone);
        if (buyer == null)
            throw new NotFoundException("BuyerNotFoundWithPhone", phone);
        var response = new SingleBuyerResponse
        {
            Id = buyer.Id,
            BuyerType = buyer.BuyerType,
            Name = buyer.Name,
            Email = buyer.Email,
            Phone = buyer.Phone,
            CompanyName = buyer.CompanyName,
            TaxNumber = buyer.TaxNumber,
            TaxOffice = buyer.TaxOffice,
            Description = buyer.Description,
            LinkedUserId = buyer.LinkedUserId,
            IsDeleted = buyer.IsDeleted,
            CreatedAt = buyer.CreatedAt,
            CreatedBy = buyer.CreatedBy,
            UpdatedAt = buyer.UpdatedAt,
            UpdatedBy = buyer.UpdatedBy,
            DeletedAt = buyer.DeletedAt,
            DeletedBy = buyer.DeletedBy,
            DeleteReason = buyer.DeleteReason
        };
        return ControllerResponseBuilder.Success(response);
    }

    public async Task<bool> IsEmailUniqueAsync(string email, Guid? excludeId = null)
    {
        return await _buyerRepository.IsEmailUniqueAsync(email, excludeId);
    }

    public async Task<bool> IsPhoneUniqueAsync(string phone, Guid? excludeId = null)
    {
        return await _buyerRepository.IsPhoneUniqueAsync(phone, excludeId);
    }

    [Cacheable(CacheKeyPrefix = nameof(CacheKeys.BuyersPrefix), ExpirationInMinutes = 60)]
    public async Task<BaseControllerResponse<BuyerListPaginatedResponse>> GetFilteredPaginatedBuyersAsync(BuyerFilterRequest request)
    {
        var (data, totalCount) = await _buyerRepository.GetFilteredPaginatedAsync(request);

        var mapped = data.Select(b => new SingleBuyerResponse
        {
            Id = b.Id,
            BuyerType = b.BuyerType,
            Name = b.Name,
            Email = b.Email,
            Phone = b.Phone,
            CompanyName = b.CompanyName,
            TaxNumber = b.TaxNumber,
            TaxOffice = b.TaxOffice,
            Description = b.Description,
            LinkedUserId = b.LinkedUserId,
            IsDeleted = b.IsDeleted,
            CreatedAt = b.CreatedAt,
            CreatedBy = b.CreatedBy,
            UpdatedAt = b.UpdatedAt,
            UpdatedBy = b.UpdatedBy,
            DeletedAt = b.DeletedAt,
            DeletedBy = b.DeletedBy,
            DeleteReason = b.DeleteReason
        }).ToList();

        var paginated = new PaginatedData<SingleBuyerResponse>
        {
            Items = mapped,
            TotalCount = totalCount,
            Page = request.Page,
            PageSize = request.PageSize,
            TotalPage = (int)Math.Ceiling((double)totalCount / request.PageSize)
        };

        return ControllerResponseBuilder.Success(new BuyerListPaginatedResponse
        {
            Buyers = paginated
        });
    }
}