using ACUEDUCTO2.Share.Models;
using PredioEntity = ACUEDUCTO2.Share.Models.Predio;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace ACUEDUCTO2.Data
{
    public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : IdentityDbContext<ApplicationUser, ApplicationRole, string>(options)
    {
        // Core entities
        public DbSet<Predio> Predio { get; set; }
        public DbSet<TipoConsumo> TipoConsumo { get; set; }
        // Procesos
        // The `Proceso` type is a UI/menu script and does not have a backing table in the
        // production database. Prevent EF from mapping it so runtime queries won't fail.
        public DbSet<Proceso> Proceso { get; set; }
        public DbSet<TipoProceso> TipoProceso { get; set; }
        //  Solicitud
        public DbSet<Solicitud> Solicitud { get; set; }
        public DbSet<EstadoSolicitud> EstadoSolicitud { get; set; }
        public DbSet<TipoSolicitud> TipoSolicitud { get; set; }
        // Segiridad
        public DbSet<Seguridad> Seguridad { get; set; }

        // Plural aliases used by UI code (keep compatibility)
        public DbSet<Predio> Predios { get => Predio; set => Predio = value; }

        // Financiero / facturación
        public DbSet<Factura> Facturacions { get; set; }
        public DbSet<Factura> Facturas { get => Facturacions; set => Facturacions = value; }
        public DbSet<Recaudo> Ingresos { get; set; }
        public DbSet<Periodo> Periodos { get; set; }
        public DbSet<Periodo> Planillas { get => Periodos; set => Periodos = value; }

        // Note: ACUEDUCTO2 uses `Periodo` as the canonical billing period model.
        // Removed legacy `Planillas` alias to avoid confusion — use `Periodos` throughout the codebase.

        // Catalogs
        public DbSet<Periodo> Periodo { get; set; }
        public DbSet<Estrato> Estratos { get; set; }
        public DbSet<Recaudo> Recaudos { get; set; }
        public DbSet<Consumo> Consumos { get; set; }
        public DbSet<Tarifa> Tarifas { get; set; }
        // Trabajos / Mantenimiento
        public DbSet<Trabajos> Trabajo { get; set; }
        public DbSet<TipoTrabajo> TiposTrabajos { get; set; }
        public DbSet<TrabajoEquipo> TrabajosEquipos { get; set; }
        public DbSet<TrabajoMaterial> TrabajosMateriales { get; set; }
        public DbSet<TrabajoPersonal> TrabajosPersonales { get; set; }
        // Usuarios (tabla de usuarios de facturación / clientes)
        public DbSet<Usuario> Usuarios { get; set; }
        public DbSet<Autorizado> Autorizados { get; set; }
        // Permisos de menú
        public DbSet<MenuPermission> MenuPermissions { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            // Ensure Identity base mappings are applied (keys for Identity types, relationships, etc.)
            base.OnModelCreating(builder);

            // Ignore entities that are not present in the database schema.
            // `Proceso` is used only for menu/script generation and must not be mapped.
            builder.Ignore<Proceso>();

            // Map entities to existing table names to match the production database.
            // EF pluralizes DbSet names by convention which can cause invalid object name
            // errors when the actual table name is singular. Map `Autorizado` explicitly.
            builder.Entity<Autorizado>().ToTable("Autorizado");

            // MenuPermission — map to the existing legacy table.
            builder.Entity<MenuPermission>(entity =>
            {
                entity.ToTable("MenuPermission");
                entity.HasKey(m => m.Id);
                entity.Property(m => m.CanView).HasDefaultValue(true);
                entity.Property(m => m.CanCreate).HasDefaultValue(false);
                entity.Property(m => m.CanEdit).HasDefaultValue(false);
                entity.Property(m => m.CanDelete).HasDefaultValue(false);
                entity.Property(m => m.CreatedAt).HasColumnType("datetime");
            });

            // Map Periodo to the existing `Periodo` table (singular). The migration
            // `20260309034611_uno` that would rename it to `Periodos` and rename the
            // PK column `IDPlanilla` to `Guid` has NOT been applied to the database.
            // Map CLR property names to the actual pre-migration column names.
            builder.Entity<Periodo>(entity =>
            {
                entity.ToTable("Periodo");
                entity.HasKey(p => p.Guid);
                entity.Property(p => p.Guid).HasColumnName("IDPlanilla").HasMaxLength(100);
            });

            // Map Usuario entity to the existing table name and ensure its key is configured
            // so EF Core includes it in the model. Adjust column names if your schema differs.
            builder.Entity<Usuario>(entity =>
            {
                entity.ToTable("Usuario");
                entity.HasKey(u => u.Guid);
                entity.Property(u => u.Identificacion).HasMaxLength(100);
                entity.Property(u => u.ApellidosNombres).HasMaxLength(250);
                entity.Property(u => u.CorreoElectronico).HasMaxLength(250);
                entity.Property(u => u.Telefono).HasMaxLength(50);
                entity.Property(u => u.Observaciones).HasMaxLength(1000).IsRequired(false);
                entity.Property(u => u.Imagen).HasColumnType("varbinary(max)").IsRequired(false);
            });

            // Map Tarifa to the existing legacy table name (singular) to avoid
            // runtime errors like: SqlException: Invalid object name 'Tarifas'.
            builder.Entity<Tarifa>(entity =>
            {
                entity.ToTable("Tarifa");
                entity.HasKey(t => t.Guid);
                entity.Property(t => t.ValorM3).HasColumnName("ValorUnidad");

                // Legacy table does not contain these columns.
                entity.Ignore(t => t.IdPredio);
                entity.Ignore(t => t.Descripcion);
            });

            // Trabajos / mantenimiento mapping
            builder.Entity<TipoTrabajo>(entity =>
            {
                entity.ToTable("TiposTrabajos");
                entity.HasKey(t => t.IdTipoTrabajo);
                entity.Property(t => t.IdTipoTrabajo).HasColumnName("IdTipoTrabajo");
                entity.Property(t => t.Nombre).HasColumnName("Nombre").HasMaxLength(250).IsRequired(false);
                entity.Property(t => t.Descripcion).HasColumnName("Descripcion").IsRequired(false);
            });

            builder.Entity<Trabajos>(entity =>
            {
                // Table in legacy DB is named TRABAJOS
                entity.ToTable("TRABAJOS");
                entity.HasKey(t => t.IdTrabajo);
                entity.Property(t => t.IdTrabajo).HasColumnName("IdTrabajo");
                entity.Property(t => t.Descripcion).HasColumnName("Descripcion").IsRequired(false);
                entity.Property(t => t.TipoTrabajo).HasColumnName("TipoTrabajo");
                entity.Property(t => t.Estado).HasColumnName("Estado").HasMaxLength(100).IsRequired(false);
                entity.Property(t => t.Personal).HasColumnName("Personal").IsRequired(false);
                entity.Property(t => t.FechaInicio).HasColumnName("FechaInicio");
                entity.Property(t => t.FechaFinalizacion).HasColumnName("FechaFinalizacion");
                entity.Property(t => t.IdPredio).HasColumnName("IdPredio").HasMaxLength(100).IsRequired(false);

                entity.HasOne(t => t.TipoTrabajoNavigation)
                      .WithMany(tt => tt.Trabajo)
                      .HasForeignKey(t => t.TipoTrabajo)
                      .HasConstraintName("FK_Trabajo_TipoTrabajo");
            });

            builder.Entity<TrabajoEquipo>(entity =>
            {
                entity.ToTable("TrabajosEquipos");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).HasColumnName("Id");
                entity.Property(e => e.IdTrabajo).HasColumnName("IdTrabajo");
                entity.Property(e => e.Equipo).HasColumnName("Equipo").IsRequired(false);
                entity.Property(e => e.Cantidad).HasColumnName("Cantidad");
            });

            builder.Entity<TrabajoMaterial>(entity =>
            {
                entity.ToTable("TrabajosMateriales");
                entity.HasKey(m => m.Id);
                entity.Property(m => m.Id).HasColumnName("Id");
                entity.Property(m => m.IdTrabajo).HasColumnName("IdTrabajo");
                entity.Property(m => m.Material).HasColumnName("Material").IsRequired(false);
                entity.Property(m => m.Cantidad).HasColumnName("Cantidad");
            });

            builder.Entity<TrabajoPersonal>(entity =>
            {
                entity.ToTable("TrabajosPersonal");
                entity.HasKey(p => p.Id);
                entity.Property(p => p.Id).HasColumnName("Id");
                entity.Property(p => p.IdTrabajo).HasColumnName("IdTrabajo");
                entity.Property(p => p.PersonalNombre).HasColumnName("PersonalNombre").IsRequired(false);
                entity.Property(p => p.Rol).HasColumnName("Rol").IsRequired(false);
            });

            // Ensure column mappings match the existing database schema.
            // Many legacy schemas use `IdTipoConsumo` as the primary key column name
            // for the `TipoConsumo` table. Map the CLR property `Id` to that column
            // so EF generates SQL using the correct column name and avoids queries
            // that reference a non-existing `Id` column.
            builder.Entity<TipoConsumo>().Property(tc => tc.Id).HasColumnName("IdTipoConsumo");

            // Predio foreign keys: map CLR properties to actual DB column names and
            // configure the relationships so EF doesn't create convention-based
            // column names like `EstratoNavigationIdEstrato` or `TipoConsumoNavigationId`.
            builder.Entity<Factura>(entity =>
            {
                entity.ToTable("Facturacion");
                entity.HasKey(f => f.Guid);
                entity.Property(f => f.Guid).HasColumnName("Idfactura").HasMaxLength(100);
                entity.Property(f => f.IdPeriodo).HasColumnName("Idplanilla").HasMaxLength(100).IsRequired(false);
                entity.Property(f => f.IdPredio).HasColumnName("Idpredio").HasMaxLength(100).IsRequired(false);
                entity.Property(f => f.NotaDeFactura).HasColumnName("NotasFactura").IsRequired(false);
                entity.Property(f => f.NotaDeRecaudo).HasColumnName("NotasRecaudo").IsRequired(false);

                entity.HasOne(f => f.IdPredioNavigation)
                      .WithMany()
                      .HasForeignKey(f => f.IdPredio)
                      .HasConstraintName("FK_Factura_Predio");

                entity.HasOne(f => f.IdPeriodoNavigation)
                      .WithMany()
                      .HasForeignKey(f => f.IdPeriodo)
                      .HasConstraintName("FK_Factura_Periodo");
            });

            builder.Entity<Predio>(entity =>
            {
                // Map to the exact table name used by the legacy DB
                entity.ToTable("Predio");

                // Key
                entity.HasKey(p => p.IdPredio);

                // Column mappings: ensure EF uses the exact column names present in the DB
                entity.Property(p => p.IdPredio).HasColumnName("IdPredio");
                entity.Property(p => p.ApellidosNombresUsuario).HasColumnName("ApellidosNombresUsuario").HasMaxLength(500).IsRequired(false);
                entity.Property(p => p.CodigoUsuario).HasColumnName("CodigoUsuario").HasMaxLength(100).IsRequired(false);
                entity.Property(p => p.IdentificacionFacturacion).HasColumnName("IdentificacionFacturacion").HasMaxLength(100).IsRequired(false);
                entity.Property(p => p.Estrato).HasColumnName("IdEstrato");
                entity.Property(p => p.Ruta).HasColumnName("Ruta");
                entity.Property(p => p.Direccion).HasColumnName("Direccion").HasMaxLength(500).IsRequired(false);
                entity.Property(p => p.NumeroCatastral).HasColumnName("NumeroCatastral").HasMaxLength(200).IsRequired(false);
                entity.Property(p => p.InformacionCatastral).HasColumnName("InformacionCatastral").IsRequired(false);
                entity.Property(p => p.ExtensionM2).HasColumnName("ExtensionM2");
                entity.Property(p => p.M2construidos).HasColumnName("M2construidos");
                entity.Property(p => p.NumeroMedidor).HasColumnName("NumeroMedidor").HasMaxLength(100).IsRequired(false);
                entity.Property(p => p.MarcaMedidor).HasColumnName("MarcaMedidor").HasMaxLength(100).IsRequired(false);
                entity.Property(p => p.EstadoMedidor).HasColumnName("EstadoMedidor").HasMaxLength(100).IsRequired(false);
                entity.Property(p => p.UltimaMedicion).HasColumnName("UltimaMedicion");
                entity.Property(p => p.ContadorEnSeguimiento).HasColumnName("ContadorEnSeguimiento");
                entity.Property(p => p.Activo).HasColumnName("Activo");
                entity.Property(p => p.FechaHoraRegistro).HasColumnName("FechaHoraRegistro");
                entity.Property(p => p.IdTipoConsumo).HasColumnName("IdTipoConsumo");
                entity.Property(p => p.Observaciones).HasColumnName("Observaciones").IsRequired(false);

                // Configure navigation relationships explicitly
                entity.HasOne(p => p.EstratoNavigation)
                      .WithMany()
                      .HasForeignKey(p => p.Estrato)
                      .HasConstraintName("FK_Predio_Estrato");

                entity.HasOne(p => p.TipoConsumoNavigation)
                      .WithMany(tc => tc.Predios)
                      .HasForeignKey(p => p.IdTipoConsumo)
                      .HasConstraintName("FK_Predio_TipoConsumo");
            });
        }
    }
}
