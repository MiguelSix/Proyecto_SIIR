﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace testBlog.AccesoDatos.Data.Repository.IRepository
{
    public interface IContenedorTrabajo : IDisposable
    {
        void Save();

    }
}