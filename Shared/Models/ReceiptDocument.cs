namespace WarehouseApp.Shared.Models;

public class ReceiptDocument
{
    public int Id { get; set; }

    public string Number { get; set; } = null!;

    public DateTime Date { get; set; }

    public ICollection<ReceiptItem> Items { get; set; } = new List<ReceiptItem>();
}