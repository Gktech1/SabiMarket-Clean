namespace SabiMarket.Application.DTOs
{
    public class PaginationFilter
    {
        public PaginationFilter(int pageNumber, int pageSize)
        {
            PageNumber = pageNumber < 1 ? 1 : pageNumber;
            PageSize = pageSize is > 100 or < 1 ? 100 : pageSize;
        }

        public PaginationFilter()
        {
            PageNumber = 1;
            PageSize = 100;
        }

        public int PageNumber { get; set; }
        public int PageSize { get; set; }
    }
}
