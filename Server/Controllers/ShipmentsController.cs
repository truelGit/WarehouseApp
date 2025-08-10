using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WarehouseApp.Server.Data;
using WarehouseApp.Server.Models;
using WarehouseApp.Shared.Dto;

namespace WarehouseApp.Server.Controllers
{
	[ApiController]
	[Route("api/shipments")]
	public class ShipmentsController : ControllerBase
	{
		private readonly WarehouseDbContext _context;

		public ShipmentsController(WarehouseDbContext context)
		{
			_context = context;
		}

		[HttpGet("GetShipments")]
		public async Task<IActionResult> GetShipments()
		{
			var shipments = await _context.ShipmentDocuments
				.Include(s => s.Client)
				.Select(s => new ShipmentDto
				{
					Id = s.Id,
					Number = s.Number,
					Date = s.Date,
					ClientName = s.Client.Name,
					Status = s.Status
				})
				.ToListAsync();

			return Ok(shipments);
		}

		[HttpPost]
		public async Task<IActionResult> CreateShipment([FromBody] ShipmentCreateDto dto)
		{
			if (dto.Items == null || !dto.Items.Any())
				return BadRequest(new { error = "Документ отгрузки не может быть пустым." });

			var clientExists = await _context.Clients.AnyAsync(c => c.Id == dto.ClientId);
			if (!clientExists)
				return BadRequest(new { error = "Клиент не найден." });

			var shipment = new ShipmentDocument
			{
				Number = dto.Number,
				ClientId = dto.ClientId,
				Date = dto.Date,
				Status = ShipmentStatus.Draft,
				Items = dto.Items.Select(i => new ShipmentItem
				{
					ResourceId = i.ResourceId,
					UnitId = i.UnitId,
					Quantity = i.Quantity
				}).ToList()
			};

			_context.ShipmentDocuments.Add(shipment);
			await _context.SaveChangesAsync();

			return CreatedAtAction(nameof(GetShipmentById), new { id = shipment.Id }, new { shipment.Id });
		}

		[HttpGet("{id}")]
		public async Task<IActionResult> GetShipmentById(int id)
		{
			var shipment = await _context.ShipmentDocuments
				.Include(s => s.Client)
				.Include(s => s.Items)
					.ThenInclude(i => i.Resource)
				.Include(s => s.Items)
					.ThenInclude(i => i.Unit)
				.FirstOrDefaultAsync(s => s.Id == id);

			if (shipment == null)
				return NotFound();

			return Ok(new
			{
				shipment.Id,
				shipment.Number,
				shipment.Date,
				ClientName = shipment.Client.Name,
				shipment.Status,
				Items = shipment.Items.Select(i => new
				{
					i.ResourceId,
					ResourceName = i.Resource.Name,
					i.UnitId,
					UnitName = i.Unit.Name,
					i.Quantity
				})
			});
		}
	}
}
