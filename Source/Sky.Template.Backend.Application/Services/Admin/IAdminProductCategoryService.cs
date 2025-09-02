using Sky.Template.Backend.Core.Constants;
using Sky.Template.Backend.Contract.Requests.Products;
using Sky.Template.Backend.Core.BaseResponse;
using Sky.Template.Backend.Contract.Responses.ProductResponses;
using Sky.Template.Backend.Core.Exceptions;
using Sky.Template.Backend.Infrastructure.Entities.Product;
using Sky.Template.Backend.Infrastructure.Repositories;
using Sky.Template.Backend.Infrastructure.Repositories.UnitOfWork;
using Microsoft.AspNetCore.Http;
using Sky.Template.Backend.Core.Extensions;
using Sky.Template.Backend.Core.Aspects.Autofac.SecuredOperation;
using Sky.Template.Backend.Core.Aspects.Autofac.Caching;
using Sky.Template.Backend.Core.Aspects.Autofac.Authorization;
using Sky.Template.Backend.Core.CrossCuttingConcerns.Caching;

namespace Sky.Template.Backend.Application.Services.Admin;

public interface IAdminProductCategoryService
{
    Task<BaseControllerResponse<ProductCategoryListResponse>> GetAllCategoriesAsync();
    Task<BaseControllerResponse<SingleProductCategoryResponse>> GetCategoryByIdAsync(Guid id);
    Task<BaseControllerResponse<SingleProductCategoryResponse>> CreateCategoryAsync(CreateProductCategoryRequest request);
    Task<BaseControllerResponse<SingleProductCategoryResponse>> UpdateCategoryAsync(Guid id, UpdateProductCategoryRequest request);
    Task<BaseControllerResponse> SoftDeleteCategoryAsync(Guid id);
    Task<BaseControllerResponse> HardDeleteCategoryAsync(Guid id);
    Task<BaseControllerResponse<SingleProductCategoryResponse>> GetCategoryByNameAsync(string name);
    Task<BaseControllerResponse<ProductCategoryListPaginatedResponse>> GetFilteredPaginatedCategoriesAsync(ProductCategoryFilterRequest request);
    Task<bool> IsCategoryNameUniqueAsync(string name, Guid? excludeId = null);
}

public class AdminProductCategoryService : IAdminProductCategoryService
{
    private readonly IProductCategoryRepository _categoryRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public AdminProductCategoryService(IProductCategoryRepository categoryRepository, IUnitOfWork unitOfWork, IHttpContextAccessor httpContextAccessor)
    {
        _categoryRepository = categoryRepository;
        _unitOfWork = unitOfWork;
        _httpContextAccessor = httpContextAccessor;
    }

    [Cacheable(CacheKeyPrefix = nameof(CacheKeys.CategoriesPrefix), ExpirationInMinutes = 60)]
    public async Task<BaseControllerResponse<ProductCategoryListResponse>> GetAllCategoriesAsync()
    {
        var categories = await _categoryRepository.GetAllAsync();
        var response = new ProductCategoryListResponse
        {
            Categories = categories.Select(c => new SingleProductCategoryResponse
            {
                Id = c.Id,
                Name = c.Name,
                ParentCategoryId = c.ParentCategoryId,
                Description = c.Description,
                IsDeleted = c.IsDeleted,
                CreatedAt = c.CreatedAt,
                CreatedBy = c.CreatedBy,
                UpdatedAt = c.UpdatedAt,
                UpdatedBy = c.UpdatedBy,
                DeletedAt = c.DeletedAt,
                DeletedBy = c.DeletedBy,
                DeleteReason = c.DeleteReason
            }).ToList(),
            TotalCount = categories.Count()
        };
        return ControllerResponseBuilder.Success(response);
    }

    [Cacheable(CacheKeyPrefix = nameof(CacheKeys.CategoriesPrefix), ExpirationInMinutes = 60)]
    [EnsureUserIsValid(new[] { "id" })]
    public async Task<BaseControllerResponse<SingleProductCategoryResponse>> GetCategoryByIdAsync(Guid id)
    {
        var category = await _categoryRepository.GetByIdAsync(id);
        if (category == null)
            throw new NotFoundException("CategoryNotFound", id);
        var response = new SingleProductCategoryResponse
        {
            Id = category.Id,
            Name = category.Name,
            ParentCategoryId = category.ParentCategoryId,
            Description = category.Description,
            IsDeleted = category.IsDeleted,
            CreatedAt = category.CreatedAt,
            CreatedBy = category.CreatedBy,
            UpdatedAt = category.UpdatedAt,
            UpdatedBy = category.UpdatedBy,
            DeletedAt = category.DeletedAt,
            DeletedBy = category.DeletedBy,
            DeleteReason = category.DeleteReason
        };
        return ControllerResponseBuilder.Success(response);
    }

    [InvalidateCache(CacheKeys.CategoriesPattern)]
    public async Task<BaseControllerResponse<SingleProductCategoryResponse>> CreateCategoryAsync(CreateProductCategoryRequest request)
    {
        if (!await IsCategoryNameUniqueAsync(request.Name))
            throw new BusinessRulesException("Category.NameAlreadyExists");

        var userId = _httpContextAccessor.HttpContext.GetUserId();
        var category = new ProductCategoryEntity
        {
            Name = request.Name,
            Description = request.Description,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = userId,
            IsDeleted = false
        };

        await _categoryRepository.CreateAsync(category);

        var response = new SingleProductCategoryResponse
        {
            Id = category.Id,
            Name = category.Name,
            ParentCategoryId = category.ParentCategoryId,
            Description = category.Description,
            IsDeleted = category.IsDeleted,
            CreatedAt = category.CreatedAt,
            CreatedBy = category.CreatedBy,
            UpdatedAt = category.UpdatedAt,
            UpdatedBy = category.UpdatedBy,
            DeletedAt = category.DeletedAt,
            DeletedBy = category.DeletedBy,
            DeleteReason = category.DeleteReason
        };
        return ControllerResponseBuilder.Success(response, "CategoryCreatedSuccessfully");
    }

    [InvalidateCache(CacheKeys.CategoriesPattern)]
    [EnsureUserIsValid(new[] { "id" })]
    public async Task<BaseControllerResponse<SingleProductCategoryResponse>> UpdateCategoryAsync(Guid id, UpdateProductCategoryRequest request)
    {
        var category = await _categoryRepository.GetByIdAsync(id);
        if (category == null)
            throw new NotFoundException("CategoryNotFound", id);
        if (!await IsCategoryNameUniqueAsync(request.Name, id))
            throw new BusinessRulesException("Category.NameAlreadyExists");

        var userId = _httpContextAccessor.HttpContext.GetUserId();
        category.Name = request.Name;
        category.Description = request.Description;
        category.UpdatedAt = DateTime.UtcNow;
        category.UpdatedBy = userId;

        await _categoryRepository.UpdateAsync(category);

        var response = new SingleProductCategoryResponse
        {
            Id = category.Id,
            Name = category.Name,
            ParentCategoryId = category.ParentCategoryId,
            Description = category.Description,
            IsDeleted = category.IsDeleted,
            CreatedAt = category.CreatedAt,
            CreatedBy = category.CreatedBy,
            UpdatedAt = category.UpdatedAt,
            UpdatedBy = category.UpdatedBy,
            DeletedAt = category.DeletedAt,
            DeletedBy = category.DeletedBy,
            DeleteReason = category.DeleteReason
        };
        return ControllerResponseBuilder.Success(response, "CategoryUpdatedSuccessfully");
    }

    [InvalidateCache(CacheKeys.CategoriesPattern)]
    [EnsureUserIsValid(new[] { "id" })]
    public async Task<BaseControllerResponse> SoftDeleteCategoryAsync(Guid id)
    {
        var category = await _categoryRepository.GetByIdAsync(id);
        if (category == null)
            throw new NotFoundException("CategoryNotFound", id);

        var userId = _httpContextAccessor.HttpContext.GetUserId();
        category.IsDeleted = true;
        category.DeletedAt = DateTime.UtcNow;
        category.DeletedBy = userId;

        await _categoryRepository.UpdateAsync(category);

        return ControllerResponseBuilder.Success();
    }

    [HasPermission(Permissions.Common.HardDelete)]
    [InvalidateCache(CacheKeys.CategoriesPattern)]
    [EnsureUserIsValid(new[] { "id" })]
    public async Task<BaseControllerResponse> HardDeleteCategoryAsync(Guid id)
    {
        var category = await _categoryRepository.GetByIdAsync(id);
        if (category == null)
            throw new NotFoundException("CategoryNotFound", id);

        await _categoryRepository.DeleteAsync(id);

        return ControllerResponseBuilder.Success();
    }

    [Cacheable(CacheKeyPrefix = nameof(CacheKeys.CategoriesPrefix), ExpirationInMinutes = 60)]
    public async Task<BaseControllerResponse<SingleProductCategoryResponse>> GetCategoryByNameAsync(string name)
    {
        var category = await _categoryRepository.GetCategoryByNameAsync(name);
        if (category == null)
            throw new NotFoundException("CategoryNotFoundWithName", name);
        var response = new SingleProductCategoryResponse
        {
            Id = category.Id,
            Name = category.Name,
            ParentCategoryId = category.ParentCategoryId,
            Description = category.Description,
            IsDeleted = category.IsDeleted,
            CreatedAt = category.CreatedAt,
            CreatedBy = category.CreatedBy,
            UpdatedAt = category.UpdatedAt,
            UpdatedBy = category.UpdatedBy,
            DeletedAt = category.DeletedAt,
            DeletedBy = category.DeletedBy,
            DeleteReason = category.DeleteReason
        };
        return ControllerResponseBuilder.Success(response);
    }

    [Cacheable(CacheKeyPrefix = nameof(CacheKeys.CategoriesPrefix), ExpirationInMinutes = 60)]
    public async Task<BaseControllerResponse<ProductCategoryListPaginatedResponse>> GetFilteredPaginatedCategoriesAsync(ProductCategoryFilterRequest request)
    {
        var (data, totalCount) = await _categoryRepository.GetFilteredPaginatedAsync(request);

        var mapped = data.Select(c => new SingleProductCategoryResponse
        {
            Id = c.Id,
            Name = c.Name,
            ParentCategoryId = c.ParentCategoryId,
            Description = c.Description,
            IsDeleted = c.IsDeleted,
            CreatedAt = c.CreatedAt,
            CreatedBy = c.CreatedBy,
            UpdatedAt = c.UpdatedAt,
            UpdatedBy = c.UpdatedBy,
            DeletedAt = c.DeletedAt,
            DeletedBy = c.DeletedBy,
            DeleteReason = c.DeleteReason
        }).ToList();

        var paginated = new PaginatedData<SingleProductCategoryResponse>
        {
            Items = mapped,
            TotalCount = totalCount,
            Page = request.Page,
            PageSize = request.PageSize,
            TotalPage = (int)Math.Ceiling((double)totalCount / request.PageSize)
        };

        return ControllerResponseBuilder.Success(new ProductCategoryListPaginatedResponse
        {
            Categories = paginated
        });
    }

    public async Task<bool> IsCategoryNameUniqueAsync(string name, Guid? excludeId = null)
    {
        return await _categoryRepository.IsCategoryNameUniqueAsync(name, excludeId);
    }
}

