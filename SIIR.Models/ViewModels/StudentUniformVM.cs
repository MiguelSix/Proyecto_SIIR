using Microsoft.AspNetCore.Mvc.Rendering;

namespace SIIR.Models.ViewModels
{
	public class StudentUniformVM
	{
		public Student? student { get; set; }
		public List<Uniform>? uniforms { get; set; }

		public List<string> namesUniform { get; set; } = new List<string>();

	}
}
