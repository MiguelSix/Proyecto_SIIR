// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
#nullable disable

using System;
using System.ComponentModel.DataAnnotations;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using SIIR.Data;
using SIIR.Models;

namespace SIIR.Areas.Identity.Pages.Account.Manage
{
    public class IndexModel : PageModel
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly ApplicationDbContext _context;

        public IndexModel(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            IWebHostEnvironment webHostEnvironment,
            ApplicationDbContext context)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _webHostEnvironment = webHostEnvironment;
            _context = context;
        }

        public string Username { get; set; }

        [TempData]
        public string StatusMessage { get; set; }

        [BindProperty]
        public InputModel Input { get; set; }

        public class InputModel
        {

            [Required(ErrorMessage = "El nombre es requerido")]
            [Display(Name = "Nombre")]
            public string Name { get; set; }

            [Required(ErrorMessage = "El apellido paterno es requerido")]
            [Display(Name = "Apellido Paterno")]
            public string LastName { get; set; }

            [Required(ErrorMessage = "El apellido materno es requerido")]
            [Display(Name = "Apellido Materno")]
            public string SecondLastName { get; set; }

            [Phone]
            [Display(Name = "Teléfono")]
            public string PhoneNumber { get; set; }

            [Display(Name = "Foto de perfil")]
            public IFormFile ImageFile { get; set; }

            public string ImageUrl { get; set; }

            [Display(Name = "CV")]
            public IFormFile CVFile { get; set; }
            public string CVUrl { get; set; }
        }

        private async Task LoadAsync(ApplicationUser user)
        {
            var userName = await _userManager.GetUserNameAsync(user);
            var phoneNumber = await _userManager.GetPhoneNumberAsync(user);
            var roles = await _userManager.GetRolesAsync(user);
            var currentRole = roles.FirstOrDefault();

            Username = userName;

            Input = new InputModel
            {
                PhoneNumber = phoneNumber
            };

            // Cargar los datos según el rol
            switch (currentRole)
            {
                case "Admin" when user.AdminId.HasValue:
                    var admin = await _context.Admins
                        .FirstOrDefaultAsync(a => a.Id == user.AdminId);
                    if (admin != null)
                    {
                        Input.Name = admin.Name;
                        Input.LastName = admin.LastName;
                        Input.SecondLastName = admin.SecondLastName;
                    }
                    break;

                case "Coach" when user.CoachId.HasValue:
                    var coach = await _context.Coaches
                        .FirstOrDefaultAsync(c => c.Id == user.CoachId);
                    if (coach != null)
                    {
                        Input.Name = coach.Name;
                        Input.LastName = coach.LastName;
                        Input.SecondLastName = coach.SecondLastName;
                        Input.ImageUrl = coach.ImageUrl;
                        Input.CVUrl = coach.CVUrl;
                    }
                    break;

                case "Student" when user.StudentId.HasValue:
                    var student = await _context.Students
                        .FirstOrDefaultAsync(s => s.Id == user.StudentId);
                    if (student != null)
                    {
                        Input.Name = student.Name;
                        Input.LastName = student.LastName;
                        Input.SecondLastName = student.SecondLastName;
                        Input.ImageUrl = student.ImageUrl;
                    }
                    break;
            }
        }

        public async Task<IActionResult> OnGetAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound($"No se pudo cargar el usuario con ID '{_userManager.GetUserId(User)}'.");
            }

            var roles = await _userManager.GetRolesAsync(user);

            await LoadAsync(user);
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound($"No se pudo cargar el usuario con ID '{_userManager.GetUserId(User)}'.");
            }

            if (!ModelState.IsValid)
            {
                await LoadAsync(user);
                return Page();
            }

            var roles = await _userManager.GetRolesAsync(user);
            var currentRole = roles.FirstOrDefault();

            switch (currentRole)
            {
                case "Admin" when user.AdminId.HasValue:
                    var admin = await _context.Admins.FindAsync(user.AdminId);
                    if (admin != null)
                    {
                        admin.Name = Input.Name;
                        admin.LastName = Input.LastName;
                        admin.SecondLastName = Input.SecondLastName;
                        _context.Admins.Update(admin);
                    }
                    break;

                case "Coach" when user.CoachId.HasValue:
                    var coach = await _context.Coaches.FindAsync(user.CoachId);
                    if (coach != null)
                    {
                        coach.Name = Input.Name;
                        coach.LastName = Input.LastName;
                        coach.SecondLastName = Input.SecondLastName;

                        if (Input.ImageFile != null)
                        {
                            var fileName = $"{Guid.NewGuid()}{Path.GetExtension(Input.ImageFile.FileName)}";
                            var uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "images", "coaches");

                            if (!Directory.Exists(uploadsFolder))
                            {
                                Directory.CreateDirectory(uploadsFolder);
                            }

                            if (!string.IsNullOrEmpty(coach.ImageUrl))
                            {
                                var oldImagePath = Path.Combine(_webHostEnvironment.WebRootPath,
                                    coach.ImageUrl.TrimStart('/'));
                                if (System.IO.File.Exists(oldImagePath))
                                {
                                    System.IO.File.Delete(oldImagePath);
                                }
                            }

                            var filePath = Path.Combine(uploadsFolder, fileName);
                            using (var fileStream = new FileStream(filePath, FileMode.Create))
                            {
                                await Input.ImageFile.CopyToAsync(fileStream);
                            }

                            coach.ImageUrl = $"/images/coaches/{fileName}";
                        }

                        if (Input.CVFile != null)
                        {
                            var fileName = $"{Guid.NewGuid()}{Path.GetExtension(Input.CVFile.FileName)}";
                            var uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "documents", "cv");

                            if (!Directory.Exists(uploadsFolder))
                            {
                                Directory.CreateDirectory(uploadsFolder);
                            }

                            if (!string.IsNullOrEmpty(coach.CVUrl))
                            {
                                var oldCVPath = Path.Combine(_webHostEnvironment.WebRootPath,
                                    coach.CVUrl.TrimStart('/'));
                                if (System.IO.File.Exists(oldCVPath))
                                {
                                    System.IO.File.Delete(oldCVPath);
                                }
                            }

                            var filePath = Path.Combine(uploadsFolder, fileName);
                            using (var fileStream = new FileStream(filePath, FileMode.Create))
                            {
                                await Input.CVFile.CopyToAsync(fileStream);
                            }

                            coach.CVUrl = $"/documents/cv/{fileName}";
                        }
                            _context.Coaches.Update(coach);
                    }
                    break;

                case "Student" when user.StudentId.HasValue:
                    var student = await _context.Students.FindAsync(user.StudentId);
                    if (student != null)
                    {
                        student.Name = Input.Name;
                        student.LastName = Input.LastName;
                        student.SecondLastName = Input.SecondLastName;

                        if (Input.ImageFile != null)
                        {
                            var fileName = $"{Guid.NewGuid()}{Path.GetExtension(Input.ImageFile.FileName)}";
                            var uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "images", "students");

                            if (!Directory.Exists(uploadsFolder))
                            {
                                Directory.CreateDirectory(uploadsFolder);
                            }

                            if (!string.IsNullOrEmpty(student.ImageUrl))
                            {
                                var oldImagePath = Path.Combine(_webHostEnvironment.WebRootPath,
                                    student.ImageUrl.TrimStart('/'));
                                if (System.IO.File.Exists(oldImagePath))
                                {
                                    System.IO.File.Delete(oldImagePath);
                                }
                            }

                            var filePath = Path.Combine(uploadsFolder, fileName);
                            using (var fileStream = new FileStream(filePath, FileMode.Create))
                            {
                                await Input.ImageFile.CopyToAsync(fileStream);
                            }

                            student.ImageUrl = $"/images/students/{fileName}";
                        }

                        _context.Students.Update(student);
                    }
                    break;
            }

            if (Input.PhoneNumber != user.PhoneNumber)
            {
                var setPhoneResult = await _userManager.SetPhoneNumberAsync(user, Input.PhoneNumber);
                if (!setPhoneResult.Succeeded)
                {
                    StatusMessage = "Error al intentar actualizar el número de teléfono.";
                    return RedirectToPage();
                }
            }

            await _context.SaveChangesAsync();
            await _signInManager.RefreshSignInAsync(user);
            StatusMessage = "Tu perfil ha sido actualizado";
            return RedirectToPage();
        }
    }
}
