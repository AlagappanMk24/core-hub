namespace Core_API.Application.Common.Results
{
    //public class PaginatedResult<T>
    //{
    //    public List<T> Items { get; set; } = [];
    //    public int TotalCount { get; set; }
    //    public int PageNumber { get; set; }
    //    public int PageSize { get; set; }
    //    public int TotalPages => (int)Math.Ceiling(TotalCount / (double)PageSize);
    //}

    /// <summary>
    /// Represents a paginated result set
    /// </summary>
    /// <typeparam name="T">The type of items in the result</typeparam>
    public class PaginatedResult<T>
    {
        public List<T> Items { get; set; }
        public int TotalCount { get; set; }
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public int TotalPages { get; set; }
        public bool HasPreviousPage { get; set; }
        public bool HasNextPage { get; set; }

        public PaginatedResult()
        {
            Items = new List<T>();
        }

        public PaginatedResult(List<T> items, int totalCount, int pageNumber, int pageSize)
        {
            Items = items;
            TotalCount = totalCount;
            PageNumber = pageNumber;
            PageSize = pageSize;
            TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize);
            HasPreviousPage = pageNumber > 1;
            HasNextPage = pageNumber < TotalPages;
        }
    }
}