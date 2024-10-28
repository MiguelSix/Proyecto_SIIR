using Microsoft.EntityFrameworkCore;
using SIIR.DataAccess.Data.Repository.IRepository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace SIIR.DataAccess.Data.Repository
{
    public class Repository<T> : IRepository<T> where T : class
    {
        protected readonly DbContext Context;
        internal DbSet<T> dbSet;

        public Repository(DbContext context)
        {
            Context = context;
            this.dbSet = context.Set<T>();
        }

        public void Add(T entity)
        {
            dbSet.Add(entity);
        }
        public T GetById(int? id)
        {
            return dbSet.Find(id);
        }

        public IEnumerable<T> GetAll(Expression<Func<T, bool>>? filter = null, Func<IQueryable<T>, IOrderedQueryable<T>>? orderBy = null, string? includeProperties = null)
        {
            // creates a query IQueryable<T> that is equal to dbSet
            IQueryable<T> query = dbSet;

            // if filter is not null, then query is equal to query.Where(filter)
            if (filter != null)
            {
                query = query.Where(filter);
            }

            // It includes the navegation properties
            if (includeProperties != null)
            {
                // splits the string includeProperties by ',' and removes empty entries
                foreach (var includeProperty in includeProperties.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
                {
                    query = query.Include(includeProperty);
                }
            }

            // applies the orderBy function to query if it is not null
            if (orderBy != null)
            {
                // returns the result of orderBy(query) as a List
                return orderBy(query).ToList();
            }

            // returns the result of query as a List if dont have orderBy
            return query.ToList();
        }

        public T GetFirstOrDefault(Expression<Func<T, bool>>? filter = null, string? includeProperties = null)
        {
            IQueryable<T> query = dbSet;

            if (filter != null)
            {
                query = query.Where(filter);
            }

            // It includes the navegation properties
            if (includeProperties != null)
            {
                // splits the string includeProperties by ',' and removes empty entries
                foreach (var includeProperty in includeProperties.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
                {
                    query = query.Include(includeProperty);
                }
            }

            // returns the first element of query or null if query is empty
            return query.FirstOrDefault();
        }

        public void Remove(int id)
        {
            T entityToRemove = dbSet.Find(id);
        }

        public void Remove(T entity)
        {
            dbSet.Remove(entity);
        }
    }
}
