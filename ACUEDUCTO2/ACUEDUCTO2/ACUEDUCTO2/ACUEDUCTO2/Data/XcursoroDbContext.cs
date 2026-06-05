using ACUEDUCTO2.Share.Models;
using Microsoft.EntityFrameworkCore;

namespace ACUEDUCTO2.Data
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
        public DbSet<Periodo> Periodo { get; set; }
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
                entity.HasKey(e => e.IdPredio);
                entity.Property(e => e.IdPredio).ValueGeneratedNever();
                entity.ToTable("Predio");

                // Map legacy column names explicitly to avoid missing column errors
                entity.Property(e => e.IdTipoConsumo).HasColumnName("TipoConsumo");
                entity.Property(e => e.Estrato).HasColumnName("Estrato");

                // Ignore navigation properties so EF does not auto-discover
                // Estrato/TipoConsumo with mismatched v1-v2 schemas.
                entity.Ignore(e => e.EstratoNavigation);
                entity.Ignore(e => e.TipoConsumoNavigation);
            });

            builder.Entity<Periodo>(entity =>
            {
                entity.HasKey(e => e.Guid);
                entity.ToTable("Periodo");

                // v1 Periodo table only has IDPeriodo, DescripcionCorta, DescripcionLarga.
                // Ignore all v2-only properties so EF does not query columns
                // that do not exist in the v1 database.
                entity.Ignore(e => e.Orden);
                entity.Ignore(e => e.NombreCortoPeriodo);
                entity.Ignore(e => e.NombreLargoPeriodo);
                entity.Ignore(e => e.FechaHoraIniciaPeriodo);
                entity.Ignore(e => e.FechaHoraTerminaPeriodo);
                entity.Ignore(e => e.FechaHoraCreacionRegistro);
                entity.Ignore(e => e.FechaHoraIniciaRecaudo);
                entity.Ignore(e => e.FechaHoraTerminaRecaudo);
                entity.Ignore(e => e.UsuarioSistemaCreaRegistro);
                entity.Ignore(e => e.Descripcion);
                entity.Ignore(e => e.PeriodoReferenciaId);
                entity.Ignore(e => e.FechaHoraIniciaRegistroConsumo);
                entity.Ignore(e => e.PorcentajeMora);
                entity.Ignore(e => e.PorcentajeSubsidio);
                entity.Ignore(e => e.FechaHoraTerminaRegistroConsumo);
                entity.Ignore(e => e.EncargadoRegistroConsumos);
                entity.Ignore(e => e.FechaHoraFacturacion);
                entity.Ignore(e => e.EncargadoFacturacion);
                entity.Ignore(e => e.UltimaFacturaRegistrada);
                entity.Ignore(e => e.FechaLimitePago);
                entity.Ignore(e => e.FechaSuspension);
                entity.Ignore(e => e.TotalPredios);
                entity.Ignore(e => e.CantidadConsumoM3Total);
                entity.Ignore(e => e.ValorTotalConsumoBasico);
                entity.Ignore(e => e.ValorTotalConsumoAdicional);
                entity.Ignore(e => e.ValorTotalConsumoM3);
                entity.Ignore(e => e.ValorTotalMora);
                entity.Ignore(e => e.ValorTotalSobreTasa);
                entity.Ignore(e => e.ValorTotalSubsidio);
                entity.Ignore(e => e.ValorTotalOtros);
                entity.Ignore(e => e.ValorTotalPlanilla);
                entity.Ignore(e => e.ValorTotalRecaudo);
                entity.Ignore(e => e.Observaciones);
                entity.Ignore(e => e.Abierta);
            });

            builder.Entity<TipoConsumo>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.ToTable("TipoConsumo");

                // Avoid generating shadow FKs like TipoConsumoId/TipoConsumoId1
                entity.Ignore(e => e.Predios);
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
