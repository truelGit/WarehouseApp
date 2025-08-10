using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WarehouseApp.Server.Data;
using WarehouseApp.Server.Models;
using WarehouseApp.Server.Services;
using WarehouseApp.Shared.Dto;

[ApiController]
[Route("api/shipments")]
public class ShipmentsController : ControllerBase
{
	private readonly ShipmentService _shipmentService;
	private readonly WarehouseDbContext _context;

	public ShipmentsController(ShipmentService shipmentService, WarehouseDbContext context)
	{
		_shipmentService = shipmentService;
		_context = context;
	}

	// PUT api/shipments/{id}/status
	[HttpPut("{id}/status")]
	public async Task<IActionResult> ChangeStatus(int id, [FromBody] ShipmentStatusUpdateDto dto)
	{
		try
		{
			bool success = await _shipmentService.ChangeStatusAsync(id, dto.NewStatus);
			if (success)
			{
				return NoContent();
			}
			else
			{
				return BadRequest("Не удалось изменить статус.");
			}
		}
		catch (Exception ex)
		{
			return BadRequest(new { error = ex.Message });
		}
	}

	// GET api/shipments
	[HttpGet]
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
}