using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace RaftLabsAssignement.Models
{
    public class ApiResponse
    {
        public class PagedUserResponse
        {
            [JsonPropertyName("page")]
            public int Page { get; set; }

            [JsonPropertyName("per_page")]
            public int PerPage { get; set; }

            [JsonPropertyName("total")]
            public int Total { get; set; }

            [JsonPropertyName("total_pages")]
            public int TotalPages { get; set; }

            [JsonPropertyName("data")]
            public List<UserDto> Data { get; set; } = new();

        }

        public class UserDto
        {
            [JsonPropertyName("id")]
            public int Id { get; set; }
            [JsonPropertyName("email")]
            public string Email { get; set; } = string.Empty;
            [JsonPropertyName("first_name")]
            public string FirstName { get; set; } = string.Empty;
            [JsonPropertyName("last_name")]
            public string LastName { get; set; } = string.Empty;
            [JsonPropertyName("avatar")]
            public string Avatar { get; set; } = string.Empty;
        }
    }
}
