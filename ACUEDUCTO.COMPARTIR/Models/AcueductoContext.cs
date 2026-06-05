using Microsoft.EntityFrameworkCore;
using System.Configuration;

namespace ACUEDUCTO.Models;

public partial class AcueductoContext : DbContext
{
    public AcueductoContext()
    {
    }

    public AcueductoContext(DbContextOptions<AcueductoContext> options)
        : base(options)
    {
    }

    public virtual DbSet<AspNetRole> AspNetRoles { get; set; }

    public virtual DbSet<AspNetRoleClaim> AspNetRoleClaims { get; set; }

    public virtual DbSet<AspNetUser> AspNetUsers { get; set; }

    public virtual DbSet<AspNetUserClaim> AspNetUserClaims { get; set; }

    public virtual DbSet<AspNetUserLogin> AspNetUserLogins { get; set; }

    public virtual DbSet<AspNetUserToken> AspNetUserTokens { get; set; }

    public virtual DbSet<CuentaPorCobrar> CuentaPorCobrars { get; set; }

    public virtual DbSet<CuentaPorPagar> CuentaPorPagars { get; set; }

    public virtual DbSet<Documentacion> Documentacions { get; set; }

    public virtual DbSet<Egreso> Egresos { get; set; }

    public virtual DbSet<Equipo> Equipos { get; set; }

    public virtual DbSet<Estado> Estados { get; set; }

    public virtual DbSet<Estrato> Estratos { get; set; }

    public virtual DbSet<Facturacion> Facturacions { get; set; }

    public virtual DbSet<Indicador> Indicadores { get; set; }

    public virtual DbSet<Ingreso> Ingresos { get; set; }

    public virtual DbSet<Inventario> Inventarios { get; set; }

    public virtual DbSet<Material> Materiales { get; set; }

    public virtual DbSet<MovimientoCaja> MovimientoCajas { get; set; }

    public virtual DbSet<MovimientoInventario> MovimientoInventarios { get; set; }

    public virtual DbSet<Periodo> Periodos { get; set; }

    public virtual DbSet<Personal> Personals { get; set; }

    public virtual DbSet<Planilla> Planillas { get; set; }

    public virtual DbSet<Predio> Predios { get; set; }

    public virtual DbSet<Recaudo> Recaudos { get; set; }

    public virtual DbSet<SerieDocumentacion> SerieDocumentacions { get; set; }

    public virtual DbSet<Solicitud> Solicitudes { get; set; }

    public virtual DbSet<Tarifa> Tarifas { get; set; }

    public virtual DbSet<Tipo> Tipos { get; set; }

    public virtual DbSet<TipoConsumo> TipoConsumos { get; set; }

    public virtual DbSet<TipoDocumentacion> TipoDocumentacions { get; set; }

    public virtual DbSet<TiposTrabajo> TiposTrabajos { get; set; }

    public virtual DbSet<Trabajo> Trabajos { get; set; }

    public virtual DbSet<Usuario> Usuarios { get; set; }

    public virtual DbSet<MenuPermission> MenuPermissions { get; set; } // NEW

    public virtual DbSet<OcrCorrection> OcrCorrections { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder) { }
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<AspNetRole>(static entity =>
        {
            entity.HasIndex(static e => e.NormalizedName, "RoleNameIndex")
                .IsUnique()
                .HasFilter("([NormalizedName] IS NOT NULL)");

            entity.Property(static e => e.Name).HasMaxLength(256);
            entity.Property(static e => e.NormalizedName).HasMaxLength(256);
        });

        modelBuilder.Entity<AspNetRoleClaim>(static entity =>
        {
            entity.HasIndex(static e => e.RoleId, "IX_AspNetRoleClaims_RoleId");

            entity.HasOne(static d => d.Role).WithMany(static p => p.AspNetRoleClaims).HasForeignKey(static d => d.RoleId);
        });

        modelBuilder.Entity<AspNetUser>(static entity =>
        {
            entity.HasIndex(static e => e.NormalizedEmail, "EmailIndex");

            entity.HasIndex(static e => e.NormalizedUserName, "UserNameIndex")
                .IsUnique()
                .HasFilter("([NormalizedUserName] IS NOT NULL)");

            entity.Property(static e => e.Email).HasMaxLength(256);
            entity.Property(static e => e.NormalizedEmail).HasMaxLength(256);
            entity.Property(static e => e.NormalizedUserName).HasMaxLength(256);
            entity.Property(static e => e.UserName).HasMaxLength(256);

            entity.HasMany(static d => d.Roles).WithMany(static p => p.Users)
                .UsingEntity<Dictionary<string, object>>(
                    "AspNetUserRole",
                    static r => r.HasOne<AspNetRole>().WithMany().HasForeignKey("RoleId"),
                    static l => l.HasOne<AspNetUser>().WithMany().HasForeignKey("UserId"),
                    static j =>
                    {
                        j.HasKey("UserId", "RoleId");
                        j.ToTable("AspNetUserRoles");
                        j.HasIndex(new[] { "RoleId" }, "IX_AspNetUserRoles_RoleId");
                    });
        });

        modelBuilder.Entity<AspNetUserClaim>(static entity =>
        {
            entity.HasIndex(static e => e.UserId, "IX_AspNetUserClaims_UserId");

            entity.HasOne(static d => d.User).WithMany(static p => p.AspNetUserClaims).HasForeignKey(static d => d.UserId);
        });

        modelBuilder.Entity<AspNetUserLogin>(static entity =>
        {
            entity.HasKey(static e => new { e.LoginProvider, e.ProviderKey });

            entity.HasIndex(static e => e.UserId, "IX_AspNetUserLogins_UserId");

            entity.HasOne(static d => d.User).WithMany(static p => p.AspNetUserLogins).HasForeignKey(static d => d.UserId);
        });

        modelBuilder.Entity<AspNetUserToken>(static entity =>
        {
            entity.HasKey(static e => new { e.UserId, e.LoginProvider, e.Name });

            entity.HasOne(static d => d.User).WithMany(static p => p.AspNetUserTokens).HasForeignKey(static d => d.UserId);
        });

        modelBuilder.Entity<CuentaPorCobrar>(static entity =>
        {
            entity.HasKey(static e => e.IdcuentaxC);

            entity.ToTable("CuentaPorCobrar");

            entity.Property(static e => e.IdcuentaxC)
                .HasMaxLength(50)
                .HasColumnName("IDCuentaxC");
            entity.Property(static e => e.Descripcion).HasMaxLength(200);
            entity.Property(static e => e.EnFactura).HasDefaultValue(false);
            entity.Property(static e => e.FechaHora).HasColumnType("datetime");
            entity.Property(static e => e.Identificacion).HasMaxLength(50);
            entity.Property(static e => e.Vencimiento).HasColumnType("datetime");

            entity.HasOne(static d => d.IdentificacionNavigation).WithMany(static p => p.CuentaPorCobrars)
                .HasForeignKey(static d => d.Identificacion)
                .HasConstraintName("FK_CuentaPorCobrar_Usuario");
        });

        modelBuilder.Entity<CuentaPorPagar>(static entity =>
        {
            entity.HasKey(static e => e.IdcuentaxP);

            entity.ToTable("CuentaPorPagar");

            entity.Property(static e => e.IdcuentaxP)
                .HasMaxLength(50)
                .HasColumnName("IDCuentaxP");
            entity.Property(static e => e.Descripcion).HasMaxLength(200);
            entity.Property(static e => e.FechaHora).HasColumnType("datetime");
            entity.Property(static e => e.Identificacion).HasMaxLength(50);
            entity.Property(static e => e.Vencimiento).HasColumnType("datetime");

            entity.HasOne(static d => d.IdentificacionNavigation).WithMany(static p => p.CuentaPorPagars)
                .HasForeignKey(static d => d.Identificacion)
                .HasConstraintName("FK_CuentaPorPagar_Usuario");
        });

        modelBuilder.Entity<Documentacion>(static entity =>
        {
            entity.ToTable("Documentacion");

            entity.Property(static e => e.Id).HasMaxLength(50);
            entity.Property(static e => e.Codigo).HasMaxLength(50);
            entity.Property(static e => e.Usuario).HasMaxLength(50);

            entity.HasOne(static d => d.IdserieNavigation).WithMany(static p => p.Documentacions)
                .HasForeignKey(static d => d.Idserie)
                .HasConstraintName("FK_Documentacion_SerieDocumentacion");

            entity.HasOne(static d => d.IdtipoNavigation).WithMany(static p => p.Documentacions)
                .HasForeignKey(static d => d.Idtipo)
                .HasConstraintName("FK_Documentacion_TipoDocumentacion");
        });

        modelBuilder.Entity<Egreso>(static entity =>
        {
            entity.HasKey(static e => e.Idegreso);

            entity.ToTable("Egreso");

            entity.Property(static e => e.Idegreso)
                .HasMaxLength(50)
                .HasColumnName("IDEgreso");
            entity.Property(static e => e.Concepto).HasMaxLength(200);
            entity.Property(static e => e.FechaHora).HasColumnType("datetime");
            entity.Property(static e => e.Idcxp)
                .HasMaxLength(50)
                .HasColumnName("IDCXP");
            entity.Property(static e => e.Idfactura)
                .HasMaxLength(50)
                .HasColumnName("IDFactura");
            entity.Property(static e => e.Idproveedor)
                .HasMaxLength(50)
                .HasColumnName("IDProveedor");
        });

        modelBuilder.Entity<Equipo>(static entity =>
        {
            entity.HasKey(static e => e.IdEquipo);

            entity.Property(static e => e.Nombre)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(static e => e.Tipo)
                .HasMaxLength(50)
                .IsUnicode(false);
        });

        modelBuilder.Entity<Estado>(static entity =>
        {
            entity.HasKey(static e => e.Id);

            entity.ToTable("Estado");

            //entity.Property(e => e.Id).ValueGeneratedNever();
            entity.Property(static e => e.Nombre).HasMaxLength(50);
        });

        modelBuilder.Entity<Estrato>(static entity =>
        {
            entity.HasKey(static e => e.Idestrato);

            entity.ToTable("Estrato");

            entity.Property(static e => e.Idestrato)
                .ValueGeneratedNever()
                .HasColumnName("IDEstrato");
            entity.Property(static e => e.Descripcion).HasMaxLength(200);
        });

        modelBuilder.Entity<Facturacion>(static entity =>
        {
            entity.HasKey(static e => e.Idfactura);

            entity.ToTable("Facturacion");

            entity.Property(static e => e.Idfactura)
                .HasMaxLength(50)
                .HasColumnName("IDFactura");
            entity.Property(static e => e.CodigoFactura).HasMaxLength(50);
            entity.Property(static e => e.CodigoUsuario).HasMaxLength(50);
            entity.Property(static e => e.FechaHoraRegistro).HasColumnType("datetime");
            entity.Property(static e => e.Idplanilla)
                .HasMaxLength(50)
                .HasColumnName("IDPlanilla");
            entity.Property(static e => e.Idpredio)
                .HasMaxLength(50)
                .HasColumnName("IDPredio");
            entity.Property(static e => e.NotasFactura).HasMaxLength(500);
            entity.Property(static e => e.NotasRecaudo).HasMaxLength(500);

            entity.HasOne(static d => d.IdplanillaNavigation).WithMany(static p => p.Facturacions)
                .HasForeignKey(static d => d.Idplanilla)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Facturacion_Planilla");

            entity.HasOne(static d => d.IdpredioNavigation).WithMany(static p => p.Facturacions)
                .HasForeignKey(static d => d.Idpredio)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Facturacion_Predio1");
        });

        modelBuilder.Entity<Indicador>(static entity =>
        {
            entity.HasKey(static e => e.Idindicador);

            entity.ToTable("Indicador");

            entity.Property(static e => e.Idindicador).HasColumnName("IDIndicador");
            entity.Property(static e => e.Categoría).HasMaxLength(255);
            entity.Property(static e => e.Descripcion).HasMaxLength(255);
            entity.Property(static e => e.Frecuencia).HasMaxLength(255);
            entity.Property(static e => e.FórmulaUnidad).HasMaxLength(255);
            entity.Property(static e => e.MetaUmbral).HasMaxLength(255);
            entity.Property(static e => e.Responsable).HasMaxLength(50);
            entity.Property(static e => e.SemáforoGestión).HasMaxLength(255);
        });

        modelBuilder.Entity<Ingreso>(static entity =>
        {
            entity.HasKey(static e => e.Idingreso);

            entity.ToTable("Ingreso");

            entity.Property(static e => e.Idingreso)
                .HasMaxLength(50)
                .HasColumnName("IDIngreso");
            entity.Property(static e => e.Concepto).HasMaxLength(200);
            entity.Property(static e => e.FechaHora).HasColumnType("datetime");
            entity.Property(static e => e.Idcliente)
                .HasMaxLength(50)
                .HasColumnName("IDCliente");
            entity.Property(static e => e.Idcxc)
                .HasMaxLength(50)
                .HasColumnName("IDCXC");
            entity.Property(static e => e.Idfactura)
                .HasMaxLength(50)
                .HasColumnName("IDFactura");
        });

        modelBuilder.Entity<Inventario>(static entity =>
        {
            entity.HasKey(static e => e.Idinventario);

            entity.ToTable("Inventario");

            entity.Property(static e => e.Idinventario)
                .HasMaxLength(50)
                .HasColumnName("IDInventario");
            entity.Property(static e => e.Codigo).HasMaxLength(50);
            entity.Property(static e => e.DescripcionCorta).HasMaxLength(200);
            entity.Property(static e => e.DescripcionLarga).HasMaxLength(500);
            entity.Property(static e => e.FechaHoraRegistro).HasColumnType("datetime");
            entity.Property(static e => e.Medida).HasMaxLength(50);
            entity.Property(static e => e.Unidad).HasMaxLength(50);
        });

        modelBuilder.Entity<Material>(static entity =>
        {
            entity.HasKey(static e => e.IdMaterial).HasName("PK__Material__94356E587A1EBC9C");

            entity.Property(static e => e.Descripcion)
                .HasMaxLength(200)
                .IsUnicode(false);
            entity.Property(static e => e.Nombre)
                .HasMaxLength(50)
                .IsUnicode(false);
        });

        modelBuilder.Entity<MovimientoCaja>(static entity =>
        {
            entity.HasKey(static e => e.Idmov);

            entity.ToTable("MovimientoCaja");

            entity.Property(static e => e.Idmov).HasColumnName("IDMov");
            entity.Property(static e => e.FechaHora).HasColumnType("datetime");
            entity.Property(static e => e.Iddocumento)
                .HasMaxLength(50)
                .HasColumnName("IDDocumento");
        });

        modelBuilder.Entity<MovimientoInventario>(static entity =>
        {
            entity.HasKey(static e => e.Idmov);

            entity.ToTable("MovimientoInventario");

            entity.Property(static e => e.Idmov)
                .HasMaxLength(50)
                .HasColumnName("IDMov");
            entity.Property(static e => e.FechaHora).HasColumnType("datetime");
            entity.Property(static e => e.Idegreso)
                .HasMaxLength(50)
                .HasColumnName("IDEgreso");
            entity.Property(static e => e.Idfactura)
                .HasMaxLength(50)
                .HasColumnName("IDFactura");
            entity.Property(static e => e.Idingreso)
                .HasMaxLength(50)
                .HasColumnName("IDIngreso");
            entity.Property(static e => e.Idinventario)
                .HasMaxLength(50)
                .HasColumnName("IDInventario");

            entity.HasOne(static d => d.IdegresoNavigation).WithMany(static p => p.MovimientoInventarios)
                .HasForeignKey(static d => d.Idegreso)
                .HasConstraintName("FK_MovimientoInventario_Egreso");

            entity.HasOne(static d => d.IdingresoNavigation).WithMany(static p => p.MovimientoInventarios)
                .HasForeignKey(static d => d.Idingreso)
                .HasConstraintName("FK_MovimientoInventario_Ingreso");

            entity.HasOne(static d => d.IdinventarioNavigation).WithMany(static p => p.MovimientoInventarios)
                .HasForeignKey(static d => d.Idinventario)
                .HasConstraintName("FK_MovimientoInventario_Inventario");
        });

        modelBuilder.Entity<Periodo>(static entity =>
        {
            entity.HasKey(static e => e.Idperiodo);

            entity.ToTable("Periodo");

            entity.Property(static e => e.Idperiodo).HasColumnName("IDPeriodo");
            entity.Property(static e => e.DescripcionCorta).HasMaxLength(10);
            entity.Property(static e => e.DescripcionLarga).HasMaxLength(100);
        });

        modelBuilder.Entity<Personal>(static entity =>
        {
            entity.HasKey(static e => e.IdPersonal).HasName("PK__Personal__05A9201BFC6BB440");

            entity.ToTable("Personal");

            entity.Property(static e => e.Cargo)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(static e => e.Nombre)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(static e => e.Usuario).HasMaxLength(50);

            //entity.HasOne(d => d.UsuarioNavigation).WithMany(p => p.Personals)
            //    .HasForeignKey(d => d.Usuario)
            //    .OnDelete(DeleteBehavior.ClientSetNull)
            //    .HasConstraintName("FK_Personal_Usuario");
        });

        modelBuilder.Entity<Planilla>(static entity =>
        {
            entity.HasKey(static e => e.Idplanilla);

            entity.ToTable("Planilla");

            entity.Property(static e => e.Idplanilla)
                .HasMaxLength(50)
                .HasColumnName("IDPlanilla");
            entity.Property(static e => e.CantidadConsumoM3total).HasColumnName("CantidadConsumoM3Total");
            entity.Property(static e => e.Descripcion).HasMaxLength(200);
            entity.Property(static e => e.EncargadoFacturacion).HasMaxLength(50);
            entity.Property(static e => e.EncargadoRegistroConsumos).HasMaxLength(50);
            entity.Property(static e => e.FechaHoraFacturacion).HasColumnType("datetime");
            entity.Property(static e => e.FechaHoraRegistroConsumo).HasColumnType("datetime");
            entity.Property(static e => e.FechaHoraRegistroConsumos).HasColumnType("datetime");
            entity.Property(static e => e.Idperiodo).HasColumnName("IDPeriodo");
            entity.Property(static e => e.Observaciones).HasMaxLength(500);
            entity.Property(static e => e.Abierta).HasDefaultValue(false);
            entity.Property(static e => e.UltimaFacturaRegistrada).HasMaxLength(50);

            entity.HasOne(static d => d.IdperiodoNavigation).WithMany(static p => p.Planillas)
                .HasForeignKey(static d => d.Idperiodo)
                .HasConstraintName("FK_Planilla_Periodo");

        });

        modelBuilder.Entity<Predio>(static entity =>
        {
            entity.HasKey(static e => e.Idpredio);

            entity.ToTable("Predio");

            entity.Property(static e => e.Idpredio)
                .HasMaxLength(50)
                .HasColumnName("IDPredio");
            entity.Property(static e => e.Activo).HasDefaultValue(true);
            entity.Property(static e => e.ApellidosNombresUsuario).HasMaxLength(500);
            entity.Property(static e => e.CantidadCoutasCobros).HasDefaultValue(0);
            entity.Property(static e => e.CantidadCuotasPagadas).HasDefaultValue(0);
            entity.Property(static e => e.CodigoUsuario).HasMaxLength(50);
            entity.Property(static e => e.ConsumoM3promedio).HasColumnName("ConsumoM3Promedio");
            entity.Property(static e => e.ContadorEnSeguimiento).HasDefaultValue(false);
            entity.Property(static e => e.Direccion).HasMaxLength(200);
            entity.Property(static e => e.EnviaCorreo).HasDefaultValue(false);
            entity.Property(static e => e.EnviaWhatsapp).HasDefaultValue(false);
            entity.Property(static e => e.EstadoMedidor).HasMaxLength(50);
            entity.Property(static e => e.ExtensionM2).HasDefaultValue(0);
            entity.Property(static e => e.FechaHoraRegistro).HasColumnType("datetime");
            entity.Property(static e => e.IdentificacionFacturacion).HasMaxLength(50);
            entity.Property(static e => e.ImprimeFactura).HasDefaultValue(true);
            entity.Property(static e => e.Latitud).HasDefaultValue(0.0);
            entity.Property(static e => e.Longitud).HasDefaultValue(0.0);
            entity.Property(static e => e.M2construidos)
                .HasDefaultValue(0)
                .HasColumnName("M2Construidos");
            entity.Property(static e => e.MarcaMedidor).HasMaxLength(50);
            entity.Property(static e => e.NumeroCatastral).HasMaxLength(50);
            entity.Property(static e => e.NumeroMedidor).HasMaxLength(50);
            entity.Property(static e => e.PuntosFuturos).HasDefaultValue(0);
            entity.Property(static e => e.TieneSubsidio).HasDefaultValue(false);
            entity.Property(static e => e.TipoConsumo).HasDefaultValue(5);
            entity.Property(static e => e.ValorTotalCobros).HasDefaultValue(0.0);
            entity.Property(static e => e.ValorTotalDeuda).HasDefaultValue(0.0);
            entity.Property(static e => e.ValorTotalPagos).HasDefaultValue(0.0);
            entity.Property(static e => e.PUEAA_TanqueReservaAcueducto).HasDefaultValue(false);

            entity.HasOne(static d => d.EstratoNavigation).WithMany(static p => p.Predios)
                .HasForeignKey(static d => d.Estrato)
                .HasConstraintName("FK_Predio_Estrato");

            entity.HasOne(static d => d.IdentificacionFacturacionNavigation).WithMany(static p => p.Predios)
                .HasForeignKey(static d => d.IdentificacionFacturacion)
                .HasConstraintName("FK_Predio_Usuario");

            entity.HasOne(static d => d.TipoConsumoNavigation).WithMany(static p => p.Predios)
                .HasForeignKey(static d => d.TipoConsumo)
                .HasConstraintName("FK_Predio_TipoConsumo");
        });

        modelBuilder.Entity<Recaudo>(static entity =>
        {
            entity.HasKey(static e => e.Idrecaudo);

            entity.ToTable("Recaudo");

            entity.Property(static e => e.Idrecaudo)
                .HasMaxLength(50)
                .HasColumnName("IDRecaudo");
            entity.Property(static e => e.Concepto).HasMaxLength(200);
            entity.Property(static e => e.FechaHora).HasColumnType("datetime");
            entity.Property(static e => e.Idfactura)
                .HasMaxLength(50)
                .HasColumnName("IDFactura");
            entity.Property(static e => e.Idusuario)
                .HasMaxLength(50)
                .HasColumnName("IDUsuario");
            entity.Property(static e => e.Lugar).HasMaxLength(50);
        });

        modelBuilder.Entity<SerieDocumentacion>(static entity =>
        {
            entity.ToTable("SerieDocumentacion");

            entity.Property(static e => e.Descripcion)
                .HasMaxLength(500)
                .IsUnicode(false);
            entity.Property(static e => e.Nombre)
                .HasMaxLength(100)
                .IsUnicode(false);
        });

        modelBuilder.Entity<Solicitud>(static entity =>
        {
            entity.HasKey(static e => e.Id).HasName("PK_Solicitudes");

            entity.ToTable("Solicitud");

            entity.Property(static e => e.Codigo).HasMaxLength(50);
            entity.Property(static e => e.FechaHoraActuaMejora).HasColumnType("datetime");
            entity.Property(static e => e.FechaHoraCerrada).HasColumnType("datetime");
            entity.Property(static e => e.FechaHoraHacer).HasColumnType("datetime");
            entity.Property(static e => e.FechaHoraPlanear).HasColumnType("datetime");
            entity.Property(static e => e.FechaHoraRecepcion).HasColumnType("datetime");
            entity.Property(static e => e.FechaHoraVerifica).HasColumnType("datetime");
            entity.Property(static e => e.IdUsuario).HasMaxLength(50);

            entity.HasOne(static d => d.EstadoNavigation).WithMany(static p => p.Solicitudes)
                .HasForeignKey(static d => d.EstadoID)
                .HasConstraintName("FK_Solicitudes_Estado");

            entity.HasOne(static d => d.IdUsuarioNavigation).WithMany(static p => p.Solicitudes)
                .HasForeignKey(static d => d.IdUsuario)
                .HasConstraintName("FK_Solicitudes_Usuario");

            entity.HasOne(static d => d.TipoNavigation).WithMany(static p => p.Solicitudes)
                .HasForeignKey(static d => d.Tipo)
                .HasConstraintName("FK_Solicitudes_Tipo");
        });

        modelBuilder.Entity<Tarifa>(static entity =>
        {
            entity.HasKey(static e => e.Tipo);

            entity.Property(static e => e.Tipo).HasMaxLength(50);
        });

        modelBuilder.Entity<Tipo>(static entity =>
        {
            entity.ToTable("Tipo");

            // Id is generated by the database and should not be manually set
            entity.HasKey(static e => e.Id);
            entity.Property(static e => e.Id).ValueGeneratedOnAdd();

            entity.Property(static e => e.Nombre)
                .HasMaxLength(50)
                .IsRequired();
        });

        modelBuilder.Entity<TipoConsumo>(static entity =>
        {
            entity.ToTable("TipoConsumo");

            entity.Property(static e => e.Categoria).HasMaxLength(50);
            entity.Property(static e => e.Observaciones).HasMaxLength(500);
            entity.Property(static e => e.Rango).HasMaxLength(100);
        });

        modelBuilder.Entity<TipoDocumentacion>(static entity =>
        {
            entity.ToTable("TipoDocumentacion");

            entity.Property(static e => e.Descripcion).HasMaxLength(500);
            entity.Property(static e => e.Nombre).HasMaxLength(50);
        });

        modelBuilder.Entity<TiposTrabajo>(static entity =>
        {
            entity.HasKey(static e => e.IdTipoTrabajo).HasName("PK__TiposTra__8816D7FF65D6A6D6");

            entity.ToTable("TiposTrabajo");

            entity.Property(static e => e.Descripcion)
                .HasMaxLength(200)
                .IsUnicode(false);
            entity.Property(static e => e.Nombre)
                .HasMaxLength(50)
                .IsUnicode(false);
        });

        modelBuilder.Entity<Trabajo>(static entity =>
        {
            entity.ToTable("Trabajos"); // align with the actual table name in DB
    entity.HasKey(static e => e.IdTrabajo).HasName("PK__Trabajos__4FB29E3479D7D263");

    entity.Property(static e => e.Descripcion).HasMaxLength(200).IsUnicode(false);
    entity.Property(static e => e.Estado).HasMaxLength(50).IsUnicode(false);
    entity.Property(static e => e.FechaFinalizacion).HasColumnType("datetime");
    entity.Property(static e => e.FechaInicio).HasColumnType("datetime");

    entity.Property(static e => e.Idplanilla).HasMaxLength(50).HasColumnName("IDPlanilla");
    entity.Property(static e => e.IdPredio).HasMaxLength(50).HasColumnName("IDPredio");
    entity.Property(static e => e.IdInventario).HasMaxLength(50).HasColumnName("IDInventario");
    entity.Property(static e => e.Observaciones).HasColumnName("Observaciones");
    entity.Property(static e => e.Materiales).IsUnicode(false);
    entity.Property(static e => e.Personal).IsUnicode(false);
        });

        //modelBuilder.Entity<TrabajosMateriales>(entity =>
        //{
        //    entity.HasKey(e => new { e.IdTrabajo, e.IdMaterial }).HasName("PK__Trabajos__D6F1C8D130B7D9F2");

        //    //entity.HasOne(d => d.IdMaterialNavigation).WithMany(p => p.TrabajosMateriales)
        //    //    .HasForeignKey(d => d.IdMaterial)
        //    //    .OnDelete(DeleteBehavior.ClientSetNull)
        //    //    .HasConstraintName("FK__TrabajosM__IdMat__68336F3E");

        //    //entity.HasOne(d => d.IdTrabajoNavigation).WithMany(p => p.TrabajosMateriales)
        //    //    .HasForeignKey(d => d.IdTrabajo)
        //    //    .OnDelete(DeleteBehavior.ClientSetNull)
        //    //    .HasConstraintName("FK__TrabajosM__IdTra__05C3D225");
        //});

        //modelBuilder.Entity<TrabajosPersonal>(entity =>
        //{
        //    entity.HasKey(e => new { e.IdTrabajo, e.IdPersonal }).HasName("PK__Trabajos__EFE80C3507F61E4B");

        //    entity.ToTable("TrabajosPersonal");

        //    entity.Property(e => e.HorasTrabajadas).HasColumnType("decimal(10, 2)");

        //    //entity.HasOne(d => d.IdPersonalNavigation).WithMany(p => p.TrabajosPersonals)
        //    //    .HasForeignKey(d => d.IdPersonal)
        //    //    .OnDelete(DeleteBehavior.ClientSetNull)
        //    //    .HasConstraintName("FK__TrabajosP__IdPer__02E7657A");

        //    //entity.HasOne(d => d.IdTrabajoNavigation).WithMany(p => p.TrabajosPersonals)
        //    //    .HasForeignKey(d => d.IdTrabajo)
        //    //    .OnDelete(DeleteBehavior.ClientSetNull)
        //    //    .HasConstraintName("FK__TrabajosP__IdTra__01F34141");
        //});

        modelBuilder.Entity<Usuario>(static entity =>
        {
            entity.HasKey(static e => e.Identificacion);

            entity.ToTable("Usuario");

            entity.Property(static e => e.Identificacion).HasMaxLength(50);
            entity.Property(static e => e.ApellidosNombres).HasMaxLength(100);
            entity.Property(static e => e.Celular).HasMaxLength(50);
            entity.Property(static e => e.CorreoElectronico).HasMaxLength(100);
            entity.Property(static e => e.NivelEducativo).HasMaxLength(100);
        });

        modelBuilder.Entity<MenuPermission>(static entity =>
        {
            entity.ToTable("MenuPermission");
            entity.HasKey(static e => e.Id);
            entity.Property(static e => e.Id).HasMaxLength(50);
            entity.Property(static e => e.UserName).HasMaxLength(256);
            entity.Property(static e => e.Role).HasMaxLength(100);
            entity.Property(static e => e.MenuKey).HasMaxLength(100).IsRequired();
            entity.Property(static e => e.CanView).HasDefaultValue(true);
            entity.Property(static e => e.CanCreate).HasDefaultValue(false);
            entity.Property(static e => e.CanEdit).HasDefaultValue(false);
            entity.Property(static e => e.CanDelete).HasDefaultValue(false);
            entity.Property(static e => e.CreatedAt).HasColumnType("datetime");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
