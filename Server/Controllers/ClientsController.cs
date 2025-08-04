using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WarehouseApp.Server.Data;

namespace WarehouseApp.Server.Controllers
{
	[ApiController]
	[Route("api/clients")]
	public class ClientsController : ControllerBase
	{
		private readonly WarehouseDbContext _context;

		public ClientsController(WarehouseDbContext context)
		{
			_context = context;
		}

		[HttpGet]
		public async Task<IActionResult> GetClients([FromQuery] bool? isArchived = null)
		{
			var query = _context.Clients.AsQueryable();
			if (isArchived.HasValue)
				query = query.Where(c => c.IsArchived == isArchived.Value);

			return Ok(await query.ToListAsync());
		}

		[HttpPost]
		public async Task<IActionResult> Create(WarehouseApp.Server.Models.Client client)
		{
			if (await _context.Clients.AnyAsync(c => c.Name == client.Name))
				return Conflict("Client with this name already exists.");

			_context.Clients.Add(client);
			await _context.SaveChangesAsync();
			return CreatedAtAction(nameof(GetClients), new { id = client.Id }, client);
		}

		[HttpPatch("{id}/archive")]
		public async Task<IActionResult> Archive(int id)
		{
			var client = await _context.Clients.FindAsync(id);
			if (client == null) return NotFound();

			client.IsArchived = true;
			await _context.SaveChangesAsync();
			return NoContent();
		}
	}
}
