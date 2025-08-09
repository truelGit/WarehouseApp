using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WarehouseApp.Server.Data;
using WarehouseApp.Server.Models;

namespace WarehouseApp.Server.Controllers
{
	[ApiController]
	[Route("api/units")]
	public class UnitsController : ControllerBase
	{
		private readonly WarehouseDbContext _context;

		public UnitsController(WarehouseDbContext context)
		{
			_context = context;
		}

		[HttpGet]
		public async Task<IActionResult> GetUnits([FromQuery] bool? isArchived = null)
		{
			IQueryable<Unit> query = _context.Units;
			if (isArchived.HasValue)
				query = query.Where(u => u.IsArchived == isArchived.Value);

			return Ok(await query.ToListAsync());
		}

		[HttpPost]
		public async Task<IActionResult> CreateUnit(Unit unit)
		{
			if (await _context.Units.AnyAsync(u => u.Name == unit.Name))
				return Conflict("Уже существует такая единица измерения.");

			_context.Units.Add(unit);
			await _context.SaveChangesAsync();

			return CreatedAtAction(nameof(GetUnits), new { id = unit.Id }, unit);
		}

		[HttpPatch("{id}/archive")]
		public async Task<IActionResult> ArchiveUnit(int id)
		{
			var unit = await _context.Units.FindAsync(id);
			if (unit == null) return NotFound();

			unit.IsArchived = true;
			await _context.SaveChangesAsync();

			return NoContent();
		}

		[HttpPatch("{id}/unarchive")]
		public async Task<IActionResult> UnarchiveUnit(int id)
		{
			var unit = await _context.Units.FindAsync(id);
			if (unit == null) return NotFound();

			unit.IsArchived = false;
			await _context.SaveChangesAsync();

			return NoContent();
		}

		[HttpDelete("{id}")]
		public async Task<IActionResult> DeleteUnit(int id)
		{
			var unit = await _context.Units.FindAsync(id);
			if (unit == null)
				return NotFound();

			bool isUsed = await _context.ReceiptItems.AnyAsync(i => i.UnitId == id);
			if (isUsed)
			{
				return BadRequest("Невозможно удалить единицу измерения — используется где-то.");
			}

			_context.Units.Remove(unit);
			await _context.SaveChangesAsync();

			return NoContent();
		}

		[HttpPut("{id}")]
		public async Task<IActionResult> UpdateUnit(int id, Unit updatedUnit)
		{
			if (id != updatedUnit.Id)
				return BadRequest("ID в пути и теле запроса не совпадают.");

			// Проверяем, есть ли другая единица с таким же именем
			if (await _context.Units.AnyAsync(u => u.Name == updatedUnit.Name && u.Id != id))
				return Conflict("Уже существует такая единица измерения.");

			var unit = await _context.Units.FindAsync(id);
			if (unit == null)
				return NotFound();

			unit.Name = updatedUnit.Name;
			unit.IsArchived = updatedUnit.IsArchived;

			await _context.SaveChangesAsync();

			return NoContent();
		}
	}
}
