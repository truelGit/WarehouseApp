using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WarehouseApp.Shared.Dto
{
	public class ShipmentDto
	{
		public int Id { get; set; }
		public string Number { get; set; } = "";
		public DateTime Date { get; set; }
		public string ClientName { get; set; } = "";
		public ShipmentStatus Status { get; set; }
	}
}

public enum ShipmentStatus
{
	Draft = 0,   // Черновик — можно редактировать, баланс не меняется
	Signed = 1,  // Подписан — баланс уменьшен
	Revoked = 2  // Отозван — баланс восстановлен
}



// DTO для передачи нового статуса
public class ShipmentStatusUpdateDto
{
	public ShipmentStatus NewStatus { get; set; }
}



public class ErrorDto
{
	public string? Error { get; set; }
}