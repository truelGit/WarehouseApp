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
		public async Task<IActionResult> GetBalance([FromQuery] int? resourceId, [FromQuery] int? unitId)
		{
			var query = _context.Balances
				.Include(b => b.Resource)
				.Include(b => b.Unit)
				.AsQueryable();

			if (resourceId.HasValue)
				query = query.Where(b => b.ResourceId == resourceId.Value);

			if (unitId.HasValue)
				query = query.Where(b => b.UnitId == unitId.Value);

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
