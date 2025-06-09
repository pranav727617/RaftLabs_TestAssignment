using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Moq.Protected;
using RaftLabsAssignement.Config;
using RaftLabsAssignement.Models;
using RaftLabsAssignement.Services;
using Xunit;

public class ExternalUserServiceTests
{
    private readonly IMemoryCache _memoryCache;
    private readonly ILogger<ExternalUserService> _logger;
    private readonly IOptions<ReqresApiOptions> _options;

    public ExternalUserServiceTests()
    {
        _memoryCache = new MemoryCache(new MemoryCacheOptions());
        _logger = new LoggerFactory().CreateLogger<ExternalUserService>();
        _options = Options.Create(new ReqresApiOptions
        {
            BaseUrl = "https://reqres.in/api/",
            CacheExpirationSeconds = 60
        });
    }

    private HttpClient CreateHttpClientMock(HttpResponseMessage responseMessage)
    {
        var handlerMock = new Mock<HttpMessageHandler>(MockBehavior.Strict);

        handlerMock
           .Protected()
           .Setup<Task<HttpResponseMessage>>(
              "SendAsync",
              ItExpr.IsAny<HttpRequestMessage>(),
              ItExpr.IsAny<CancellationToken>())
           .ReturnsAsync(responseMessage)
           .Verifiable();

        return new HttpClient(handlerMock.Object)
        {
            BaseAddress = new Uri(_options.Value.BaseUrl)
        };
    }

    [Fact]
    public async Task GetUserByIdAsync_ReturnsUser_WhenUserExists()
    {
        // Arrange
        var jsonResponse = @"{
            ""data"": {
                ""id"": 1,
                ""email"": ""george.bluth@reqres.in"",
                ""first_name"": ""George"",
                ""last_name"": ""Bluth"",
                ""avatar"": ""https://reqres.in/img/faces/1-image.jpg""
            }
        }";

        var responseMessage = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(jsonResponse, System.Text.Encoding.UTF8, "application/json")
        };

        var httpClient = CreateHttpClientMock(responseMessage);
        var service = new ExternalUserService(httpClient, _options, _logger, _memoryCache);

        // Act
        var user = await service.GetUserByIdAsync(1);

        // Assert
        Assert.NotNull(user);
        Assert.Equal(1, user.Id);
        Assert.Equal("george.bluth@reqres.in", user.Email);
        Assert.Equal("George", user.FirstName);
        Assert.Equal("Bluth", user.LastName);
        Assert.Equal("https://reqres.in/img/faces/1-image.jpg", user.Avatar);
    }

    [Fact]
    public async Task GetUserByIdAsync_ReturnsNull_WhenUserNotFound()
    {
        // Arrange
        var responseMessage = new HttpResponseMessage(HttpStatusCode.NotFound);
        var httpClient = CreateHttpClientMock(responseMessage);
        var service = new ExternalUserService(httpClient, _options, _logger, _memoryCache);

        // Act
        var user = await service.GetUserByIdAsync(999);

        // Assert
        Assert.Null(user);
    }

    [Fact]
    public async Task GetAllUsersAsync_ReturnsAllUsersAcrossPages()
    {
        // Arrange
        var page1Json = @"{
            ""page"": 1,
            ""per_page"": 2,
            ""total"": 4,
            ""total_pages"": 2,
            ""data"": [
                {
                    ""id"": 1,
                    ""email"": ""george.bluth@reqres.in"",
                    ""first_name"": ""George"",
                    ""last_name"": ""Bluth"",
                    ""avatar"": ""https://reqres.in/img/faces/1-image.jpg""
                },
                {
                    ""id"": 2,
                    ""email"": ""janet.weaver@reqres.in"",
                    ""first_name"": ""Janet"",
                    ""last_name"": ""Weaver"",
                    ""avatar"": ""https://reqres.in/img/faces/2-image.jpg""
                }
            ]
        }";

        var page2Json = @"{
            ""page"": 2,
            ""per_page"": 2,
            ""total"": 4,
            ""total_pages"": 2,
            ""data"": [
                {
                    ""id"": 3,
                    ""email"": ""emma.wong@reqres.in"",
                    ""first_name"": ""Emma"",
                    ""last_name"": ""Wong"",
                    ""avatar"": ""https://reqres.in/img/faces/3-image.jpg""
                },
                {
                    ""id"": 4,
                    ""email"": ""eve.holt@reqres.in"",
                    ""first_name"": ""Eve"",
                    ""last_name"": ""Holt"",
                    ""avatar"": ""https://reqres.in/img/faces/4-image.jpg""
                }
            ]
        }";

        var handlerMock = new Mock<HttpMessageHandler>(MockBehavior.Strict);

        handlerMock
           .Protected()
           .SetupSequence<Task<HttpResponseMessage>>(
              "SendAsync",
              ItExpr.Is<HttpRequestMessage>(req => req.RequestUri!.ToString().Contains("page=1")),
              ItExpr.IsAny<CancellationToken>())
           .ReturnsAsync(new HttpResponseMessage
           {
               StatusCode = HttpStatusCode.OK,
               Content = new StringContent(page1Json, System.Text.Encoding.UTF8, "application/json"),
           });

        handlerMock
           .Protected()
           .SetupSequence<Task<HttpResponseMessage>>(
              "SendAsync",
              ItExpr.Is<HttpRequestMessage>(req => req.RequestUri!.ToString().Contains("page=2")),
              ItExpr.IsAny<CancellationToken>())
           .ReturnsAsync(new HttpResponseMessage
           {
               StatusCode = HttpStatusCode.OK,
               Content = new StringContent(page2Json, System.Text.Encoding.UTF8, "application/json"),
           });

        var httpClient = new HttpClient(handlerMock.Object)
        {
            BaseAddress = new Uri(_options.Value.BaseUrl)
        };

        var service = new ExternalUserService(httpClient, _options, _logger, _memoryCache);

        // Act
        var users = await service.GetAllUsersAsync();

        // Assert
        Assert.NotNull(users);
        var userList = new List<User>(users);
        Assert.Equal(4, userList.Count);
        Assert.Equal("george.bluth@reqres.in", userList[0].Email);
        Assert.Equal("eve.holt@reqres.in", userList[3].Email);
    }

    [Fact]
    public async Task GetUserByIdAsync_ThrowsOnHttpRequestException_ForUnexpectedStatus()
    {
        // Arrange
        var responseMessage = new HttpResponseMessage(HttpStatusCode.InternalServerError);
        var httpClient = CreateHttpClientMock(responseMessage);
        var service = new ExternalUserService(httpClient, _options, _logger, _memoryCache);

        // Act & Assert
        await Assert.ThrowsAsync<HttpRequestException>(() => service.GetUserByIdAsync(1));
    }
}
