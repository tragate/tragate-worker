using System.Collections.Generic;
using Tragate.Console.Dto;

namespace Tragate.Console.Repository
{
    public interface IProductRepository
    {
        List<ProductDto> GetProducts(int page, int pageSize);
    }
}