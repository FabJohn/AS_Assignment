using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Threading.Tasks;
using WebApplication1.Model; // Ensure this namespace points to your ApplicationUser

namespace WebApplication1.Pages
{
    [Authorize]
    public class IndexModel : PageModel
    {
        private readonly UserManager<ApplicationUser> _userManager;

        public IndexModel(UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
        }

        public ApplicationUser CurrentUser { get; set; } // Use ApplicationUser directly

        public async Task OnGetAsync()
        {
            // Get the currently authenticated user
            CurrentUser = await _userManager.GetUserAsync(User);
        }

        // Function to decrypt the credit card number on the fly
        public string DecryptCreditCard(string encryptedCreditCardNo)
        {
            return EncryptionHelper.Decrypt(encryptedCreditCardNo);
        }
    }
}