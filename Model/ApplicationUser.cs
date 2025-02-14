using Microsoft.AspNetCore.Identity;

namespace WebApplication1.Model
{
    public class ApplicationUser : IdentityUser
    {
        public string FullName { get; set; } // Full Name
        public string CreditCardNo { get; set; } // Credit Card Number
        public string Gender { get; set; } // Gender
        public string MobileNo { get; set; } // Mobile Number
        public string DeliveryAddress { get; set; } // Delivery Address
        public string AboutMe { get; set; } // About Me
        public string PhotoPath { get; set; } // Path to the uploaded photo
        public string? SessionToken { get; set; } // Session token for tracking active sessions
        // Add password history
        public List<string> PasswordHistory { get; set; } = new List<string>();
    }
}