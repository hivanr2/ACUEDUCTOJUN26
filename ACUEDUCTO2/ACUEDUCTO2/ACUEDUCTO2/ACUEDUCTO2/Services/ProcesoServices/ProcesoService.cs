using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using ACUEDUCTO2.Data;
using ACUEDUCTO2.Share.Models;
using System;

namespace ACUEDUCTO2.Services.ProcesoServices
{
    public class ProcesoService
    {
        private readonly IDbContextFactory<ApplicationDbContext> _dbFactory;

        public ProcesoService(IDbContextFactory<ApplicationDbContext> dbFactory)
        {
            _dbFactory = dbFactory;
        }

        public async Task<List<Proceso>> GetAllAsync(CancellationToken ct = default)
        {
            await using var db = await _dbFactory.CreateDbContextAsync(ct);
            return await db.Proceso
                .OrderBy(c => c.Objeto)
                .ThenBy(c => c.Nombre)
                .ThenBy(c => c.IdTipoProceso)
                .AsNoTracking()
                .ToListAsync(ct);
        }

        public async Task<Proceso?> GetByIdAsync(string id, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(id)) return null;
            await using var db = await _dbFactory.CreateDbContextAsync(ct);
            return await db.Proceso.AsNoTracking().FirstOrDefaultAsync(p => p.Id == id, ct);
        }

        public async Task<List<TipoProceso>> GetTiposProcesoAsync(CancellationToken ct = default)
        {
            await using var db = await _dbFactory.CreateDbContextAsync(ct);
            return await db.TipoProceso.OrderBy(t => t.Nombre).AsNoTracking().ToListAsync(ct);
        }

        /// <summary>
        /// Actualiza un tipo de proceso existente.
        /// </summary>
        public async Task UpdateTipoProcesoAsync(TipoProceso tipoProceso, CancellationToken ct = default)
        {
            if (tipoProceso is null) throw new ArgumentNullException(nameof(tipoProceso));

            await using var db = await _dbFactory.CreateDbContextAsync(ct);
            db.TipoProceso.Update(tipoProceso);
            await db.SaveChangesAsync(ct);
        }

        /// <summary>
        /// Elimina un tipo de proceso por id.
        /// </summary>
        public async Task DeleteTipoProcesoAsync(int id, CancellationToken ct = default)
        {
            if (id <= 0) return;

            await using var db = await _dbFactory.CreateDbContextAsync(ct);
            var entidad = await db.TipoProceso.FindAsync(new object[] { id }, ct);
            if (entidad is null) return;

            db.TipoProceso.Remove(entidad);
            await db.SaveChangesAsync(ct);
        }

        public async Task CreateAsync(Proceso proceso, CancellationToken ct = default)
        {
            if (proceso is null) throw new ArgumentNullException(nameof(proceso));

            await using var db = await _dbFactory.CreateDbContextAsync(ct);
            db.Proceso.Add(proceso);
            await db.SaveChangesAsync(ct);
        }

        public async Task UpdateAsync(Proceso proceso, CancellationToken ct = default)
        {
            if (proceso is null) throw new ArgumentNullException(nameof(proceso));

            await using var db = await _dbFactory.CreateDbContextAsync(ct);
            try
            {
                db.Proceso.Update(proceso);
                await db.SaveChangesAsync(ct);
            }
            catch (DbUpdateConcurrencyException ex)
            {
                throw new InvalidOperationException("Conflicto de concurrencia al actualizar el proceso. Intente de nuevo.", ex);
            }
        }

        public async Task DeleteAsync(string id, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(id)) return;

            await using var db = await _dbFactory.CreateDbContextAsync(ct);
            try
            {
                var entidad = await db.Proceso.FindAsync(new object[] { id }, ct);
                if (entidad is null) return;
                db.Proceso.Remove(entidad);
                await db.SaveChangesAsync(ct);
            }
            catch (DbUpdateConcurrencyException ex)
            {
                throw new InvalidOperationException("Conflicto de concurrencia al eliminar el proceso. Intente de nuevo.", ex);
            }
        }
    }
}
