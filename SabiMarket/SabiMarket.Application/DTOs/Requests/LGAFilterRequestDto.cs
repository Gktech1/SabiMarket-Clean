namespace SabiMarket.Application.DTOs.Requests
{
    public class LGAFilterRequestDto
    {
        public string? Name { get; set; }
        public string State { get; set; }
        public string? LGA { get; set; }
        //public bool? HasActiveMarkets { get; set; }
      /*  public string SortBy { get; set; }
        public string SortOrder { get; set; } = "asc";*/
    }
}
