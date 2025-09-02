using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using AutoFixture;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Moq;
using Sky.Template.Backend.Application.Services.User;
using Sky.Template.Backend.Contract.Requests.CartItems;
using Sky.Template.Backend.Infrastructure.Entities.Product;
using Sky.Template.Backend.Infrastructure.Entities.Sales;
using Sky.Template.Backend.Infrastructure.Repositories;
using Xunit;

namespace Sky.Template.Backend.UnitTests;

public class UserCartItemServiceTests
{
    private readonly Fixture _fixture = new();

    private UserCartItemService CreateService(
        Mock<ICartItemRepository> itemRepo,
        Mock<ICartRepository> cartRepo,
        Mock<IProductRepository> productRepo)
    {
        var accessor = new Mock<IHttpContextAccessor>();
        var context = new DefaultHttpContext();
        context.User = new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim(ClaimTypes.NameIdentifier, Guid.NewGuid().ToString()) }));
        context.Request.Headers["Accept-Language"] = "en";
        accessor.Setup(a => a.HttpContext).Returns(context);
        return new UserCartItemService(itemRepo.Object, cartRepo.Object, productRepo.Object, accessor.Object);
    }

    [Fact]
    public async Task GetCartItems_ReturnsItems()
    {
        var itemRepo = new Mock<ICartItemRepository>();
        itemRepo.Setup(r => r.GetByCartIdAsync(It.IsAny<Guid>(), It.IsAny<string>()))
            .ReturnsAsync(new List<CartItemLocalizedJoinEntity> { new() { Id = Guid.NewGuid(), CartId = Guid.NewGuid(), ProductId = Guid.NewGuid(), Quantity = 1, UnitPrice = 10, Currency = "USD", Status = "ACTIVE", ProductName = "Prod", CreatedAt = DateTime.UtcNow } });
        var cartRepo = new Mock<ICartRepository>();
        var productRepo = new Mock<IProductRepository>();
        var service = CreateService(itemRepo, cartRepo, productRepo);

        var result = await service.GetByCartIdAsync(Guid.NewGuid());

        result.Data.Should().NotBeNull();
        itemRepo.Verify(r => r.GetByCartIdAsync(It.IsAny<Guid>(), It.IsAny<string>()), Times.Once);
    }

    [Fact]
    public async Task CreateCartItem_CallsCreate()
    {
        var cartId = Guid.NewGuid();
        var productId = Guid.NewGuid();
        var cartRepo = new Mock<ICartRepository>();
        cartRepo.Setup(r => r.GetByIdAsync(cartId)).ReturnsAsync(new CartEntity { Id = cartId, Currency = "USD", Status = "OPEN", CreatedAt = DateTime.UtcNow });
        cartRepo.Setup(r => r.CalculateTotalPriceAsync(cartId)).ReturnsAsync(0);
        cartRepo.Setup(r => r.UpdateAsync(It.IsAny<CartEntity>())).ReturnsAsync(new CartEntity());

        var productRepo = new Mock<IProductRepository>();
        productRepo.Setup(r => r.GetByIdAsync(productId)).ReturnsAsync(new ProductEntity { Id = productId, Price = 5, Status = "ACTIVE", CreatedAt = DateTime.UtcNow });

        var itemRepo = new Mock<ICartItemRepository>();
        itemRepo.Setup(r => r.GetByCartAndProductAsync(cartId, productId)).ReturnsAsync((CartItemEntity?)null);
        itemRepo.Setup(r => r.CreateAsync(It.IsAny<CartItemEntity>())).ReturnsAsync(new CartItemEntity { Id = Guid.NewGuid(), CartId = cartId, ProductId = productId, Quantity = 1, UnitPrice = 5, Currency = "USD", Status = "ACTIVE", CreatedAt = DateTime.UtcNow });
        itemRepo.Setup(r => r.GetByIdWithProductAsync(It.IsAny<Guid>(), It.IsAny<string>())).ReturnsAsync(new CartItemLocalizedJoinEntity { Id = Guid.NewGuid(), CartId = cartId, ProductId = productId, Quantity = 1, UnitPrice = 5, Currency = "USD", Status = "ACTIVE", ProductName = "Prod", CreatedAt = DateTime.UtcNow });

        var service = CreateService(itemRepo, cartRepo, productRepo);
        var request = new CreateCartItemRequest { CartId = cartId, ProductId = productId, Quantity = 1 };

        await service.CreateAsync(request);

        itemRepo.Verify(r => r.CreateAsync(It.IsAny<CartItemEntity>()), Times.Once);
    }
}
