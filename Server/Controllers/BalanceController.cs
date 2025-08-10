using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WarehouseApp.Server.Data;
using WarehouseApp.Shared.Dto;

namespace WarehouseApp.Server.Controllers
{
	[ApiController]
	[Route("api/balance")]
	public class BalanceController : ControllerBase
	{
		private readonly WarehouseDbContext _context;

		public BalanceController(WarehouseDbContext context)
		{
			_context = context;
		}

		[HttpGet]
		public async Task<IActionResult> GetBalance([FromQuery] List<int> resourceId, [FromQuery] List<int> unitId)
		{
			var query = _context.Balances
				.Include(b => b.Resource)
				.Include(b => b.Unit)
				.AsQueryable();

			if (resourceId != null && resourceId.Any())
				query = query.Where(b => resourceId.Contains(b.ResourceId));

			if (unitId != null && unitId.Any())
				query = query.Where(b => unitId.Contains(b.UnitId));

			var balances = await query
				.Select(b => new BalanceDto
				{
					ResourceId = b.ResourceId,
					ResourceName = b.Resource.Name,
					UnitId = b.UnitId,
					UnitName = b.Unit.Name,
					Quantity = b.Quantity
				})
				.ToListAsync();

			return Ok(balances);
		}
	}
}
