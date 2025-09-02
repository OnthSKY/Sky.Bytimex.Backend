using Sky.Template.Backend.Core.BaseResponse;
using Sky.Template.Backend.Contract.Responses.ProductResponses;
using Sky.Template.Backend.Infrastructure.Repositories;
using Sky.Template.Backend.Core.Aspects.Autofac.Caching;
using Sky.Template.Backend.Core.CrossCuttingConcerns.Caching;

namespace Sky.Template.Backend.Application.Services.User;

public interface IUserProductCategoryService
{
    Task<BaseControllerResponse<ProductCategoryListResponse>> GetAllCategoriesAsync();
    Task<BaseControllerResponse<SingleProductCategoryResponse>> GetCategoryByNameAsync(string name);
}

public class UserProductCategoryService : IUserProductCategoryService
{
    private readonly IProductCategoryRepository _categoryRepository;

    public UserProductCategoryService(IProductCategoryRepository categoryRepository)
    {
        _categoryRepository = categoryRepository;
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
    public async Task<BaseControllerResponse<SingleProductCategoryResponse>> GetCategoryByNameAsync(string name)
    {
        var category = await _categoryRepository.GetCategoryByNameAsync(name);
        if (category == null)
            throw new Core.Exceptions.NotFoundException("CategoryNotFoundWithName", name);
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
}

