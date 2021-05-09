using System.Collections.Generic;
using Tragate.Console.Dto;

namespace Tragate.Console.Repository
{
    public interface ICompanyDataRepository
    {
        List<CompanyDataDto> GetCompanyData(int page, int pageSize);
    }
}