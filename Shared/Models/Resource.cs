namespace WarehouseApp.Shared.Models;

public class Resource
{
    public int Id { get; set; }
    public string Name { get; set; } = null!;
    public bool IsArchived { get; set; }
}