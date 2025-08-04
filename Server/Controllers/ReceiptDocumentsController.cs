using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WarehouseApp.Server.Data;
using WarehouseApp.Server.Models;

namespace WarehouseApp.Server.Controllers
{
	[ApiController]
	[Route("api/receipts")]
	public class ReceiptDocumentsController : ControllerBase
	{
		private readonly WarehouseDbContext _context;

		public ReceiptDocumentsController(WarehouseDbContext context)
		{
			_context = context;
		}

		[HttpGet]
		public async Task<IActionResult> GetReceipts()
		{
			var receipts = await _context.ReceiptDocuments
				.Include(r => r.Items)
					.ThenInclude(i => i.Resource)
				.Include(r => r.Items)
					.ThenInclude(i => i.Unit)
				.OrderByDescending(r => r.Date)
				.ToListAsync();

			var result = receipts.Select(r => new ReceiptDto
			{
				Id = r.Id,
				Number = r.Number,
				Date = r.Date,
				Items = r.Items.Select(i => new ReceiptItemDto
				{
					ResourceName = i.Resource.Name,
					UnitName = i.Unit.Name,
					Quantity = i.Quantity
				}).ToList()
			}).ToList();

			return Ok(result);
		}

		// DTO классы можно вынести в отдельный файл
		public class ReceiptDto
		{
			public int Id { get; set; }
			public string Number { get; set; } = "";
			public DateTime Date { get; set; }
			public List<ReceiptItemDto> Items { get; set; } = new();
		}

		public class ReceiptItemDto
		{
			public string ResourceName { get; set; } = "";
			public string UnitName { get; set; } = "";
			public decimal Quantity { get; set; }
		}
	}
}
