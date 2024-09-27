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
        void Save();

    }
}
