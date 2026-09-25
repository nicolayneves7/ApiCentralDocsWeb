using ApiCentralDocsWeb.Data;
using ApiCentralDocsWeb.Interfaces;
using ApiCentralDocsWeb.Model;
using ApiCentralDocsWeb.Model.DTO;
using Microsoft.EntityFrameworkCore;

namespace ApiCentralDocsWeb.Services
{
    public class UsuarioService
    {
        private readonly AppDbContext _context;
        private readonly ITokenService _tokenService;

        public UsuarioService(AppDbContext context, ITokenService tokenService)
        {
            _context = context;
            _tokenService = tokenService;
        }

        private static UsuarioDTO ParaDTO(Usuario usuario)
        {
            return new UsuarioDTO
            {
                Id = usuario.Id,
                Nome = usuario.Nome,
                CPF = usuario.CPF,
                Email = usuario.Email,
                DataCriacao = usuario.DataCriacao,
                Ativo = usuario.Ativo
            };
        }

        public async Task<List<UsuarioDTO>> GetAllUsuarios(int usuarioLogadoId)
        {
            var usuarios = await _context.Usuarios
                .Where(u => u.Id == usuarioLogadoId)
                .ToListAsync();

            return usuarios.Select(ParaDTO).ToList();
        }

        public async Task<dynamic> GetUsuarioById(int id)
        {
            var usuario = await _context.Usuarios.FindAsync(id);

            if (usuario == null)
            {
                return new
                {
                    Erro = true,
                    Mensagem = $"Usuário com id {id} não encontrado"
                };
            }

            return new
            {
                Erro = false,
                Usuario = ParaDTO(usuario)
            };
        }

        public async Task<dynamic> CriarUsuario(CriarUsuarioDTO dados)
        {
            if (dados.Senha != dados.ConfirmarSenha)
            {
                return new
                {
                    Erro = true,
                    Mensagem = "As senhas não coincidem"
                };
            }
            var usuarioExistente = await _context.Usuarios
                .FirstOrDefaultAsync(usuario => usuario.CPF == dados.CPF || usuario.Email == dados.Email);

            if (usuarioExistente != null)
            {
                if (usuarioExistente.CPF == dados.CPF)
                {
                    return new { Erro = true, Mensagem = "CPF já cadastrado" };
                }

                if (usuarioExistente.Email == dados.Email)
                {
                    return new { Erro = true, Mensagem = "E-mail já cadastrado" };
                }
            }

            var usuario = new Usuario
            {
                Nome = dados.Nome,
                CPF = dados.CPF,
                Email = dados.Email,
                Senha = CryptoService.EncryptPassword(dados.Senha)
            };

            _context.Usuarios.Add(usuario);
            await _context.SaveChangesAsync();

            return new { Erro = false, Usuario = ParaDTO(usuario) };
        }

        public async Task<dynamic> AtualizarUsuario(int id, AtualizarUsuarioDTO dados)
        {
            var usuario = await _context.Usuarios.FindAsync(id);

            if (usuario == null)
                return new { Erro = true, Mensagem = "Usuário não encontrado" };

            usuario.Nome = dados.Nome;
            usuario.Email = dados.Email;

            if (!string.IsNullOrEmpty(dados.Senha))
            {
                usuario.Senha = CryptoService.EncryptPassword(dados.Senha);
            }

            await _context.SaveChangesAsync();

            return new { Erro = false, Usuario = ParaDTO(usuario) };
        }

        public async Task<dynamic> DeletarUsuario(int id)
        {
            var usuario = await _context.Usuarios.FindAsync(id);

            if (usuario == null)
                return new { Erro = true, Mensagem = "Usuário não encontrado" };

            _context.Usuarios.Remove(usuario);
            await _context.SaveChangesAsync();

            return new { Erro = false };
        }
        public async Task<dynamic> AlterarSenha(int id, AlterarSenhaDTO dados)
        {
            var usuario = await _context.Usuarios.FindAsync(id);

            if (usuario == null)
                return new { Erro = true, Mensagem = "Usuário não encontrado" };

            bool senhaAtualValida = CryptoService.VerifyPassword(dados.SenhaAtual, usuario.Senha);

            if (!senhaAtualValida)
                return new { Erro = true, Mensagem = "Senha atual incorreta" };

            if (string.IsNullOrWhiteSpace(dados.NovaSenha) || dados.NovaSenha.Length < 6)
                return new { Erro = true, Mensagem = "A nova senha deve ter pelo menos 6 caracteres" };

            usuario.Senha = CryptoService.EncryptPassword(dados.NovaSenha);
            await _context.SaveChangesAsync();

            return new { Erro = false, Mensagem = "Senha alterada com sucesso" };
        }

        public async Task<dynamic> Login(LoginDTO dados)
        {
            var usuario = await _context.Usuarios
                .Include(u => u.Documentos)
                    .ThenInclude(d => d.TipoDocumento)
                .FirstOrDefaultAsync(u => u.Email == dados.Email);

            if (usuario == null)
            {
                return new { Erro = true, Mensagem = "Email ou senha inválidos" };
            }

            bool senhaValida = CryptoService.VerifyPassword(dados.Senha, usuario.Senha);

            if (!senhaValida)
            {
                return new { Erro = true, Mensagem = "Email ou senha inválidos" };
            }

            var token = _tokenService.GerarToken(usuario);
            return new
            {
                Erro = false,
                Token = token,
                Usuario = new
                {
                    usuario.Id,
                    usuario.Nome,
                    usuario.Email,
                    Documentos = usuario.Documentos.Select(d => new
                    {
                        d.Id,
                        d.Numero,
                        d.OrgaoEmissor,
                        d.CidadeEmissao,
                        Tipo = d.TipoDocumento.Nome
                    })
                }
            };
        }
    }
}