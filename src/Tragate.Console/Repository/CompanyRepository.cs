using Dapper;
using System.Data;
using System.Linq;
using Tragate.Console.Dto;
using Tragate.Console.Helper;
using System.Collections.Generic;
using Nest;

namespace Tragate.Console.Repository
{
    public class CompanyRepository : ICompanyRepository
    {
        private readonly IDbConnection _dbConnection;

        public CompanyRepository(IDbConnection dbConnection){
            _dbConnection = dbConnection;
        }

        /// <summary>
        /// Denormaline olan bir tabloda User'a gerek yoktur yalnız dapper mapping'i kullanmak için yaptım.
        /// MultipMapping özelliği tek tabloda yok,Mapping var ama sonra loop'a sokup alanlar üzerinde düzenleme yapmak
        /// gerekiyor.Aykırı bir iş oldugunu kabul ediyorum :)Kendi içinde user için de  full mapping yapan dapper query<T> özelliği olsaydı
        /// çok güzel olurdu diye düşünüyorum.
        /// </summary>
        /// <param name="page"></param>
        /// <param name="pageSize"></param>
        /// <returns></returns>
        public List<CompanyDto> GetCompanies(int page, int pageSize){
            var sql = $@"SELECT
                          c.*,
                          u.*
                        FROM ReIndexedCompany c WITH (NOLOCK)
                            INNER JOIN [User] u WITH (NOLOCK) ON u.Id = c.Id
                        WHERE c.StatusId = 3
                        ORDER BY 1 desc OFFSET {page * pageSize} ROWS FETCH NEXT {pageSize} ROWS ONLY";

            var result = _dbConnection
                .Query<CompanyDto, UserDto, CompanyDto>(
                    sql, (c, u) =>
                    {
                        c.Title = c.FullName;
                        c.User = new UserDto()
                        {
                            FullName = c.FullName,
                            ProfileImagePath = c.ProfileImagePath.CheckCompanyProfileImage(),
                            Location = new LocationDto()
                            {
                                Name = c.Location
                            }
                        };
                        c.CategoryTagString = c.CategoryTags?.Split(',').ToArray();
                        c.JoinField = JoinField.Root<CompanyDto>();
                        c.CategoryText = c.CategoryTags;
                        return c;
                    }).ToList();

            return result;
        }
    }
}