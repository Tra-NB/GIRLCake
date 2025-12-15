using Asm.Server.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Asm.Server.Data
{
	public class AppDbContext : IdentityDbContext<AppUser>
	{
		public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
		{

		}

		public DbSet<Product> Products { get; set; }
		public DbSet<Category> Categories { get; set; }
		public DbSet<Food> Foods { get; set; }
		public DbSet<Combo> Combos { get; set; }
		public DbSet<ComboFood> ComboFoods { get; set; }
		public DbSet<Order> Orders { get; set; }
		public DbSet<OrderDetail> OrderDetails { get; set; }
		public DbSet<Cart> Carts { get; set; }
		public DbSet<CartDetail> CartDetails { get; set; }
		public DbSet<Voucher> Vouchers { get; set; }
		public DbSet<Review> Reviews { get; set; }

		protected override void OnModelCreating(ModelBuilder modelBuilder)
		{
			base.OnModelCreating(modelBuilder);

			modelBuilder.Entity<Product>()
				.HasDiscriminator<string>("ProductType")
				.HasValue<Food>("Food")
				.HasValue<Combo>("Combo");

			modelBuilder.Entity<OrderDetail>()
				.HasOne(d => d.Order)
				.WithMany(o => o.OrderDetails)
				.HasForeignKey(d => d.OrderId)
				.OnDelete(DeleteBehavior.Cascade);

			modelBuilder.Entity<CartDetail>()
				.HasOne(d => d.Cart)
				.WithMany(o => o.CartDetails)
				.HasForeignKey(d => d.CartId)
				.OnDelete(DeleteBehavior.Cascade);

			modelBuilder.Entity<ComboFood>()
				.HasOne(cf => cf.Combo)
				.WithMany(c => c.ComboFoods)
				.HasForeignKey(cf => cf.ComboId)
				.OnDelete(DeleteBehavior.Restrict); 

			modelBuilder.Entity<ComboFood>()
				.HasOne(cf => cf.Food)
				.WithMany(f => f.ComboFoods)
				.HasForeignKey(cf => cf.FoodId)
				.OnDelete(DeleteBehavior.Restrict); 


			modelBuilder.Entity<Category>()
				.HasMany(c => c.Foods)
				.WithOne(f => f.Category)
				.HasForeignKey(f => f.CategoryId)
				.OnDelete(DeleteBehavior.Restrict);

			modelBuilder.Entity<Order>()
				.HasOne(o => o.Voucher)
				.WithMany()             
				.HasForeignKey(o => o.VoucherId)
				.OnDelete(DeleteBehavior.Restrict);

			modelBuilder.Entity<Review>()
				.HasOne(r => r.Product)
				.WithMany()
				.HasForeignKey(r => r.ProductId)
				.OnDelete(DeleteBehavior.Restrict);
		}
	}
}
