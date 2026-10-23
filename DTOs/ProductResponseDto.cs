namespace AdvancedEFCoreDemo.DTOs;

public class ProductResponseDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public int CategoryId { get; set; }
    public string CategoryName { get; set; } = string.Empty;

    // JSON shows this as a Base64 string. The client sends it back unchanged when updating.
    public byte[] RowVersion { get; set; } = Array.Empty<byte>();
}
