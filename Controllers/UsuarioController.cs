using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using ApiCentralDocsWeb.Model.DTO;
using ApiCentralDocsWeb.Services;
using System.Security.Claims;

namespace ApiCentralDocsWeb.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class UsuarioController : ControllerBase
    {
        private readonly UsuarioService _usuarioService;

        public UsuarioController(UsuarioService usuarioService)
        {
            _usuarioService = usuarioService;
        }

        private int? ObterUsuarioIdLogado()
        {
            var claim = User.FindFirst(ClaimTypes.NameIdentifier)
                        ?? User.FindFirst("sub");

            if (claim != null && int.TryParse(claim.Value, out int id))
            {
                return id;
            }

            return null;
        }

        // ============================================================
        // LOGIN
        // POST: /api/Usuario/login
        // ============================================================
        [HttpPost("login")]
        [AllowAnonymous]
        public async Task<IActionResult> Login([FromBody] LoginDTO dados)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var resultado = await _usuarioService.Login(dados);

            return Ok(resultado);
        }

        // ============================================================
        // BUSCAR TODOS OS USUÁRIOS
        // GET: /api/Usuario/GetAll
        // ============================================================
        [HttpGet("GetAll")]
        public async Task<IActionResult> GetAllUsuarios()
        {
            var usuarioLogadoId = ObterUsuarioIdLogado();

            if (usuarioLogadoId == null)
                return Unauthorized("Usuário não identificado no token.");

            var usuarios =
                await _usuarioService.GetAllUsuarios(usuarioLogadoId.Value);

            return Ok(usuarios);
        }

        // ============================================================
        // BUSCAR USUÁRIO POR ID
        // GET: /api/Usuario/GetById/{id}
        // ============================================================
        [HttpGet("GetById/{id}")]
        public async Task<IActionResult> GetUsuarioById([FromRoute] int id)
        {
            var usuarioLogadoId = ObterUsuarioIdLogado();

            if (usuarioLogadoId == null)
                return Unauthorized("Usuário não identificado no token.");

            if (usuarioLogadoId != id)
                return Forbid();

            var usuario =
                await _usuarioService.GetUsuarioById(id);

            if (usuario == null)
            {
                return BadRequest(new
                {
                    erro = true,
                    mensagem = $"Usuário com id {id} não encontrado"
                });
            }

            return Ok(usuario);
        }

        // ============================================================
        // CRIAR USUÁRIO
        // POST: /api/Usuario/CriarUsuario
        // ============================================================
        [HttpPost("CriarUsuario")]
        [AllowAnonymous]
        public async Task<IActionResult> CriarUsuario(
            [FromBody] CriarUsuarioDTO dadosUsuario)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var resultado =
                await _usuarioService.CriarUsuario(dadosUsuario);

            var propErro =
                resultado?.GetType().GetProperty("Erro");

            if (propErro != null)
            {
                bool temErro =
                    (bool)propErro.GetValue(resultado)!;

                if (temErro)
                    return BadRequest(resultado);
            }

            return Ok(resultado);
        }

        // ============================================================
        // DELETAR USUÁRIO
        // DELETE: /api/Usuario/DeletarUsuario/{id}
        // ============================================================
        [HttpDelete("DeletarUsuario/{id}")]
        public async Task<IActionResult> DeletarUsuario(
            [FromRoute] int id)
        {
            var usuarioLogadoId = ObterUsuarioIdLogado();

            if (usuarioLogadoId == null)
                return Unauthorized("Usuário não identificado no token.");

            if (usuarioLogadoId != id)
                return Forbid();

            var resultado =
                await _usuarioService.DeletarUsuario(id);

            var propErro =
                resultado?.GetType().GetProperty("Erro");

            if (propErro != null)
            {
                bool temErro =
                    (bool)propErro.GetValue(resultado)!;

                if (temErro)
                    return BadRequest(resultado);
            }

            return Ok(new
            {
                mensagem =
                    $"Usuário com id {id} foi deletado com sucesso"
            });
        }
    }
}