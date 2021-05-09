using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Dapper;
using Tragate.Console.Dto;

namespace Tragate.Console.Repository
{
    public class CompanyDataRepository : ICompanyDataRepository
    {
        private readonly IDbConnection _dbConnection;

        public CompanyDataRepository(IDbConnection dbConnection){
            _dbConnection = dbConnection;
        }

        /// <summary>
        /// Get Company Data From DB for Elasticsearch
        /// </summary>
        /// <param name="page"></param>
        /// <param name="pageSize"></param>
        /// <returns></returns>
        public List<CompanyDataDto> GetCompanyData(int page, int pageSize){
            var sql = $@"
                        SELECT 
                          Id, Title, Country, Membership, StatusId, CompanyId,CompanyProfileLink,Tags
                        FROM CompanyData
                        ORDER BY 1 desc OFFSET {page * pageSize} ROWS FETCH NEXT {pageSize} ROWS ONLY";

            return _dbConnection.Query<CompanyDataDto>(sql).ToList();
        }
    }
}