using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace ApiCentralDocsWeb.Model
{
    public class Documento
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Número do documento é obrigatório")]
        public string Numero { get; set; } = string.Empty;

        [Required(ErrorMessage = "Órgão emissor é obrigatório")]
        public string OrgaoEmissor { get; set; } = string.Empty;

        public DateTime DataEmissao { get; set; }

        public string CidadeEmissao { get; set; } = string.Empty;
        public bool Ativo { get; set; } = true;

        [ForeignKey("Usuario")]
        public int UsuarioId { get; set; }


        [JsonIgnore]

        public Usuario? Usuario { get; set; }

        [ForeignKey("TipoDocumento")]
        public int TipoDocumentoId { get; set; }

        public TipoDocumento? TipoDocumento { get; set; }

        public List<Foto> Fotos { get; set; } = new();
    }
}