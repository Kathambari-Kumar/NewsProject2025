using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using NewsProject.Models.DB;
using System.ComponentModel.DataAnnotations;

namespace NewsProject.Areas.Identity.Pages.Account.Manage
{
    public class ChangeAddressModel : PageModel
    {
        private readonly UserManager<User> _userManager;
        private readonly SignInManager<User> _signInManager;
        private readonly ILogger<ChangePasswordModel> _logger;

        public ChangeAddressModel(
            UserManager<User> userManager,
            SignInManager<User> signInManager,
            ILogger<ChangePasswordModel> logger)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _logger = logger;
        }

        [BindProperty]
        public InputModel Input { get; set; }
        public string DeliveryAddress { get; set; }

        [TempData]
        public string StatusMessage { get; set; }

        public class InputModel
        {

            [Required]
            [DataType(DataType.Text)]
            [Display(Name = "New Address")]
            public string NewAddress { get; set; }
        }
        public async Task<IActionResult> OnGet()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }
            TempData["OldAddress"] = user.DeliveryAddress;
            return Page();
        }

        public async Task<IActionResult> OnPost()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }
            user.DeliveryAddress = Input.NewAddress;

            var changeAddressResult = await _userManager.UpdateAsync(user);
            if (!changeAddressResult.Succeeded)
            {
                foreach (var error in changeAddressResult.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
                return Page();
            }

            _logger.LogInformation("User changed their password successfully.");
            StatusMessage = "Your address has been changed.";
            return RedirectToPage();
        }
    }
}

