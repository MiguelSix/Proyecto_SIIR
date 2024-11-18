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
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.Extensions.Logging;
using SIIR.DataAccess.Data.Repository;
using SIIR.DataAccess.Data.Repository.IRepository;
using SIIR.Models;
using SIIR.Models.ViewModels;
using SIIR.Utilities;
using static QuestPDF.Helpers.Colors;

namespace SIIR.Areas.Identity.Pages.Account
{
    [Authorize(Roles = "Admin, Coach")]
    public class RegisterModel : PageModel
    {
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IUserStore<ApplicationUser> _userStore;
        private readonly IUserEmailStore<ApplicationUser> _emailStore;
        private readonly ILogger<RegisterModel> _logger;
        private readonly IEmailSender _emailSender;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly ITeamRepository _teamRepository;
		private readonly IContenedorTrabajo _contenedorTrabajo;


		public RegisterModel(
            UserManager<ApplicationUser> userManager,
            IUserStore<ApplicationUser> userStore,
            SignInManager<ApplicationUser> signInManager,
            ILogger<RegisterModel> logger,
            IEmailSender emailSender,
            RoleManager<IdentityRole> roleManager,
            ITeamRepository teamRepository,
            IContenedorTrabajo contenedorTrabajo)
        {
            _userManager = userManager;
            _userStore = userStore;
            _emailStore = GetEmailStore();
            _signInManager = signInManager;
            _logger = logger;
            _emailSender = emailSender;
            _roleManager = roleManager;
            _teamRepository = teamRepository;
            _contenedorTrabajo = contenedorTrabajo;
		}

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        [BindProperty]
        public InputModel Input { get; set; }
        public IEnumerable<SelectListItem> TeamList { get; set; }

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
            /// <summary>
            ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
            ///     directly from your code. This API may change or be removed in future releases.
            /// </summary>
            [Required(ErrorMessage = "El campo correo es obligatorio")]
            [EmailAddress(ErrorMessage = "El formato del correo electrónico no es válido")]
            [RegularExpression(@"^[^@]+@queretaro\.tecnm\.mx$",
            ErrorMessage = "Solo se permiten correos con el dominio @queretaro.tecnm.mx")]
            [Display(Name = "Correo")]
            public string Email { get; set; }

            /// <summary>
            ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
            ///     directly from your code. This API may change or be removed in future releases.
            /// </summary>
            [Required(ErrorMessage = "El campo Contraseña es obligatorio")]
            [StringLength(100, ErrorMessage = "La {0} debe tener al menos {2} y un máximo de {1} caracteres de longitud.", MinimumLength = 6)]
            [DataType(DataType.Password)]
            [Display(Name = "Contraseña")]
            public string Password { get; set; }

            /// <summary>
            ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
            ///     directly from your code. This API may change or be removed in future releases.
            /// </summary>
            [DataType(DataType.Password)]
            [Display(Name = "Confirmar contraseña")]
            [Compare("Password", ErrorMessage = "Las contraseñas no coinciden.")]
            public string ConfirmPassword { get; set; }

            [StringLength(50, ErrorMessage = "El nombre no puede exceder los 50 caracteres.")]
            [Display(Name = "Nombre")]
            public string Name { get; set; }

            [StringLength(50, ErrorMessage = "El apellido paterno no puede exceder los 50 caracteres.")]
            [Display(Name = "Apellido Paterno")]
            public string LastName { get; set; }

            [StringLength(50, ErrorMessage = "El apellido materno no puede exceder los 50 caracteres.")]
            [Display(Name = "Apellido Materno")]
            public string SecondLastName { get; set; }
            [Display(Name = "Equipo")]
            public int? TeamId { get; set; }

        }


        public async Task OnGetAsync(string returnUrl = null)
        {
            ReturnUrl = returnUrl;
            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();
            TeamList = _teamRepository.GetListaTeams();
        }

        public async Task<IActionResult> OnPostAsync(string returnUrl = null)
        {
            returnUrl ??= Url.Content("~/");
            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();

            var selectedRole = Request.Form["rdUserRole"].ToString();

            if (ModelState.IsValid && !string.IsNullOrEmpty(selectedRole))
            {
                var user = CreateUser();

                await _userStore.SetUserNameAsync(user, Input.Email, CancellationToken.None);
                await _emailStore.SetEmailAsync(user, Input.Email, CancellationToken.None);

                // Ensure roles exist
                await EnsureRoleExists(CNT.AdminRole);
                await EnsureRoleExists(CNT.StudentRole);
                await EnsureRoleExists(CNT.CoachRole);

                switch (selectedRole)
                {
                    case CNT.AdminRole:
                        var admin = new Models.Admin()
                        {
                            Name = Input.Name,
                            LastName = Input.LastName,
                            SecondLastName = Input.SecondLastName
                        };
                        user.Admin = admin;
                        break;
                    case CNT.CoachRole:
                        var coach = new Models.Coach()
                        {
                            Name = Input.Name,
                            LastName = Input.LastName,
                            SecondLastName = Input.SecondLastName,
                        };
                        user.Coach = coach;
                        break;
                    case CNT.StudentRole:
                        if (!Input.TeamId.HasValue)
                        {
                            ModelState.AddModelError(string.Empty, "Debe seleccionar un equipo para el estudiante.");
                            TeamList = _teamRepository.GetListaTeams();
                            return Page();
                        }

                        var team = _teamRepository.GetFirstOrDefault(t => t.Id == Input.TeamId.Value);
                        if (team == null || team.CoachId == null)
                        {
                            ModelState.AddModelError(string.Empty, "El equipo seleccionado no tiene un entrenador asignado.");
                            TeamList = _teamRepository.GetListaTeams();
                            return Page();
                        }

                        var student = new Models.Student
                        {
                            Name = Input.Name,
                            LastName = Input.LastName,
                            SecondLastName = Input.SecondLastName,
                            Email = Input.Email,
                            TeamId = Input.TeamId.Value,
                            CoachId = team.CoachId,
						};

						user.Student = student;

						break;
                    default:
                        ModelState.AddModelError(string.Empty, "Invalid role selection");
                        return Page();
                }

                var result = await _userManager.CreateAsync(user, Input.Password);

                if (result.Succeeded)
                {
                    await _userManager.AddToRoleAsync(user, selectedRole);
                    _logger.LogInformation("User created a new account with password.");

                    TempData["Message"] = $"Usuario {Input.Email} registrado exitosamente como {selectedRole}";
					TempData["Type"] = "success";

                    if (user.StudentId.HasValue)
                    {
                        CreateUniformStudent(user.Student.Id, user.Student.Team.RepresentativeId);
                    }
						// Redirigir al listado de usuarios
						return RedirectToAction("Index", "Users", new { area = "Admin" });
                }
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }

            // If we got this far, something failed, redisplay form
            TeamList = _teamRepository.GetListaTeams();
            return Page();
        }

        private void CreateUniformStudent(int studentId, int representativeId) 
        {
			var representativeUniforms = _contenedorTrabajo.RepresentativeUniformCatalog
				.GetAll()
		        .Where(u => u.RepresentativeId == representativeId)
		        .ToList();

            foreach(var representativeUniform in representativeUniforms)
            {
                var uniform = new Uniform();
                uniform.StudentId = studentId;
                uniform.RepresentativeId = representativeId;
                uniform.UniformCatalogId = representativeUniform.UniformCatalogId;
				_contenedorTrabajo.Uniform.Add(uniform);
                _contenedorTrabajo.Save();
			}
		}

        private async Task EnsureRoleExists(string roleName)
        {
            if (!await _roleManager.RoleExistsAsync(roleName))
            {
                await _roleManager.CreateAsync(new IdentityRole(roleName));
            }
        }

        private ApplicationUser CreateUser()
        {
            try
            {
                return Activator.CreateInstance<ApplicationUser>();
            }
            catch
            {
                throw new InvalidOperationException($"Can't create an instance of '{nameof(ApplicationUser)}'. " +
                    $"Ensure that '{nameof(ApplicationUser)}' is not an abstract class and has a parameterless constructor, or alternatively " +
                    $"override the register page in /Areas/Identity/Pages/Account/Register.cshtml");
            }
        }

        private IUserEmailStore<ApplicationUser> GetEmailStore()
        {
            if (!_userManager.SupportsUserEmail)
            {
                throw new NotSupportedException("The default UI requires a user store with email support.");
            }
            return (IUserEmailStore<ApplicationUser>)_userStore;
        }

        private string GetDashboardUrl(string role)
        {
            switch (role)
            {
                case CNT.AdminRole:
                    return "/Admin/Home";
                case CNT.CoachRole:
                    return "/Coach/Home";
                case CNT.StudentRole:
                    return "/Student/Home";
                default:
                    return "/";
            }
        }
    }
}
