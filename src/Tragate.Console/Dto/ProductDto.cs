using System;
using Nest;
using Tragate.Console.Dto.Base;

namespace Tragate.Console.Dto
{
    [ElasticsearchType(Name = "product")]
    public class ProductDto : Root
    {
        public Guid UuId { get; set; }
        public int ProductId { get; set; }
        public string ListImagePath { get; set; }
        public string Brand { get; set; }
        public string ModelNumber { get; set; }
        public decimal PriceLow { get; set; }
        public decimal PriceHigh { get; set; }
        public int CurrencyId { get; set; }
        public string Currency { get; set; }
        public int UnitTypeId { get; set; }
        public string UnitType { get; set; }
        public int? MinimumOrder { get; set; }
        public int CategoryId { get; set; }
        public int StatusId { get; set; }
        public DateTime CreatedDate { get; set; }
        public int CompanyId { get; set; }

        [Text(Ignore = true)]
        public string Tags { get; set; }

        [Text(Name = "tags")]
        public string[] TagString { get; set; }

        /// <summary>
        /// This field using for path hierarchical aggregate category
        /// </summary>
        public CategoryNodeDto CategoryTree { get; set; }
    }
}