using ACUEDUCTO2-0_0.Compartir.MODELS;
using Data;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace ACUEDUCTO2-0_0.Data
{
    public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : IdentityDbContext<ApplicationUser>(options)
    {
        // Core entities
        public DbSet<Predio> Predio { get; set; }
        public DbSet<TipoConsumo> TipoConsumo { get; set; }
        public DbSet<Proceso> Proceso { get; set; }
        public DbSet<Solicitud> Solicitud { get; set; }
        public DbSet<EstadoSolicitud> EstadoSolicitud { get; set; }
        public DbSet<TipoSolicitud> TipoSolicitud { get; set; }
        public DbSet<Seguridad> Seguridad { get; set; }

        // Plural aliases used by UI code (keep compatibility)
        public DbSet<Predio> Predios { get => Predio; set => Predio = value; }

        // Financiero / facturación
        public DbSet<Factura> Facturacions { get; set; }
        public DbSet<Recaudo> Ingresos { get; set; }

        // Catalogs
        public DbSet<Estrato> Estratos { get; set; }
        public DbSet<Recaudo> Recaudos { get; set; }
        public DbSet<Consumo> Consumos { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<Seguridad>(entity =>
            {
                entity.HasKey(e => e.Guid);
                entity.ToTable("Seguridad");
            });

            builder.Entity<Proceso>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).ValueGeneratedNever();
                entity.ToTable("Proceso");
            });

            // Configure primary keys for shared models
            builder.Entity<Solicitud>(entity =>
            {
                entity.HasKey(e => e.Guid);
                entity.ToTable("Solicitud");
            });

            builder.Entity<TipoSolicitud>(entity =>
            {
                entity.HasKey(e => e.Guid);
                entity.ToTable("TipoSolicitud");
            });
            builder.Entity<Predio>(entity =>
            {
                entity.HasKey(e => e.IdPredio);
                entity.ToTable("Predio");

                entity.Property(e => e.Estrato).HasColumnName("IdEstrato");

                entity.HasOne(d => d.TipoConsumoNavigation)
                    .WithMany(p => p.Predios)
                    .HasForeignKey(d => d.IdTipoConsumo)
                    .HasConstraintName("FK_Predio_TipoConsumo");

                entity.HasOne(d => d.EstratoNavigation)
                    .WithMany()
                    .HasForeignKey(d => d.Estrato)
                    .HasConstraintName("FK_Predio_Estrato");

                entity.HasOne(d => d.IdentificacionFacturacionNavigation)
                    .WithMany()
                    .HasForeignKey(d => d.IdentificacionFacturacion)
                    .HasPrincipalKey(u => u.Identificacion)
                    .HasConstraintName("FK_Predio_Usuario");
            });

            builder.Entity<TipoConsumo>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.ToTable("TipoConsumo");
                entity.Property(e => e.Id).HasColumnName("IdTipoConsumo");
            });

            builder.Entity<EstadoSolicitud>(entity =>
            {
                entity.HasKey(e => e.Guid);
                entity.ToTable("EstadoSolicitud");
            });

            builder.Entity<Autorizado>(entity =>
            {
                entity.HasKey(e => e.Guid);
                entity.ToTable("Autorizado");
            });

            builder.Entity<Usuario>(entity =>
            {
                entity.HasKey(e => e.Guid);
                entity.ToTable("Usuario");
            });
            builder.Entity<Estrato>(entity =>
            {
                entity.HasKey(e => e.IdEstrato);
                entity.ToTable("Estrato");
            });
            builder.Entity<Factura>(entity =>
            {
                entity.HasKey(e => e.Guid);
                entity.ToTable("Factura");
            });
            builder.Entity<Recaudo>(entity =>
            {
                entity.HasKey(e => e.Guid);
                entity.ToTable("Recaudo");
            });
            builder.Entity<Consumo>(entity =>
            {
                entity.HasKey(e => e.Guid);
                entity.ToTable("Consumo");
            });
        }
    }
}
