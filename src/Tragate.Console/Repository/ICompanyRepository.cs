using System.Collections.Generic;
using Tragate.Console.Dto;

namespace Tragate.Console.Repository
{
    public interface ICompanyRepository
    {
        List<CompanyDto> GetCompanies(int page, int pageSize);
    }
}