public class Product
{
    public int Id { get; set; }
    public string Name { get; set; }
    public int UnitPrice { get; set; }
    public int CategoryId { get; set; }
    public Category Category { get; set; } = null!;
    public ICollection<OrderItem> ProductOrders { get; } 
                                        = new List<OrderItem>();
    public Product(string name)
    {
        Name = name;
    }     
}