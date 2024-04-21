namespace Search.Api;

public class Product
{
    public int Id { get; set; }
    public string Name { get; set; }
    public decimal Price { get; set; }
    public string Country { get; set; }
    public bool? IsFreeForm { get; set; }
}
