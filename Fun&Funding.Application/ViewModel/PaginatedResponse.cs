namespace Fun_Funding.Application.ViewModel
{
    public class PaginatedResponse<T>
    {
        public int PageSize { get; set; }
        public int PageIndex { get; set; }
        public int TotalItems { get; set; }
        public int TotalPages { get; set; }
        public IEnumerable<T> Items { get; set; }

        public PaginatedResponse() { }
        public PaginatedResponse(int pageSize, int pageIndex, int totalItems, int totalPages, IEnumerable<T> items)
        {
            PageSize = pageSize;
            PageIndex = pageIndex;
            TotalItems = totalItems;
            TotalPages = totalPages;
            Items = items;
        }
    }
}
