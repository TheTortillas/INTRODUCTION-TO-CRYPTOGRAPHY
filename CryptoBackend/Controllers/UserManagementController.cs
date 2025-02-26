using CryptoBackend.DTOs;
using CryptoBackend.DTOs.Auth;
using CryptoBackend.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.Security.Cryptography;

namespace CryptoBackend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserManagementController : Controller
    {
        IConfiguration _config;
        DBManager _dbManager;
        IPasswordHasher<UserDTO> _passwordHasher;

        public UserManagementController(IConfiguration conf)
        {
            _config = conf;
            _dbManager = new DBManager(_config);
            _passwordHasher = new PasswordHasher<UserDTO>();
        }

        // Método para generar un salt
        private string GenerateSalt(int size = 16)
        {
            var saltBytes = new byte[size];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(saltBytes);
            }
            return Convert.ToBase64String(saltBytes);
        }


        [HttpPost("Register", Name = "PostRegister")]
        public async Task<IActionResult> Register([FromBody] RegistrationRequest request)
        {
            try
            {
                var salt = GenerateSalt();
                var saltedPassword = request.Password + salt;

                var user = new UserDTO
                {
                    Firstname = request.Firstname,
                    Lastname = request.Lastname,
                    SecondLastname = request.SecondLastname,
                    Email = request.Email,
                    Password = _passwordHasher.HashPassword(null, saltedPassword),  // https://andrewlock.net/exploring-the-asp-net-core-identity-passwordhasher/
                    Salt = salt,
                };

                var success = await _dbManager.sign_up(user);

                if (!success)
                {
                    return BadRequest(new { message = "Error al registrar el usuario" });
                }

                return Ok(new { message = "Usuario registrado exitosamente" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"Error interno del servidor: {ex.Message}" });
            }
        }

        [HttpPost("Login", Name = "PostLogin")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            try
            {
                // Primero buscamos el usuario por email
                var user = await _dbManager.FindByEmailAsync(request.Email);

                if (user == null)
                {
                    return Unauthorized(new { message = "Usuario no encontrado" });
                }

                // Verificamos la contraseña
                var saltedPassword = request.Password + user.Salt;
                var passwordVerificationResult = _passwordHasher.VerifyHashedPassword(null, user.Password, saltedPassword);

                if (passwordVerificationResult == PasswordVerificationResult.Failed)
                {
                    return Unauthorized(new { message = "Contraseña incorrecta" });
                }

                // Si todo es correcto, generamos el token
                var jwtService = new JwtService(_config);
                string token = jwtService.CreateToken(user);

                return Ok(new AuthResponse { Token = token });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"Error interno del servidor: {ex.Message}" });
            }
        }

        [HttpPost("RefreshToken", Name = "PostRefreshToken")]
        public IActionResult RefreshToken()
        {
            var authHeader = Request.Headers["Authorization"].ToString();
            if (string.IsNullOrEmpty(authHeader) || !authHeader.StartsWith("Bearer "))
            {
                return Unauthorized(new { message = "Token no proporcionado." });
            }

            var token = authHeader.Substring("Bearer ".Length).Trim();

            try
            {
                var jwtService = new JwtService(_config);
                var newToken = jwtService.RefreshToken(token);
                return Ok(new { token = newToken });
            }
            catch (SecurityTokenException ex)
            {
                Console.WriteLine($"Error al validar el token: {ex.Message}");
                return Unauthorized(new { message = "Token inválido o expirado." });
            }
        }
    }
}
