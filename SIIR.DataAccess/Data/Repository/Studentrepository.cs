using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SIIR.Data;
using SIIR.DataAccess.Data.Repository.IRepository;
using SIIR.Models;
using SIIR.Models.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SIIR.DataAccess.Data.Repository
{
    public class StudentRepository: Repository<Student>, IStudentRepository
    {
        private readonly ApplicationDbContext _db;

        public StudentRepository(ApplicationDbContext db) : base(db)
        {
            _db = db;
        }

        public IEnumerable<SelectListItem> GetStudentsList()
        {
            return _db.Students.Select(i => new SelectListItem
            {
                Text = i.Name + " " + i.LastName,
                Value = i.Id.ToString()
            });
        }

        public void Update(Student student)
        {
            var objFromDb = _db.Students.FirstOrDefault(s => s.Id == student.Id);

            if (objFromDb != null)
            {
                objFromDb.Name = student.Name;
                objFromDb.LastName = student.LastName;
                objFromDb.SecondLastName = student.SecondLastName;
                objFromDb.ControlNumber = student.ControlNumber;
                objFromDb.Curp = student.Curp;
                objFromDb.BirthDate = student.BirthDate;
                objFromDb.Career = student.Career;
                objFromDb.Semester = student.Semester;
                objFromDb.Phone = student.Phone;
                objFromDb.Age = student.Age;
                objFromDb.BloodType = student.BloodType;
                objFromDb.Email = student.Email;
                objFromDb.Weight = student.Weight;
                objFromDb.Height = student.Height;
                objFromDb.Allergies = student.Allergies;
                objFromDb.Nss = student.Nss;
                objFromDb.ImageUrl = student.ImageUrl;

                _db.SaveChanges();
            }
        }

        public void AssignCaptain(Student student)
        {
            // Desasigna al capitán actual si hay uno
            var currentCaptain = _db.Students.FirstOrDefault(s => s.IsCaptain);
            if (currentCaptain != null && currentCaptain.Id != student.Id)
            {
                currentCaptain.IsCaptain = false;
            }

            // Asigna al nuevo capitán
            var objFromDb = _db.Students.FirstOrDefault(s => s.Id == student.Id);
            if (objFromDb != null)
            {
                objFromDb.IsCaptain = true; // Asegurarse de que esté asignado
                _db.SaveChanges();
            }
        }

        public void UnassignCaptain(Student student)
        {
            var objFromDb = _db.Students.FirstOrDefault(s => s.Id == student.Id);

            if (objFromDb != null)
            {
                objFromDb.IsCaptain = student.IsCaptain;

                _db.SaveChanges();
            }
        }
    }
}
