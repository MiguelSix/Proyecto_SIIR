using Microsoft.AspNetCore.Mvc.Rendering;
using SIIR.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SIIR.DataAccess.Data.Repository.IRepository
{
    public interface ICoachRepository: IRepository<Coach>
    {
        IEnumerable<SelectListItem> GetCoachesList();
        
        //Implementamos el Update, ya que Irepositorie ya cuenta con GetId, Remove, GetLista, etc
        void Update(Coach coach);
    }
}
