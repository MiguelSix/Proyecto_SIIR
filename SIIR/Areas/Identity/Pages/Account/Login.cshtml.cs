// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
#nullable disable

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using SIIR.Models;
using SIIR.Utilities;

namespace SIIR.Areas.Identity.Pages.Account
{
    public class LoginModel : PageModel
    {
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly ILogger<LoginModel> _logger;
        private readonly UserManager<ApplicationUser> _userManager;

        public LoginModel(SignInManager<ApplicationUser> signInManager, ILogger<LoginModel> logger, UserManager<ApplicationUser> userManager)
        {
            _signInManager = signInManager;
            _logger = logger;
            _userManager = userManager;
        }

        private string GetDashboardUrl(ApplicationUser user)
        {
            if (_userManager.IsInRoleAsync(user, CNT.AdminRole).Result)
            {
                return "/Admin/Home";
            }
            else if (_userManager.IsInRoleAsync(user, CNT.CoachRole).Result)
            {
                return "/Coach/Home";
            }
            else if (_userManager.IsInRoleAsync(user, CNT.StudentRole).Result)
            {
                return "/Student/Home";
            }
            else
            {
                return "/";
            }
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
        public IList<AuthenticationScheme> ExternalLogins { get; set; }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public string ReturnUrl { get; set; }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        [TempData]
        public string ErrorMessage { get; set; }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public class InputModel
        {
            /// <summary>
            ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
            ///     directly from your code. This API may change or be removed in future releases.
            /// </summary>
            [Required(ErrorMessage = "El campo correo es obligatorio")]
            [EmailAddress(ErrorMessage = "El formato del correo electrónico no es válido")]
            [Display(Name = "Correo")]
            public string Email { get; set; }

            /// <summary>
            ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
            ///     directly from your code. This API may change or be removed in future releases.
            /// </summary>
            [Required(ErrorMessage = "El campo contraseña es obligatorio")]
            [DataType(DataType.Password)]
            [Display(Name = "Contraseña")]
            public string Password { get; set; }

            /// <summary>
            ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
            ///     directly from your code. This API may change or be removed in future releases.
            /// </summary>
            [Display(Name = "Recordarme?")]
            public bool RememberMe { get; set; }
        }

        public async Task OnGetAsync(string returnUrl = null)
        {
            if (!string.IsNullOrEmpty(ErrorMessage))
            {
                ModelState.AddModelError(string.Empty, ErrorMessage);
            }

            returnUrl ??= Url.Content("~/");

            // Clear the existing external cookie to ensure a clean login process
            await HttpContext.SignOutAsync(IdentityConstants.ExternalScheme);

            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();

            ReturnUrl = returnUrl;
        }

        public async Task<IActionResult> OnPostAsync(string returnUrl = null)
        {
            returnUrl ??= Url.Content("~/");

            if (ModelState.IsValid)
            {
                // Primero verificamos si el usuario existe
                var user = await _userManager.FindByEmailAsync(Input.Email);
                if (user == null)
                {
                    ModelState.AddModelError(string.Empty, "Intento de Login Fallido!!");
                    return Page();
                }

                // Verificamos si el usuario está bloqueado
                if (await _userManager.IsLockedOutAsync(user))
                {
                    _logger.LogWarning("User account locked out.");
                    ModelState.AddModelError(string.Empty, "Esta cuenta ha sido bloqueada, por favor contacte al Administrador");
                    return Page();
                }

                // Intentamos el inicio de sesión usando SignInManager
                var result = await _signInManager.PasswordSignInAsync(Input.Email,
                                                                    Input.Password,
                                                                    Input.RememberMe,
                                                                    lockoutOnFailure: true); // La cuenta se bloquea despues de varios try

                if (result.Succeeded)
                {
                    _logger.LogInformation("User logged in successfully.");
                    return LocalRedirect(GetDashboardUrl(user));
                }

                if (result.IsLockedOut)
                {
                    _logger.LogWarning("Esta cuenta ha sido bloqueada, por favor contacte al Administrador");
                    return RedirectToPage("./Lockout");
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "Intento de Login Fallido!!");
                    return Page();
                }
            }

            // If we got this far, something failed, redisplay form
            return Page();
        }
    }
}
