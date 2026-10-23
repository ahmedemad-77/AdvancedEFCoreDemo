namespace AdvancedEFCoreDemo.Models;

public class Category
{
    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;

    // One Category has many Products
    public ICollection<Product> Products { get; set; } = new List<Product>();
}
