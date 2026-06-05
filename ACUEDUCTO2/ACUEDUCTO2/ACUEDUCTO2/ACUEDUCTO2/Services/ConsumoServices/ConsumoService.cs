using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ACUEDUCTO2.Data;
using ACUEDUCTO2.Share.Models;
using Microsoft.EntityFrameworkCore;

namespace ACUEDUCTO2.Services.ConsumoServices
{
    /// <summary>
    /// Servicio de acceso a datos para la entidad Consumo en v2.
    /// </summary>
    public class ConsumoService
    {
        private readonly IDbContextFactory<ApplicationDbContext> _dbFactory;

        public ConsumoService(IDbContextFactory<ApplicationDbContext> dbFactory)
        {
            _dbFactory = dbFactory ?? throw new ArgumentNullException(nameof(dbFactory));
        }

        /// <summary>
        /// Obtiene el listado de consumos registrados en v2, incluyendo el nombre corto
        /// del periodo y el nombre del predio asociado a la factura. Si los valores de
        /// medición en Consumo están en cero, se usan como respaldo los valores de Factura.
        /// </summary>
        public async Task<List<ConsumoListItem>> GetListingAsync(string? _identificacionUsuario, CancellationToken ct = default)
        {
            await using var db = await _dbFactory.CreateDbContextAsync(ct);

            var query =
                from c in db.Consumos.AsNoTracking()
                join f in db.Facturacions.AsNoTracking()
                    on c.IdFactura equals f.Guid into facturas
                from f in facturas.DefaultIfEmpty()
                join p in db.Periodos.AsNoTracking()
                    on f.IdPeriodo equals p.Guid into periodos
                from p in periodos.DefaultIfEmpty()
                join pr in db.Predio.AsNoTracking()
                    on f.IdPredio equals pr.IdPredio into predios
                from pr in predios.DefaultIfEmpty()
                select new { c, f, p, pr };

            // Materializamos primero en memoria porque usamos lógica que no se puede traducir
            // a SQL (null-conditional, Math.Round sobre double?).
            var raw = await query
                .OrderByDescending(x => x.c.FechaHoraCreacionRegistro)
                .ThenByDescending(x => x.c.Guid)
                .ToListAsync(ct);

            var items = new List<ConsumoListItem>(raw.Count);
            foreach (var x in raw)
            {
                var item = new ConsumoListItem
                {
                    Guid = x.c.Guid,
                    NombrePeriodoCorto = x.p != null ? x.p.NombreCortoPeriodo : string.Empty,
                    NombrePredio = x.pr != null ? x.pr.ApellidosNombresUsuario : string.Empty,
                    IdFactura = x.c.IdFactura,
                    EnSeguimiento = x.c.EnSeguimiento,
                    Observaciones = x.c.Observaciones,
                    // Mostramos la fecha/hora de registro de la factura como fecha de registro
                    // del consumo en los listados. Si no hay factura asociada, usamos la del consumo.
                    FechaHoraCreacionRegistro = x.f?.FechaHoraRegistro ?? x.c.FechaHoraCreacionRegistro,
                    UsuarioSistemaCreaRegistro = x.c.UsuarioSistemaCreaRegistro
                };

                // Si por algún motivo los valores migrados están en 0, usamos como respaldo
                // las mediciones provenientes de la factura v2.
                if (x.c.MedicionAntereiorM3 != 0)
                {
                    item.MedicionAnteriorM3 = x.c.MedicionAntereiorM3;
                }
                else if (x.f?.MedicionAnterior is double ma)
                {
                    item.MedicionAnteriorM3 = (int)Math.Round(ma, MidpointRounding.AwayFromZero);
                }

                if (x.c.MedicionActualM3 != 0)
                {
                    item.MedicionActualM3 = x.c.MedicionActualM3;
                }
                else if (x.f?.MedicionActual is double mc)
                {
                    item.MedicionActualM3 = (int)Math.Round(mc, MidpointRounding.AwayFromZero);
                }

                if (x.c.M3Consumidos != 0)
                {
                    item.M3Consumidos = x.c.M3Consumidos;
                }
                else if (x.f?.ConsumoM3 is double c3)
                {
                    item.M3Consumidos = (int)Math.Round(c3, MidpointRounding.AwayFromZero);
                }

                items.Add(item);
            }

            return items;
        }
    }

    /// <summary>
    /// Proyección liviana para mostrar consumos en listados.
    /// </summary>
    public sealed class ConsumoListItem
    {
        public string Guid { get; set; } = string.Empty;
        public string NombrePeriodoCorto { get; set; } = string.Empty;
        public string NombrePredio { get; set; } = string.Empty;
        public string IdFactura { get; set; } = string.Empty;
        public int MedicionAnteriorM3 { get; set; }
        public int MedicionActualM3 { get; set; }
        public int M3Consumidos { get; set; }
        public bool EnSeguimiento { get; set; }
        public string Observaciones { get; set; } = string.Empty;
        public DateTime FechaHoraCreacionRegistro { get; set; }
        public string UsuarioSistemaCreaRegistro { get; set; } = string.Empty;
    }
}
