using Microsoft.AspNetCore.Mvc.Rendering;

namespace SIIR.Models.ViewModels
{
	public class StudentUniformVM
	{
		public Student? student { get; set; }
		public List<Uniform>? Uniforms { get; set; }
		public int? playerNumber { get; set; }
	}
}
