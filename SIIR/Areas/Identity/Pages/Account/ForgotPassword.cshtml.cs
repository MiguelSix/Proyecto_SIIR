// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
#nullable disable

using System;
using System.ComponentModel.DataAnnotations;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.WebUtilities;
using SIIR.Models;

namespace SIIR.Areas.Identity.Pages.Account
{
    public class ForgotPasswordModel : PageModel
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IEmailSender _emailSender;

        public ForgotPasswordModel(UserManager<ApplicationUser> userManager, IEmailSender emailSender)
        {
            _userManager = userManager;
            _emailSender = emailSender;
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
        public class InputModel
        {
            /// <summary>
            ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
            ///     directly from your code. This API may change or be removed in future releases.
            /// </summary>
            [Required]
            [EmailAddress]
            public string Email { get; set; }
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (ModelState.IsValid)
            {
                var user = await _userManager.FindByEmailAsync(Input.Email);
                if (user == null)
                {
                    // Don't reveal that the user does not exist or is not confirmed
                    return RedirectToPage("./ForgotPasswordConfirmation");
                }

                var code = await _userManager.GeneratePasswordResetTokenAsync(user);
                code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
                var callbackUrl = Url.Page(
                    "/Account/ResetPassword",
                    pageHandler: null,
                    values: new { area = "Identity", code },
                    protocol: Request.Scheme);

                var emailTemplate = $@"
            <!DOCTYPE html>
            <html>
            <head>
                <meta charset='UTF-8'>
                <meta name='viewport' content='width=device-width, initial-scale=1.0'>
                <title>Restablecer Contraseña</title>
                <style>
                    body {{
                        font-family: Arial, sans-serif;
                        line-height: 1.6;
                        color: #333333;
                        margin: 0;
                        padding: 0;
                    }}
                    .container {{
                        max-width: 600px;
                        margin: 0 auto;
                        padding: 20px;
                    }}
                    .header {{
                        background-color: #1a73e8;
                        color: white;
                        padding: 20px;
                        text-align: center;
                        border-radius: 5px 5px 0 0;
                    }}
                    .content {{
                        background-color: #ffffff;
                        padding: 20px;
                        border: 1px solid #dddddd;
                        border-radius: 0 0 5px 5px;
                    }}
                    .button {{
                        display: inline-block;
                        padding: 12px 24px;
                        background-color: #1a73e8;
                        color: white !important;
                        text-decoration: none;
                        border-radius: 5px;
                        margin: 20px 0;
                    }}
                    .footer {{
                        text-align: center;
                        margin-top: 20px;
                        color: #666666;
                        font-size: 12px;
                    }}
                </style>
            </head>
            <body>
                <div class='container'>
                    <div class='header'>
                        <h1>Restablecer Contraseña</h1>
                    </div>
                    <div class='content'>
                        <p>Hola,</p>
                        <p>Hemos recibido una solicitud para restablecer la contraseña de tu cuenta.</p>
                        <p>Si no has solicitado este cambio, puedes ignorar este correo electrónico.</p>
                        <p>Para continuar con el proceso de restablecimiento de contraseña, por favor haz clic en el siguiente botón:</p>
                        <div style='text-align: center;'>
                            <a href='{HtmlEncoder.Default.Encode(callbackUrl)}' class='button'>Restablecer Contraseña</a>
                        </div>
                        <p>Si el botón no funciona, también puedes copiar y pegar el siguiente enlace en tu navegador:</p>
                        <p style='word-break: break-all;'>{HtmlEncoder.Default.Encode(callbackUrl)}</p>
                        <p>Este enlace expirará en 24 horas por razones de seguridad.</p>
                    </div>
                    <div class='footer'>
                        <p>Este es un correo electrónico automático, por favor no responda a este mensaje.</p>
                        <p>© {DateTime.Now.Year} SIIR. Todos los derechos reservados.</p>
                    </div>
                </div>
            </body>
            </html>";

                await _emailSender.SendEmailAsync(
                    Input.Email,
                    "Restablecimiento de Contraseña",
                    emailTemplate);

                return RedirectToPage("./ForgotPasswordConfirmation");
            }
            return Page();
        }
    }
}
