using Fun_Funding.Application.IRepository;
using Fun_Funding.Application.ViewModel;
using Fun_Funding.Infrastructure.Persistence.Database;
using MongoDB.Driver;
using System.Linq.Expressions;

namespace Fun_Funding.Infrastructure.Persistence.Repository
{
    public class MongoBaseRepository<T> : IMongoBaseRepository<T> where T : class
    {
        private readonly IMongoCollection<T> _collection;

        public MongoBaseRepository(MongoDBContext mongoDB, string collectionName)
        {
            // Get the collection for type T from the database
            _collection = mongoDB.GetCollection<T>(collectionName);
        }

        // Create a new document
        public void Create(T entity)
        {
            _collection.InsertOne(entity);
        }

        // Get a single document based on a filter
        public T Get(Expression<Func<T, bool>> filter)
        {
            return _collection.Find(filter).FirstOrDefault();
        }

        public List<T> GetList(Expression<Func<T, bool>> filter)
        {
            var combinedFilter = Builders<T>.Filter.And(filter, Builders<T>.Filter.Eq("IsDelete", false));
            return _collection.Find(combinedFilter).ToList();
        }

        public List<T> GetAll()
        {
            var filter = Builders<T>.Filter.Eq("IsDelete", false);
            return _collection.Find(filter).ToList();
        }


        // Remove a document based on a filter
        public void Remove(Expression<Func<T, bool>> filter)
        {
            _collection.DeleteOne(filter);

        }

        // Update a document based on a filter
        public void Update(Expression<Func<T, bool>> filter, UpdateDefinition<T> updateDefinition)
        {
            _collection.UpdateOne(filter, updateDefinition);
        }


        public void SoftRemove(Expression<Func<T, bool>> filter, UpdateDefinition<T> updateDefinition)
        {
            _collection.UpdateOne(filter, updateDefinition);
        }

        public PaginatedResponse<T> GetAllPaged(ListRequest request, Expression<Func<T, bool>> filter = null)
        {
            // Apply filter (search criteria)
            var query = _collection.Find(filter ?? (x => true)); // Default to no filter if none is provided

            // Sorting logic
            if (!string.IsNullOrEmpty(request.OrderBy))
            {
                var sortDefinition = request.IsAscending.GetValueOrDefault() ?
                    Builders<T>.Sort.Ascending(request.OrderBy) :
                    Builders<T>.Sort.Descending(request.OrderBy);
                query = query.Sort(sortDefinition);
            }

            // Paging logic
            var totalItems = query.CountDocuments();
            var totalPages = (int)Math.Ceiling((double)totalItems / request.PageSize.GetValueOrDefault(10));

            var items = query.Skip((request.PageIndex.GetValueOrDefault(1) - 1) * request.PageSize.GetValueOrDefault(10))
                             .Limit(request.PageSize)
                             .ToList();

            // Return paginated response
            return new PaginatedResponse<T>(
                pageSize: request.PageSize.GetValueOrDefault(10),
                pageIndex: request.PageIndex.GetValueOrDefault(1),
                totalItems: (int)totalItems,
                totalPages: totalPages,
                items: items
            );
        }

        public IQueryable<T> GetQueryable()
        {
            return _collection.AsQueryable();
        }
        public async Task CreateAsync(T entity)
        {
            await _collection.InsertOneAsync(entity);
        }

        public async Task<List<T>> GetAllAsync(FilterDefinition<T>? filter = null, SortDefinition<T>? sort = null)
        {
            if (filter == null)
            {
                filter = Builders<T>.Filter.Empty;
            }
            if (sort == null) return await _collection.Find(filter).ToListAsync();

            return await _collection.Find(filter).Sort(sort).ToListAsync();
        }
    }

}
