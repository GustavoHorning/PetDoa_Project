using PetDoa.Models;

namespace PetDoa.DTOs
{
    public class OAuthSignInResult
    {
        public Donor? Donor { get; set; } 
        public bool RequiresPasswordLogin { get; set; } = false; 
        public string? ErrorMessage { get; set; } 
        public bool Success => Donor != null && ErrorMessage == null && !RequiresPasswordLogin;
    }
}
