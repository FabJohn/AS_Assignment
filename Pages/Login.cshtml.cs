using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using WebApplication1.Model;
using WebApplication1.ViewModels;
using System;
using System.Threading.Tasks;

namespace WebApplication1.Pages
{
    public class LoginModel : PageModel
    {
        [BindProperty]
        public Login LModel { get; set; }

        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly UserManager<ApplicationUser> _userManager;

        public LoginModel(SignInManager<ApplicationUser> signInManager, UserManager<ApplicationUser> userManager)
        {
            _signInManager = signInManager;
            _userManager = userManager;
        }

        public void OnGet()
        {
            // Initialize LModel here
            LModel = new Login();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            var user = await _userManager.FindByEmailAsync(LModel.Email);
            if (user != null)
            {
                // Check if the user is locked out
                if (await _userManager.IsLockedOutAsync(user))
                {
                    ModelState.AddModelError(string.Empty, "Account is temporarily locked out due to multiple failed login attempts. Please try again later.");
                    return Page();
                }

                var result = await _signInManager.PasswordSignInAsync(LModel.Email, LModel.Password, LModel.RememberMe, lockoutOnFailure: true);

                if (result.Succeeded)
                {
                    // Reset the access failed count on successful login
                    await _userManager.ResetAccessFailedCountAsync(user);

                    // Generate a unique session token
                    var sessionToken = Guid.NewGuid().ToString();

                    // Store the session token in the database
                    user.SessionToken = sessionToken;
                    await _userManager.UpdateAsync(user);

                    // Store the session token in the session
                    HttpContext.Session.SetString("SessionToken", sessionToken);
                    Console.WriteLine("SessionToken: " + sessionToken);
                    HttpContext.Session.SetString("UserId", user.Id);
                    HttpContext.Session.SetString("UserName", user.UserName);

                    return RedirectToPage("Index");
                }
                else if (result.IsLockedOut)
                {
                    ModelState.AddModelError(string.Empty, "Account is temporarily locked out due to multiple failed login attempts. Please try again later.");
                    return Page();
                }
                else
                {
                    // Increment the access failed count
                    await _userManager.AccessFailedAsync(user);

                    // Get the current access failed count
                    int accessFailedCount = await _userManager.GetAccessFailedCountAsync(user);

                    // Calculate remaining attempts
                    int remainingAttempts = 3 - accessFailedCount;

                    if (remainingAttempts > 0)
                    {
                        ModelState.AddModelError(string.Empty, $"Invalid login attempt. You have {remainingAttempts} more attempts before your account is locked.");
                    }
                    else
                    {
                        // Lock the user out for 1 minute
                        await _userManager.SetLockoutEndDateAsync(user, DateTimeOffset.UtcNow.AddMinutes(1));
                        ModelState.AddModelError(string.Empty, "Account is temporarily locked out due to multiple failed login attempts. Please try again later.");
                    }

                    return Page();
                }
            }
            else
            {
                ModelState.AddModelError(string.Empty, "Invalid login attempt.");
                return Page();
            }
        }
    }
}