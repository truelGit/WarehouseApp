namespace WarehouseApp.Server.Models;

public class Unit
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public bool IsArchived { get; set; }

    // Навигационные свойства (если понадобится позже)
    // public ICollection<ReceiptItem> ReceiptItems { get; set; } = new List<ReceiptItem>();
   // public ICollection<ShipmentItem> ShipmentItems { get; set; } = new List<ShipmentItem>();
}