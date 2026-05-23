using CIVS.Data;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace CIVS.Middleware
{
    public class SessionTokenValidationMiddleware
    {
        private readonly RequestDelegate _next;

        public SessionTokenValidationMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context, AppDbContext db)
        {
            var path = context.Request.Path;

            // No validar rutas públicas ni archivos estáticos
            if (path.StartsWithSegments("/Auth") ||
                path.StartsWithSegments("/css") ||
                path.StartsWithSegments("/js") ||
                path.StartsWithSegments("/lib") ||
                path.StartsWithSegments("/images") ||
                path.StartsWithSegments("/favicon.ico"))
            {
                await _next(context);
                return;
            }

            // Si no hay usuario autenticado, dejar seguir normal
            if (context.User.Identity?.IsAuthenticated != true)
            {
                await _next(context);
                return;
            }

            var userIdClaim = context.User.FindFirstValue(ClaimTypes.NameIdentifier);
            var sessionTokenClaim = context.User.FindFirstValue("SessionToken");

            // Si la cookie está corrupta o incompleta, cerrar sesión y volver al login
            if (string.IsNullOrWhiteSpace(userIdClaim) ||
                string.IsNullOrWhiteSpace(sessionTokenClaim) ||
                !int.TryParse(userIdClaim, out int usuarioId))
            {
                await CerrarSesionYRedirigir(context);
                return;
            }

            var usuario = await db.Usuarios
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.UsuarioId == usuarioId);

            // Si el usuario ya no existe o está inactivo
            if (usuario == null || !usuario.UsuarioEstado)
            {
                await CerrarSesionYRedirigir(context);
                return;
            }

            // Si el token de sesión no existe en BD
            if (string.IsNullOrWhiteSpace(usuario.SessionToken) ||
                usuario.SessionTokenExpiry == null)
            {
                await CerrarSesionYRedirigir(context);
                return;
            }

            // Si el token no coincide con el de la cookie
            if (usuario.SessionToken != sessionTokenClaim)
            {
                await CerrarSesionYRedirigir(context);
                return;
            }

            // Si la sesión expiró
            if (usuario.SessionTokenExpiry <= DateTime.UtcNow)
            {
                await CerrarSesionYRedirigir(context);
                return;
            }

            await _next(context);
        }

        private static async Task CerrarSesionYRedirigir(HttpContext context)
        {
            await context.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

            if (EsAjax(context.Request))
            {
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                await context.Response.WriteAsJsonAsync(new
                {
                    ok = false,
                    mensaje = "La sesión expiró. Inicie sesión nuevamente."
                });

                return;
            }

            context.Response.Redirect("/Auth/Login");
        }

        private static bool EsAjax(HttpRequest request)
        {
            return request.Headers["X-Requested-With"] == "XMLHttpRequest";
        }
    }

    public static class SessionTokenValidationMiddlewareExtensions
    {
        public static IApplicationBuilder UseSessionTokenValidation(this IApplicationBuilder app)
        {
            return app.UseMiddleware<SessionTokenValidationMiddleware>();
        }
    }
}