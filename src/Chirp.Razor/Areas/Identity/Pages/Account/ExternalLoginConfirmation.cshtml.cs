#nullable disable

using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using Chirp.Domain.Entities; // <-- TILFØJ DENNE
using Chirp.Infrastructure.Data; // <-- TILFØJ DENNE

namespace Chirp.Razor.Areas.Identity.Pages.Account
{
    [AllowAnonymous]
    public class ExternalLoginConfirmationModel : PageModel
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly ILogger<ExternalLoginConfirmationModel> _logger;
        private readonly ChirpDbContext _context; // <-- TILFØJ DENNE LINJE

        public ExternalLoginConfirmationModel(
            UserManager<IdentityUser> userManager,
            SignInManager<IdentityUser> signInManager,
            ILogger<ExternalLoginConfirmationModel> logger,
            ChirpDbContext context) // <-- TILFØJ 'context' HER
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _logger = logger;
            _context = context; // <-- TILFØJ DENNE LINJE
        }

        [BindProperty]
        public InputModel Input { get; set; }
        public string ProviderDisplayName { get; set; }
        public string ReturnUrl { get; set; }

        public class InputModel
        {
            [Required]
            [Display(Name = "Username")]
            public string DisplayName { get; set; }
            
            [Required]
            [EmailAddress]
            public string Email { get; set; }
        }

        public async Task<IActionResult> OnGetAsync(string email, string displayName, string returnUrl = null)
        {
            // Hent data fra 'ExternalLogin.cshtml.cs' og for-udfyld formularen
            Input = new InputModel
            {
                Email = email,
                DisplayName = displayName
            };
            
            ProviderDisplayName = (await _signInManager.GetExternalLoginInfoAsync()).ProviderDisplayName;
            ReturnUrl = returnUrl;
            return Page();
        }

        public async Task<IActionResult> OnPostConfirmationAsync(string returnUrl = null)
        {
            returnUrl = returnUrl ?? Url.Content("~/");
            var info = await _signInManager.GetExternalLoginInfoAsync();
            if (info == null)
            {
                return RedirectToPage("./Login", new { ReturnUrl = returnUrl });
            }

            if (ModelState.IsValid)
            {
                var user = new IdentityUser { UserName = Input.Email, Email = Input.Email };
                var result = await _userManager.CreateAsync(user);
                
                if (result.Succeeded)
                {
                    result = await _userManager.AddLoginAsync(user, info);
                    if (result.Succeeded)
                    {
                        _logger.LogInformation("User created an account using {Name} provider.", info.LoginProvider);

                        // ----- START PÅ KODE DU SKAL KOPIERE -----
                        // Denne logik er kopieret fra din 'Register.cshtml.cs'
                        
                        var author = new Author
                        {
                            Name = Input.DisplayName,
                            Email = Input.Email,
                            ApplicationUserId = user.Id
                        };
                        
                        _context.Authors.Add(author);
                        await _context.SaveChangesAsync();
                        
                        // ----- SLUT PÅ DEN KOPIEREDE KODE -----

                        await _signInManager.SignInAsync(user, isPersistent: false);
                        return LocalRedirect(returnUrl);
                    }
                }
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }

            ProviderDisplayName = info.ProviderDisplayName;
            ReturnUrl = returnUrl;
            return Page();
        }
    }
}