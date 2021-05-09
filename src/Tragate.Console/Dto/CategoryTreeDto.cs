namespace Tragate.Console.Dto
{
    public class CategoryTreeDto
    {
        public int Id { get; set; }
        public int? ParentId { get; set; }
        public string Title { get; set; }
        public string Slug { get; set; }
    }
}