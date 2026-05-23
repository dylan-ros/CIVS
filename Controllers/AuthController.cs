using CIVS.Data;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace CIVS.Controllers
{
    public class AuthController : Controller
    {
        private readonly AppDbContext _context;

        public AuthController(AppDbContext context)
        {
            _context = context;
        }

        // GET: /Auth/Login
        [HttpGet]
        [AllowAnonymous]
        [ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
        public async Task<IActionResult> Login()
        {
            TempData.Clear();

            // Si hay una sesión vieja, la cerramos para evitar cookies/token viejos.
            if (User.Identity?.IsAuthenticated == true)
            {
                await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            }

            Response.Headers["Cache-Control"] = "no-store, no-cache, must-revalidate";
            Response.Headers["Pragma"] = "no-cache";
            Response.Headers["Expires"] = "0";

            return View();
        }

        // POST: /Auth/Login
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(string usernameOrEmail, string password, bool rememberMe = false)
        {
            TempData.Clear();

            if (string.IsNullOrWhiteSpace(usernameOrEmail) || string.IsNullOrWhiteSpace(password))
            {
                ViewBag.Error = "Ingresá tu usuario y contraseña.";
                return View();
            }

            usernameOrEmail = usernameOrEmail.Trim();

            var usuario = await _context.Usuarios
                .Include(u => u.UsuarioRoles)
                    .ThenInclude(ur => ur.Rol)
                .FirstOrDefaultAsync(u =>
                    (u.UsuarioUsername == usernameOrEmail ||
                     u.UsuarioEmail == usernameOrEmail) &&
                    u.UsuarioEstado == true);

            if (usuario == null || !VerificarPassword(password, usuario.UsuarioPasswordHash))
            {
                ViewBag.Error = "Usuario o contraseña incorrectos.";
                return View();
            }

            // Generar token único de sesión.
            var sessionToken = GenerarTokenSesion();

            var tokenExpiry = rememberMe
                ? DateTime.UtcNow.AddDays(7)
                : DateTime.UtcNow.AddHours(8);

            usuario.SessionToken = sessionToken;
            usuario.SessionTokenExpiry = tokenExpiry;
            await _context.SaveChangesAsync();

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, usuario.UsuarioId.ToString()),
                new Claim(ClaimTypes.Name, usuario.UsuarioUsername),
                new Claim(ClaimTypes.Email, usuario.UsuarioEmail ?? string.Empty),
                new Claim("NombreCompleto", $"{usuario.UsuarioNombres} {usuario.UsuarioApellidos}".Trim()),
                new Claim("SessionToken", sessionToken)
            };

            foreach (var ur in usuario.UsuarioRoles.Where(r => r.UsuarioRolEstado))
            {
                claims.Add(new Claim(ClaimTypes.Role, ur.Rol.RolNombre));
            }

            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var principal = new ClaimsPrincipal(identity);

            var properties = new AuthenticationProperties
            {
                IsPersistent = rememberMe,
                ExpiresUtc = tokenExpiry,
                AllowRefresh = false
            };

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                principal,
                properties);

            var rolesUsuario = usuario.UsuarioRoles
                .Where(ur => ur.UsuarioRolEstado)
                .Select(ur => ur.Rol.RolNombre)
                .ToList();

            if (rolesUsuario.Contains("Contabilidad") || rolesUsuario.Contains("Cajero"))
            {
                return RedirectToAction("Index", "CorteCaja");
            }

            return RedirectToAction("Index", "Home");
        }

        // POST: /Auth/Logout
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            TempData.Clear();

            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

            return RedirectToAction("Login", "Auth");
        }

        // GET: /Auth/AccesoDenegado
        [HttpGet]
        [AllowAnonymous]
        public IActionResult AccesoDenegado()
        {
            return View();
        }

        // GET: /Auth/OlvidePassword
        [HttpGet]
        [AllowAnonymous]
        public IActionResult OlvidePassword()
        {
            TempData.Clear();
            return View();
        }

        // POST: /Auth/OlvidePassword
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> OlvidePassword(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
            {
                ViewBag.Error = "Por favor ingresá tu correo electrónico.";
                return View();
            }

            email = email.Trim();

            var usuario = await _context.Usuarios
                .FirstOrDefaultAsync(u => u.UsuarioEmail == email && u.UsuarioEstado == true);

            if (usuario != null)
            {
                var resetToken = GenerarTokenSesion();
                usuario.PasswordResetToken = resetToken;
                usuario.PasswordResetTokenExpiry = DateTime.UtcNow.AddHours(1);

                await _context.SaveChangesAsync();

                var resetUrl = Url.Action("ResetPassword", "Auth",
                    new { token = resetToken },
                    Request.Scheme);

                ViewBag.Success = $"Se ha generado un enlace de recuperación. URL: {resetUrl} (En producción esto se enviaría por email)";
            }
            else
            {
                ViewBag.Success = "Si el correo existe en nuestro sistema, recibirás las instrucciones de recuperación.";
            }

            return View();
        }

        // GET: /Auth/ResetPassword?token=xxx
        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> ResetPassword(string token)
        {
            if (string.IsNullOrWhiteSpace(token))
            {
                ViewBag.Error = "Token inválido.";
                return View();
            }

            var usuario = await _context.Usuarios
                .FirstOrDefaultAsync(u =>
                    u.PasswordResetToken == token &&
                    u.PasswordResetTokenExpiry > DateTime.UtcNow &&
                    u.UsuarioEstado == true);

            if (usuario == null)
            {
                ViewBag.Error = "El enlace de recuperación es inválido o ha expirado.";
                return View();
            }

            ViewBag.Token = token;
            ViewBag.Email = usuario.UsuarioEmail;
            return View();
        }

        // POST: /Auth/ResetPassword
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResetPassword(string token, string newPassword, string confirmPassword)
        {
            if (string.IsNullOrWhiteSpace(token))
            {
                ViewBag.Error = "Token inválido.";
                return View();
            }

            if (string.IsNullOrWhiteSpace(newPassword) || newPassword.Length < 6)
            {
                ViewBag.Error = "La contraseña debe tener al menos 6 caracteres.";
                ViewBag.Token = token;
                return View();
            }

            if (newPassword != confirmPassword)
            {
                ViewBag.Error = "Las contraseñas no coinciden.";
                ViewBag.Token = token;
                return View();
            }

            var usuario = await _context.Usuarios
                .FirstOrDefaultAsync(u =>
                    u.PasswordResetToken == token &&
                    u.PasswordResetTokenExpiry > DateTime.UtcNow &&
                    u.UsuarioEstado == true);

            if (usuario == null)
            {
                ViewBag.Error = "El enlace de recuperación es inválido o ha expirado.";
                return View();
            }

            usuario.UsuarioPasswordHash = GenerarHash(newPassword);
            usuario.PasswordResetToken = null;
            usuario.PasswordResetTokenExpiry = null;
            usuario.SessionToken = null;
            usuario.SessionTokenExpiry = null;

            await _context.SaveChangesAsync();

            ViewBag.Success = "Tu contraseña ha sido actualizada exitosamente. Ya podés iniciar sesión.";
            return View("ResetPasswordSuccess");
        }

        // POST: /Auth/ResetPasswordAdmin
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,Administrador")]
        public async Task<IActionResult> ResetPasswordAdmin(int usuarioId, string newPassword)
        {
            if (string.IsNullOrWhiteSpace(newPassword) || newPassword.Length < 6)
            {
                return Json(new
                {
                    success = false,
                    message = "La contraseña debe tener al menos 6 caracteres."
                });
            }

            var usuario = await _context.Usuarios.FindAsync(usuarioId);

            if (usuario == null)
            {
                return Json(new
                {
                    success = false,
                    message = "Usuario no encontrado."
                });
            }

            usuario.UsuarioPasswordHash = GenerarHash(newPassword);
            usuario.SessionToken = null;
            usuario.SessionTokenExpiry = null;

            await _context.SaveChangesAsync();

            return Json(new
            {
                success = true,
                message = $"Contraseña actualizada para {usuario.UsuarioUsername}. Nueva contraseña: {newPassword}"
            });
        }

        private static bool VerificarPassword(string password, string hash)
        {
            if (string.IsNullOrWhiteSpace(hash))
                return false;

            using var sha = SHA256.Create();
            var computed = Convert.ToHexString(
                sha.ComputeHash(Encoding.UTF8.GetBytes(password))).ToLower();

            return computed == hash.ToLower();
        }

        public static string GenerarHash(string password)
        {
            using var sha = SHA256.Create();
            return Convert.ToHexString(
                sha.ComputeHash(Encoding.UTF8.GetBytes(password))).ToLower();
        }

        private static string GenerarTokenSesion()
        {
            var randomBytes = new byte[32];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(randomBytes);
            return Convert.ToBase64String(randomBytes);
        }
    }
}
