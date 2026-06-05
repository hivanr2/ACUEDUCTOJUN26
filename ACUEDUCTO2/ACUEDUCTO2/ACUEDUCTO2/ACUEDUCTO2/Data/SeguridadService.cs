using ACUEDUCTO2.Share.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace ACUEDUCTO2.Data
{
    public class SeguridadService
    {
        private readonly IDbContextFactory<ApplicationDbContext> _dbFactory;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<ApplicationRole> _roleManager;

        public SeguridadService(IDbContextFactory<ApplicationDbContext> dbFactory, UserManager<ApplicationUser> userManager, RoleManager<ApplicationRole> roleManager)
        {
            _dbFactory = dbFactory ?? throw new ArgumentNullException(nameof(dbFactory));
            _userManager = userManager ?? throw new ArgumentNullException(nameof(userManager));
            _roleManager = roleManager ?? throw new ArgumentNullException(nameof(roleManager));
        }

        // ========== Menu / Authorization ==========

        public async Task<List<DefinicionProceso>> ObtenerProcesosAutorizadosAsync(string idUsuario)
        {
            var user = await _userManager.FindByIdAsync(idUsuario);
            if (user == null) return new List<DefinicionProceso>();

            var roles = await _userManager.GetRolesAsync(user);

            // Administradores have access to everything
            if (roles.Contains("Administrador"))
            {
                return ProcesosCatalogo.Todos;
            }

            using var ctx = _dbFactory.CreateDbContext();
            var procesosAutorizados = await ctx.Seguridad
                .Where(s => roles.Contains(s.IdRol))
                .Select(s => s.Proceso)
                .Distinct()
                .ToListAsync();

            return ProcesosCatalogo.Todos
                .Where(p => procesosAutorizados.Contains(p.Nombre))
                .ToList();
        }

        public async Task<Dictionary<string, List<DefinicionProceso>>> ObtenerMenuPorObjetoAsync(string idUsuario)
        {
            var procesos = await ObtenerProcesosAutorizadosAsync(idUsuario);
            return procesos
                .GroupBy(p => p.Objeto)
                .ToDictionary(g => g.Key, g => g.ToList());
        }

        // ========== Role Management ==========

        public async Task<List<ApplicationRole>> ObtenerRolesAsync()
        {
            // Use a short-lived DbContext to list roles to avoid concurrent operations
            using var ctx = _dbFactory.CreateDbContext();
            return await ctx.Roles.OrderBy(r => r.Name).ToListAsync();
        }

        public async Task<IdentityResult> CrearRolAsync(string nombre)
        {
            if (await _roleManager.RoleExistsAsync(nombre))
                return IdentityResult.Failed(new IdentityError { Description = $"El rol '{nombre}' ya existe." });

            return await _roleManager.CreateAsync(new ApplicationRole { Name = nombre });
        }

        public async Task<IdentityResult> EditarRolAsync(string nombreActual, string nuevoNombre)
        {
            if (nombreActual == "Administrador")
                return IdentityResult.Failed(new IdentityError { Description = "The 'Administrador' role cannot be renamed." });

            if (string.IsNullOrWhiteSpace(nuevoNombre))
                return IdentityResult.Failed(new IdentityError { Description = "The new name cannot be empty." });

            nuevoNombre = nuevoNombre.Trim();

            if (nombreActual == nuevoNombre)
                return IdentityResult.Success;

            if (await _roleManager.RoleExistsAsync(nuevoNombre))
                return IdentityResult.Failed(new IdentityError { Description = $"The role '{nuevoNombre}' already exists." });

            var role = await _roleManager.FindByNameAsync(nombreActual);
            if (role == null)
                return IdentityResult.Failed(new IdentityError { Description = "Role not found." });

            // Update role name in AspNetRoles
            role.Name = nuevoNombre;
            role.NormalizedName = nuevoNombre.ToUpperInvariant();
            var result = await _roleManager.UpdateAsync(role);

            if (result.Succeeded)
            {
                // Update references in Seguridad table
            using (var ctx = _dbFactory.CreateDbContext())
            {
                var accesos = await ctx.Seguridad.Where(s => s.IdRol == nombreActual).ToListAsync();
                foreach (var acceso in accesos)
                {
                    acceso.IdRol = nuevoNombre;
                }
                await ctx.SaveChangesAsync();
            }
            }

            return result;
        }

        public async Task<IdentityResult> EliminarRolAsync(string nombre)
        {
            if (nombre == "Administrador")
                return IdentityResult.Failed(new IdentityError { Description = "The 'Administradores' role cannot be deleted." });

            var role = await _roleManager.FindByNameAsync(nombre);
            if (role == null)
                return IdentityResult.Failed(new IdentityError { Description = "Role not found." });

            // Remove associated access entries
            using (var ctx = _dbFactory.CreateDbContext())
            {
                var accesos = await ctx.Seguridad.Where(s => s.IdRol == nombre).ToListAsync();
                ctx.Seguridad.RemoveRange(accesos);
                await ctx.SaveChangesAsync();
            }

            return await _roleManager.DeleteAsync(role);
        }

        // ========== Role-Process Assignments ==========

        public async Task<List<Seguridad>> ObtenerAccesosPorRolAsync(string idRol)
        {
            using var ctx = _dbFactory.CreateDbContext();
            return await ctx.Seguridad
                .Where(s => s.IdRol == idRol)
                .OrderBy(s => s.Proceso)
                .ToListAsync();
        }

        public async Task<List<string>> ObtenerNombresProcesosDeRolAsync(string idRol)
        {
            using var ctx = _dbFactory.CreateDbContext();
            return await ctx.Seguridad
                .Where(s => s.IdRol == idRol)
                .Select(s => s.Proceso)
                .ToListAsync();
        }

        public async Task AgregarAccesoAsync(string idRol, string proceso, string usuarioSistema)
        {
            using (var ctx = _dbFactory.CreateDbContext())
            {
                var existe = await ctx.Seguridad.AnyAsync(s => s.IdRol == idRol && s.Proceso == proceso);
                if (!existe)
                {
                    ctx.Seguridad.Add(new Seguridad
                    {
                        Guid = System.Guid.NewGuid().ToString(),
                        IdRol = idRol,
                        Proceso = proceso,
                        Observaciones = "Assigned via admin panel",
                        FechaHoraCreacionRegistro = DateTime.UtcNow,
                        UsuarioSistemaCreaRegistro = usuarioSistema
                    });
                    await ctx.SaveChangesAsync();
                }
            }
        }

        public async Task EliminarAccesoAsync(string idRol, string proceso)
        {
            using (var ctx = _dbFactory.CreateDbContext())
            {
                var acceso = await ctx.Seguridad.FirstOrDefaultAsync(s => s.IdRol == idRol && s.Proceso == proceso);
                if (acceso != null)
                {
                    ctx.Seguridad.Remove(acceso);
                    await ctx.SaveChangesAsync();
                }
            }
        }

        public async Task ActualizarAccesosDeRolAsync(string idRol, List<string> procesosSeleccionados, string usuarioSistema)
        {
            using (var ctx = _dbFactory.CreateDbContext())
            {
                var actuales = await ctx.Seguridad.Where(s => s.IdRol == idRol).ToListAsync();
                var actualesNombres = actuales.Select(a => a.Proceso).ToList();

                // Remove unselected
                var paraEliminar = actuales.Where(a => !procesosSeleccionados.Contains(a.Proceso)).ToList();
                ctx.Seguridad.RemoveRange(paraEliminar);

                // Add new
                var paraAgregar = procesosSeleccionados.Where(p => !actualesNombres.Contains(p)).ToList();
                foreach (var proceso in paraAgregar)
                {
                    ctx.Seguridad.Add(new Seguridad
                    {
                        Guid = System.Guid.NewGuid().ToString(),
                        IdRol = idRol,
                        Proceso = proceso,
                        Observaciones = "Assigned via admin panel",
                        FechaHoraCreacionRegistro = DateTime.UtcNow,
                        UsuarioSistemaCreaRegistro = usuarioSistema
                    });
                }

                await ctx.SaveChangesAsync();
            }
        }

        // ========== User-Role Assignments ==========

        public async Task<List<ApplicationUser>> ObtenerUsuariosAsync()
        {
            // UserManager.Users is backed by Identity's DbContext; use a short-lived context to avoid conflicts
            using var ctx = _dbFactory.CreateDbContext();
            return await ctx.Users.OrderBy(u => u.Email).ToListAsync();
        }

        public async Task<IList<string>> ObtenerRolesDeUsuarioAsync(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return new List<string>();
            return await _userManager.GetRolesAsync(user);
        }

        public async Task<IdentityResult> AsignarRolAUsuarioAsync(string userId, string roleName)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                return IdentityResult.Failed(new IdentityError { Description = "User not found." });

            if (await _userManager.IsInRoleAsync(user, roleName))
                return IdentityResult.Success;

            return await _userManager.AddToRoleAsync(user, roleName);
        }

        public async Task<IdentityResult> RemoverRolDeUsuarioAsync(string userId, string roleName)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                return IdentityResult.Failed(new IdentityError { Description = "User not found." });

            return await _userManager.RemoveFromRoleAsync(user, roleName);
        }

        // ========== Security Search ==========

        public async Task<List<ApplicationUser>> ObtenerUsuariosEnRolAsync(string roleName)
        {
            var users = await _userManager.GetUsersInRoleAsync(roleName);
            return users.OrderBy(u => u.Email).ToList();
        }

        public async Task<List<string>> ObtenerRolesConProcesoAsync(string proceso)
        {
            using var ctx = _dbFactory.CreateDbContext();
            return await ctx.Seguridad
                .Where(s => s.Proceso == proceso)
                .Select(s => s.IdRol)
                .Distinct()
                .OrderBy(r => r)
                .ToListAsync();
        }

        public async Task<List<ApplicationUser>> ObtenerUsuariosConAccesoAProcesoAsync(string proceso)
        {
            var rolesConProceso = await ObtenerRolesConProcesoAsync(proceso);

            // Include Administradores since they have access to everything
            if (!rolesConProceso.Contains("Administradores"))
                rolesConProceso.Add("Administradores");

            var usuariosUnicos = new Dictionary<string, ApplicationUser>();
            foreach (var rol in rolesConProceso)
            {
                // use UserManager for role membership (it will use the scoped Identity context)
                var usersInRole = await _userManager.GetUsersInRoleAsync(rol);
                foreach (var user in usersInRole)
                {
                    usuariosUnicos.TryAdd(user.Id, user);
                }
            }

            return usuariosUnicos.Values.OrderBy(u => u.Email).ToList();
        }
    }
}
