using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Threading.Tasks;
using WebApplication1.Model;

namespace WebApplication1.Pages
{
    public class ForgotPasswordModel : PageModel
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly EmailService _emailService;

        public ForgotPasswordModel(UserManager<ApplicationUser> userManager, EmailService emailService)
        {
            _userManager = userManager;
            _emailService = emailService;
        }

        [BindProperty]
        public string Email { get; set; }

        public async Task<IActionResult> OnPostAsync()
        {
            if (string.IsNullOrEmpty(Email))
            {
                ModelState.AddModelError(string.Empty, "Email is required.");
                return Page();
            }

            var user = await _userManager.FindByEmailAsync(Email);
            if (user == null)
            {
                ModelState.AddModelError(string.Empty, "No user found with this email.");
                return Page();
            }

            // Generate the password reset token
            var token = await _userManager.GeneratePasswordResetTokenAsync(user);

            // Create the reset link
            var resetLink = Url.Page("/ResetPassword", pageHandler: null, values: new { token, email = user.Email }, protocol: Request.Scheme);
            Console.WriteLine("Reset link:" + resetLink);
            
            // Send the reset link via email
            var subject = "Password Reset Request";
            var body = $"Please reset your password by <a href='{resetLink}'>clicking here</a>.";
            await _emailService.SendEmailAsync(user.Email, subject, body);

            TempData["Message"] = "A password reset link has been sent to your email.";
            return RedirectToPage("/Login");
        }
    }
}