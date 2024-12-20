﻿using SIIR.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SIIR.DataAccess.Data.Repository.IRepository
{
    public interface IUserRepository : IRepository<ApplicationUser>
    {
        void LockUser(string userId);
        void UnlockUser(string userId);
        void Update(ApplicationUser user);
    }
}
