using AdvancedEFCoreDemo.Data;
using AdvancedEFCoreDemo.DTOs;
using AdvancedEFCoreDemo.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AdvancedEFCoreDemo.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProductsController : ControllerBase
{
    private const int MaxPageSize = 100;

    private readonly AppDbContext _context;

    public ProductsController(AppDbContext context)
    {
        _context = context;
    }

    // GET /api/products?pageNumber=1&pageSize=10&search=lap
    // Requirements 1, 2 and 3 (search uses the index on Name)
    [HttpGet]
    public async Task<ActionResult<PagedResponse<ProductResponseDto>>> GetProducts(
        int pageNumber = 1,
        int pageSize = 10,
        string? search = null)
    {
        if (pageNumber < 1)
        {
            return BadRequest(new { message = "pageNumber must be 1 or greater." });
        }

        if (pageSize < 1 || pageSize > MaxPageSize)
        {
            return BadRequest(new { message = $"pageSize must be between 1 and {MaxPageSize}." });
        }

        // AsNoTracking: read-only query, no change tracking overhead.
        // The global query filter automatically adds: WHERE IsDeleted = 0
        IQueryable<Product> query = _context.Products.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim();

            // StartsWith -> LIKE 'term%' which CAN use the index on Name.
            // Contains   -> LIKE '%term%' which usually cannot.
            query = query.Where(p => p.Name.StartsWith(term));
        }

        var totalCount = await query.CountAsync();

        var items = await query
            .OrderBy(p => p.Id)                     // stable order BEFORE paging
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .Select(p => new ProductResponseDto     // only the columns we need
            {
                Id = p.Id,
                Name = p.Name,
                Price = p.Price,
                CategoryId = p.CategoryId,
                CategoryName = p.Category.Name,     // JOIN in the same SQL query (no N+1)
                RowVersion = p.RowVersion
            })
            .ToListAsync();

        var response = new PagedResponse<ProductResponseDto>
        {
            Items = items,
            PageNumber = pageNumber,
            PageSize = pageSize,
            TotalCount = totalCount,
            TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize)
        };

        return Ok(response);
    }

    // GET /api/products/5  (returns the RowVersion so the client can use it in PUT)
    [HttpGet("{id:int}")]
    public async Task<ActionResult<ProductResponseDto>> GetProduct(int id)
    {
        var product = await _context.Products
            .AsNoTracking()
            .Where(p => p.Id == id)
            .Select(p => new ProductResponseDto
            {
                Id = p.Id,
                Name = p.Name,
                Price = p.Price,
                CategoryId = p.CategoryId,
                CategoryName = p.Category.Name,
                RowVersion = p.RowVersion
            })
            .FirstOrDefaultAsync();

        if (product is null)
        {
            return NotFound(new { message = $"Product {id} was not found." });
        }

        return Ok(product);
    }

    // PUT /api/products/5   Requirements 5 and 6
    [HttpPut("{id:int}")]
    public async Task<ActionResult<ProductResponseDto>> UpdateProduct(int id, UpdateProductDto dto)
    {
        if (dto.RowVersion is null || dto.RowVersion.Length == 0)
        {
            return BadRequest(new { message = "RowVersion is required." });
        }

        // 1. Load the TRACKED entity from the database (deleted products are filtered out).
        var product = await _context.Products.FirstOrDefaultAsync(p => p.Id == id);

        if (product is null)
        {
            return NotFound(new { message = $"Product {id} was not found." });
        }

        // 2. Make sure the requested category exists.
        var category = await _context.Categories
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == dto.CategoryId);

        if (category is null)
        {
            return BadRequest(new { message = $"Category {dto.CategoryId} does not exist." });
        }

        // 3. Quick early check: is the client's version already stale?
        if (!product.RowVersion.SequenceEqual(dto.RowVersion))
        {
            return Conflict(ConflictMessage());
        }

        // 4. Tell EF Core which RowVersion the client saw. EF Core will put it in the
        //    WHERE clause of the UPDATE: WHERE Id = @id AND RowVersion = @expected
        //    (this also protects against a change made between step 1 and SaveChanges).
        _context.Entry(product).Property(p => p.RowVersion).OriginalValue = dto.RowVersion;

        // 5. Change ONLY the permitted fields. IsDeleted is never touched.
        product.Name = dto.Name.Trim();
        product.Price = dto.Price;
        product.CategoryId = dto.CategoryId;

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            // 0 rows were updated: someone else changed (or deleted) the row first.
            return Conflict(ConflictMessage());
        }

        // After SaveChanges, product.RowVersion holds the NEW value generated by SQL Server.
        return Ok(new ProductResponseDto
        {
            Id = product.Id,
            Name = product.Name,
            Price = product.Price,
            CategoryId = product.CategoryId,
            CategoryName = category.Name,
            RowVersion = product.RowVersion
        });
    }

    // DELETE /api/products/5   Requirement 4 (soft delete)
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> DeleteProduct(int id)
    {
        var product = await _context.Products.FirstOrDefaultAsync(p => p.Id == id);

        if (product is null)
        {
            return NotFound(new { message = $"Product {id} was not found." });
        }

        // Soft delete: we do NOT remove the row, we only mark it.
        product.IsDeleted = true;

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            return Conflict(ConflictMessage());
        }

        return NoContent();
    }

    private static object ConflictMessage() => new
    {
        message = "The product was modified or deleted by another user. " +
                  "Please GET the product again to receive the latest data and RowVersion, then retry."
    };
}
