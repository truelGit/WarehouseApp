using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WarehouseApp.Server.Data;
using WarehouseApp.Shared.Models;

namespace WarehouseApp.Server.Controllers;

[ApiController]
[Route("api/resources")]
public class ResourcesController : ControllerBase
{
    private readonly WarehouseDbContext _context;

    public ResourcesController(WarehouseDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<IActionResult> GetResources([FromQuery] bool? isArchived = null)
    {
        IQueryable<Resource> query = _context.Resources;

        if (isArchived.HasValue)
        {
            query = query.Where(r => r.IsArchived == isArchived.Value);
        }

        var list = await query.ToListAsync();
        return Ok(list);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetResource(int id)
    {
        var resource = await _context.Resources.FindAsync(id);
        if (resource == null)
            return NotFound();

        return Ok(resource);
    }

    [HttpPost]
    public async Task<IActionResult> CreateResource(Resource resource)
    {
        if (await _context.Resources.AnyAsync(r => r.Name == resource.Name))
            return Conflict("Resource with this name already exists.");

        _context.Resources.Add(resource);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetResource), new { id = resource.Id }, resource);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateResource(int id, Resource updatedResource)
    {
        if (id != updatedResource.Id)
            return BadRequest();

        if (await _context.Resources.AnyAsync(r => r.Name == updatedResource.Name && r.Id != id))
            return Conflict("Resource with this name already exists.");

        var resource = await _context.Resources.FindAsync(id);
        if (resource == null)
            return NotFound();

        resource.Name = updatedResource.Name;
        resource.IsArchived = updatedResource.IsArchived;

        await _context.SaveChangesAsync();

        return NoContent();
    }

    [HttpPatch("{id}/archive")]
    public async Task<IActionResult> ArchiveResource(int id)
    {
        var resource = await _context.Resources.FindAsync(id);
        if (resource == null)
            return NotFound();

        resource.IsArchived = true;
        await _context.SaveChangesAsync();

        return NoContent();
    }

	[HttpPatch("{id}/unarchive")]
	public async Task<IActionResult> UnarchiveResource(int id)
	{
		var resource = await _context.Resources.FindAsync(id);
		if (resource == null) return NotFound();

		resource.IsArchived = false;
		await _context.SaveChangesAsync();

		return NoContent();
	}

	[HttpDelete("{id}")]
	public async Task<IActionResult> DeleteResource(int id)
	{
		var resource = await _context.Resources.FindAsync(id);
		if (resource == null)
			return NotFound();

		// Проверка: используется ли в поступлениях
		bool isUsed = await _context.ReceiptItems.AnyAsync(i => i.ResourceId == id);
		if (isUsed)
		{
			return BadRequest("Невозможно удалить ресурс — он используется.");
		}

		_context.Resources.Remove(resource);
		await _context.SaveChangesAsync();

		return NoContent();
	}
}