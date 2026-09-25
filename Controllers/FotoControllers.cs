using ApiCentralDocsWeb.Model.DTO;
using ApiCentralDocsWeb.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace ApiCentralDocsWeb.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class FotoController : ControllerBase
    {
        private readonly FotoService _service;

        public FotoController(FotoService service)
        {
            _service = service;
        }

        private int? ObterUsuarioIdLogado()
        {
            var idClaim =
                User.FindFirst(ClaimTypes.NameIdentifier)?.Value ??
                User.FindFirst("id")?.Value ??
                User.FindFirst("Id")?.Value ??
                User.FindFirst("sub")?.Value;

            if (int.TryParse(idClaim, out int usuarioId))
            {
                return usuarioId;
            }

            return null;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var usuarioLogadoId = ObterUsuarioIdLogado();

            if (usuarioLogadoId == null)
                return Unauthorized("Usuário não identificado no token.");

            var fotos = await _service.GetAll(usuarioLogadoId.Value);
            return Ok(fotos);
        }

        [HttpPost]
        public async Task<IActionResult> Criar([FromBody] CriarFotoDTO dados)
        {
            var usuarioLogadoId = ObterUsuarioIdLogado();

            if (usuarioLogadoId == null)
                return Unauthorized("Usuário não identificado no token.");

            var result = await _service.Criar(dados, usuarioLogadoId.Value);

            if (result.Erro)
                return result.Proibido == true ? Forbid() : BadRequest(result);

            return Ok(result);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Deletar(int id)
        {
            var usuarioLogadoId = ObterUsuarioIdLogado();

            if (usuarioLogadoId == null)
                return Unauthorized("Usuário não identificado no token.");

            var result = await _service.Deletar(id, usuarioLogadoId.Value);

            if (result.Erro)
                return result.Proibido == true ? Forbid() : NotFound(result);

            return Ok(result);
        }
    }
}