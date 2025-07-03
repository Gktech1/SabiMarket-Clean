using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SabiMarket.Application.DTOs.Responses;

namespace SabiMarket.Application.DTOs
{
    public class PaginatorDto<T>
    {
        public T? PageItems { get; set; }
        public int PageSize { get; set; }
        public int CurrentPage { get; set; }
        public int NumberOfPages { get; set; }
        public int TotalItems { get; set; }
    }
}
