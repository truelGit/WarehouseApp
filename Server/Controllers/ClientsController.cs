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
			IQueryable<WarehouseApp.Server.Models.Client> query = _context.Clients;
			if (isArchived.HasValue)
				query = query.Where(c => c.IsArchived == isArchived.Value);

			return Ok(await query.ToListAsync());
		}

		[HttpPost]
		public async Task<IActionResult> CreateClient(WarehouseApp.Server.Models.Client client)
		{
			if (await _context.Clients.AnyAsync(c => c.Name == client.Name))
				return Conflict("Client with this name already exists.");

			_context.Clients.Add(client);
			await _context.SaveChangesAsync();

			return CreatedAtAction(nameof(GetClients), new { id = client.Id }, client);
		}

		[HttpPut("{id}")]
		public async Task<IActionResult> UpdateClient(int id, WarehouseApp.Server.Models.Client updatedClient)
		{
			if (id != updatedClient.Id)
				return BadRequest();

			if (await _context.Clients.AnyAsync(c => c.Name == updatedClient.Name && c.Id != id))
				return Conflict("Client with this name already exists.");

			var client = await _context.Clients.FindAsync(id);
			if (client == null)
				return NotFound();

			client.Name = updatedClient.Name;
			client.Address = updatedClient.Address;
			client.IsArchived = updatedClient.IsArchived;

			await _context.SaveChangesAsync();

			return NoContent();
		}

		[HttpPatch("{id}/archive")]
		public async Task<IActionResult> ArchiveClient(int id)
		{
			var client = await _context.Clients.FindAsync(id);
			if (client == null) return NotFound();

			client.IsArchived = true;
			await _context.SaveChangesAsync();

			return NoContent();
		}

		[HttpPatch("{id}/unarchive")]
		public async Task<IActionResult> UnarchiveClient(int id)
		{
			var client = await _context.Clients.FindAsync(id);
			if (client == null) return NotFound();

			client.IsArchived = false;
			await _context.SaveChangesAsync();

			return NoContent();
		}

		[HttpDelete("{id}")]
		public async Task<IActionResult> DeleteClient(int id)
		{
			var client = await _context.Clients.FindAsync(id);
			if (client == null)
				return NotFound();

			// Можно добавить проверку использования клиента перед удалением

			_context.Clients.Remove(client);
			await _context.SaveChangesAsync();

			return NoContent();
		}
	}
}
