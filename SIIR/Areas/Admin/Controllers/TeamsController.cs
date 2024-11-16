using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Newtonsoft.Json.Linq;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using SIIR.DataAccess.Data.Repository;
using SIIR.DataAccess.Data.Repository.IRepository;
using SIIR.Models;
using SIIR.Models.ViewModels;
using SIIR.Utilities;
using System.Drawing;
using System.Security.Policy;
using static QuestPDF.Helpers.Colors;

namespace SIIR.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class TeamsController : Controller
    {
        private readonly IContenedorTrabajo _contenedorTrabajo;
        private readonly IWebHostEnvironment _hostingEnvironment;

        public TeamsController(IContenedorTrabajo contenedorTrabajo,
            IWebHostEnvironment hostingEnvironment)
        {
            _contenedorTrabajo = contenedorTrabajo;
            _hostingEnvironment = hostingEnvironment;
        }

        [HttpGet]
        [Authorize(Roles = "Admin")]
        public IActionResult Index(string? category)
        {
            ViewData["Category"] = category;
            return View();
        }


        [HttpGet]
        [Authorize(Roles = "Admin")]
        public IActionResult Create()
        {
            TeamVM teamVM = new()
            {
                Team = new(),
                RepresentativeList = _contenedorTrabajo.Representative.GetRepresentativesList(),
                CoachList = _contenedorTrabajo.Coach.GetCoachesList(),
            };
            return View(teamVM);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public IActionResult Create(TeamVM teamVM)
        {
            ModelState.Remove("Team.ImageUrl");
            ModelState.Remove("Team.Name");
            
            if (ModelState.IsValid)
            {
                string webRootPath = _hostingEnvironment.WebRootPath;
                var files = HttpContext.Request.Form.Files;

                teamVM.Team.Representative = _contenedorTrabajo.Representative.GetById(teamVM.Team.RepresentativeId);
                if (teamVM.Team.Category.ToLower() == "mixto")
                {
                    teamVM.Team.Name = teamVM.Team.Representative.Name;
                }
                else
                {
                    teamVM.Team.Name = teamVM.Team.Representative.Name + " " + teamVM.Team.Category;
                }
                
                var existingTeam = _contenedorTrabajo.Team.GetFirstOrDefault(t => t.Name == teamVM.Team.Name);
                if (existingTeam != null)
                {
                    // Agrega un error personalizado al ModelState
                    ModelState.AddModelError("Team.RepresentativeId", "El equipo " + teamVM.Team.Name + " ya existe");
                }
                else if (teamVM.Team.Id == 0 && files.Count() > 0)
                {
                    // Nuevo equipo
                    string fileName = Guid.NewGuid().ToString();
                    var uploads = Path.Combine(webRootPath, @"images\teams");
                    var extension = Path.GetExtension(files[0].FileName);

                    using (var fileStreams = new FileStream(Path.Combine(uploads, fileName + extension), FileMode.Create))
                    {
                        files[0].CopyTo(fileStreams);
                    }

                    teamVM.Team.ImageUrl = @"\images\teams\" + fileName + extension;
                    _contenedorTrabajo.Team.Add(teamVM.Team);
                    _contenedorTrabajo.Save();
                    return RedirectToAction(nameof(Index));
                }
            }
            ModelState.AddModelError("Team.ImageUrl", "Debes seleccionar una imagen");
            teamVM.RepresentativeList = _contenedorTrabajo.Representative.GetRepresentativesList();
            teamVM.CoachList = _contenedorTrabajo.Coach.GetCoachesList();

			return View(teamVM);
        }


        [HttpGet]
        [Authorize(Roles = "Admin")]
        public IActionResult Edit(int? id)
        {
            TeamVM teamVM = new()
            {
                Team = new(),
                RepresentativeList = _contenedorTrabajo.Representative.GetRepresentativesList(),
                CoachList = _contenedorTrabajo.Coach.GetCoachesList(),
            };
            if (id != null)
            {
                teamVM.Team = _contenedorTrabajo.Team.GetById(id.GetValueOrDefault());
            }
            return View(teamVM);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public IActionResult Edit(TeamVM teamVM)
        {
            // Remover la validación de la imagen si no se selecciona una nueva imagen
            ModelState.Remove("Team.ImageUrl");
            ModelState.Remove("Team.Name");
            if (ModelState.IsValid)
            {
                var teamFromDb = _contenedorTrabajo.Team.GetById(teamVM.Team.Id);
                teamVM.Team.Representative = _contenedorTrabajo.Representative.GetById(teamVM.Team.RepresentativeId);
                if (teamVM.Team.Category.ToLower() == "mixto")
                {
                    teamVM.Team.Name = teamVM.Team.Representative.Name;
                }
                else
                {
                    teamVM.Team.Name = teamVM.Team.Representative.Name + " " + teamVM.Team.Category;
                }

                if (teamFromDb.Name != teamVM.Team.Name)
                {
                    var existingTeam = _contenedorTrabajo.Team.GetFirstOrDefault(t => t.Name == teamVM.Team.Name);
                    if (existingTeam != null)
                    {
                        ModelState.AddModelError("Team.RepresentativeId", "El equipo " + teamVM.Team.Name + " ya existe");
                        teamVM.RepresentativeList = _contenedorTrabajo.Representative.GetRepresentativesList();
                        teamVM.CoachList = _contenedorTrabajo.Coach.GetCoachesList();
                        return View(teamVM);
                    }
                }

                string webRootPath = _hostingEnvironment.WebRootPath;
                var files = HttpContext.Request.Form.Files;
                

                if (files.Count > 0)
                {
                    // Editar Imagen
                    string fileName = Guid.NewGuid().ToString();
                    var uploads = Path.Combine(webRootPath, @"images\teams");
                    var extension_new = Path.GetExtension(files[0].FileName);

                    // Eliminar la imagen anterior
                    if (!string.IsNullOrEmpty(teamFromDb.ImageUrl))
                    {
                        var imagePath = Path.Combine(webRootPath, teamFromDb.ImageUrl.TrimStart('\\'));
                        if (System.IO.File.Exists(imagePath))
                        {
                            System.IO.File.Delete(imagePath);
                        }
                    }

                    // Subir nueva imagen
                    using (var fileStreams = new FileStream(Path.Combine(uploads, fileName + extension_new), FileMode.Create))
                    {
                        files[0].CopyTo(fileStreams);
                    }

                    // Guardar la nueva ruta de la imagen
                    teamFromDb.ImageUrl = @"\images\teams\" + fileName + extension_new;
                }
                else
                {
                    // No se seleccionó una nueva imagen, mantener la imagen existente
                    teamVM.Team.ImageUrl = teamFromDb.ImageUrl;
                }

                _contenedorTrabajo.Team.Update(teamVM.Team);
                _contenedorTrabajo.Save();
                return RedirectToAction(nameof(Index));
            }

            // Si el ModelState no es válido, volver a cargar las listas y retornar la vista
            teamVM.RepresentativeList = _contenedorTrabajo.Representative.GetRepresentativesList();
            teamVM.CoachList = _contenedorTrabajo.Coach.GetCoachesList();
            return View(teamVM);
        }

        [HttpGet]
        [Authorize(Roles = "Admin, Coach")]
        public IActionResult Roster(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var team = _contenedorTrabajo.Team.GetById(id.Value);
            team.Representative = _contenedorTrabajo.Representative.GetById(team.RepresentativeId);
            if (team == null)
            {
                return NotFound();
            }
            // Data del equipo
            team.Coach = _contenedorTrabajo.Coach.GetById(team.CoachId);
            // Obtener todos los usuarios que no están bloqueados
            var users = _contenedorTrabajo.User.GetAll(u => u.LockoutEnd == null && u.StudentId != null).Select(u => u.StudentId).ToList();
            // Lista de estudiantes que pertenecen al equipo y no están bloqueados como usuarios
            var students = _contenedorTrabajo.Student.GetAll(s => s.TeamId == id.Value && users.Contains(s.Id)).ToList();
            var captain = students.FirstOrDefault(s => s.IsCaptain);

            // Get the userRole and put it in the ViewBag
            ViewBag.UserRole = User.IsInRole("Admin") ? "Admin" : User.IsInRole("Coach") ? "Coach" : "Student";

            TeamVM teamVM = new()
            {
                Team = team,
                StudentList = students.Select(s => new SelectListItem
                {
                    Text = $"{s.Name} {s.LastName} {s.SecondLastName}",
                    Value = s.Id.ToString()
                }),
                Captain = captain,
            };

            return View(teamVM);
        }

        #region API CALLS

        [HttpPost]
        [Authorize(Roles = "Admin, Coach")]
        public IActionResult ChangeCaptain(int teamId, int newCaptainId)
        {
            try
            {
                // Get current captain if exists
                var currentCaptain = _contenedorTrabajo.Student.GetAll(s => s.TeamId == teamId && s.IsCaptain).FirstOrDefault();

                // Remove captain status from current captain
                if (currentCaptain != null)
                {
                    currentCaptain.IsCaptain = false;
                    _contenedorTrabajo.Student.Update(currentCaptain);
                }  

                // Set new captain
                var newCaptain = _contenedorTrabajo.Student.GetById(newCaptainId);
                if (newCaptain == null || newCaptain.TeamId != teamId)
                {
                    return Json(new { success = false, message = "Estudiante no encontrado o no pertenece al equipo" });
                }

                newCaptain.IsCaptain = true;
                _contenedorTrabajo.Student.Update(newCaptain);
                _contenedorTrabajo.Save();

                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error al cambiar el capitán: " + ex.Message });
            }
        }

        //Método para generar el PDF de todos los estudiantes de un equipo
        [HttpPost]
        [Authorize(Roles = "Admin, Coach, Student")]
        public IActionResult GenerateStudentsCertificates(int teamId)
        {
            var users = _contenedorTrabajo.User.GetAll(u => u.LockoutEnd == null && u.StudentId != null).Select(u => u.StudentId).ToList();
            var students = _contenedorTrabajo.Student.GetAll(s => s.TeamId == teamId && users.Contains(s.Id)).ToList();

            if (students.Count == 0)
            {
                return NotFound();
            }

            // Obtener el nombre del equipo
            var team = _contenedorTrabajo.Team.GetById(teamId);
            if (team == null)
            {
                return NotFound();
            }
            var teamName = team.Name.Replace(" ", "_"); // Reemplaza espacios por guiones bajos para evitar problemas en el nombre del archivo
            var date = DateTime.Now.ToString("yyyy-MM-dd");
            var fileName = $"Informacion_{teamName}_{date}.pdf";

            var document = QuestPDF.Fluent.Document.Create(container =>
            {
                foreach (var student in students)
                {
                    var coach = _contenedorTrabajo.Coach.GetById(team.CoachId);

                    container.Page(page =>
                    {
                        page.Size(PageSizes.A4);
                        page.Margin(1, Unit.Centimetre);

                        page.Content().Element(c => CreateStudentCell(c, student, coach, team));
                        page.Footer().Text(text => text.CurrentPageNumber());
                    });
                }
            });

            byte[] pdfBytes = document.GeneratePdf();

            // Configurar el encabezado Content-Disposition con el nombre personalizado
            Response.Headers.Add("Content-Disposition", $"attachment; filename=\"{fileName}\"");
            return File(pdfBytes, "application/pdf");
        }


        // Método para generar el PDF de un solo estudiante
        [HttpPost]
        [Authorize(Roles = "Admin, Coach, Student")]
        public IActionResult GenerateStudentCertificate(int id)
        {
            // Obtener el estudiante por su ID
            var student = _contenedorTrabajo.Student.GetById(id);
            if (student == null)
            {
                return NotFound("Estudiante no encontrado");
            }

            var team = _contenedorTrabajo.Team.GetById(student.TeamId);
            var coach = _contenedorTrabajo.Coach.GetById(team.CoachId);

            // Formato de fecha
            var date = DateTime.Now.ToString("yyyy-MM-dd");
            var studentName = student.Name.Replace(" ", "_"); // Reemplaza espacios por guiones bajos
            var studentControlNumber = student.ControlNumber;
            var fileName = $"Informacion_{studentName}_{studentControlNumber}_{date}.pdf";

            // Crear el documento PDF usando QuestPDF
            var document = QuestPDF.Fluent.Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(1, Unit.Centimetre);
                    page.Content().Element(c => CreateStudentCell(c, student, coach, team));
                    page.Footer().Text(text => text.CurrentPageNumber());
                });
            });

            byte[] pdfBytes = document.GeneratePdf();

            // Configurar el encabezado Content-Disposition con el nombre personalizado
            Response.Headers.Add("Content-Disposition", $"attachment; filename=\"{fileName}\"");
            return File(pdfBytes, "application/pdf");
        }

        private static void CreateStudentCell(IContainer container, Models.Student student,  Models.Coach coach, Models.Team team)
        {
            string imageUrl = student.ImageUrl != null && student.ImageUrl.StartsWith("/")
                ? student.ImageUrl.Substring(1)
                : student.ImageUrl ?? "";

            imageUrl = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", imageUrl.TrimStart('\\'));

            if(!System.IO.File.Exists(imageUrl))
            {
                imageUrl = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", "zorro_default.png");
            }

            byte[] imageBytes = Array.Empty<byte>();

            imageBytes = System.IO.File.ReadAllBytes(imageUrl);

            container.Padding(2)
                .Border(1)
                .BorderColor(Colors.Black)
                .Background(Colors.Grey.Lighten4)
                .DefaultTextStyle(x => x.FontSize(14).LineHeight(1.5f))
                .Column(column =>
                {
                    // Row for Image and Name
                    column.Item().Padding(5).Row(row =>
                    {
                        // Centering the image and name
                        row.RelativeItem().Column(innerColumn =>
                        {
                            innerColumn.Item()
                                .AlignMiddle()
                                .AlignCenter()
                                .Width(140) // Define the width of the image
                                .Height(140) // Define the height of the image
                                .Image(imageBytes);

                            // Name above and below the image
                            innerColumn.Item().Text($"{student.Name ?? "Sin Actualizar"} {student.LastName ?? "Sin Actualizar"} {student.SecondLastName ?? "Sin Actualizar"}").Bold().FontSize(18).AlignCenter();
                        });
                    });

                    // Two-column layout for the rest of the information
                    column.Item().PaddingLeft(10).PaddingBottom(5).Row(row =>
                    {
                        row.RelativeItem().Column(leftColumn =>
                        {
                            leftColumn.Item().Text("Número de control").Bold();
                            leftColumn.Item().Text(student.ControlNumber ?? "Sin Actualizar");

                            leftColumn.Item().Text("Carrera").Bold();
                            leftColumn.Item().Text(student.Career ?? "Sin Actualizar");

                            leftColumn.Item().Text("CURP").Bold();
                            leftColumn.Item().Text(student.Curp ?? "Sin Actualizar");

                            leftColumn.Item().Text("Fecha de Nacimiento").Bold();
                            leftColumn.Item().Text(student.BirthDate ?? "Sin Actualizar");

                            leftColumn.Item().Text("Teléfono").Bold();
                            leftColumn.Item().Text(student.Phone ?? "Sin Actualizar");

                            leftColumn.Item().Text("NSS").Bold();
                            leftColumn.Item().Text(student.Nss ?? "Sin Actualizar");

                            leftColumn.Item().Text("Número de Jugador").Bold();
                            leftColumn.Item().Text(student.numberUniform?.ToString() ?? "Sin Actualizar");

                            leftColumn.Item().Text("Fecha de Ingreso").Bold();
                            leftColumn.Item().Text(student.enrollmentData?.ToString() ?? "Sin Actualizar");
                        });

                        row.RelativeItem().Column(rightColumn =>
                        {
                            rightColumn.Item().Text("Edad").Bold();
                            rightColumn.Item().Text(student.Age?.ToString() ?? "Sin Actualizar");

                            rightColumn.Item().Text("Tipo de Sangre").Bold();
                            rightColumn.Item().Text(student.BloodType ?? "Sin Actualizar");

                            rightColumn.Item().Text("Peso").Bold();
                            rightColumn.Item().Text(student.Weight ?? "Sin Actualizar");

                            rightColumn.Item().Text("Altura").Bold();
                            rightColumn.Item().Text(student.Height ?? "Sin Actualizar");

                            rightColumn.Item().Text("Alergias").Bold();
                            rightColumn.Item().Text(student.Allergies ?? "Sin Actualizar");

                            rightColumn.Item().Text("Entrenador").Bold();
                            rightColumn.Item().Text(coach.Name ?? "Sin Actualizar");

                            rightColumn.Item().Text("Equipo").Bold();
                            rightColumn.Item().Text(team.Name ?? "Sin Actualizar");

                            rightColumn.Item().Text("Capitán").Bold();
                            rightColumn.Item().Text(student.IsCaptain ? "Sí" : "No");
                        });
                    });
                });
        }

        // Cedula
        [HttpPost]
        [Authorize(Roles = "Admin, Coach, Student")]
        public IActionResult GenerateCertificate([FromBody] CertificateRequest request)
        {
            if (request.Students == null || request.Students.Count == 0)
            {
                return BadRequest("No se han seleccionado estudiantes.");
            }

            // Crear el documento PDF usando QuestPDF y agregar la información de los estudiantes
            var document = QuestPDF.Fluent.Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(0.5f, Unit.Centimetre);


                    page.Content().Table(table =>
                    {
                        table.ColumnsDefinition(columns =>
                        {
                            columns.RelativeColumn();
                            columns.RelativeColumn();
                        });

                        // Añadir filas para los estudiantes en un bucle
                        for (int i = 0; i < request.Students.Count; i++)
                        {
                            table.Cell().ShowEntire().Element(c => CreateStudentCellCertificate(c, request.Students[i]));
                            
                        }
                        if (request.Coach != null)
                            table.Cell().ShowEntire().Element(c => CreateCoachCellCertificate(c, request.Coach, request.Team));
                    });

                    page.Footer().Text(text =>
                    {
                        text.CurrentPageNumber();
                    });
                });
            });

            // Generar y devolver el PDF
            using var stream = new MemoryStream();
            document.GeneratePdf(stream);
            stream.Position = 0;

            return File(stream.ToArray(), "application/pdf", "Cedula.pdf");
        }

        private static void HeaderCells(ColumnDescriptor column)
        {
            column.Item().Row(row =>
            {
                var logoSEP = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", "SEP-LOGO.png");
                var logoTecNM = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", "Logo_TECNM_AZUL.png");

                if (System.IO.File.Exists(logoSEP) && System.IO.File.Exists(logoTecNM))
                {
                    // Leer la imagen de SEP
                    byte[] imageSEPBytes = System.IO.File.ReadAllBytes(logoSEP);

                    // Leer la imagen de TecNM
                    byte[] imageTecNMBytes = System.IO.File.ReadAllBytes(logoTecNM);

                    // Imagen de SEP en contenedor alineado
                    row.ConstantItem(3.5f, Unit.Centimetre)
                       .AlignMiddle() // Alinea todo el contenedor verticalmente al centro
                       .Element(container =>
                       {
                           container
                                .PaddingLeft(5)
                               .Image(imageSEPBytes)
                               .FitArea();
                       });

                    // Espacio relativo para mantener separación entre imágenes
                    row.RelativeItem();

                    // Imagen de TecNM en contenedor alineado
                    row.ConstantItem(2.5f, Unit.Centimetre)

                       .AlignMiddle()
                       .Element(container =>
                       {
                           container
                               .PaddingRight(10)
                               .Image(imageTecNMBytes)
                               .FitArea();
                       });
                }

            });
        }

        //CEDULA generar celdas
        private static void CreateStudentCellCertificate(IContainer container, Models.Student student)
        {
            string imageUrl = student.ImageUrl != null && student.ImageUrl.StartsWith("/")
                ? student.ImageUrl.Substring(1)
                : student.ImageUrl ?? "";

            imageUrl = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", imageUrl.TrimStart('\\'));

            if (!System.IO.File.Exists(imageUrl))
            {
                imageUrl = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", "zorro_default.png");
            }

            byte[] imageBytes = Array.Empty<byte>();

            imageBytes = System.IO.File.ReadAllBytes(imageUrl);


            container
                .PaddingVertical(2)
                .PaddingHorizontal(10)
                .Border(1)
                .BorderColor(Colors.Black)
                .Background(White)
                .DefaultTextStyle(x => x.FontSize(8).LineHeight(1.5f))
                .Column(column =>
                {
                    HeaderCells(column);

                    column.Item()
                    .AlignCenter()
                    .Text($"{student.Name ?? "Sin actualizar"} {student.LastName ?? "Sin actualizar"} {student.SecondLastName ?? "Sin actualizar"}")
                    .FontSize(14);

                    column.Item().AlignCenter().Text($"{student.Career ?? "Carrera sin actualizar"}");

                    column.Item().Padding(10).Row(row =>
                    {
                        row.ConstantItem(3f, Unit.Centimetre)
                            .Height(3f, Unit.Centimetre)
                            .Width(2.5f, Unit.Centimetre)
                            .AlignMiddle()
                            .AlignCenter()
                            .Image(imageBytes)
                            .FitArea();

                        row.RelativeItem().PaddingTop(20).Column(col =>
                        {
                            col.Item().Text("Número de control").Bold().AlignCenter();
                            col.Item().PaddingBottom(10).Text(student.ControlNumber ?? "Sin actualizar").AlignCenter();

                            col.Item().Text("Semestre").Bold().AlignCenter();
                            col.Item().Text($"{(student.Semester != null ? $"{student.Semester}° semestre" : "Sin actualizar")}").AlignCenter();
                        });
                    });

                    column.Item().PaddingLeft(10).PaddingBottom(15).Column(innerColumn =>
                    {
                        innerColumn.Item().Text("Fecha de ingreso").Bold();
                        innerColumn.Item().Text($"{(student.enrollmentData.HasValue ? student.enrollmentData : "Sin actualizar")}");
                    });

                    column.Item().Background("#1A3C6E").PaddingVertical(3).Text("Firma").Bold().FontSize(10).FontColor(White).AlignCenter();
                });
        }

        private static void CreateCoachCellCertificate(IContainer container, Models.Coach coach, string teamName)
        {
            string imageUrl = coach.ImageUrl != null && coach.ImageUrl.StartsWith("/")
                ? coach.ImageUrl.Substring(1)
                : coach.ImageUrl ?? "";

            imageUrl = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", imageUrl.TrimStart('\\'));

            if (!System.IO.File.Exists(imageUrl))
            {
                imageUrl = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", "zorro_default.png");
            }

            byte[] imageBytes = Array.Empty<byte>();

            imageBytes = System.IO.File.ReadAllBytes(imageUrl);

            container
                .PaddingVertical(2)
                .PaddingHorizontal(10)
                .Border(1)
                .BorderColor(Colors.Black)
                .Background(White) 
                .Column(column =>
                {
                    HeaderCells(column);

                    // Cabecera: nombre del equipo centrado
                    column.Item().PaddingBottom(10).Text(teamName).FontSize(10).Bold().LineHeight(1.5f).AlignCenter();
                    column.Item().Text("Entrenador").FontSize(8).AlignCenter();

                    // Imagen del entrenador centrada
                    column.Item().AlignCenter().Element(container =>
                    {
                        container
                            .Width(3.5f, Unit.Centimetre)
                            .Height(4.5f, Unit.Centimetre)
                            .Image(imageBytes)
                            .FitArea();
                    });


                    // Nombre del entrenador debajo de la imagen, centrado
                    column.Item().PaddingTop(10).PaddingBottom(10).Text($"{coach.Name} {coach.LastName} {coach.SecondLastName}").FontSize(12).AlignCenter();
                });
        }

        [HttpGet]
        [Authorize(Roles = "Admin, Coach, ")]
        public IActionResult GetAll(string? category)
        {
            if (category == "deportivos")
            {

                // Filtra los equipos cuyos representantes tienen categoría "deportivo"
                return Json (new
                {
                    data = _contenedorTrabajo.Team.GetAll(
                    filter: t => t.Representative.Category == "Deportivo",
                    includeProperties: "Representative,Coach"
                )
                });
            }
            else if (category == "culturales")
            {
                // Filtra los equipos cuyos representantes tienen categoría "cultural"
                return Json(new
                {
                    data = _contenedorTrabajo.Team.GetAll(
                    filter: t => t.Representative.Category == "Cultural",
                    includeProperties: "Representative,Coach"
                )
                });
            }
            return Json(new { data = _contenedorTrabajo.Team.GetAll(includeProperties: "Representative,Coach") });
        }

        [HttpGet]
        [Authorize(Roles = "Admin, Coach, Student")]
        public IActionResult GetStudentsByTeamId(int teamId)
        {
            var users = _contenedorTrabajo.User.GetAll(u => u.LockoutEnd == null && u.StudentId != null).Select(u => u.StudentId).ToList();
            // Lista de estudiantes que pertenecen al equipo y no están bloqueados como usuarios
            var students = _contenedorTrabajo.Student.GetAll(s => s.TeamId == teamId && users.Contains(s.Id)).ToList();
            return Json(new { data = students });
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public IActionResult GenerateUniformInfoPdf([FromBody] List<StudentInfo> students, [FromQuery] string category, [FromQuery] string teamName)
        {
            // IDs de los estudiantes
            var studentIds = students.Select(s => s.Id).ToList();

            // Obtener uniformes según los IDs de estudiantes proporcionados, ordenados
            var uniforms = _contenedorTrabajo.Uniform.GetAll(
                filter: u => studentIds.Contains(u.StudentId),
                orderBy: q => q.OrderBy(u => u.StudentId)
                .ThenBy(u => u.RepresentativeUniformCatalog.UniformCatalog.Name),
                includeProperties: "RepresentativeUniformCatalog.UniformCatalog"
            )
            .Select(u => new
            {
                u.Id,
                u.StudentId,
                u.size,
                UniformCatalogName = u.RepresentativeUniformCatalog.UniformCatalog.Name 
            }).ToList();

            if (uniforms == null || !uniforms.Any())
            {
                return new ObjectResult(new { success = false, message = "Error al generar el archivo PDF" })
                {
                    StatusCode = 404 
                };
            } 
            // Crear el documento PDF
            var date = DateTime.Now.ToString("yyyy-MM-dd");
            var fileName = $"Uniformes_{date}.pdf";
            var uniformCount = new Dictionary<string, Dictionary<string, int>>();

            var document = QuestPDF.Fluent.Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(2, Unit.Centimetre);
                    page.MarginTop(0);

                    HeaderPdf(page);

                    page.Content().Column(col =>
                    {
                        // Título
                        col.Item().PaddingBottom(15).Text($"Uniformes {teamName}").FontSize(18).Bold().AlignCenter();

                        foreach (var student in students)
                        {
                            col.Item()
                                .Border(0.5f)
                                .Background("#1A3C6E")
                                .PaddingHorizontal(2.5f)
                                .Row(row =>
                            {
                                row.RelativeItem().Text(
                                    string.IsNullOrWhiteSpace($"{student.Name} {student.LastName} {student.SecondLastName}")
                                    ? "Sin actualizar"
                                    : $"{student.Name} {student.LastName} {student.SecondLastName}"
                                ).FontSize(12).FontColor(White).Bold();
                                    
                                    if(category == Categorys.Deportivo)
                                        row.RelativeItem().AlignRight().Text(student.Number.HasValue ? $"Número: {student.Number}" : " Número sin actualizar").FontColor(White).FontSize(12);
                            });


                            // Tabla de uniformes
                            col.Item().PaddingBottom(10).Table(table =>
                            {
                                // Encabezados
                                table.ColumnsDefinition(columns =>
                                {
                                    columns.RelativeColumn(); 
                                    columns.RelativeColumn(); 
                                });

                                table.Header(header =>
                                {
                                    header.Cell().Border(0.5f).PaddingLeft(2.5f).Text("Prenda").Bold();
                                    header.Cell().Border(0.5f).PaddingLeft(2.5f).Text("Talla").Bold();
                                });

                                foreach (var uniform in uniforms.Where(u => u.StudentId == student.Id))
                                {
                                    table.Cell().Border(0.5f).PaddingLeft(2.5f).Text(uniform.UniformCatalogName?.ToString() ?? "Uniforme no identificado");
                                    table.Cell().Border(0.5f).PaddingLeft(2.5f).Text(uniform.size?.ToString() ?? "Sin actualizar");

                                    if (!uniformCount.ContainsKey(uniform.UniformCatalogName?.ToString() ?? "Uniforme no identificado"))
                                    {
                                        uniformCount[uniform.UniformCatalogName?.ToString() ?? "Uniforme no identificado"] = new Dictionary<string, int>();
                                    }

                                    if (uniformCount[uniform.UniformCatalogName?.ToString() ?? "Uniforme no identificado"].ContainsKey(uniform.size?.ToString() ?? "Sin actualizar"))
                                    {
                                        uniformCount[uniform.UniformCatalogName?.ToString() ?? "Uniforme no identificado"][uniform.size?.ToString() ?? "Sin actualizar"]++;
                                    }
                                    else
                                    {
                                        uniformCount[uniform.UniformCatalogName?.ToString() ?? "Uniforme no identificado"][uniform.size?.ToString() ?? "Sin actualizar"] = 1;
                                    }
                                }
                            });
                        }


                        //Contador para el resumen
                        col.Item().PaddingBottom(10).PaddingTop(20).Text("Resumen").AlignCenter().FontSize(16).Bold();

                        foreach (var uniformEntry in uniformCount)
                        {
                            // Título de la prenda
                            col.Item().Background("#1A3C6E").Border(0.5f).PaddingLeft(2.5f).Text(uniformEntry.Key).FontColor(White).FontSize(14).Bold();
                            
                            // Tabla para las tallas y cantidades
                            col.Item().PaddingBottom(10).Table(sizeTable =>
                            {
                                // Definir columnas
                                sizeTable.ColumnsDefinition(columns =>
                                {
                                    columns.RelativeColumn(); // Talla
                                    columns.RelativeColumn(); // Cantidad
                                });

                                // Encabezados
                                sizeTable.Header(header =>
                                {
                                    header.Cell().Border(0.5f).PaddingLeft(2.5f).Text("Talla").Bold();
                                    header.Cell().Border(0.5f).PaddingLeft(2.5f).Text("Cantidad").Bold();
                                });

                                // Agregar datos del diccionario secundario
                                foreach (var sizeEntry in uniformEntry.Value.OrderBy(entry => GetSizeOrder(entry.Key)))
                                {
                                    sizeTable.Cell().Border(0.5f).PaddingLeft(2.5f).Text(sizeEntry.Key); // Talla
                                    sizeTable.Cell().Border(0.5f).PaddingLeft(2.5f).Text(sizeEntry.Value.ToString()); // Cantidad
                                }
                            });
                        }
                    });

                    page.Footer().AlignRight().Text(text =>
                    {
                        text.CurrentPageNumber();
                    });
                });
            });

            byte[] pdfBytes = document.GeneratePdf();

            // Configurar el encabezado Content-Disposition con el nombre personalizado
            return File(pdfBytes, "application/pdf", fileName);
        }

        // Método auxiliar para obtener el índice de la talla según el enum
        private int GetSizeOrder(string size)
        {
            return Enum.TryParse(size, out Models.Size parsedSize) ? (int)parsedSize : int.MaxValue;
        }

        public void HeaderPdf(PageDescriptor page)
        {
            page.Header().Row(row =>
            {
                var logoSEP = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", "SEP-LOGO.png");
                var logoTecNM = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", "Logo_TECNM_AZUL.png");

                if (System.IO.File.Exists(logoSEP) && System.IO.File.Exists(logoTecNM))
                {
                    // Leer la imagen de SEP
                    byte[] imageSEPBytes = System.IO.File.ReadAllBytes(logoSEP);

                    // Leer la imagen de TecNM
                    byte[] imageTecNMBytes = System.IO.File.ReadAllBytes(logoTecNM);

                    // Imagen de SEP en contenedor alineado
                    row.ConstantItem(7f, Unit.Centimetre)
                       .AlignMiddle() // Alinea todo el contenedor verticalmente al centro
                       .Element(container =>
                       {
                           container
                               .Height(3.5f, Unit.Centimetre) // Altura específica para la imagen de SEP
                               .AlignMiddle() // Centra la imagen dentro del contenedor
                               .Image(imageSEPBytes)
                               .FitArea();
                       });

                    // Espacio relativo para mantener separación entre imágenes
                    row.RelativeItem();

                    // Imagen de TecNM en contenedor alineado
                    row.ConstantItem(5f, Unit.Centimetre)
                       .AlignMiddle() // Alinea todo el contenedor verticalmente al centro
                       .Element(container =>
                       {
                           container
                               .Height(2f, Unit.Centimetre) // Altura específica para la imagen de TecNM
                               .AlignMiddle() // Centra la imagen dentro del contenedor
                               .Image(imageTecNMBytes)
                               .FitArea();
                       });
                }

            });
        }

        [HttpPost]
        public IActionResult GenerateListStudentPdf([FromBody] List<Models.Student> students, [FromQuery] string teamName)
        {

            var date = DateTime.Now.ToString("yyyy-MM-dd");
            var fileName = $"Lista_Estudiantes_{date}.pdf";

            var document = QuestPDF.Fluent.Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(2f, Unit.Centimetre);
                    page.MarginTop(0);


                    HeaderPdf(page);

                    page.Content().Column(col =>
                    {
                        // Título
                        col.Item().PaddingBottom(15).Text($"Lista de estudiantes {teamName.ToLower()}").FontSize(18).Bold().AlignCenter();

                        // Tabla de estudiantes
                        col.Item().Border(0.5f).Table(table =>
                        {
                            // Definir las columnas
                            table.ColumnsDefinition(columns =>
                            {
                                columns.RelativeColumn(1); // Semestre
                                columns.RelativeColumn(4); // Nombre completo
                                columns.RelativeColumn(2); // Número de control
                            });

                            // Encabezados
                            table.Header(header =>
                            {
                                header.Cell().Border(0.5f).Background("#1A3C6E").PaddingLeft(2.5f).Text("Semestre").Bold().FontColor(White).FontSize(12);
                                header.Cell().Border(0.5f).Background("#1A3C6E").PaddingLeft(2.5f).Text("Nombre completo").Bold().FontColor(White).FontSize(12);
                                header.Cell().Border(0.5f).Background("#1A3C6E").PaddingLeft(2f).Text("No. Control").Bold().FontColor(White).FontSize(12);
                            });

                            // Agregar filas de estudiantes
                            foreach (var student in students)
                            {
                                table.Cell().Border(0.5f).PaddingLeft(2.5f).Text(!string.IsNullOrWhiteSpace(student.Semester) ? student.Semester : "Sin actualizar").FontSize(10);
                                table.Cell().Border(0.5f).PaddingLeft(2.5f).Text(
                                    !string.IsNullOrWhiteSpace($"{student.Name} {student.LastName} {student.SecondLastName}")
                                    ? $"{student.Name} {student.LastName} {student.SecondLastName}"
                                    : "Sin actualizar"
                                ).FontSize(10);
                                table.Cell().Border(0.5f).PaddingLeft(2.5f).Text(!string.IsNullOrWhiteSpace(student.ControlNumber) ? student.ControlNumber : "Sin actualizar").FontSize(10);
                            }
                        });
                    });

                    // Pie de página
                    page.Footer().AlignRight().Text(text =>
                    {
                        text.CurrentPageNumber();
                    });
                });
            });

            byte[] pdfBytes = document.GeneratePdf();

            // Configurar el encabezado Content-Disposition con el nombre personalizado
            return File(pdfBytes, "application/pdf", fileName);
        }


    [HttpDelete]
        [Authorize(Roles = "Admin")]
        public IActionResult Delete(int id)
        {
            var objFromDb = _contenedorTrabajo.Team.GetById(id);

            if (objFromDb == null)
            {
                return Json(new { success = false, message = "Error al borrar el equipo" });
            }

            // Verificar si el equipo tiene estudiantes asignados
            var teamWithStudents = _contenedorTrabajo.Student.GetAll(s => s.TeamId == id);

            //Si el equipo tiene alumnos asignados, no se puede borrar  
            if (teamWithStudents.Any())
            {
                return Json(new { success = false, message = "No se puede borrar el equipo porque tiene alumnos asignados" });
            }

            // Si llegamos aquí, podemos borrar la imagen y el equipo
            string webRootPath = _hostingEnvironment.WebRootPath;
            var imagePath = Path.Combine(webRootPath, objFromDb.ImageUrl.TrimStart('\\'));

            if (System.IO.File.Exists(imagePath))
            {
                System.IO.File.Delete(imagePath);
            }

            _contenedorTrabajo.Team.Remove(objFromDb);
            _contenedorTrabajo.Save();
            return Json(new { success = true, message = "Equipo borrado exitosamente" });
        }

        #endregion

    }
}


public class StudentInfo
{
    public int Id { get; set; }
    public string? Name { get; set; }
    public string? LastName { get; set; }
    public string? SecondLastName { get; set; }
    public int? Number { get; set; }
}
