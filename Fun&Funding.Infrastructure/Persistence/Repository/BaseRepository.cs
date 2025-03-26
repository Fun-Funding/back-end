using Fun_Funding.Application.IRepository;
using Fun_Funding.Domain.Entity;
using Fun_Funding.Infrastructure.Persistence.Database;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace Fun_Funding.Infrastructure.Persistence.Repository
{
    public class BaseRepository<T> : IBaseRepository<T> where T : class
    {
        protected readonly MyDbContext _context;
        private readonly DbSet<T> _entitySet;

        protected BaseRepository(MyDbContext context)
        {
            _context = context;
            _entitySet = _context.Set<T>();
        }

        public virtual void Add(T entity)
        {
            _context.Add(entity);
        }

        public virtual async Task AddAsync(T entity, CancellationToken cancellationToken = default)
        {
            await _context.AddAsync(entity, cancellationToken);
        }

        public virtual void AddRange(IEnumerable<T> entities)
        {
            _context.AddRange(entities);
        }

        public virtual async Task AddRangeAsync(IEnumerable<T> entities, CancellationToken cancellationToken = default)
        {
            await _context.AddRangeAsync(entities, cancellationToken);
        }

        public virtual IEnumerable<T> GetAll(Expression<Func<T, bool>> predicate = null)
        {
            if (predicate == null) return _entitySet.AsNoTracking().ToList();
            return _entitySet
                .Where(predicate)
                .Where(e => EF.Property<bool>(e, "IsDeleted") == false)
                .ToList();
        }

        public virtual T Get(Expression<Func<T, bool>> predicate)
        {
            return _entitySet.FirstOrDefault(predicate);
        }

        public IEnumerable<T> GetAll()
        {
            return _entitySet.Where(e => EF.Property<bool>(e, "IsDeleted") == false).ToList();
        }

        public virtual async Task<IEnumerable<T>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            return await _entitySet.Where(e => EF.Property<bool>(e, "IsDeleted") == false).ToListAsync(cancellationToken);
        }

        public virtual async Task<IEnumerable<T>> GetAllAsync()
        {
            return await _entitySet.Where(e => EF.Property<bool>(e, "IsDeleted") == false).ToListAsync();
        }

        public virtual async Task<IEnumerable<T>> GetAllAsync(
            Expression<Func<T, bool>> filter = null,
            Expression<Func<T, object>> orderBy = null,
            bool isAscending = false,
            string includeProperties = "",
            int? pageIndex = 1,
            int? pageSize = 10,
            CancellationToken cancellationToken = default)
        {
            IQueryable<T> query = _entitySet.Where(e => EF.Property<bool>(e, "IsDeleted") == false);

            if (filter != null) { query = query.Where(filter); }

            if (orderBy != null)
            {
                query = isAscending ? query.OrderBy(orderBy) : query.OrderByDescending(orderBy);
            }

            if (includeProperties != "")
            {
                foreach (var includeProperty in includeProperties.Split
                    (new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
                {
                    query = query.Include(includeProperty).AsNoTracking();
                }
            }

            if (pageIndex.HasValue && pageSize.HasValue)
            {
                query = query.Skip((pageIndex.Value - 1) * pageSize.Value).Take(pageSize.Value);
            }

            return await query.ToListAsync();
        }

        public virtual async Task<IEnumerable<T>> GetAllCombinedFilterAsync(
            List<Func<IQueryable<T>, IQueryable<T>>> filters = null,
            Expression<Func<T, object>> orderBy = null,
            bool isAscending = false,
            string includeProperties = "",
            int? pageIndex = 1,
            int? pageSize = 10,
            CancellationToken cancellationToken = default)
        {
            IQueryable<T> query = _entitySet.Where(e => EF.Property<bool>(e, "IsDeleted") == false);

            // Apply filters dynamically
            if (filters != null && filters.Any())
            {
                foreach (var filter in filters)
                {
                    query = filter(query);
                }
            }

            // Apply sorting
            if (orderBy != null)
            {
                query = isAscending ? query.OrderBy(orderBy) : query.OrderByDescending(orderBy);
            }

            // Include related properties
            if (!string.IsNullOrWhiteSpace(includeProperties))
            {
                foreach (var includeProperty in includeProperties.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
                {
                    query = query.Include(includeProperty).AsNoTracking();
                }
            }

            // Apply pagination
            if (pageIndex.HasValue && pageSize.HasValue)
            {
                query = query.Skip((pageIndex.Value - 1) * pageSize.Value).Take(pageSize.Value);
            }

            return await query.ToListAsync(cancellationToken);
        }


        public virtual async Task<T> GetAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default)
        {
            return await _entitySet.FirstOrDefaultAsync(predicate, cancellationToken);
        }

        public virtual T GetById(object id)
        {
            return _entitySet.Find(id);
        }

        public virtual async Task<T> GetByIdAsync(object id, CancellationToken cancellationToken = default)
        {
            return await _entitySet.FindAsync(new object[] { id }, cancellationToken);
        }

        public virtual void Remove(T entity)
        {
            _context.Remove(entity);
        }

        // Retrieve deleted entities no pagination
        public async Task<IEnumerable<T>> GetAllDeletedNoPaginationAsync(Expression<Func<T, bool>>? predicate = null)
        {
            if (predicate == null) return await _entitySet.AsNoTracking().ToListAsync();
            return await _entitySet
                .Where(predicate)
                .AsNoTracking()
                .ToListAsync();
        }

        public IEnumerable<T> GetAllDeletedNoPagination(Expression<Func<T, bool>>? predicate = null)
        {
            if (predicate == null) return _entitySet.AsNoTracking().ToList();
            return _entitySet
                .Where(predicate)
                .AsNoTracking()
                .ToList();
        }

        // Asynchronously retrieve deleted entities
        public async Task<IEnumerable<T>> GetAllDeletedAsync(
            Expression<Func<T, bool>> filter = null,
            Expression<Func<T, object>> orderBy = null,
            bool isAscending = false,
            string includeProperties = "",
            int? pageIndex = null,
            int? pageSize = null,
            CancellationToken cancellationToken = default)
        {
            IQueryable<T> query = _entitySet.AsNoTracking();

            if (filter != null) { query = query.Where(filter); }

            if (orderBy != null)
            {
                query = isAscending ? query.OrderBy(orderBy) : query.OrderByDescending(orderBy);
            }

            if (includeProperties != "")
            {
                foreach (var includeProperty in includeProperties.Split
                    (new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
                {
                    query = query.Include(includeProperty);
                }
            }

            if (pageIndex.HasValue && pageSize.HasValue)
            {
                query = query.Skip((pageIndex.Value - 1) * pageSize.Value).Take(pageSize.Value);
            }

            return await query.ToListAsync();
        }

        // Get a single deleted entity by a condition
        public T GetDeleted(Expression<Func<T, bool>> predicate)
        {
            return _entitySet.AsNoTracking().FirstOrDefault(predicate);
        }

        // Asynchronously get a single deleted entity by a condition
        public async Task<T> GetDeletedAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default)
        {
            return await _entitySet.AsNoTracking().FirstOrDefaultAsync(predicate, cancellationToken);
        }
        public virtual void RemoveRange(IEnumerable<T> entities)
        {
            _context.RemoveRange(entities);
        }

        public virtual void Update(T entity)
        {
            _entitySet.Update(entity).State = EntityState.Modified;
            //_context.Update(entity);
        }

        public virtual void UpdateRange(IEnumerable<T> entities)
        {
            //_context.UpdateRange(entities);
            _entitySet.UpdateRange(entities);
        }

        public IQueryable<T> GetQueryable()
        {
            return _entitySet.AsQueryable();
        }

        public virtual void Attach(T entity)
        {
            _context.Attach(entity);
        }

        public virtual void Detach(T entity)
        {
            var entry = _context.Entry(entity);
            if (entry != null)
            {
                entry.State = EntityState.Detached;
            }
        }
    }
}
