using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
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

public class ShipmentCreateDto
{
	[Required]
	public string Number { get; set; } = null!;

	[Required(ErrorMessage = "Клиент обязателен")]
	public int ClientId { get; set; }

	[Required(ErrorMessage = "Дата обязательна")]
	public DateTime Date { get; set; }

	[MinLength(1, ErrorMessage = "Документ не может быть пустым")]
	public List<ShipmentItemCreateDto> Items { get; set; } = new();
}

public class ShipmentItemCreateDto
{
	public int ResourceId { get; set; }
	public int UnitId { get; set; }
	public decimal Quantity { get; set; }
}

public class ClientDto
{
	public int Id { get; set; }
	public string Name { get; set; } = "";
}

public class ResourceDto
{
	public int Id { get; set; }
	public string Name { get; set; } = "";
}

public class UnitDto
{
	public int Id { get; set; }
	public string Name { get; set; } = "";
}