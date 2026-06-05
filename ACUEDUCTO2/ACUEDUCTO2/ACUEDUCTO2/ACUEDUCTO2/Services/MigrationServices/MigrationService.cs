using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using ACUEDUCTO2.Data;
using ACUEDUCTO2.Share.Models;
using Microsoft.EntityFrameworkCore;

namespace ACUEDUCTO2.Services.MigrationServices
{
    public class MigrationService
    {
        private readonly IDbContextFactory<XcursoroDbContext> _srcFactory;
        private readonly IDbContextFactory<ApplicationDbContext> _destFactory;
        private readonly ILogger<MigrationService> _logger;

        public MigrationService(
            IDbContextFactory<XcursoroDbContext> srcFactory,
            IDbContextFactory<ApplicationDbContext> destFactory,
            ILogger<MigrationService> logger)
        {
            _srcFactory = srcFactory;
            _destFactory = destFactory;
            _logger = logger;
        }

        private static async Task<string> FindExistingTableAsync(DbConnection conn, CancellationToken cancellationToken, params string[] candidates)
        {
            var wasOpen = conn.State == ConnectionState.Open;
            if (!wasOpen) await conn.OpenAsync(cancellationToken);
            try
            {
                foreach (var name in candidates)
                {
                    using var cmd = conn.CreateCommand();
                    cmd.CommandText = "SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = @name";
                    var p = cmd.CreateParameter(); p.ParameterName = "@name"; p.Value = name; cmd.Parameters.Add(p);
                    var r = await cmd.ExecuteScalarAsync(cancellationToken);
                    if (r != null) return name;
                }
                return string.Empty;
            }
            finally
            {
                if (!wasOpen) await conn.CloseAsync();
            }
        }

        private static async Task<string> ListTablesAsync(DbConnection conn, CancellationToken cancellationToken)
        {
            var list = new List<string>();
            var wasOpen = conn.State == ConnectionState.Open;
            if (!wasOpen) await conn.OpenAsync(cancellationToken);
            try
            {
                using var cmd = conn.CreateCommand();
                cmd.CommandText = "SELECT TABLE_NAME FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_TYPE='BASE TABLE' ORDER BY TABLE_NAME";
                using var rdr = await cmd.ExecuteReaderAsync(cancellationToken);
                while (await rdr.ReadAsync(cancellationToken)) list.Add(rdr.GetString(0));
            }
            finally
            {
                if (!wasOpen) await conn.CloseAsync();
            }
            return string.Join(", ", list.Take(200));
        }

        public async Task<int> MigratePrediosAsync(CancellationToken cancellationToken = default)
        {
            await using var src = _srcFactory.CreateDbContext();
            await using var dest = _destFactory.CreateDbContext();

            var connection = src.Database.GetDbConnection();

            var tipoConsumoColumn = await FindFirstExistingPredioColumnAsync(connection, cancellationToken, "IdTipoConsumo", "TipoConsumo");
            var estratoColumn = await FindFirstExistingPredioColumnAsync(connection, cancellationToken, "IdEstrato", "Estrato");

            var tipoConsumoLookup = await ReadPredioIntColumnAsync(connection, tipoConsumoColumn, cancellationToken);
            var estratoLookup = await ReadPredioIntColumnAsync(connection, estratoColumn, cancellationToken);

            var srcPredios = await src.Predio.AsNoTracking().ToListAsync(cancellationToken);
            var added = 0;
            var updated = 0;

            foreach (var sp in srcPredios)
            {
                cancellationToken.ThrowIfCancellationRequested();

                var idTipoConsumo = ResolveFromLookup(tipoConsumoLookup, sp.IdPredio, sp.IdTipoConsumo);
                var idEstrato = ResolveFromLookup(estratoLookup, sp.IdPredio, sp.Estrato);

                // Avoid duplicates by primary key
                var exists = await dest.Predio.FindAsync(new object[] { sp.IdPredio }, cancellationToken);
                if (exists != null)
                {
                    var changed = false;
                    if (NeedsIdentificacionUpdate(exists, sp))
                    {
                        exists.IdentificacionFacturacion = sp.IdentificacionFacturacion;
                        changed = true;
                    }

                    if (idTipoConsumo != exists.IdTipoConsumo && idTipoConsumo.HasValue)
                    {
                        exists.IdTipoConsumo = idTipoConsumo;
                        changed = true;
                    }

                    if (idEstrato != exists.Estrato && idEstrato.HasValue)
                    {
                        exists.Estrato = idEstrato;
                        changed = true;
                    }

                    if (changed)
                    {
                        updated++;
                    }

                    continue;
                }

                // Simple shallow copy — adjust if models differ
                var np = new Predio
                {
                    IdPredio = sp.IdPredio,
                    ApellidosNombresUsuario = sp.ApellidosNombresUsuario,
                    CodigoUsuario = sp.CodigoUsuario,
                    IdentificacionFacturacion = sp.IdentificacionFacturacion,
                    Estrato = idEstrato,
                    Ruta = sp.Ruta,
                    Direccion = sp.Direccion,
                    NumeroCatastral = sp.NumeroCatastral,
                    InformacionCatastral = sp.InformacionCatastral,
                    ExtensionM2 = sp.ExtensionM2,
                    M2construidos = sp.M2construidos,
                    PuntosFuturos = sp.PuntosFuturos,
                    NumeroMedidor = sp.NumeroMedidor,
                    MarcaMedidor = sp.MarcaMedidor,
                    EstadoMedidor = sp.EstadoMedidor,
                    UltimaMedicion = sp.UltimaMedicion,
                    ConsumoM3promedio = sp.ConsumoM3promedio,
                    UltimoConsumoM3 = sp.UltimoConsumoM3,
                    ValorTotalCobros = sp.ValorTotalCobros,
                    CantidadCoutasCobros = sp.CantidadCoutasCobros,
                    CantidadCuotasPagadas = sp.CantidadCuotasPagadas,
                    ValorTotalPagos = sp.ValorTotalPagos,
                    ValorTotalDeuda = sp.ValorTotalDeuda,
                    TieneSubsidio = sp.TieneSubsidio,
                    ImprimeFactura = sp.ImprimeFactura,
                    EnviaWhatsapp = sp.EnviaWhatsapp,
                    EnviaCorreo = sp.EnviaCorreo,
                    ContadorEnSeguimiento = sp.ContadorEnSeguimiento,
                    Latitud = sp.Latitud,
                    Longitud = sp.Longitud,
                    Activo = sp.Activo,
                    FechaHoraRegistro = sp.FechaHoraRegistro,
                    IdTipoConsumo = idTipoConsumo,
                    PUEAA_TanqueReservaAcueducto = sp.PUEAA_TanqueReservaAcueducto,
                    PUEAA_TanqueReservaAcueductoObservaciones = sp.PUEAA_TanqueReservaAcueductoObservaciones,
                    PUEAA_TanqueReservaLluvia = sp.PUEAA_TanqueReservaLluvia,
                    PUEAA_TanqueReservaLluviaObservaciones = sp.PUEAA_TanqueReservaLluviaObservaciones,
                    PUEAA_ReciclajeAguas = sp.PUEAA_ReciclajeAguas,
                    PUEAA_ReciclajeAguasObservaciones = sp.PUEAA_ReciclajeAguasObservaciones,
                    PUEAA_RecirculacionAguas = sp.PUEAA_RecirculacionAguas,
                    PUEAA_RecirculacionAguasObservaciones = sp.PUEAA_RecirculacionAguasObservaciones,
                    PUEAA_ControlPerdidas = sp.PUEAA_ControlPerdidas,
                    PUEAA_ControlPerdidasObservaciones = sp.PUEAA_ControlPerdidasObservaciones,
                    PUEAA_MantenimientoPiscina = sp.PUEAA_MantenimientoPiscina,
                    PUEAA_MantenimientoPiscinaObservaciones = sp.PUEAA_MantenimientoPiscinaObservaciones,
                    PUEAA_OtrosProcesos = sp.PUEAA_OtrosProcesos,
                    PUEAA_OtrosProcesosObservaciones = sp.PUEAA_OtrosProcesosObservaciones,
                    Observaciones = sp.Observaciones
                };

                dest.Predio.Add(np);
                added++;
            }

            if (added > 0 || updated > 0)
                await dest.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Migrated {Added} predios and synced IdentificacionFacturacion on {Updated} registros", added, updated);
            return added + updated;
        }

        /// <summary>
        /// Reads Planilla rows from the v1 database via raw ADO.NET and writes
        /// them into the v2 Periodo table. No Planilla entity or table is
        /// created in the v2 database.
        /// </summary>
        public async Task<int> MigratePlanillasAsync(CancellationToken cancellationToken = default)
        {
            await using var src = _srcFactory.CreateDbContext();
            await using var dest = _destFactory.CreateDbContext();

            // Existing keys in v2 to avoid duplicates
            var existingKeys = (await dest.Periodos
                .AsNoTracking()
                .Select(p => p.Guid)
                .ToListAsync(cancellationToken))
                .ToHashSet(StringComparer.OrdinalIgnoreCase);

            // Read v1 Planilla via raw SQL — no entity needed
            var connection = src.Database.GetDbConnection();
            await connection.OpenAsync(cancellationToken);

            // Detectar si la tabla Planilla tiene las columnas legacy de registro de consumo
            var hasInicioCol = false;
            var hasFinCol = false;

            await using (var checkCmd = connection.CreateCommand())
            {
                checkCmd.CommandText = "SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[Planilla]') AND name = 'FechaHoraRegistroConsumo'";
                var r1 = await checkCmd.ExecuteScalarAsync(cancellationToken);
                hasInicioCol = r1 != null;
            }

            await using (var checkCmd2 = connection.CreateCommand())
            {
                checkCmd2.CommandText = "SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[Planilla]') AND name = 'FechaHoraRegistroConsumos'";
                var r2 = await checkCmd2.ExecuteScalarAsync(cancellationToken);
                hasFinCol = r2 != null;
            }

            // Construir SELECT dinámico incluyendo las columnas si existen
            var select = new System.Text.StringBuilder();
            select.Append("SELECT IDPlanilla, Descripcion, IDPeriodo, ");
            if (hasInicioCol)
                select.Append("FechaHoraRegistroConsumo, ");
            if (hasFinCol)
                select.Append("FechaHoraRegistroConsumos, ");
            select.Append("PorcentajeMora, PorcentajeSubsidio, EncargadoRegistroConsumos, ");
            select.Append("FechaHoraFacturacion, EncargadoFacturacion, UltimaFacturaRegistrada, ");
            select.Append("FechaLimitePago, FechaSuspension, TotalPredios, ");
            select.Append("CantidadConsumoM3Total, ValorTotalConsumoBasico, ValorTotalConsumoAdicional, ");
            select.Append("ValorTotalConsumoM3, ValorTotalMora, ValorTotalSobreTasa, ");
            select.Append("ValorTotalSubsidio, ValorTotalOtros, ValorTotalPlanilla, ");
            select.Append("ValorTotalRecaudo, Observaciones, Abierta FROM Planilla");

            await using var cmd = connection.CreateCommand();
            cmd.CommandText = select.ToString();

            await using var reader = await cmd.ExecuteReaderAsync(cancellationToken);
            var added = 0;
            var now = DateTime.UtcNow;

            // Next Orden value — start after the current max
            var maxOrden = existingKeys.Count > 0
                ? await dest.Periodos.AsNoTracking().MaxAsync(p => p.Orden, cancellationToken)
                : 0;

            while (await reader.ReadAsync(cancellationToken))
            {
                cancellationToken.ThrowIfCancellationRequested();

                var idPlanilla = reader.GetString(reader.GetOrdinal("IDPlanilla"));
                if (existingKeys.Contains(idPlanilla))
                    continue;

                maxOrden++;

                var fechaFact = ReadNullableDateTime(reader, "FechaHoraFacturacion") ?? now;
                var fechaLimite = ReadNullableDateTime(reader, "FechaLimitePago") ?? fechaFact;

                var periodo = new Periodo
                {
                    Guid = idPlanilla,

                    // Columnas NOT NULL propias de v2 — valores razonables
                    Orden = maxOrden,
                    NombreCortoPeriodo = ReadStringOrDefault(reader, "Descripcion"),
                    NombreLargoPeriodo = ReadStringOrDefault(reader, "Descripcion"),
                    // Sin columnas específicas de consumo en v1, usamos la fecha de facturación
                    // como inicio de periodo y de recaudo.
                    FechaHoraIniciaPeriodo = fechaFact,
                    FechaHoraTerminaPeriodo = fechaFact,
                    FechaHoraCreacionRegistro = now,
                    FechaHoraIniciaRecaudo = fechaFact,
                    FechaHoraTerminaRecaudo = fechaLimite,
                    UsuarioSistemaCreaRegistro = "Migración V1",

                    // Columnas heredadas de Planilla v1 (las que existen en esta versión)
                    Descripcion = ReadStringOrDefault(reader, "Descripcion"),
                    PeriodoReferenciaId = ReadNullableInt(reader, "IDPeriodo"),
                    // Asignar inicio/fin de registro de consumos desde Planilla v1 si están disponibles
                    FechaHoraIniciaRegistroConsumo = hasInicioCol ? ReadNullableDateTime(reader, "FechaHoraRegistroConsumo") : null,
                    PorcentajeMora = ReadNullableDouble(reader, "PorcentajeMora"),
                    PorcentajeSubsidio = ReadNullableDouble(reader, "PorcentajeSubsidio"),
                    FechaHoraTerminaRegistroConsumo = hasFinCol ? ReadNullableDateTime(reader, "FechaHoraRegistroConsumos") : null,
                    EncargadoRegistroConsumos = ReadStringOrDefault(reader, "EncargadoRegistroConsumos"),
                    FechaHoraFacturacion = fechaFact,
                    EncargadoFacturacion = ReadStringOrDefault(reader, "EncargadoFacturacion"),
                    UltimaFacturaRegistrada = ReadStringOrDefault(reader, "UltimaFacturaRegistrada"),
                    FechaLimitePago = fechaLimite,
                    FechaSuspension = ReadNullableDateTime(reader, "FechaSuspension"),
                    TotalPredios = ReadNullableInt(reader, "TotalPredios"),
                    CantidadConsumoM3Total = ReadNullableDouble(reader, "CantidadConsumoM3Total"),
                    ValorTotalConsumoBasico = ReadNullableDouble(reader, "ValorTotalConsumoBasico"),
                    ValorTotalConsumoAdicional = ReadNullableDouble(reader, "ValorTotalConsumoAdicional"),
                    ValorTotalConsumoM3 = ReadNullableDouble(reader, "ValorTotalConsumoM3"),
                    ValorTotalMora = ReadNullableDouble(reader, "ValorTotalMora"),
                    ValorTotalSobreTasa = ReadNullableDouble(reader, "ValorTotalSobreTasa"),
                    ValorTotalSubsidio = ReadNullableDouble(reader, "ValorTotalSubsidio"),
                    ValorTotalOtros = ReadNullableDouble(reader, "ValorTotalOtros"),
                    ValorTotalPlanilla = ReadNullableDouble(reader, "ValorTotalPlanilla"),
                    ValorTotalRecaudo = ReadNullableDouble(reader, "ValorTotalRecaudo"),
                    Observaciones = ReadStringOrDefault(reader, "Observaciones"),
                    Abierta = !reader.IsDBNull(reader.GetOrdinal("Abierta"))
                              && reader.GetBoolean(reader.GetOrdinal("Abierta"))
                };

                dest.Periodos.Add(periodo);
                added++;
            }

            if (added > 0)
                await dest.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Migrated {Count} planillas from v1 to v2 Periodo", added);
            return added;
        }

        /// <summary>
        /// Migra los registros de la tabla Facturacion de la base de datos v1
        /// hacia la tabla Facturacion de v2 (entidad Factura).
        /// No duplica facturas ya existentes en v2 (se identifican por IDFactura).
        /// </summary>
        public async Task<int> MigrateFacturacionAsync(CancellationToken cancellationToken = default)
        {
            await using var src = _srcFactory.CreateDbContext();
            await using var dest = _destFactory.CreateDbContext();

            // Claves existentes en v2 para evitar duplicados
            var existingKeys = (await dest.Facturacions
                    .AsNoTracking()
                    .Select(f => f.Guid)
                    .ToListAsync(cancellationToken))
                .ToHashSet(StringComparer.OrdinalIgnoreCase);

            var connection = src.Database.GetDbConnection();
            await connection.OpenAsync(cancellationToken);

            await using var cmd = connection.CreateCommand();
            cmd.CommandText = @"
                SELECT IDFactura, CodigoFactura, IDPlanilla, IDPredio, CodigoUsuario,
                       FechaHoraRegistro, MedicionAnterior, MedicionActual, ConsumoM3,
                       CargoFijo, ValorConsumo, PorcentajeSobreTasa, ValorSobreTasa,
                       PorcentajeMora, ValorMora, PorcentajeSubsidio, ValorSubsidio,
                       OtrosValores, ValorPeriodo, DeudaAnterior, RecaudoPeriodoTotal,
                       DeudaPasa, NotasFactura, NotasRecaudo
                FROM Facturacion";

            await using var reader = await cmd.ExecuteReaderAsync(cancellationToken);
            var added = 0;

            while (await reader.ReadAsync(cancellationToken))
            {
                cancellationToken.ThrowIfCancellationRequested();

                var idFactura = reader.GetString(reader.GetOrdinal("IDFactura"));
                if (existingKeys.Contains(idFactura))
                {
                    continue;
                }

                var factura = new Factura
                {
                    Guid = idFactura,
                    CodigoFactura = ReadStringOrDefault(reader, "CodigoFactura"),
                    IdPeriodo = ReadStringOrDefault(reader, "IDPlanilla"),
                    IdPredio = ReadStringOrDefault(reader, "IDPredio"),
                    CodigoUsuario = ReadStringOrDefault(reader, "CodigoUsuario"),
                    FechaHoraRegistro = ReadNullableDateTime(reader, "FechaHoraRegistro"),
                    MedicionAnterior = ReadNullableDouble(reader, "MedicionAnterior"),
                    MedicionActual = ReadNullableDouble(reader, "MedicionActual"),
                    ConsumoM3 = ReadNullableDouble(reader, "ConsumoM3"),
                    CargoFijo = ReadNullableDouble(reader, "CargoFijo"),
                    ValorConsumo = ReadNullableDouble(reader, "ValorConsumo"),
                    PorcentajeSobreTasa = ReadNullableDouble(reader, "PorcentajeSobreTasa"),
                    ValorSobreTasa = ReadNullableDouble(reader, "ValorSobreTasa"),
                    PorcentajeMora = ReadNullableDouble(reader, "PorcentajeMora"),
                    ValorMora = ReadNullableDouble(reader, "ValorMora"),
                    PorcentajeSubsidio = ReadNullableDouble(reader, "PorcentajeSubsidio"),
                    ValorSubsidio = ReadNullableDouble(reader, "ValorSubsidio"),
                    OtrosValores = ReadNullableDouble(reader, "OtrosValores"),
                    ValorPeriodo = ReadNullableDouble(reader, "ValorPeriodo"),
                    DeudaAnterior = ReadNullableDouble(reader, "DeudaAnterior"),
                    RecaudoPeriodoTotal = ReadNullableDouble(reader, "RecaudoPeriodoTotal"),
                    DeudaPasa = ReadNullableDouble(reader, "DeudaPasa"),
                    NotaDeFactura = ReadStringOrDefault(reader, "NotasFactura"),
                    NotaDeRecaudo = ReadStringOrDefault(reader, "NotasRecaudo")
                };

                dest.Facturacions.Add(factura);
                added++;
            }

            if (added > 0)
            {
                await dest.SaveChangesAsync(cancellationToken);
            }

            _logger.LogInformation("Migrated {Count} facturas from v1 Facturacion to v2 Facturacion", added);
            return added;
        }

        /// <summary>
        /// Migra los trabajos y sus tablas relacionadas (TiposTrabajos, TrabajosEquipos,
        /// TrabajosMateriales, TrabajosPersonal) desde la BD v1 a v2.
        /// Evita duplicados usando el IdTrabajo de la fuente.
        /// </summary>
        public async Task<int> MigrateTrabajosAsync(CancellationToken cancellationToken = default)
        {
            await using var src = _srcFactory.CreateDbContext();
            await using var dest = _destFactory.CreateDbContext();

            // Ensure destination tables exist by executing the provided SQL script if available.
            await EnsureTrabajoTablesExistAsync(dest, cancellationToken);

            // Evitar duplicados por IdTrabajo
            var existingIds = (await dest.Trabajo.AsNoTracking().Select(t => t.IdTrabajo).ToListAsync(cancellationToken)).ToHashSet();

            var conn = src.Database.GetDbConnection();
            await conn.OpenAsync(cancellationToken);
            var addedTrabajos = 0;
            var addedEquipos = 0;
            var addedMateriales = 0;
            var addedPersonal = 0;

            try
            {
                // Leer trabajos desde v1
                // Buscar el nombre real de la tabla en la BD origen para soportar variaciones
                var trabajosTable = await FindExistingTableAsync(conn, cancellationToken, "TRABAJOS", "TRABAJO", "Trabajo", "Trabajos", "TRABAJOS_V1", "TRABAJO_V1");
                if (string.IsNullOrWhiteSpace(trabajosTable))
                {
                    // Listar tablas detectadas para diagnóstico
                    var available = await ListTablesAsync(conn, cancellationToken);
                    throw new InvalidOperationException($"No se encontró la tabla de trabajos en la base origen. Tablas disponibles: {available}");
                }

                using var cmd = conn.CreateCommand();
                cmd.CommandText = $"SELECT IdTrabajo, Descripcion, TipoTrabajo, Estado, Personal, FechaInicio, FechaFinalizacion, IdPredio FROM [{trabajosTable}]";

                using var reader = await cmd.ExecuteReaderAsync(cancellationToken);
                var trabajosToInsert = new List<Trabajos>();
                var trabajoIds = new List<int>();

                while (await reader.ReadAsync(cancellationToken))
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    var id = reader.IsDBNull(0) ? 0 : reader.GetInt32(0);
                    if (id == 0) continue;
                    if (existingIds.Contains(id)) continue;

                    var descripcion = reader.IsDBNull(1) ? string.Empty : reader.GetString(1);
                    var tipoTrabajo = reader.IsDBNull(2) ? 0 : reader.GetInt32(2);
                    var estado = reader.IsDBNull(3) ? string.Empty : reader.GetString(3);
                    var personal = reader.IsDBNull(4) ? string.Empty : reader.GetString(4);
                    DateTime fechaInicio = DateTime.MinValue;
                    DateTime fechaFin = DateTime.MinValue;
                    if (!reader.IsDBNull(5)) fechaInicio = reader.GetFieldValue<DateTime>(5);
                    if (!reader.IsDBNull(6)) fechaFin = reader.GetFieldValue<DateTime>(6);
                    var idPredio = reader.IsDBNull(7) ? string.Empty : reader.GetString(7);

                    var t = new Trabajos
                    {
                        IdTrabajo = id,
                        Descripcion = descripcion,
                        TipoTrabajo = tipoTrabajo,
                        Estado = estado,
                        Personal = personal,
                        // Use MinValue as default when source has no date; destination model requires non-null
                        FechaInicio = fechaInicio == DateTime.MinValue ? DateTime.MinValue : fechaInicio,
                        FechaFinalizacion = fechaFin == DateTime.MinValue ? DateTime.MinValue : fechaFin,
                        IdPredio = idPredio
                    };

                    trabajosToInsert.Add(t);
                    trabajoIds.Add(id);
                }

                if (trabajosToInsert.Count > 0)
                {
                    // Insertar trabajos
                    dest.Trabajo.AddRange(trabajosToInsert);
                    await dest.SaveChangesAsync(cancellationToken);
                    addedTrabajos = trabajosToInsert.Count;
                }

                // Insertar equipos relacionados
                if (trabajoIds.Count > 0)
                {
                    var equiposTable = await FindExistingTableAsync(conn, cancellationToken, "TRABAJOS_EQUIPOS", "TRABAJOS_EQUIPO", "TrabajosEquipos", "TRABAJOS_EQUIPOS_V1");
                    if (!string.IsNullOrWhiteSpace(equiposTable))
                    {
                        using var cmdEq = conn.CreateCommand();
                        cmdEq.CommandText = $"SELECT Id, IdTrabajo, Equipo, Cantidad FROM [{equiposTable}] WHERE IdTrabajo IN ({string.Join(',', trabajoIds)})";
                    using var rdrEq = await cmdEq.ExecuteReaderAsync(cancellationToken);
                    var equipos = new List<TrabajoEquipo>();
                    while (await rdrEq.ReadAsync(cancellationToken))
                    {
                        var idTrabajo = rdrEq.IsDBNull(1) ? 0 : rdrEq.GetInt32(1);
                        if (idTrabajo == 0) continue;
                        var equipo = rdrEq.IsDBNull(2) ? string.Empty : rdrEq.GetString(2);
                        var cantidad = rdrEq.IsDBNull(3) ? (int?)null : rdrEq.GetInt32(3);
                        equipos.Add(new TrabajoEquipo { IdTrabajo = idTrabajo, Equipo = equipo, Cantidad = cantidad });
                    }
                    if (equipos.Count > 0)
                    {
                        dest.TrabajosEquipos.AddRange(equipos);
                        await dest.SaveChangesAsync(cancellationToken);
                        addedEquipos = equipos.Count;
                    }
                    }

                }

                // Insertar materiales relacionados
                if (trabajoIds.Count > 0)
                {
                    var materialesTable = await FindExistingTableAsync(conn, cancellationToken, "TRABAJOS_MATERIALES", "TRABAJOS_MATERIAL", "TrabajosMateriales", "TRABAJOS_MATERIALES_V1");
                    if (!string.IsNullOrWhiteSpace(materialesTable))
                    {
                        using var cmdMat = conn.CreateCommand();
                        cmdMat.CommandText = $"SELECT Id, IdTrabajo, Material, Cantidad FROM [{materialesTable}] WHERE IdTrabajo IN ({string.Join(',', trabajoIds)})";
                        using var rdrMat = await cmdMat.ExecuteReaderAsync(cancellationToken);
                        var materiales = new List<TrabajoMaterial>();
                        while (await rdrMat.ReadAsync(cancellationToken))
                        {
                            var idTrabajo = rdrMat.IsDBNull(1) ? 0 : rdrMat.GetInt32(1);
                            if (idTrabajo == 0) continue;
                            var material = rdrMat.IsDBNull(2) ? string.Empty : rdrMat.GetString(2);
                            var cantidad = rdrMat.IsDBNull(3) ? (double?)null : rdrMat.GetDouble(3);
                            materiales.Add(new TrabajoMaterial { IdTrabajo = idTrabajo, Material = material, Cantidad = cantidad });
                        }
                        if (materiales.Count > 0)
                        {
                            dest.TrabajosMateriales.AddRange(materiales);
                            await dest.SaveChangesAsync(cancellationToken);
                            addedMateriales = materiales.Count;
                        }
                    }
                }

                // Insertar personal relacionado
                if (trabajoIds.Count > 0)
                {
                    var personalTable = await FindExistingTableAsync(conn, cancellationToken, "TRABAJOS_PERSONAL", "TRABAJOS_PERSONALS", "TrabajosPersonal", "TRABAJOS_PERSONAL_V1");
                    if (!string.IsNullOrWhiteSpace(personalTable))
                    {
                        using var cmdPer = conn.CreateCommand();
                        cmdPer.CommandText = $"SELECT Id, IdTrabajo, PersonalNombre, Rol FROM [{personalTable}] WHERE IdTrabajo IN ({string.Join(',', trabajoIds)})";
                        using var rdrPer = await cmdPer.ExecuteReaderAsync(cancellationToken);
                        var personals = new List<TrabajoPersonal>();
                        while (await rdrPer.ReadAsync(cancellationToken))
                        {
                            var idTrabajo = rdrPer.IsDBNull(1) ? 0 : rdrPer.GetInt32(1);
                            if (idTrabajo == 0) continue;
                            var nombre = rdrPer.IsDBNull(2) ? string.Empty : rdrPer.GetString(2);
                            var rol = rdrPer.IsDBNull(3) ? string.Empty : rdrPer.GetString(3);
                            personals.Add(new TrabajoPersonal { IdTrabajo = idTrabajo, PersonalNombre = nombre, Rol = rol });
                        }
                        if (personals.Count > 0)
                        {
                            dest.TrabajosPersonales.AddRange(personals);
                            await dest.SaveChangesAsync(cancellationToken);
                            addedPersonal = personals.Count;
                        }
                    }
                }
            }
            finally
            {
                await conn.CloseAsync();
            }

            _logger.LogInformation("Migrated trabajos: {T}, equipos: {E}, materiales: {M}, personal: {P}", addedTrabajos, addedEquipos, addedMateriales, addedPersonal);
            return addedTrabajos + addedEquipos + addedMateriales + addedPersonal;
        }

        /// <summary>
        /// Migra consumos v2 a partir de los datos de Facturacion v1.
        /// Crea un registro en Consumo por cada factura que aún no tenga consumo asociado,
        /// usando el IDFactura de v1 como IdFactura en v2.
        /// </summary>
        public async Task<int> MigrateConsumosAsync(CancellationToken cancellationToken = default)
        {
            await using var src = _srcFactory.CreateDbContext();
            await using var dest = _destFactory.CreateDbContext();

            // Cargar consumos existentes indexados por IdFactura para poder
            // actualizar registros que ya existen (por ejemplo, con valores en 0)
            var existingByFactura = await dest.Consumos
                .Where(c => !string.IsNullOrWhiteSpace(c.IdFactura))
                .ToDictionaryAsync(c => c.IdFactura!, c => c, StringComparer.OrdinalIgnoreCase, cancellationToken);

            var connection = src.Database.GetDbConnection();
            await connection.OpenAsync(cancellationToken);

            await using var cmd = connection.CreateCommand();
            cmd.CommandText = @"
                SELECT IDFactura,
                       MedicionAnterior,
                       MedicionActual,
                       ConsumoM3,
                       NotasFactura,
                       FechaHoraRegistro
                FROM Facturacion";

            await using var reader = await cmd.ExecuteReaderAsync(cancellationToken);
            var added = 0;
            var updated = 0;
            var now = DateTime.UtcNow;

            while (await reader.ReadAsync(cancellationToken))
            {
                cancellationToken.ThrowIfCancellationRequested();

                var idFactura = reader.GetString(reader.GetOrdinal("IDFactura"));

                // En v1 estos campos suelen ser numéricos (double/decimal). Leemos como double
                // y convertimos a int para el modelo de Consumo v2.
                var medAnteriorV1 = ReadNullableDouble(reader, "MedicionAnterior") ?? 0d;
                var medActualV1 = ReadNullableDouble(reader, "MedicionActual") ?? 0d;
                var consumoV1 = ReadNullableDouble(reader, "ConsumoM3") ?? 0d;

                var medAntInt = (int)Math.Round(medAnteriorV1, MidpointRounding.AwayFromZero);
                var medActInt = (int)Math.Round(medActualV1, MidpointRounding.AwayFromZero);
                var consInt = (int)Math.Round(consumoV1, MidpointRounding.AwayFromZero);

                var isNew = false;
                if (!existingByFactura.TryGetValue(idFactura, out var consumo))
                {
                    consumo = new Consumo
                    {
                        Guid = idFactura,
                        IdFactura = idFactura
                    };

                    dest.Consumos.Add(consumo);
                    existingByFactura[idFactura] = consumo;
                    isNew = true;
                    added++;
                }

                consumo.MedicionAntereiorM3 = medAntInt;
                consumo.MedicionActualM3 = medActInt;
                consumo.M3Consumidos = consInt;
                consumo.EnSeguimiento = false;
                consumo.Observaciones = ReadStringOrDefault(reader, "NotasFactura");
                consumo.FechaHoraCreacionRegistro = ReadNullableDateTime(reader, "FechaHoraRegistro") ?? now;
                consumo.UsuarioSistemaCreaRegistro = "Migracion V1 a V2";

                if (!isNew)
                {
                    updated++;
                }
            }

            if (added > 0 || updated > 0)
            {
                await dest.SaveChangesAsync(cancellationToken);
            }

            _logger.LogInformation("Migrated {Added} new consumos and updated {Updated} existing consumos from v1 Facturacion to v2 Consumo", added, updated);
            return added + updated;
        }

        private static string ReadStringOrDefault(DbDataReader r, string col)
        {
            var i = r.GetOrdinal(col);
            return r.IsDBNull(i) ? string.Empty : r.GetString(i);
        }

        private static async Task<string?> FindFirstExistingPredioColumnAsync(DbConnection connection, CancellationToken cancellationToken, params string[] candidateNames)
        {
            var wasOpen = connection.State == ConnectionState.Open;
            if (!wasOpen)
            {
                await connection.OpenAsync(cancellationToken);
            }

            try
            {
                foreach (var name in candidateNames)
                {
                    await using var cmd = connection.CreateCommand();
                    cmd.CommandText = @"SELECT TOP (1) 1
                                           FROM sys.columns c
                                           INNER JOIN sys.tables t ON c.object_id = t.object_id
                                           INNER JOIN sys.schemas s ON t.schema_id = s.schema_id
                                           WHERE t.name = 'Predio'
                                             AND s.name = 'dbo'
                                             AND c.name = @name";
                    var p = cmd.CreateParameter();
                    p.ParameterName = "@name";
                    p.Value = name;
                    cmd.Parameters.Add(p);

                    var exists = await cmd.ExecuteScalarAsync(cancellationToken);
                    if (exists != null)
                    {
                        return name;
                    }
                }

                return null;
            }
            finally
            {
                if (!wasOpen)
                {
                    await connection.CloseAsync();
                }
            }
        }

        private static int? ResolveFromLookup(Dictionary<string, int?> lookup, string key, int? fallback)
        {
            if (lookup.TryGetValue(key, out var value) && value.HasValue)
            {
                return value;
            }

            return fallback;
        }

        private static async Task<Dictionary<string, int?>> ReadPredioIntColumnAsync(DbConnection connection, string columnName, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(columnName))
            {
                return new Dictionary<string, int?>(StringComparer.OrdinalIgnoreCase);
            }

            var result = new Dictionary<string, int?>(StringComparer.OrdinalIgnoreCase);

            var wasOpen = connection.State == ConnectionState.Open;
            if (!wasOpen)
            {
                await connection.OpenAsync(cancellationToken);
            }

            await using (var cmd = connection.CreateCommand())
            {
                cmd.CommandText = $"SELECT [IdPredio], TRY_CONVERT(int, [{columnName}]) AS Value FROM [Predio]";

                await using var reader = await cmd.ExecuteReaderAsync(cancellationToken);
                while (await reader.ReadAsync(cancellationToken))
                {
                    var idPredio = reader.GetString(0);
                    int? value = reader.IsDBNull(1) ? null : reader.GetInt32(1);
                    result[idPredio] = value;
                }
            }

            if (!wasOpen)
            {
                await connection.CloseAsync();
            }

            return result;
        }

        private static int? ReadNullableInt(DbDataReader r, string col)
        {
            var i = r.GetOrdinal(col);
            return r.IsDBNull(i) ? null : r.GetInt32(i);
        }

        private static double? ReadNullableDouble(DbDataReader r, string col)
        {
            var i = r.GetOrdinal(col);
            if (r.IsDBNull(i))
                return null;

            // En v1 las columnas numéricas pueden ser float, decimal, numeric, money, int, etc.
            // GetDouble() solo funciona con float; usamos Convert.ToDouble para cualquier tipo.
            return Convert.ToDouble(r.GetValue(i));
        }

        private static DateTime? ReadNullableDateTime(DbDataReader r, string col)
        {
            var i = r.GetOrdinal(col);
            return r.IsDBNull(i) ? null : r.GetDateTime(i);
        }

        // Attempt to locate the CreateTrabajoTables.sql script in likely locations.
        private static string? FindScriptFile(string fileName = "CreateTrabajoTables.sql")
        {
            // Check base directory and current directory and walk up a few levels
            var candidates = new[]
            {
                AppContext.BaseDirectory,
                Environment.CurrentDirectory,
                Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) ?? string.Empty
            };

            foreach (var baseDir in candidates.Where(d => !string.IsNullOrWhiteSpace(d)))
            {
                var path = Path.Combine(baseDir, "Scripts", fileName);
                if (File.Exists(path)) return path;

                path = Path.Combine(baseDir, fileName);
                if (File.Exists(path)) return path;

                // walk up to 4 levels looking for the file
                var dir = new DirectoryInfo(baseDir);
                for (var i = 0; i < 4 && dir.Parent != null; i++)
                {
                    dir = dir.Parent;
                    path = Path.Combine(dir.FullName, "Scripts", fileName);
                    if (File.Exists(path)) return path;
                    path = Path.Combine(dir.FullName, fileName);
                    if (File.Exists(path)) return path;
                }
            }

            return null;
        }

        private static async Task ExecuteSqlScriptAsync(DbConnection conn, string script, CancellationToken cancellationToken)
        {
            // Split batches by lines containing only GO (case-insensitive)
            var batches = Regex.Split(script, @"^\s*GO\s*$", RegexOptions.Multiline | RegexOptions.IgnoreCase);

            var wasOpen = conn.State == ConnectionState.Open;
            if (!wasOpen) await conn.OpenAsync(cancellationToken);
            try
            {
                foreach (var batch in batches)
                {
                    var trimmed = batch.Trim();
                    if (string.IsNullOrWhiteSpace(trimmed)) continue;
                    using var cmd = conn.CreateCommand();
                    cmd.CommandText = trimmed;
                    cmd.CommandType = CommandType.Text;
                    await cmd.ExecuteNonQueryAsync(cancellationToken);
                }
            }
            finally
            {
                if (!wasOpen) await conn.CloseAsync();
            }
        }

        private async Task EnsureTrabajoTablesExistAsync(DbContext destContext, CancellationToken cancellationToken)
        {
            try
            {
                // Check if at least one Trabajo-related table exists in destination
                var conn = destContext.Database.GetDbConnection();
                var wasOpen = conn.State == ConnectionState.Open;
                if (!wasOpen) await conn.OpenAsync(cancellationToken);
                try
                {
                    using var check = conn.CreateCommand();
                    check.CommandText = "SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME IN ('TRABAJOS','Trabajos','Trabajo','TiposTrabajos','TiposTrabajo')";
                    var r = await check.ExecuteScalarAsync(cancellationToken);
                    if (r != null)
                    {
                        _logger.LogDebug("Trabajo tables already exist in destination database.");
                        return;
                    }
                }
                finally
                {
                    if (!wasOpen) await conn.CloseAsync();
                }

                // Not found: try to locate and run the SQL script
                var scriptPath = FindScriptFile();
                if (scriptPath == null)
                {
                    _logger.LogWarning("Trabajo tables not found and SQL script 'CreateTrabajoTables.sql' was not located. Skipping automatic creation.");
                    return;
                }

                _logger.LogInformation("Creating Trabajo tables in destination using script: {Script}", scriptPath);
                var script = await File.ReadAllTextAsync(scriptPath, cancellationToken);

                // Execute script against destination DB
                var destConn = destContext.Database.GetDbConnection();
                await ExecuteSqlScriptAsync(destConn, script, cancellationToken);
                _logger.LogInformation("Executed Trabajo tables creation script successfully.");
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to ensure Trabajo tables exist in destination database.");
            }
        }

        public async Task MigrateAllAsync(CancellationToken cancellationToken = default)
        {
            // Add other entity migrations here as needed
            await MigratePrediosAsync(cancellationToken);
            await MigratePlanillasAsync(cancellationToken);
        }

        private static bool NeedsIdentificacionUpdate(Predio destino, Predio origen)
        {
            var destinoId = destino?.IdentificacionFacturacion;
            var origenId = origen?.IdentificacionFacturacion;

            if (string.IsNullOrWhiteSpace(origenId))
            {
                return false;
            }

            if (string.IsNullOrWhiteSpace(destinoId))
            {
                return true;
            }

            return !string.Equals(destinoId, origenId, StringComparison.OrdinalIgnoreCase);
        }
    }
}
