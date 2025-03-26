namespace Fun_Funding.Application.ViewModel
{
    public class ListRequest
    {
        public string? OrderBy { get; set; }
        public string? SearchValue { get; set; }
        public int? PageIndex { get; set; } = 1;
        public int? PageSize { get; set; } = 10;
        public bool? IsAscending { get; set; } = false;
        public object? From { get; set; }
        public object? To { get; set; }
    }
}
