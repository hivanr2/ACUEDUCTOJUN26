using ACUEDUCTO2-0.Compartir.MODELS;
using Microsoft.EntityFrameworkCore;

namespace ACUEDUCTO2-0.Data
{
    public class XcursoroDbContext : DbContext
    {
        public XcursoroDbContext(DbContextOptions<XcursoroDbContext> options) : base(options)
        {
        }

        public DbSet<Seguridad> Seguridad { get; set; }
        public DbSet<Proceso> Proceso { get; set; }
        public DbSet<Solicitud> Solicitud { get; set; }
        public DbSet<TipoSolicitud> TipoSolicitud { get; set; }
        public DbSet<EstadoSolicitud> EstadoSolicitud { get; set; }
        public DbSet<Predio> Predio { get; set; }
        public DbSet<TipoConsumo> TipoConsumo { get; set; }
        public DbSet<Autorizado> Autorizado { get; set; }
        public DbSet<Usuario> Usuario { get; set; }

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
                // Predio primary key is IdPredio in shared models
                entity.HasKey(e => e.IdPredio);
                entity.Property(e => e.IdPredio).ValueGeneratedNever();
                entity.ToTable("Predio");
            });

            builder.Entity<TipoConsumo>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.ToTable("TipoConsumo");
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
        }
    }
}
