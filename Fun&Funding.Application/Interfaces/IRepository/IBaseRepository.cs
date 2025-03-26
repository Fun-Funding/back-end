using System.Linq.Expressions;

namespace Fun_Funding.Application.IRepository
{
    public interface IBaseRepository<T> where T : class
    {
        T Get(Expression<Func<T, bool>> predicate);
        T GetById(object id);
        Task<T> GetByIdAsync(object id, CancellationToken cancellationToken = default);
        IEnumerable<T> GetAll();
        IEnumerable<T> GetAll(Expression<Func<T, bool>> predicate = null);
        // Soft delete-related methods
        Task<IEnumerable<T>> GetAllDeletedNoPaginationAsync(Expression<Func<T, bool>>? predicate = null); // Retrieve deleted entities no pagination
        IEnumerable<T> GetAllDeletedNoPagination(Expression<Func<T, bool>>? predicate = null);
        Task<IEnumerable<T>> GetAllDeletedAsync(
            Expression<Func<T, bool>> filter = null,
            Expression<Func<T, object>> orderBy = null,
            bool isAscending = false,
            string includeProperties = "",
            int? pageIndex = null,
            int? pageSize = null,
            CancellationToken cancellationToken = default);
        T GetDeleted(Expression<Func<T, bool>> predicate); // Get deleted by condition
        Task<T> GetDeletedAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default);
        void Add(T entity);
        void AddRange(IEnumerable<T> entities);
        void Update(T entity);
        void UpdateRange(IEnumerable<T> entities);
        void Remove(T entity);
        void RemoveRange(IEnumerable<T> entities);
        Task<T> GetAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default);
        Task<IEnumerable<T>> GetAllAsync(CancellationToken cancellationToken = default);
        Task<IEnumerable<T>> GetAllAsync();
        Task<IEnumerable<T>> GetAllAsync(
            Expression<Func<T, bool>> filter = null,
            Expression<Func<T, object>> orderBy = null,
            bool isAscending = false,
            string includeProperties = "",
            int? pageIndex = null,
            int? pageSize = null,
            CancellationToken cancellationToken = default);

        Task<IEnumerable<T>> GetAllCombinedFilterAsync(
            List<Func<IQueryable<T>, IQueryable<T>>> filters = null,
            Expression<Func<T, object>> orderBy = null,
            bool isAscending = false,
            string includeProperties = "",
            int? pageIndex = 1,
            int? pageSize = 10,
            CancellationToken cancellationToken = default);
        
            Task AddAsync(T entity, CancellationToken cancellationToken = default);
        Task AddRangeAsync(IEnumerable<T> entities, CancellationToken cancellationToken = default);
        IQueryable<T> GetQueryable();

        void Attach(T entity);
        void Detach(T entity);
    }
}
