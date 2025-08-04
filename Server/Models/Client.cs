namespace WarehouseApp.Server.Models;

public class Client
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public string Address { get; set; } = null!;

    public bool IsArchived { get; set; }

    public ICollection<ShipmentDocument> Shipments { get; set; } = new List<ShipmentDocument>();
}