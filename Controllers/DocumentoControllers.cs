using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ApiCentralDocsWeb.Data;
using ApiCentralDocsWeb.Model;
using ApiCentralDocsWeb.Model.DTO;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace ApiCentralDocsWeb.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class DocumentoController : ControllerBase
    {
        private readonly AppDbContext _context;

        public DocumentoController(AppDbContext context)
        {
            _context = context;
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

        private static DocumentoDTO ParaDTO(Documento documento)
        {
            return new DocumentoDTO
            {
                Id = documento.Id,
                Numero = documento.Numero,
                OrgaoEmissor = documento.OrgaoEmissor,
                CidadeEmissao = documento.CidadeEmissao,
                DataEmissao = documento.DataEmissao,
                Usuario = documento.Usuario?.Nome,
                Tipo = documento.TipoDocumento?.Nome
            };
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var usuarioId = ObterUsuarioIdLogado();

            if (usuarioId == null)
                return Unauthorized("Usuário não identificado no token.");

            var documentos = await _context.Documentos
                .Include(d => d.Usuario)
                .Include(d => d.TipoDocumento)
                .Where(d => d.UsuarioId == usuarioId && d.Ativo)
                .ToListAsync();

            var resultado = documentos.Select(ParaDTO).ToList();

            return Ok(resultado);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var usuarioId = ObterUsuarioIdLogado();

            if (usuarioId == null)
                return Unauthorized("Usuário não identificado no token.");

            var documento = await _context.Documentos
                .Include(d => d.Usuario)
                .Include(d => d.TipoDocumento)
                .FirstOrDefaultAsync(d => d.Id == id && d.UsuarioId == usuarioId && d.Ativo);

            if (documento == null)
                return NotFound("Documento não encontrado");

            return Ok(ParaDTO(documento));
        }

        [HttpPost]
        public async Task<IActionResult> Criar([FromBody] CriarDocumentoDTO dados)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var usuarioId = ObterUsuarioIdLogado();

            if (usuarioId == null)
                return Unauthorized("Usuário não identificado no token.");

            var usuario = await _context.Usuarios.FindAsync(usuarioId);

            if (usuario == null)
                return BadRequest("Usuário não encontrado");

            var tipo = await _context.TiposDocumento.FindAsync(dados.TipoDocumentoId);

            if (tipo == null)
                return BadRequest("Tipo de documento não encontrado");

            var documento = new Documento
            {
                Numero = dados.Numero,
                OrgaoEmissor = dados.OrgaoEmissor,
                DataEmissao = dados.DataEmissao,
                CidadeEmissao = dados.CidadeEmissao,
                UsuarioId = usuarioId.Value,
                TipoDocumentoId = dados.TipoDocumentoId
            };

            _context.Documentos.Add(documento);
            await _context.SaveChangesAsync();

            // Recarrega o documento com as navegações preenchidas para o DTO funcionar perfeitamente
            var documentoCriado = await _context.Documentos
                .Include(d => d.Usuario)
                .Include(d => d.TipoDocumento)
                .FirstAsync(d => d.Id == documento.Id);

            return Ok(ParaDTO(documentoCriado));
        }

        /// <summary>
        /// Soft delete: o documento não é removido do banco, só marcado
        /// como inativo (Ativo = false). Ele deixa de aparecer no GetAll
        /// e no GetById, mas continua existindo pra fins de histórico.
        /// </summary>
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var usuarioId = ObterUsuarioIdLogado();

            if (usuarioId == null)
                return Unauthorized("Usuário não identificado no token.");

            var documento = await _context.Documentos
                .FirstOrDefaultAsync(d => d.Id == id && d.UsuarioId == usuarioId && d.Ativo);

            if (documento == null)
                return NotFound("Documento não encontrado");

            documento.Ativo = false;
            await _context.SaveChangesAsync();

            return Ok("Documento excluído com sucesso");
        }
    }
}