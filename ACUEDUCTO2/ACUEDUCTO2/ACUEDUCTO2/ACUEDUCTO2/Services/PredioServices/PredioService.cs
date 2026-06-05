using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ACUEDUCTO2.Data;
using ACUEDUCTO2.Share.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;

namespace ACUEDUCTO2.Services.PredioServices
{
    /// <summary>
    /// Operaciones de acceso a datos relacionadas con la entidad Predio.
    /// </summary>
    public sealed class PredioService
    {
        private readonly IDbContextFactory<ApplicationDbContext> _dbFactory;
        private readonly ILogger<PredioService> _logger;

        public PredioService(IDbContextFactory<ApplicationDbContext> dbFactory, ILogger<PredioService> logger)
        {
            _dbFactory = dbFactory;
            _logger = logger;
        }

        /// <summary>
        /// Obtiene la lista de predios proyectada para vistas de listado con filtro opcional.
        /// </summary>
        public async Task<List<PredioListItem>> GetListingAsync(string? filter, CancellationToken ct = default)
        {
            var normalizedFilter = filter?.Trim();

            await using var db = await _dbFactory.CreateDbContextAsync(ct);

            _logger.LogDebug("Executing GetListingAsync filter={Filter}", normalizedFilter);

            try
            {
                var query = db.Predio.AsNoTracking();

                if (!string.IsNullOrWhiteSpace(normalizedFilter))
                {
                    query = query.Where(p =>
                        (p.ApellidosNombresUsuario ?? string.Empty).Contains(normalizedFilter) ||
                        (p.Direccion ?? string.Empty).Contains(normalizedFilter) ||
                        (p.NumeroCatastral ?? string.Empty).Contains(normalizedFilter) ||
                        (p.InformacionCatastral ?? string.Empty).Contains(normalizedFilter) ||
                        (p.Observaciones ?? string.Empty).Contains(normalizedFilter) ||
                        (p.Ruta != null && p.Ruta.ToString()!.Contains(normalizedFilter)));
                }

                var items = await query
                    .OrderBy(p => p.ApellidosNombresUsuario ?? string.Empty)
                    .ThenBy(p => p.Direccion ?? string.Empty)
                    .Select(p => new global::ACUEDUCTO2.Share.Models.PredioListItem
                    {
                        IdPredio = p.IdPredio,
                        ApellidosNombresUsuario = p.ApellidosNombresUsuario,
                        Ruta = p.Ruta,
                        Direccion = p.Direccion,
                        ContadorEnSeguimiento = p.ContadorEnSeguimiento,
                        Activo = p.Activo,
                        // Fill TipoConsumoCategoria later to avoid relying on EF navigation joins which may fail if schema is out of sync
                        TipoConsumoCategoria = null,
                        Observaciones = p.Observaciones,
                        IdTipoConsumo = p.IdTipoConsumo
                    })
                    .ToListAsync(ct);

                // Resolve TipoConsumo categories in memory to avoid SQL join errors when DB schema mismatches model
                var tipoIds = items.Where(i => i.IdTipoConsumo.HasValue).Select(i => i.IdTipoConsumo!.Value).Distinct().ToList();
                if (tipoIds.Any())
                {
                    var tipos = await db.TipoConsumo.Where(tc => tipoIds.Contains(tc.Id)).AsNoTracking().ToListAsync(ct);
                    var mapa = tipos.ToDictionary(t => t.Id, t => t.Categoria);
                    foreach (var it in items)
                    {
                        if (it.IdTipoConsumo.HasValue && mapa.TryGetValue(it.IdTipoConsumo.Value, out var cat))
                        {
                            it.TipoConsumoCategoria = cat;
                        }
                    }
                }

                return items;
            }
            catch (SqlException ex)
            {
                _logger.LogError(ex, "SQL error while retrieving Predio listing (filter={Filter}). Returning empty list.", normalizedFilter);
                return new List<PredioListItem>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error while retrieving Predio listing (filter={Filter}). Returning empty list.", normalizedFilter);
                return new List<PredioListItem>();
            }
        }

        /// <summary>
        /// Obtiene los predios que un usuario puede ver: sus propios predios más los
        /// autorizados por otros usuarios a través de la tabla Autorizado.
        /// </summary>
        public async Task<List<PredioListItem>> GetListingForAuthorizedUserAsync(string identificacion, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(identificacion))
            {
                return new List<PredioListItem>();
            }

            await using var db = await _dbFactory.CreateDbContextAsync(ct);

            try
            {
                // Identificaciones de quienes autorizaron a este usuario
                var idsAutorizantes = await db.Set<Autorizado>()
                    .AsNoTracking()
                    .Where(a => a.AQueAutoriza == identificacion)
                    .Select(a => a.QuienAutoriza)
                    .Distinct()
                    .ToListAsync(ct);

                // Incluir la propia identificación del usuario
                var idsPermitidos = new HashSet<string>(idsAutorizantes.Where(id => !string.IsNullOrWhiteSpace(id))!, StringComparer.OrdinalIgnoreCase);
                idsPermitidos.Add(identificacion);

                var items = await db.Predio
                    .AsNoTracking()
                    .Where(p => idsPermitidos.Contains(p.IdentificacionFacturacion!))
                    .OrderBy(p => p.ApellidosNombresUsuario ?? string.Empty)
                    .ThenBy(p => p.Direccion ?? string.Empty)
                    .Select(p => new global::ACUEDUCTO2.Share.Models.PredioListItem
                    {
                        IdPredio = p.IdPredio,
                        ApellidosNombresUsuario = p.ApellidosNombresUsuario,
                        Ruta = p.Ruta,
                        Direccion = p.Direccion,
                        ContadorEnSeguimiento = p.ContadorEnSeguimiento,
                        Activo = p.Activo,
                        TipoConsumoCategoria = null,
                        Observaciones = p.Observaciones,
                        IdTipoConsumo = p.IdTipoConsumo
                    })
                    .ToListAsync(ct);

                var tipoIds = items.Where(i => i.IdTipoConsumo.HasValue).Select(i => i.IdTipoConsumo!.Value).Distinct().ToList();
                if (tipoIds.Any())
                {
                    var tipos = await db.TipoConsumo.Where(tc => tipoIds.Contains(tc.Id)).AsNoTracking().ToListAsync(ct);
                    var mapa = tipos.ToDictionary(t => t.Id, t => t.Categoria);
                    foreach (var it in items)
                    {
                        if (it.IdTipoConsumo.HasValue && mapa.TryGetValue(it.IdTipoConsumo.Value, out var cat))
                        {
                            it.TipoConsumoCategoria = cat;
                        }
                    }
                }

                return items;
            }
            catch (SqlException ex)
            {
                _logger.LogError(ex, "SQL error while retrieving Predio listing for authorized user (identificacion={Identificacion}). Returning empty list.", identificacion);
                return new List<PredioListItem>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error while retrieving Predio listing for authorized user (identificacion={Identificacion}). Returning empty list.", identificacion);
                return new List<PredioListItem>();
            }
        }

        /// <summary>
        /// Obtiene un predio por identificador con las relaciones requeridas.
        /// </summary>
        public async Task<Predio?> GetByIdAsync(
            string id,
            bool includeTipoConsumo = false,
            bool includeUsuarioFacturacion = false,
            CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                return null;
            }

            await using var db = await _dbFactory.CreateDbContextAsync(ct);
            IQueryable<Predio> query = db.Predio;

            if (includeTipoConsumo)
            {
                query = query.Include(p => p.TipoConsumoNavigation);
            }

            // Navigation to usuario is resolved separately; no include to avoid cross-assembly dependency.

            return await query.AsNoTracking().FirstOrDefaultAsync(p => p.IdPredio == id, ct);
        }

        /// <summary>
        /// Verifies whether the referenced usuario exists before saving changes.
        /// </summary>
        public async Task<bool> UsuarioExistsAsync(string? identificacion, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(identificacion))
            {
                return false;
            }

            await using var db = await _dbFactory.CreateDbContextAsync(ct);
            return await db.Users
                .AsNoTracking()
                .AnyAsync(u => u.Identificacion == identificacion, ct);
        }

        /// <summary>
        /// Lista usuarios disponibles para facturación usando su identificación como clave.
        /// </summary>
        public async Task<List<UsuarioLookupItem>> GetUsuariosFacturacionAsync(string? filter = null, CancellationToken ct = default)
        {
            await using var db = await _dbFactory.CreateDbContextAsync(ct);

            var query = db.Users
                .AsNoTracking();

            if (!string.IsNullOrWhiteSpace(filter))
            {
                var normalized = filter.Trim();
                query = query.Where(u =>
                    u.Identificacion.Contains(normalized) ||
                    (u.ApellidosNombres ?? string.Empty).Contains(normalized));
            }

            return await query
                .OrderBy(u => u.ApellidosNombres ?? u.Identificacion)
                .Select(u => new UsuarioLookupItem
                {
                    Identificacion = u.Identificacion,
                    Nombre = u.ApellidosNombres
                })
                .ToListAsync(ct);
        }

        /// <summary>
        /// Crea un nuevo registro de predio.
        /// </summary>
        public async Task CreateAsync(Predio predio, CancellationToken ct = default)
        {
            ArgumentNullException.ThrowIfNull(predio);

            await using var db = await _dbFactory.CreateDbContextAsync(ct);
            db.Predio.Add(predio);
            await db.SaveChangesAsync(ct);
        }

        /// <summary>
        /// Actualiza un predio existente.
        /// </summary>
        public async Task UpdateAsync(Predio predio, CancellationToken ct = default)
        {
            ArgumentNullException.ThrowIfNull(predio);

            await using var db = await _dbFactory.CreateDbContextAsync(ct);
            db.Predio.Update(predio);
            try
            {
                await db.SaveChangesAsync(ct);
            }
            catch (DbUpdateConcurrencyException ex)
            {
                _logger.LogError(ex, "Conflicto de concurrencia al actualizar el predio {PredioId}", predio.IdPredio);
                throw new InvalidOperationException("Conflicto de concurrencia al actualizar el predio. Intente de nuevo.", ex);
            }
        }

        /// <summary>
        /// Elimina un predio por identificador.
        /// </summary>
        public async Task DeleteAsync(string id, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                return;
            }

            await using var db = await _dbFactory.CreateDbContextAsync(ct);
            var entity = await db.Predio.FindAsync(new object[] { id }, ct);
            if (entity is null)
            {
                return;
            }

            db.Predio.Remove(entity);
            await db.SaveChangesAsync(ct);
        }

        /// <summary>
        /// Devuelve el catálogo de tipos de consumo ordenado alfabéticamente.
        /// </summary>
        public async Task<List<TipoConsumo>> GetTipoConsumoAsync(CancellationToken ct = default)
        {
            await using var db = await _dbFactory.CreateDbContextAsync(ct);
            return await db.TipoConsumo
                .AsNoTracking()
                .OrderBy(tc => tc.Categoria)
                .ToListAsync(ct);
        }

        /// <summary>
        /// Obtiene los datos necesarios para el panel general de PUEAA.
        /// </summary>
        public async Task<List<PueaaSnapshot>> GetPueaaSnapshotAsync(CancellationToken ct = default)
        {
            await using var db = await _dbFactory.CreateDbContextAsync(ct);
            return await db.Predio
                .AsNoTracking()
                .Select(p => new PueaaSnapshot
                {
                    Id = p.IdPredio,
                    Nombre = p.ApellidosNombresUsuario,
                    TanqueReservaAcueducto = p.PUEAA_TanqueReservaAcueducto,
                    TanqueReservaAcueductoObservaciones = p.PUEAA_TanqueReservaAcueductoObservaciones,
                    TanqueReservaLluvia = p.PUEAA_TanqueReservaLluvia,
                    TanqueReservaLluviaObservaciones = p.PUEAA_TanqueReservaLluviaObservaciones,
                    ReciclajeAguas = p.PUEAA_ReciclajeAguas,
                    ReciclajeAguasObservaciones = p.PUEAA_ReciclajeAguasObservaciones,
                    RecirculacionAguas = p.PUEAA_RecirculacionAguas,
                    RecirculacionAguasObservaciones = p.PUEAA_RecirculacionAguasObservaciones,
                    ControlPerdidas = p.PUEAA_ControlPerdidas,
                    ControlPerdidasObservaciones = p.PUEAA_ControlPerdidasObservaciones,
                    MantenimientoPiscina = p.PUEAA_MantenimientoPiscina,
                    MantenimientoPiscinaObservaciones = p.PUEAA_MantenimientoPiscinaObservaciones,
                    OtrosProcesos = p.PUEAA_OtrosProcesos,
                    OtrosProcesosObservaciones = p.PUEAA_OtrosProcesosObservaciones
                })
                .ToListAsync(ct);
        }

        /// <summary>
        /// Obtiene el reporte de cantidad de predios por tipo de consumo.
        /// </summary>
        public async Task<List<TipoConsumoReportItem>> GetTipoConsumoReportAsync(CancellationToken ct = default)
        {
            await using var db = await _dbFactory.CreateDbContextAsync(ct);

            // Group by the foreign key IdTipoConsumo to avoid relying on navigation properties
            var groups = await db.Predio
                .AsNoTracking()
                .Where(p => p.IdTipoConsumo != null)
                .GroupBy(p => p.IdTipoConsumo)
                .Select(g => new { Id = g.Key!.Value, Count = g.Count() })
                .ToListAsync(ct);

            if (!groups.Any())
            {
                return new List<TipoConsumoReportItem>();
            }

            var tipoIds = groups.Select(g => g.Id).Distinct().ToList();
            var tipos = await db.TipoConsumo.Where(tc => tipoIds.Contains(tc.Id)).AsNoTracking().ToListAsync(ct);
            var mapa = tipos.ToDictionary(t => t.Id, t => t);

            var result = groups.Select(g => new TipoConsumoReportItem
            {
                Id = g.Id,
                TipoConsumo = mapa.TryGetValue(g.Id, out var t) ? t.Categoria : "Desconocido",
                Rango = mapa.TryGetValue(g.Id, out var t2) ? t2.Rango : null,
                CantidadPredios = g.Count
            })
            .OrderBy(r => r.Id)
            .ToList();

            return result;
        }
    }

    // PredioListItem is defined in ACUEDUCTO2.Share.Models to be shared across the application.

    /// <summary>
    /// Proyección de datos PUEAA por predio.
    /// </summary>
    public sealed record PueaaSnapshot
    {
        public string Id { get; init; } = string.Empty;
        public string? Nombre { get; init; }
        public bool? TanqueReservaAcueducto { get; init; }
        public string? TanqueReservaAcueductoObservaciones { get; init; }
        public bool? TanqueReservaLluvia { get; init; }
        public string? TanqueReservaLluviaObservaciones { get; init; }
        public bool? ReciclajeAguas { get; init; }
        public string? ReciclajeAguasObservaciones { get; init; }
        public bool? RecirculacionAguas { get; init; }
        public string? RecirculacionAguasObservaciones { get; init; }
        public bool? ControlPerdidas { get; init; }
        public string? ControlPerdidasObservaciones { get; init; }
        public bool? MantenimientoPiscina { get; init; }
        public string? MantenimientoPiscinaObservaciones { get; init; }
        public bool? OtrosProcesos { get; init; }
        public string? OtrosProcesosObservaciones { get; init; }
    }

    /// <summary>
    /// Proyección para reportar predios por tipo de consumo.
    /// </summary>
    public sealed record TipoConsumoReportItem
    {
        public int Id { get; init; }
        public string TipoConsumo { get; init; } = string.Empty;
        public string? Rango { get; init; }
        public int CantidadPredios { get; init; }
    }

    /// <summary>
    /// Proyección ligera de usuario para selección por identificación.
    /// </summary>
    public sealed record UsuarioLookupItem
    {
        public string Identificacion { get; init; } = string.Empty;
        public string? Nombre { get; init; }
    }
}
