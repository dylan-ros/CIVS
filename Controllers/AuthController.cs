using CIVS.Data;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
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
        public IActionResult Login()
        { 
            if (User.Identity?.IsAuthenticated == true)
                return RedirectToAction("Index", "Home");

            return View();
        }

        // POST: /Auth/Login
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(string usernameOrEmail, string password, bool rememberMe = false)
        {
            if (string.IsNullOrWhiteSpace(usernameOrEmail) || string.IsNullOrWhiteSpace(password))
            {
                ViewBag.Error = "Ingresá tu usuario y contraseña.";
                return View();
            }

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

            // Crear claims
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, usuario.UsuarioId.ToString()),
                new Claim(ClaimTypes.Name,           usuario.UsuarioUsername),
                new Claim(ClaimTypes.Email,          usuario.UsuarioEmail),
                new Claim("NombreCompleto",
                    $"{usuario.UsuarioNombres} {usuario.UsuarioApellidos}".Trim())
            };

            foreach (var ur in usuario.UsuarioRoles.Where(r => r.UsuarioRolEstado))
                claims.Add(new Claim(ClaimTypes.Role, ur.Rol.RolNombre));

            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var principal = new ClaimsPrincipal(identity);
            var properties = new AuthenticationProperties
            {
                IsPersistent = rememberMe,
                ExpiresUtc = rememberMe
                    ? DateTimeOffset.UtcNow.AddDays(7)
                    : DateTimeOffset.UtcNow.AddHours(8)
            };

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                principal, properties);

            return RedirectToAction("Index", "Home");
        }

        // POST: /Auth/Logout
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Login");
        }

        // GET: /Auth/AccesoDenegado
        [HttpGet]
        public IActionResult AccesoDenegado() => View();

        // ── Hash SHA-256 ──────────────────────────────────────────────────────
        private static bool VerificarPassword(string password, string hash)
        {
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
    }
}