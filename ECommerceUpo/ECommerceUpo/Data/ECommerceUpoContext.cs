using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.Extensions.Configuration;
using ECommerceUpo.Models;

namespace ECommerceUpo.Data
{
    public  class ECommerceUpoContext : DbContext
    {
        //PROBLEMA!
        //Setup connection string dovrebbe avvenire qua (Startup.ConfigureServices: AddDbContext chiama questo costruttore)
        //ma questo costruttore non viene mai chiamato. Connessione risolta in OnConfiguring, piu' in basso
        public ECommerceUpoContext(DbContextOptions<ECommerceUpoContext> options) : base(options)
        {
        }

        public ECommerceUpoContext() { }

        //le entita'
        public virtual DbSet<Order> Order { get; set; }
        public virtual DbSet<OrderProduct> OrderProduct { get; set; }
        public virtual DbSet<Product> Product { get; set; }
        public virtual DbSet<User> User { get; set; }

        //Connection string viene settata qua: usa le configurations create in Startup
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
                    .HasName("PK_Ordine");

                entity.Property(e => e.OrderId).HasColumnName("CD_ORDINE");

                entity.Property(e => e.UserId).HasColumnName("CD_UTENTE");

                entity.Property(e => e.Data)
                    .HasColumnName("dt_inserimento")
                    .HasColumnType("date");

                entity.Property(e => e.State)
                    .HasColumnName("stato")
                    .HasColumnType("varchar(10)");

                entity.Property(e => e.TotalPrice).HasColumnName("totale");

                entity.HasOne(d => d.UserIdNavigation)
                    .WithMany(p => p.Order)
                    .HasForeignKey(d => d.UserId)
                    .OnDelete(DeleteBehavior.Restrict)
                    .HasConstraintName("FK_Ordine_Utente");
            });

            modelBuilder.Entity<OrderProduct>(entity =>
            {
                entity.HasKey(e => new { e.OrderId, e.ProductId })
                    .HasName("PK_Ordine_Prodotto");

                entity.ToTable("Ordine_Prodotto");

                entity.Property(e => e.OrderId).HasColumnName("CD_ORDINE");

                entity.Property(e => e.ProductId).HasColumnName("CD_PRODOTTO");

                entity.Property(e => e.Quantity).HasColumnName("quantita");

                entity.HasOne(d => d.OrderIdNavigation)
                    .WithMany(p => p.OrderProduct)
                    .HasForeignKey(d => d.OrderId)
                    .OnDelete(DeleteBehavior.Restrict)
                    .HasConstraintName("FK_Ordine_Prodotto_Ordine");

                entity.HasOne(d => d.ProductIdNavigation)
                    .WithMany(p => p.OrderProduct)
                    .HasForeignKey(d => d.ProductId)
                    .OnDelete(DeleteBehavior.Restrict)
                    .HasConstraintName("FK_Ordine_Prodotto_Prodotto");
            });

            modelBuilder.Entity<Product>(entity =>
            {
                entity.HasKey(e => e.ProductId)
                    .HasName("PK_Prodotto");

                entity.Property(e => e.ProductId).HasColumnName("CD_PRODOTTO");

                entity.Property(e => e.Description)
                    .IsRequired()
                    .HasColumnName("descrizione")
                    .HasColumnType("text");

                entity.Property(e => e.Disp)
                    .IsRequired()
                    .HasColumnName("disponibile")
                    .HasColumnType("varchar(10)");

                entity.Property(e => e.Image).HasColumnName("immagine");

                entity.Property(e => e.Price).HasColumnName("prezzo");

                entity.Property(e => e.Discount).HasColumnName("sconto");

                entity.Property(e => e.Title)
                    .IsRequired()
                    .HasColumnName("titolo")
                    .HasColumnType("varchar(255)");
            });

            modelBuilder.Entity<User>(entity =>
            {
                entity.HasKey(e => e.UserId)
                    .HasName("PK_Utente");

                entity.HasIndex(e => e.Email)
                    .HasName("UQ__Utente__F3DBC57291EBE031")
                    .IsUnique();

                entity.Property(e => e.UserId).HasColumnName("CD_UTENTE");

                entity.Property(e => e.Password)
                    .IsRequired()
                    .HasColumnName("password")
                    .HasColumnType("varchar(50)");

                entity.Property(e => e.Role)
                    .HasColumnName("ruolo")
                    .HasColumnType("varchar(10)");

                entity.Property(e => e.Email)
                    .IsRequired()
                    .HasColumnName("username")
                    .HasColumnType("varchar(50)");
            });
        }
    }
}