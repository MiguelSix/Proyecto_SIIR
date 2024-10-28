using SIIR.DataAccess.Data.Repository.IRepository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SIIR.DataAccess.Data.Repository.IRepository
{
    public interface IContenedorTrabajo : IDisposable
    {
        IUserRepository User { get; }
        ITeamRepository Team { get; }
        IRepresentativeRepository Representative { get; }
        IUniformCatalogRepository UniformCatalog { get; }
        ICoachRepository Coach { get; }
        IStudentRepository Student { get; }
        IDocumentCatalogRepository DocumentCatalog { get; }
        IDocumentRepository Document { get; }

        void Save();
    }
}
