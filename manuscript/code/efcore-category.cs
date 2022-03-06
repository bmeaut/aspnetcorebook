public class Category
{
    public int Id { get; set; }
    public string Name { get; set; }
    public ICollection<Product> Products { get; } 
                                = new List<Product>();
    public Category(string name)
    {
        Name = name;
    }
}