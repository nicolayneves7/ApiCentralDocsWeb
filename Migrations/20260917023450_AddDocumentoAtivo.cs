using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ApiCentralDocsWeb.Migrations
{
    /// <inheritdoc />
    public partial class AddDocumentoAtivo : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "Ativo",
                table: "Documentos",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Ativo",
                table: "Documentos");
        }
    }
}
