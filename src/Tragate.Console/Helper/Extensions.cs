namespace Tragate.Console.Helper
{
    public static class Extensions
    {
        public static string CheckProductProfileImage(this string value){
            return !string.IsNullOrEmpty(value)
                ? $"https://cdn.tragate.com/{value}"
                : "https://cdn.tragate.com/items/product.jpg";
        }

        public static string CheckCompanyProfileImage(this string value){
            return !string.IsNullOrEmpty(value)
                ? $"https://cdn.tragate.com/{value}"
                : "https://cdn.tragate.com/items/company.jpg";
        }
    }
}