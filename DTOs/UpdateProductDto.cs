using System.ComponentModel.DataAnnotations;

namespace AdvancedEFCoreDemo.DTOs;

// Only the fields the client is allowed to change (plus RowVersion for concurrency).
// IsDeleted and Id are NOT here on purpose.
public class UpdateProductDto
{
    [Required]
    [StringLength(200, MinimumLength = 1)]
    public string Name { get; set; } = string.Empty;

    [Range(0.01, 1000000000)]
    public decimal Price { get; set; }

    [Range(1, int.MaxValue)]
    public int CategoryId { get; set; }

    // The RowVersion the client received when it read the product.
    [Required]
    public byte[] RowVersion { get; set; } = Array.Empty<byte>();
}
