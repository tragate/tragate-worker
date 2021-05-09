using Nest;
using Newtonsoft.Json;

namespace Tragate.Console.Dto.Base
{
    public class Root
    {
        public JoinField JoinField { get; set; }
        public string Slug { get; set; }
        
        /// <summary>
        /// This field using for hierarchical search
        /// </summary>
        public string CategoryPath { get; set; }

        [Text(Ignore = true)]
        public string CategoryTags { get; set; }

        [Text(Name = "categoryTags")]
        public string[] CategoryTagString { get; set; }

        public string Title { get; set; }
            
        /// <summary>
        /// This field using for full text search
        /// </summary>
        public string CategoryText { get; set; }
    }
}