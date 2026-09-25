using ApiCentralDocsWeb.Model;
using Microsoft.EntityFrameworkCore;

namespace ApiCentralDocsWeb.Data
{
    public class CentralDocsContext : DbContext
    {
        public CentralDocsContext(
            DbContextOptions<CentralDocsContext> options) : base(options)
        {
        }

        // ============================================================
        // TABELAS DO BANCO
        // ============================================================

        public DbSet<Usuario> Usuarios { get; set; }

        public DbSet<Documento> Documentos { get; set; }

        public DbSet<TipoDocumento> TiposDocumento { get; set; }
    }
}

