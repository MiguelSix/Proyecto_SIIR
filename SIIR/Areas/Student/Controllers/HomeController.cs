﻿using Microsoft.AspNetCore.Mvc;

namespace SIIR.Areas.Student.Controllers
{
	[Area("Student")]
	public class HomeController : Controller
	{
        [HttpGet]
        public IActionResult Index()
		{
			return View();
		}
        [HttpGet]
        public IActionResult Edit()
		{
			return View();
		}
	}
}