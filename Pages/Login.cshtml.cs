using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using WebApplication1.Model;
using WebApplication1.ViewModels;

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

            var result = await _signInManager.PasswordSignInAsync(LModel.Email, LModel.Password, LModel.RememberMe, lockoutOnFailure: false);
            Console.WriteLine("RESULT: " + result);
            if (result.Succeeded)
            {
                var user = await _userManager.FindByEmailAsync(LModel.Email);

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

            ModelState.AddModelError(string.Empty, "Invalid login attempt.");
            return Page();
        }
    }
}