using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.Extensions.Configuration;
using ECommerceUpo.Models;

namespace ECommerceUpo.Data
{
    public  class ECommerceUpoContext : DbContext
    {
        
        public ECommerceUpoContext(DbContextOptions<ECommerceUpoContext> options) : base(options)
        {
        }

        public ECommerceUpoContext() { }

        public virtual DbSet<Order> Order { get; set; }
        public virtual DbSet<OrderProduct> OrderProduct { get; set; }
        public virtual DbSet<Product> Product { get; set; }
        public virtual DbSet<User> User { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            var connectionString = Startup.Configuration.GetConnectionString("DefaultConnection");

            optionsBuilder.UseSqlServer(connectionString);
        }


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Order>(entity =>
            {
                entity.HasKey(e => e.OrderId)
                    .HasName("PK_Order");

                entity.Property(e => e.OrderId).HasColumnName("OrderId");

                entity.Property(e => e.UserId).HasColumnName("UserId");

                entity.Property(e => e.Data)
                    .HasColumnName("data")
                    .HasColumnType("date");

                entity.Property(e => e.State)
                    .HasColumnName("state")
                    .HasColumnType("varchar(10)");

                entity.Property(e => e.TotalPrice).HasColumnName("totalPrice");

                entity.HasOne(d => d.UserIdNavigation)
                    .WithMany(p => p.Order)
                    .HasForeignKey(d => d.UserId)
                    .OnDelete(DeleteBehavior.Restrict)
                    .HasConstraintName("FK_Order_User");
            });

            modelBuilder.Entity<OrderProduct>(entity =>
            {
                entity.HasKey(e => new { e.OrderId, e.ProductId })
                    .HasName("PK_OrderProduct");

                entity.ToTable("OrderProduct");

                entity.Property(e => e.OrderId).HasColumnName("OrderId");

                entity.Property(e => e.ProductId).HasColumnName("ProductId");

                entity.Property(e => e.Quantity).HasColumnName("Quantity");

                entity.HasOne(d => d.OrderIdNavigation)
                    .WithMany(p => p.OrderProduct)
                    .HasForeignKey(d => d.OrderId)
                    .OnDelete(DeleteBehavior.Restrict)
                    .HasConstraintName("FK_OrderProduct_Order");

                entity.HasOne(d => d.ProductIdNavigation)
                    .WithMany(p => p.OrderProduct)
                    .HasForeignKey(d => d.ProductId)
                    .OnDelete(DeleteBehavior.Restrict)
                    .HasConstraintName("FK_OrderProduct_Product");
            });

            modelBuilder.Entity<Product>(entity =>
            {
                entity.HasKey(e => e.ProductId)
                    .HasName("PK_Product");

                entity.Property(e => e.ProductId).HasColumnName("ProductId");

                entity.Property(e => e.Description)
                    .IsRequired()
                    .HasColumnName("description")
                    .HasColumnType("text");

                entity.Property(e => e.Disp)
                    .IsRequired()
                    .HasColumnName("disp")
                    .HasColumnType("varchar(10)");

                entity.Property(e => e.Image).HasColumnName("image");

                entity.Property(e => e.Price).HasColumnName("price");

                entity.Property(e => e.Discount).HasColumnName("discount");

                entity.Property(e => e.Title)
                    .IsRequired()
                    .HasColumnName("title")
                    .HasColumnType("varchar(255)");
            });

            modelBuilder.Entity<User>(entity =>
            {
                entity.HasKey(e => e.UserId)
                    .HasName("PK_User");

                entity.HasIndex(e => e.Email)
                    .HasName("UserIndex")
                    .IsUnique();

                entity.Property(e => e.UserId).HasColumnName("UserId");

                entity.Property(e => e.Password)
                    .IsRequired()
                    .HasColumnName("password")
                    .HasColumnType("varchar(50)");

                entity.Property(e => e.Role)
                    .HasColumnName("role")
                    .HasColumnType("varchar(10)");

                entity.Property(e => e.Email)
                    .IsRequired()
                    .HasColumnName("email")
                    .HasColumnType("varchar(50)");
            });
        }
    }
}