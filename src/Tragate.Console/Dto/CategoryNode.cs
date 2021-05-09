namespace Tragate.Console.Dto
{
    public class CategoryNodeDto
    {
        public string Value { get; }
        public CategoryNodeDto Right;

        public CategoryNodeDto(string value){
            this.Value = value;
        }
    }

    public class CategoryTree
    {
        public CategoryNodeDto Insert(CategoryNodeDto root, string value){
            if (root == null){
                root = new CategoryNodeDto(value);
            }
            else{
                root.Right = Insert(root.Right, value);
            }

            return root;
        }
    }
}