using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RaftLabsAssignement.Config;
using RaftLabsAssignement.Services;
using RaftLabsAssignement.Models;
using static RaftLabsAssignement.Models.ApiResponse;

namespace RaftLabsAssignement.Services
{
    public class ExternalUserService : IExternalUserService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<ExternalUserService> _logger;
        private readonly ReqresApiOptions _options;
        private readonly IMemoryCache _cache;

        public ExternalUserService(
            HttpClient httpClient,
            IOptions<ReqresApiOptions> options,
            ILogger<ExternalUserService> logger,
            IMemoryCache cache)
        {
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _options = options?.Value ?? throw new ArgumentNullException(nameof(options));
            _cache = cache ?? throw new ArgumentNullException(nameof(cache));
        }

        public async Task<User?> GetUserByIdAsync(int userId)
        {
            string cacheKey = $"User_{userId}";
            if (_cache.TryGetValue<User>(cacheKey, out var cachedUser))
            {
                _logger.LogInformation($"Cached user: {userId}");
                return cachedUser;
            }

            try
            {
                var response = await _httpClient.GetAsync($"{_options.BaseUrl}users/{userId}");
                if (response.IsSuccessStatusCode)
                {
                    var wrapper = await response.Content.ReadFromJsonAsync<ApiSingleUserResponse>();
                    if (wrapper?.Data != null)
                    {
                        var user = MapUserDtoToUser(wrapper.Data);
                        _cache.Set(cacheKey, user, TimeSpan.FromSeconds(_options.CacheExpirationSeconds));
                        return user;
                    }
                    else
                    {
                        _logger.LogWarning($"No Data for userId {userId}");
                        return null;
                    }
                }
                else if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    _logger.LogWarning($"Not Found {userId} ");
                    return null;
                }
                else
                {
                    _logger.LogError($"Failed to get user {userId}. Status: {response.StatusCode}");
                    throw new HttpRequestException($"Unexpected status code: {response.StatusCode}");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Exception: {ex.ToString()} while getting user {userId}");
                throw;
            }
        }

        public async Task<IEnumerable<User>> GetAllUsersAsync()
        {
            const string cacheKey = "AllUsers";
            if (_cache.TryGetValue<IEnumerable<User>>(cacheKey, out var cachedUsers))
            {
                _logger.LogInformation("Returning cached all users");
                return cachedUsers;
            }

            var allUsers = new List<User>();

            int page = 1;
            int totalPages;

            do
            {
                try
                {
                    var pagedResponse = await _httpClient.GetFromJsonAsync<PagedUserResponse>($"{_options.BaseUrl}users?page={page}");
                    if (pagedResponse?.Data == null)
                    {
                        _logger.LogWarning($"No user data found on page {page}");
                        break;
                    }

                    foreach (var dto in pagedResponse.Data)
                    {
                        allUsers.Add(MapUserDtoToUser(dto));
                    }

                    totalPages = pagedResponse.TotalPages;
                    page++;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error while fetching paged users");
                    throw;
                }
            } while (page <= totalPages);

            _cache.Set(cacheKey, allUsers, TimeSpan.FromSeconds(_options.CacheExpirationSeconds));

            return allUsers;
        }

        private User MapUserDtoToUser(UserDto dto) =>
            new User
            {
                Id = dto.Id,
                Email = dto.Email,
                FirstName = dto.FirstName,
                LastName = dto.LastName,
                Avatar = dto.Avatar
            };

        private class ApiSingleUserResponse
        {
            public UserDto Data { get; set; } = new UserDto();
        }
    }
}
