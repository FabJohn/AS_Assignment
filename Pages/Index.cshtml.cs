using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
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
        public async Task<IActionResult> OnPostChangePasswordAsync(string CurrentPassword, string NewPassword, string ConfirmPassword)
        {
            if (NewPassword != ConfirmPassword)
            {
                ModelState.AddModelError(string.Empty, "The new password and confirmation password do not match.");
                return Page();
            }

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToPage("/Login");
            }

            // Check if the new password is in the history
            foreach (var oldPasswordHash in user.PasswordHistory)
            {
                if (_userManager.PasswordHasher.VerifyHashedPassword(user, oldPasswordHash, NewPassword) == PasswordVerificationResult.Success)
                {
                    ModelState.AddModelError(string.Empty, "You cannot reuse your last 2 passwords.");
                    return Page();
                }
            }

            // Change password
            var changePasswordResult = await _userManager.ChangePasswordAsync(user, CurrentPassword, NewPassword);
            if (!changePasswordResult.Succeeded)
            {
                foreach (var error in changePasswordResult.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
                return Page();
            }

            // Update password history
            user.PasswordHistory.Insert(0, user.PasswordHash);
            if (user.PasswordHistory.Count > 2)
            {
                user.PasswordHistory.RemoveAt(2); // Keep only the last 2 passwords
            }

            await _userManager.UpdateAsync(user);

            // Redirect to the same page with a success message
            TempData["Message"] = "Password changed successfully.";
            return RedirectToPage();
        }
    }
}