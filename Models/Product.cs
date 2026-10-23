using System.ComponentModel.DataAnnotations;

namespace AdvancedEFCoreDemo.Models;

public class Product
{
    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public decimal Price { get; set; }

    public int CategoryId { get; set; }

    // Navigation property: each Product belongs to one Category
    public Category Category { get; set; } = null!;

    // Soft delete flag (server-controlled)
    public bool IsDeleted { get; set; }

    // Concurrency token. SQL Server changes it automatically on every UPDATE.
    [Timestamp]
    public byte[] RowVersion { get; set; } = Array.Empty<byte>();
}
