using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using ACUEDUCTO2.Data;
using ACUEDUCTO2.Share.Models;
using Microsoft.EntityFrameworkCore;

namespace ACUEDUCTO2.Services.PeriodoServices
{
    /// <summary>
    /// Servicio de acceso a datos para la entidad Periodo en v2.
    /// </summary>
    public class PeriodoService
    {
        private readonly IDbContextFactory<ApplicationDbContext> _dbFactory;

        public PeriodoService(IDbContextFactory<ApplicationDbContext> dbFactory)
        {
            _dbFactory = dbFactory;
        }

        public async Task<List<Periodo>> GetAllAsync(CancellationToken ct = default)
        {
            await using var db = await _dbFactory.CreateDbContextAsync(ct);
            return await db.Periodos
                .OrderByDescending(p => p.FechaHoraFacturacion ?? DateTime.MinValue)
                .ThenByDescending(p => p.Guid)
                .AsNoTracking()
                .ToListAsync(ct);
        }

        public async Task<Periodo?> GetByGuidAsync(string guid, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(guid)) return null;

            await using var db = await _dbFactory.CreateDbContextAsync(ct);
            return await db.Periodos
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.Guid == guid, ct);
        }

        public async Task CreateAsync(Periodo periodo, CancellationToken ct = default)
        {
            if (periodo is null) throw new ArgumentNullException(nameof(periodo));

            await using var db = await _dbFactory.CreateDbContextAsync(ct);
            db.Periodos.Add(periodo);
            await db.SaveChangesAsync(ct);
        }

        public async Task UpdateAsync(Periodo periodo, CancellationToken ct = default)
        {
            if (periodo is null) throw new ArgumentNullException(nameof(periodo));

            await using var db = await _dbFactory.CreateDbContextAsync(ct);
            try
            {
                db.Periodos.Update(periodo);
                await db.SaveChangesAsync(ct);
            }
            catch (DbUpdateConcurrencyException ex)
            {
                throw new InvalidOperationException("Conflicto de concurrencia al actualizar el periodo. Intente de nuevo.", ex);
            }
        }

        public async Task DeleteAsync(string guid, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(guid)) return;

            await using var db = await _dbFactory.CreateDbContextAsync(ct);
            try
            {
                var entidad = await db.Periodos.FindAsync(new object[] { guid }, ct);
                if (entidad is null) return;

                db.Periodos.Remove(entidad);
                await db.SaveChangesAsync(ct);
            }
            catch (DbUpdateConcurrencyException ex)
            {
                throw new InvalidOperationException("Conflicto de concurrencia al eliminar el periodo. Intente de nuevo.", ex);
            }
        }
    }
}
