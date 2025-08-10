using Microsoft.EntityFrameworkCore;
using WarehouseApp.Server.Models;
using WarehouseApp.Shared.Models;

namespace WarehouseApp.Server.Data
{
	public class WarehouseDbContext : DbContext
	{
		public WarehouseDbContext(DbContextOptions<WarehouseDbContext> options)
		: base(options)
		{
		}

		public DbSet<Resource> Resources => Set<Resource>();
		public DbSet<Unit> Units => Set<Unit>();
		public DbSet<Shared.Models.Client> Clients => Set<Shared.Models.Client>();
		public DbSet<Balance> Balances => Set<Balance>();
		public DbSet<ReceiptDocument> ReceiptDocuments => Set<ReceiptDocument>();
		public DbSet<ReceiptItem> ReceiptItems => Set<ReceiptItem>();
		public DbSet<ShipmentDocument> ShipmentDocuments => Set<ShipmentDocument>();
		public DbSet<ShipmentItem> ShipmentItems => Set<ShipmentItem>();
	}
}
