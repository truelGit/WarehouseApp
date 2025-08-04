namespace WarehouseApp.Server.Models;

public class ShipmentDocument
{
    public int Id { get; set; }

    public string Number { get; set; } = null!;

    public DateTime Date { get; set; }

    public int ClientId { get; set; }
    public Client Client { get; set; } = null!;

    public bool IsSigned { get; set; }

    public ICollection<ShipmentItem> Items { get; set; } = new List<ShipmentItem>();
}