using CIVS.Data;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace CIVS.Middleware
{
    /// <summary>
    /// Middleware que valida el token de sesión en cada petición.
    /// Si el token no coincide con el almacenado en BD o ha expirado, 
    /// cierra la sesión automáticamente.
    /// </summary>
    public class SessionTokenValidationMiddleware
    {
        private readonly RequestDelegate _next;

        public SessionTokenValidationMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context, AppDbContext dbContext)
        {
            // Solo validar si el usuario está autenticado
            if (context.User.Identity?.IsAuthenticated == true)
            {
                var userIdClaim = context.User.FindFirst(ClaimTypes.NameIdentifier);
                var sessionTokenClaim = context.User.FindFirst("SessionToken");

                if (userIdClaim != null && sessionTokenClaim != null)
                {
                    if (int.TryParse(userIdClaim.Value, out int userId))
                    {
                        var usuario = await dbContext.Usuarios
                            .AsNoTracking()
                            .FirstOrDefaultAsync(u => u.UsuarioId == userId);

                        // Validar que el token coincida y no haya expirado
                        bool tokenInvalido = usuario == null ||
                            usuario.SessionToken != sessionTokenClaim.Value ||
                            usuario.SessionTokenExpiry == null ||
                            usuario.SessionTokenExpiry < DateTime.UtcNow;

                        if (tokenInvalido)
                        {
                            // Cerrar sesión y redirigir al login
                            await context.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
                            context.Response.Redirect("/Auth/Login?sesionInvalida=true");
                            return;
                        }
                    }
                }
            }

            await _next(context);
        }
    }

    /// <summary>
    /// Extension method para facilitar el registro del middleware
    /// </summary>
    public static class SessionTokenValidationMiddlewareExtensions
    {
        public static IApplicationBuilder UseSessionTokenValidation(
            this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<SessionTokenValidationMiddleware>();
        }
    }
}