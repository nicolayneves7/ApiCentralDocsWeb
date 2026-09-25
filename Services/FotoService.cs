using ApiCentralDocsWeb.Data;
using ApiCentralDocsWeb.Model;
using ApiCentralDocsWeb.Model.DTO;
using Microsoft.EntityFrameworkCore;

namespace ApiCentralDocsWeb.Services
{
    public class FotoService
    {
        private readonly AppDbContext _context;

        public FotoService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<List<FotoDTO>> GetAll(int usuarioLogadoId)
        {
            return await _context.Fotos
                .Where(f => f.Documento.UsuarioId == usuarioLogadoId)
                .Select(f => new FotoDTO
                {
                    Id = f.Id,
                    Url = f.Url,
                    Descricao = f.Descricao,
                    DocumentoId = f.DocumentoId
                })
                .ToListAsync();
        }

        public async Task<dynamic> Criar(CriarFotoDTO dados, int usuarioLogadoId)
        {
            var documento = await _context.Documentos.FindAsync(dados.DocumentoId);

            if (documento == null)
                return new { Erro = true, Mensagem = "Documento não encontrado" };

            if (documento.UsuarioId != usuarioLogadoId)
                return new { Erro = true, Proibido = true, Mensagem = "Esse documento não pertence a você" };

            var foto = new Foto
            {
                Url = dados.Url,
                DocumentoId = dados.DocumentoId,
                Descricao = dados.Descricao
            };

            _context.Fotos.Add(foto);
            await _context.SaveChangesAsync();

            var fotoDto = new FotoDTO
            {
                Id = foto.Id,
                Url = foto.Url,
                Descricao = foto.Descricao,
                DocumentoId = foto.DocumentoId
            };

            return new { Erro = false, Foto = fotoDto };
        }

        public async Task<dynamic> Deletar(int id, int usuarioLogadoId)
        {
            var foto = await _context.Fotos
                .Include(f => f.Documento)
                .FirstOrDefaultAsync(f => f.Id == id);

            if (foto == null)
                return new { Erro = true, Mensagem = "Foto não encontrada" };

            if (foto.Documento.UsuarioId != usuarioLogadoId)
                return new { Erro = true, Proibido = true, Mensagem = "Essa foto não pertence a você" };

            _context.Fotos.Remove(foto);
            await _context.SaveChangesAsync();

            return new { Erro = false };
        }
    }
}