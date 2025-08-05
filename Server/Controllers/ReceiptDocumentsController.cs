using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WarehouseApp.Server.Data;
using WarehouseApp.Server.Models;
using NewReceiptModel = WarehouseApp.Server.Models.NewReceiptModel;

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

		[HttpGet("filtered")]
		public async Task<IActionResult> GetFilteredReceipts(
	[FromQuery] DateTime? from,
	[FromQuery] DateTime? to,
	[FromQuery] List<string>? numbers,
	[FromQuery] List<int>? resourceIds,
	[FromQuery] List<int>? unitIds)
		{
			var query = _context.ReceiptDocuments
				.Include(r => r.Items)
					.ThenInclude(i => i.Resource)
				.Include(r => r.Items)
					.ThenInclude(i => i.Unit)
				.AsQueryable();

			if (from.HasValue)
				query = query.Where(r => r.Date >= from.Value);
			if (to.HasValue)
				query = query.Where(r => r.Date <= to.Value);
			if (numbers?.Count > 0)
				query = query.Where(r => numbers.Contains(r.Number));
			if (resourceIds?.Count > 0)
				query = query.Where(r => r.Items.Any(i => resourceIds.Contains(i.ResourceId)));
			if (unitIds?.Count > 0)
				query = query.Where(r => r.Items.Any(i => unitIds.Contains(i.UnitId)));

			var receipts = await query.OrderByDescending(r => r.Date).ToListAsync();

			var result = receipts.Select(r => new ReceiptDto
			{
				Id = r.Id,
				Number = r.Number,
				Date = r.Date,
				Items = r.Items.Select(i => new ReceiptItemDto
				{
					ResourceName = i.Resource?.Name ?? "",
					UnitName = i.Unit?.Name ?? "",
					Quantity = i.Quantity
				}).ToList()
			}).ToList();

			return Ok(result);
		}

		[HttpPost]
		public async Task<IActionResult> CreateReceipt([FromBody] NewReceiptModel newReceipt)
		{
			if (!ModelState.IsValid)
			{
				return BadRequest(ModelState);
			}

			var receiptEntity = new ReceiptDocument
			{
				Number = newReceipt.Number,
				Date = newReceipt.Date,
				Items = newReceipt.Items.Select(i => new ReceiptItem
				{
					ResourceId = i.ResourceId,
					UnitId = i.UnitId,
					Quantity = i.Quantity
				}).ToList()
			};

			_context.ReceiptDocuments.Add(receiptEntity);
			await _context.SaveChangesAsync();

			var result = new ReceiptDto
			{
				Id = receiptEntity.Id,
				Number = receiptEntity.Number,
				Date = receiptEntity.Date,
				Items = receiptEntity.Items.Select(i => new ReceiptItemDto
				{
					ResourceName = "",
					UnitName = "",
					Quantity = i.Quantity
				}).ToList()
			};

			return CreatedAtAction(nameof(GetReceipts), new { id = receiptEntity.Id }, result);
		}

	}
}
