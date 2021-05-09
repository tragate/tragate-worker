using System.Collections.Generic;
using System.Data;
using System.Linq;
using Dapper;
using Nest;
using Tragate.Console.Dto;
using Tragate.Console.Helper;

namespace Tragate.Console.Repository
{
    public class ProductRepository : IProductRepository
    {
        private readonly IDbConnection _dbConnection;

        public ProductRepository(IDbConnection dbConnection){
            _dbConnection = dbConnection;
        }

        /// <summary>
        /// foreach ile birlikte n^2 notasyonunda performanssız bir code gibi gözüküyor ama
        /// anonym olarak çekseydik mapping yapmak zorunda kalacaktık.Dapper inline mapping yaptıgında da
        /// bu sefer company'i product'a joinlemek gerekecekti. Zira time out alacak sorgu.Denormalize şekilde
        /// tek tablo üzerinden çekmek en iyisi şimdilik.O yuzden foreach şimdilik daha makul ilerde bunun sqlde bile
        /// olmaması lazım.O kadar paramız yok idare etmek lazım. 
        /// </summary>
        /// <param name="page"></param>
        /// <param name="pageSize"></param>
        /// <returns></returns>
        public List<ProductDto> GetProducts(int page, int pageSize){
            var sql = $@"SELECT
                          p.*,
                          p.Id as ProductId
                        FROM ReIndexedProduct p  WITH (NOLOCK)
                          WHERE p.StatusId = 3  
                          ORDER BY 1 desc OFFSET {page * pageSize} ROWS FETCH NEXT {pageSize} ROWS ONLY";

            var result = _dbConnection.Query<ProductDto>(sql).ToList();
            foreach (var p in result){
                p.TagString = p.Tags?.Split(',').ToArray();
                p.ListImagePath = p.ListImagePath.CheckProductProfileImage();
                p.JoinField = JoinField.Link<ProductDto>(p.CompanyId);
                p.CategoryText = p.CategoryPath;
                GetNestedProductCategory(p, p.CategoryPath);
            }

            return result;
        }

        private void GetNestedProductCategory(ProductDto product, string categoryPath){
            var orderedCategory = categoryPath.Split('/').Reverse();
            var tree = new CategoryTree();
            var node = orderedCategory.Aggregate<string, CategoryNodeDto>(null,
                (current, item) => tree.Insert(current, item));

            product.CategoryTree = node;
        }
    }
}