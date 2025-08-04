namespace WarehouseApp.Server.Models;

public class ShipmentItem
{
    public int Id { get; set; }

    public int ResourceId { get; set; }
    public Resource Resource { get; set; } = null!;

    public int UnitId { get; set; }
    public Unit Unit { get; set; } = null!;

    public decimal Quantity { get; set; }

    public int ShipmentDocumentId { get; set; }
    public ShipmentDocument ShipmentDocument { get; set; } = null!;
}