using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using System.Net.Http;
using WebApplication1.Model;
using WebApplication1.ViewModels;
using System.Security.Claims;

namespace WebApplication1.Pages
{
    public class RegisterModel : PageModel
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly IConfiguration _configuration;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<RegisterModel> _logger;

        [BindProperty]
        public Register RModel { get; set; }

        public string RecaptchaSiteKey { get; private set; }

        public RegisterModel(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            IConfiguration configuration,
            IHttpClientFactory httpClientFactory,
            ILogger<RegisterModel> logger)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _configuration = configuration;
            _httpClientFactory = httpClientFactory;
            _logger = logger;
            RecaptchaSiteKey = _configuration["RecaptchaSettings:SiteKey"];
        }

        public void OnGet()
        {
            if (RModel == null)
            {
                RModel = new Register();
            }
        }

        public async Task<IActionResult> OnPostAsync()
        {
            try
            {
                if (RModel == null)
                {
                    RModel = new Register();
                }

                if (!ModelState.IsValid)
                {
                    return Page();
                }

                // Validate reCAPTCHA
                var recaptchaToken = Request.Form["g-recaptcha-response"].ToString();
                if (!await ValidateRecaptcha(recaptchaToken))
                {
                    ModelState.AddModelError(string.Empty, "Invalid reCAPTCHA verification. Please try again.");
                    return Page();
                }

                // Handle photo upload
                string photoPath = null;
                if (RModel.Photo != null && RModel.Photo.Length > 0)
                {
                    var filePath = Path.Combine("wwwroot/uploads", RModel.Photo.FileName);
                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await RModel.Photo.CopyToAsync(stream);
                    }
                    photoPath = $"/uploads/{RModel.Photo.FileName}"; // Store the relative path
                }

                // Encrypt the credit card number
                string encryptedCreditCardNo = EncryptionHelper.Encrypt(RModel.CreditCardNo);

                // Create a new ApplicationUser with custom properties
                var user = new ApplicationUser
                {
                    UserName = RModel.Email,
                    Email = RModel.Email,
                    FullName = RModel.FullName,
                    CreditCardNo = encryptedCreditCardNo, // Store the encrypted credit card number
                    Gender = RModel.Gender,
                    MobileNo = RModel.MobileNo,
                    DeliveryAddress = RModel.DeliveryAddress,
                    AboutMe = RModel.AboutMe,
                    PhotoPath = photoPath
                };

                // Create the user
                var result = await _userManager.CreateAsync(user, RModel.Password);

                if (result.Succeeded)
                {
                    // Sign in the user
                    await _signInManager.SignInAsync(user, isPersistent: false);

                    // Redirect to the Index page
                    return RedirectToPage("Index");
                }

                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }
            catch (Exception ex)
            {
                // Log the exception
                _logger.LogError(ex, "An error occurred during registration.");
                ModelState.AddModelError(string.Empty, "An error occurred during registration. Please try again.");
            }

            return Page();
        }

        private async Task<bool> ValidateRecaptcha(string token)
        {
            if (string.IsNullOrEmpty(token))
            {
                return false;
            }

            try
            {
                var client = _httpClientFactory.CreateClient();
                var response = await client.PostAsync(
                    "https://www.google.com/recaptcha/api/siteverify",
                    new FormUrlEncodedContent(new Dictionary<string, string>
                    {
                        { "secret", _configuration["RecaptchaSettings:SecretKey"] },
                        { "response", token }
                    }));

                if (response.IsSuccessStatusCode)
                {
                    var jsonResponse = await response.Content.ReadFromJsonAsync<RecaptchaResponse>();
                    return jsonResponse.Success && jsonResponse.Score >= 0.5;
                }
            }
            catch
            {
                // Log error if needed
            }

            return false;
        }

        public class RecaptchaResponse
        {
            public bool Success { get; set; }
            public double Score { get; set; }
        }
    }
}