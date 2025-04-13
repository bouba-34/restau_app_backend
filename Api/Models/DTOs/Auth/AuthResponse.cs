using System.ComponentModel.DataAnnotations;
using backend.Api.Models.Entities;

namespace backend.Api.Models.DTOs.Auth
{
    
    public class AuthResponse
    {
        public string Id { get; set; }
        public string Username { get; set; }
        public string Email { get; set; }
        public string Token { get; set; }
        public DateTime Expiration { get; set; }
        public UserType UserType { get; set; }
    }
}