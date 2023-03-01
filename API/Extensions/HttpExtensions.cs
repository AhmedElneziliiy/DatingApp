
using System.Text.Json;
using API.Helpers;

namespace API.Extensions
{
    public static class HttpExtensions
    {
        public static void AddPagintaionHeader(this HttpResponse response,int currentPage,
        int itemsPerPage,int totalItems,int totalPages)
        {
            var pagintaionHeader=new PaginationHeader(currentPage,itemsPerPage,totalItems,totalPages);

            var options= new JsonSerializerOptions
            {
                PropertyNamingPolicy=JsonNamingPolicy.CamelCase
            };
            //add pagination to response header(custom header)
            response.Headers.Add("Pagintaion",JsonSerializer.Serialize(pagintaionHeader,options));
            response.Headers.Add("Access-Control-Expose-Headers","Pagination");
        }
    }
}