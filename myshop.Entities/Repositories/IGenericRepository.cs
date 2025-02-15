using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace myshop.Entities.Repositories
{
    public interface IGenericRepository<T> where T : class
    {
        //_Context.Categories.ToList();
        //_Context.Categories.Where(x=>x.Id == id).ToList();
        //_Context.Categories.Include("Product").ToList();
        public IEnumerable<T> GetAll(Expression<Func<T, bool>>? predicate = null, string? IncludeWord = null);

        //_Context.Categories.Where(x=>x.Id == id).SingleOrDefault();
        //_Context.Categories.Include("Product").SingleOrDefault();
        T GetFirstOrDefault(Expression<Func<T, bool>>? predicate = null, string? IncludeWord = null);


        //_Context.Categories.Add(category);
        void Add(T entity);

        //_Context.Categories.Remove(category);
        void Remove(T entity);

        void RemoveRange(IEnumerable<T> entity);

        
    }
}
