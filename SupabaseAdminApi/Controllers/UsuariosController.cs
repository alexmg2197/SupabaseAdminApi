using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using SupabaseAdminApi.Modelos;
using System.Net.Http.Headers;

namespace SupabaseAdminApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UsuariosController : ControllerBase
    {
        private readonly IConfiguration _config;
        private readonly IHttpClientFactory _httpClientFactory;

        public UsuariosController(IConfiguration config, IHttpClientFactory httpClientFactory)
        {
            _config = config;
            _httpClientFactory = httpClientFactory;
        }

        [HttpPost("eliminar")]
        public async Task<IActionResult> EliminarUsuario([FromBody] EliminarUsuarioRequest request)
        {
            if (string.IsNullOrEmpty(request.AuthUserId))
                return BadRequest("Falta el ID del usuario.");

            var supabaseUrl = _config["Supabase:Url"];
            var serviceKey = _config["Supabase:ServiceRoleKey"];

            var client = _httpClientFactory.CreateClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", serviceKey);
            client.DefaultRequestHeaders.Add("apikey", serviceKey);
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            // 1. Eliminar primero de la tabla Usuarios
            var deleteUsuariosResponse = await client.DeleteAsync(
                $"{supabaseUrl}/rest/v1/Usuarios?auth_user_id=eq.{request.AuthUserId}");

            if (!deleteUsuariosResponse.IsSuccessStatusCode)
            {
                var error = await deleteUsuariosResponse.Content.ReadAsStringAsync();
                return StatusCode((int)deleteUsuariosResponse.StatusCode, $"❌ No se pudo eliminar de la tabla Usuarios: {error}");
            }

            // 2. Eliminar después de Auth (auth.users)
            var authResponse = await client.DeleteAsync(
                $"{supabaseUrl}/auth/v1/admin/users/{request.AuthUserId}");

            if (!authResponse.IsSuccessStatusCode)
            {
                var error = await authResponse.Content.ReadAsStringAsync();

                // IMPORTANTE: puedes decidir si restauras el registro en Usuarios aquí (rollback manual si es necesario)

                return StatusCode((int)authResponse.StatusCode, $"❌ Se eliminó de Usuarios pero falló en Auth: {error}");
            }

            return Ok("✅ Usuario eliminado correctamente de la tabla Usuarios y Auth.");
        }

    }
}
