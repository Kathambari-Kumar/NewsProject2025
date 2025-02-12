// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
#nullable disable

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using NewsProject.Models.DB;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Logging;
using System.ComponentModel;
using Azure.Storage.Queues.Models;
using System.Numerics;

namespace NewsProject.Areas.Identity.Pages.Account
{
    public class RegisterModel : PageModel
    {
        private readonly SignInManager<User> _signInManager;
        private readonly UserManager<User> _userManager;
        private readonly IUserStore<User> _userStore;
        private readonly IUserEmailStore<User> _emailStore;
        private readonly ILogger<RegisterModel> _logger;
        private readonly IEmailSender _emailSender;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public RegisterModel(
            UserManager<User> userManager,
            IUserStore<User> userStore,
            SignInManager<User> signInManager,
            ILogger<RegisterModel> logger,
            IEmailSender emailSender,
            IHttpContextAccessor httpContextAccessor)
        {
            _userManager = userManager;
            _userStore = userStore;
            _emailStore = GetEmailStore();
            _signInManager = signInManager;
            _logger = logger;
            _emailSender = emailSender;
            _httpContextAccessor= httpContextAccessor;
        }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        [BindProperty]
        public InputModel Input { get; set; }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public string ReturnUrl { get; set; }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public IList<AuthenticationScheme> ExternalLogins { get; set; }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public class InputModel
        {
            ///<summary>
            ///Extended Property declaration
            ///</summary>

            [DataType(DataType.Text)]
            [Display(Name = "First Name")]
            public required string FirstName { get; set; }

            [DataType(DataType.Text)]
            [Display(Name = "Last Name")]
            public required string LastName { get; set; }

            [DataType(DataType.DateTime)]
            [DisplayName("Date of Birth")]
            public DateTime DOB { get; set; }

            /// <summary>
            ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
            ///     directly from your code. This API may change or be removed in future releases.
            /// </summary>
            [Required]
            [EmailAddress]
            [Display(Name = "Email")]
            public string Email { get; set; }

            /// <summary>
            ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
            ///     directly from your code. This API may change or be removed in future releases.
            /// </summary>
            [Required]
            [StringLength(100, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 6)]
            [DataType(DataType.Password)]
            [Display(Name = "Password")]
            public string Password { get; set; }

            /// <summary>
            ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
            ///     directly from your code. This API may change or be removed in future releases.
            /// </summary>
            [DataType(DataType.Password)]
            [Display(Name = "Confirm password")]
            [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
            public string ConfirmPassword { get; set; }

            [DataType(DataType.Text)]
            [Display(Name = "User Role")]
            public required string UserRole { get; set; }

            [DataType(DataType.Text)]
            [Display(Name = "Address")]
            public required string DeliveryAddress { get; set; }
        }


        public async Task OnGetAsync(string returnUrl = null)
        {
            ReturnUrl = returnUrl;
            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();
        }

        public async Task<IActionResult> OnPostAsync(string returnUrl = null)
        {
            returnUrl ??= Url.Content("~/");
            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();
            if (ModelState.IsValid)
            {
                var user = CreateUser();
                if (!User.IsInRole("Admin"))
                {
                    Input.UserRole = "Reader";
                    user.DeliveryAddress = Input.DeliveryAddress;
                }
                user.FirstName = Input.FirstName;
                user.LastName = Input.LastName;
                user.UserRole = Input.UserRole;
                user.DOB = Input.DOB;
                await _userStore.SetUserNameAsync(user, Input.Email, CancellationToken.None);
                await _emailStore.SetEmailAsync(user, Input.Email, CancellationToken.None);
                var addtouser = await _userManager.CreateAsync(user, Input.Password);
                var addRolesToUser = await _userManager.AddToRoleAsync(user, Input.UserRole);
                if (addtouser.Succeeded && addRolesToUser.Succeeded)
                {
                    _logger.LogInformation("User created a new account with password.");

                    var userId = await _userManager.GetUserIdAsync(user);
                    var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                    code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
                    var callbackUrl = Url.Page(
                        "/Account/ConfirmEmail",
                        pageHandler: null,
                        values: new { area = "Identity", userId = userId, code = code, returnUrl = returnUrl },
                        protocol: Request.Scheme);

                    // this is to empty the RoleList session Object
                    var session = _httpContextAccessor.HttpContext.Session;
                    session.Remove("RoleList");

                    StringBuilder body = new StringBuilder();
                    body.AppendLine("<hr/>");
                    body.AppendLine("<img src='https://dragonsstorage24.blob.core.windows.net/dragoncontainer/Logo4.jpg' style=\"width:70px; height:70px\"/>");
                    body.Append("<img src='https://dragonsstorage24.blob.core.windows.net/dragoncontainer/Slogon3.jpg' style=\"width:300px; height:50px\"/>");
                    body.AppendLine("<hr/>");
                    body.AppendFormat("<b>" + "Hello {0}\n",user.FirstName + " "+ user.LastName + "," + "<b>" + "<br/>");
                    body.AppendLine("Welcome to the Digital Dragons Family!" + "<br/>");
                    body.AppendLine($"Please confirm your account by <a href = '{HtmlEncoder.Default.Encode(callbackUrl)}'> clicking here </a>.");
                    body.AppendLine("<br/><br/>"+"Best Regadrs" + "<br/>");
                    body.AppendLine("Digital Dragons Team" + "<br/>");
                    body.AppendLine("<hr/>");
                    string content = body.ToString();
                    await _emailSender.SendEmailAsync(Input.Email, "Confirm your email", content);
                    
                    //$"Please confirm your account by <a href='{HtmlEncoder.Default.Encode(callbackUrl)}'>clicking here</a>.");

                    if (User.IsInRole("Admin"))
                    {
                        TempData["Result"] = "Employee record is created successfully! Confirmation mail is sent!!";
                        return RedirectToAction("AdminEditorAuthorFrontPage", "Admin");
                    }
                    else if (_userManager.Options.SignIn.RequireConfirmedAccount)
                    {
                        return RedirectToPage("RegisterConfirmation", new { email = Input.Email, returnUrl = returnUrl });
                    }
                    else
                    {                       
                        await _signInManager.SignInAsync(user, isPersistent: false);
                        return LocalRedirect(returnUrl);                      
                    }
                }
                foreach (var error in addtouser.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }

            // If we got this far, something failed, redisplay form
            return Page();
        }

        private User CreateUser()
        {
            try
            {
                return Activator.CreateInstance<User>();
            }
            catch
            {
                throw new InvalidOperationException($"Can't create an instance of '{nameof(User)}'. " +
                    $"Ensure that '{nameof(User)}' is not an abstract class and has a parameterless constructor, or alternatively " +
                    $"override the register page in /Areas/Identity/Pages/Account/Register.cshtml");
            }
        }

        private IUserEmailStore<User> GetEmailStore()
        {
            if (!_userManager.SupportsUserEmail)
            {
                throw new NotSupportedException("The default UI requires a user store with email support.");
            }
            return (IUserEmailStore<User>)_userStore;
        }
    }
}
