using System.ComponentModel.DataAnnotations;

namespace WarehouseApp.Server.Models;

public class ReceiptItem
{
    public int Id { get; set; }

    public int ResourceId { get; set; }
    public Resource Resource { get; set; } = null!;

    public int UnitId { get; set; }
    public Unit Unit { get; set; } = null!;

    public decimal Quantity { get; set; }

    public int ReceiptDocumentId { get; set; }
    public ReceiptDocument ReceiptDocument { get; set; } = null!;
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

public class NewReceiptModel
{
	[Required]
	public string Number { get; set; } = "";

	[Required]
	public DateTime Date { get; set; }

	public List<ReceiptItemModel> Items { get; set; } = new();
}

public class ReceiptItemModel
{
	[Required]
	public int ResourceId { get; set; }

	[Required]
	public int UnitId { get; set; }

	[Range(0.01, double.MaxValue, ErrorMessage = "Quantity must be greater than 0")]
	public decimal Quantity { get; set; }
}