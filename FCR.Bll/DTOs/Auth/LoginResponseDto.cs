using System;
using System.Collections.Generic;

namespace FCR.Bll.DTOs.Auth
{
    public class LoginResponseDto
    {
        public string UserId { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? DisplayName { get; set; }
        public string Token { get; set; } = string.Empty;
        public string? RefreshToken { get; set; }
        public DateTime TokenExpiration { get; set; }
        public List<string> Roles { get; set; } = new List<string>();
    }
}