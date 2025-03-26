using Fun_Funding.Application.ViewModel;
using MongoDB.Driver;
using System.Linq.Expressions;

namespace Fun_Funding.Application.IRepository
{
    public interface IMongoBaseRepository<T> where T : class
    {
        void Create(T entity);
        List<T> GetAll();
        T Get(Expression<Func<T, bool>> filter);
        List<T> GetList(Expression<Func<T, bool>> filter);
        PaginatedResponse<T> GetAllPaged(ListRequest request, Expression<Func<T, bool>> filter = null);
        void Remove(Expression<Func<T, bool>> filter);
        void Update(Expression<Func<T, bool>> filter, UpdateDefinition<T> updateDefinition);
        public void SoftRemove(Expression<Func<T, bool>> filter, UpdateDefinition<T> updateDefinition);
        IQueryable<T> GetQueryable();
        Task CreateAsync(T entity);
        Task<List<T>> GetAllAsync(FilterDefinition<T>? filter = null, SortDefinition<T>? sort = null);

    }

}
