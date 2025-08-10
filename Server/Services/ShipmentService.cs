using Microsoft.EntityFrameworkCore;
using WarehouseApp.Server.Data;
using WarehouseApp.Server.Models;

namespace WarehouseApp.Server.Services
{
	public class ShipmentService
	{
		private readonly WarehouseDbContext _context;

		public ShipmentService(WarehouseDbContext context)
		{
			_context = context;
		}

		public async Task<bool> ChangeStatusAsync(int shipmentId, ShipmentStatus newStatus)
		{
			var shipment = await _context.ShipmentDocuments
				.Include(s => s.Items)
				.FirstOrDefaultAsync(s => s.Id == shipmentId);

			if (shipment == null)
				throw new Exception("Документ отгрузки не найден");

			if (shipment.Status == newStatus)
				return true;

			if (newStatus == ShipmentStatus.Signed)
			{
				foreach (var item in shipment.Items)
				{
					var balance = await _context.Balances
						.FirstOrDefaultAsync(b => b.ResourceId == item.ResourceId && b.UnitId == item.UnitId);

					if (balance == null || balance.Quantity < item.Quantity)
					{
						throw new Exception($"Недостаточно ресурса '{item.ResourceId}' на складе");
					}
				}

				foreach (var item in shipment.Items)
				{
					var balance = await _context.Balances
						.FirstOrDefaultAsync(b => b.ResourceId == item.ResourceId && b.UnitId == item.UnitId);

					if (balance != null)
					{
						balance.Quantity -= item.Quantity;
					}
				}
			}
			else if (shipment.Status == ShipmentStatus.Signed && newStatus == ShipmentStatus.Revoked)
			{
				foreach (var item in shipment.Items)
				{
					var balance = await _context.Balances
						.FirstOrDefaultAsync(b => b.ResourceId == item.ResourceId && b.UnitId == item.UnitId);

					if (balance != null)
					{
						balance.Quantity += item.Quantity;
					}
					else
					{
						_context.Balances.Add(new Balance
						{
							ResourceId = item.ResourceId,
							UnitId = item.UnitId,
							Quantity = item.Quantity
						});
					}
				}
			}

			shipment.Status = newStatus;
			await _context.SaveChangesAsync();

			return true;
		}
	}
}
