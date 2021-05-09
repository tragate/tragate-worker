using System.Collections.Generic;
using Nest;
using Tragate.Console.Dto.Base;

namespace Tragate.Console.Dto
{
    [ElasticsearchType(Name = "company")]
    public class CompanyDto : Root
    {
        public int Id { get; set; }
        public string EstablishmentYear { get; set; }
        public string ResponseRate { get; set; }
        public string ResponseTime { get; set; }
        public int? TransactionAmount { get; set; }
        public int? TransactionCount { get; set; }
        public int? MembershipTypeId { get; set; }
        public int? VerificationTypeId { get; set; }
        public byte StatusId { get; set; }
        public UserDto User { get; set; }

        [Text(Ignore = true)]
        public string FullName { get; set; }

        [Text(Ignore = true)]
        public string ProfileImagePath { get; set; }

        [Text(Ignore = true)]
        public string Location { get; set; }

        public string MembershipType { get; set; }
        public string VerificationType { get; set; }
    }
}