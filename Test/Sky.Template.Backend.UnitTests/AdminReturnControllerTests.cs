using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using AutoFixture;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Sky.Template.Backend.Application.Services.Admin;
using Sky.Template.Backend.Contract.Requests.Returns;
using Sky.Template.Backend.Contract.Responses.ReturnResponses;
using Sky.Template.Backend.Core.BaseResponse;
using Sky.Template.Backend.WebAPI.Controllers.Admin;
using Xunit;

namespace Sky.Template.Backend.UnitTests;

public class AdminReturnControllerTests
{
    private readonly Fixture _fixture = new();

    [Fact]
    public async Task GetReturns_ReturnsOkWithExpectedData()
    {
        var serviceMock = new Mock<IAdminReturnService>();
        var expectedItems = new List<ReturnResponse>() { new ReturnResponse() };
        var expectedPaginated = new ReturnListPaginatedResponse
        {
            Returns = new PaginatedData<ReturnResponse>
            {
                Items = expectedItems,
                TotalCount = expectedItems.Count,
                Page = 1,
                PageSize = 10,
                TotalPage = 1
            }
        };
        var expectedResponse = new BaseControllerResponse<ReturnListPaginatedResponse>
        {
            StatusCode = HttpStatusCode.OK,
            Data = expectedPaginated
        };

        serviceMock
            .Setup(s => s.GetReturnsAsync(It.IsAny<ReturnFilterRequest>()))
            .ReturnsAsync(expectedResponse);

        var controller = new ReturnController(serviceMock.Object);

        var result = await controller.GetReturns(new ReturnFilterRequest());

        result.Should().BeOfType<OkObjectResult>();
        var okResult = result as OkObjectResult;
        okResult!.Value.Should().BeEquivalentTo(expectedResponse);

        serviceMock.Verify(s => s.GetReturnsAsync(It.IsAny<ReturnFilterRequest>()), Times.Once);
    }

    [Fact]
    public async Task UpdateReturnStatus_ReturnsOk()
    {
        var serviceMock = new Mock<IAdminReturnService>();
        var response = ControllerResponseBuilder.Success(new ReturnResponse());
        serviceMock.Setup(s => s.UpdateReturnStatusAsync(It.IsAny<Guid>(), It.IsAny<UpdateReturnStatusRequest>())).ReturnsAsync(response);
        var controller = new ReturnController(serviceMock.Object);

        var result = await controller.UpdateReturnStatus(Guid.NewGuid(), new UpdateReturnStatusRequest());

        result.Should().BeOfType<OkObjectResult>();
        serviceMock.Verify(s => s.UpdateReturnStatusAsync(It.IsAny<Guid>(), It.IsAny<UpdateReturnStatusRequest>()), Times.Once);
    }

    [Fact]
    public async Task CreateReturn_ReturnsCreated()
    {
        var serviceMock = new Mock<IAdminReturnService>();
        var request = _fixture.Create<CreateReturnRequest>();
        var response = ControllerResponseBuilder.Success(new ReturnResponse(), "Created", HttpStatusCode.Created);
        serviceMock.Setup(s => s.CreateReturnAsync(request)).ReturnsAsync(response);
        var controller = new ReturnController(serviceMock.Object);

        var result = await controller.CreateReturn(request);

        result.Should().BeOfType<CreatedResult>();
        serviceMock.Verify(s => s.CreateReturnAsync(request), Times.Once);
    }

    [Fact]
    public async Task DeleteReturn_ReturnsOk()
    {
        var serviceMock = new Mock<IAdminReturnService>();
        var response = ControllerResponseBuilder.Success();
        serviceMock.Setup(s => s.DeleteReturnAsync(It.IsAny<Guid>())).ReturnsAsync(response);
        var controller = new ReturnController(serviceMock.Object);

        var result = await controller.DeleteReturn(Guid.NewGuid());

        result.Should().BeOfType<OkObjectResult>();
        serviceMock.Verify(s => s.DeleteReturnAsync(It.IsAny<Guid>()), Times.Once);
    }
}

