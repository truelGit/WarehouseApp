using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WarehouseApp.Server.Data;
using WarehouseApp.Server.Models;
using WarehouseApp.Shared.Models;
using NewReceiptModel = WarehouseApp.Shared.Models.NewReceiptModel;

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

		[HttpPut("{id}")]
		public async Task<IActionResult> UpdateReceipt(int id, [FromBody] NewReceiptModel model)
		{
			if (id != model.Id)
				return BadRequest("ID в URL и в теле не совпадают.");

			var existingReceipt = await _context.ReceiptDocuments
				.Include(r => r.Items)
				.FirstOrDefaultAsync(r => r.Id == id);

			if (existingReceipt == null)
				return NotFound();

			// Вычисляем разницу между старым и новым списком Items для обновления баланса
			var oldItems = existingReceipt.Items.ToList();
			var newItems = model.Items;

			// Сначала обрабатываем удалённые или изменённые позиции
			foreach (var oldItem in oldItems)
			{
				var newItem = newItems.FirstOrDefault(i => i.ResourceId == oldItem.ResourceId && i.UnitId == oldItem.UnitId);

				if (newItem == null)
				{
					// Позиция была удалена, уменьшаем баланс на старое количество
					await UpdateBalanceAsync(oldItem.ResourceId, oldItem.UnitId, -oldItem.Quantity);
				}
				else
				{
					// Позиция осталась, вычисляем разницу и обновляем баланс
					var diff = newItem.Quantity - oldItem.Quantity;
					if (diff != 0)
						await UpdateBalanceAsync(oldItem.ResourceId, oldItem.UnitId, diff);
				}
			}

			// Обрабатываем новые добавленные позиции, которых не было в старом списке
			var addedItems = newItems.Where(ni => !oldItems.Any(oi => oi.ResourceId == ni.ResourceId && oi.UnitId == ni.UnitId)).ToList();
			foreach (var added in addedItems)
			{
				await UpdateBalanceAsync(added.ResourceId, added.UnitId, added.Quantity);
			}

			// Обновляем поля документа
			existingReceipt.Number = model.Number;
			existingReceipt.Date = model.Date;

			// Удаляем старые позиции и заменяем новыми
			_context.ReceiptItems.RemoveRange(existingReceipt.Items);
			existingReceipt.Items = model.Items.Select(i => new ReceiptItem
			{
				ResourceId = i.ResourceId,
				UnitId = i.UnitId,
				Quantity = i.Quantity
			}).ToList();

			await _context.SaveChangesAsync();

			return NoContent();
		}


		[HttpGet("GetReceiptById/{id}")]
		public async Task<IActionResult> GetReceiptById(int id)
		{
			var receipt = await _context.ReceiptDocuments
				.Include(r => r.Items)
					.ThenInclude(i => i.Resource)
				.Include(r => r.Items)
					.ThenInclude(i => i.Unit)
				.FirstOrDefaultAsync(r => r.Id == id);

			if (receipt == null)
				return NotFound();

			var result = new ReceiptDto
			{
				Id = receipt.Id,
				Number = receipt.Number,
				Date = receipt.Date,
				Items = receipt.Items.Select(i => new ReceiptItemDto
				{
					ResourceName = i.Resource?.Name ?? "",
					UnitName = i.Unit?.Name ?? "",
					Quantity = i.Quantity
				}).ToList()
			};

			return Ok(result);
		}



		[HttpPost]
		[HttpPost]
		public async Task<IActionResult> CreateReceipt([FromBody] NewReceiptModel newReceipt)
		{
			if (!ModelState.IsValid)
				return BadRequest(ModelState);

			if (await _context.ReceiptDocuments.AnyAsync(r => r.Number == newReceipt.Number))
				return Conflict(new { error = "Документ поступления с таким номером уже существует." });

			using var transaction = await _context.Database.BeginTransactionAsync();
			try
			{
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

				foreach (var item in receiptEntity.Items)
				{
					await UpdateBalanceAsync(item.ResourceId, item.UnitId, item.Quantity);
				}

				await _context.SaveChangesAsync();
				await transaction.CommitAsync();

				var result = new ReceiptDto
				{
					Id = receiptEntity.Id,
					Number = receiptEntity.Number,
					Date = receiptEntity.Date,
					Items = receiptEntity.Items.Select(i => new ReceiptItemDto
					{
						ResourceName = _context.Resources.FirstOrDefault(r => r.Id == i.ResourceId)?.Name ?? "",
						UnitName = _context.Units.FirstOrDefault(u => u.Id == i.UnitId)?.Name ?? "",
						Quantity = i.Quantity
					}).ToList()
				};

				return CreatedAtAction(nameof(GetReceipts), new { id = receiptEntity.Id }, result);
			}
			catch (InvalidOperationException ex)
			{
				await transaction.RollbackAsync();
				return BadRequest(new { error = ex.Message });
			}
			catch (Exception)
			{
				await transaction.RollbackAsync();
				return StatusCode(500, "Внутренняя ошибка сервера");
			}
		}


		private async Task UpdateBalanceAsync(int resourceId, int unitId, decimal quantityChange)
		{
			var balance = await _context.Balances
				.FirstOrDefaultAsync(b => b.ResourceId == resourceId && b.UnitId == unitId);

			if (balance == null)
			{
				if (quantityChange < 0)
					throw new InvalidOperationException("Недостаточно ресурсов на складе.");

				balance = new Balance
				{
					ResourceId = resourceId,
					UnitId = unitId,
					Quantity = 0
				};
				_context.Balances.Add(balance);
			}

			var newQuantity = balance.Quantity + quantityChange;
			if (newQuantity < 0)
				throw new InvalidOperationException("Недостаточно ресурсов на складе.");

			balance.Quantity = newQuantity;
		}


	}
}
