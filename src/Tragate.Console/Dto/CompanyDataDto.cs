using Nest;

namespace Tragate.Console.Dto
{
    [ElasticsearchType(Name = "companydata")]
    public class CompanyDataDto
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Country { get; set; }
        public string Membership { get; set; }
        public int StatusId { get; set; }
        public int? CompanyId { get; set; }
        public string CompanyProfileLink { get; set; }
        public string Tags { get; set; }
    }
}