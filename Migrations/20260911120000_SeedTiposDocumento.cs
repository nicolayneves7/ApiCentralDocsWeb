using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ApiCentralDocsWeb.Migrations
{
    /// <inheritdoc />
    public partial class SeedTiposDocumento : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "TiposDocumento",
                columns: new[] { "Id", "Nome" },
                values: new object[,]
                {
                    { 1, "RG" },
                    { 2, "CPF" },
                    { 3, "CNH" },
                    { 4, "Certidão de Nascimento" },
                    { 5, "Certidão de Casamento" },
                    { 6, "Passaporte" },
                    { 7, "Título de Eleitor" },
                    { 8, "Carteira de Trabalho (CTPS)" },
                    { 9, "Certificado de Reservista" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "TiposDocumento",
                keyColumn: "Id",
                keyValues: new object[] { 1, 2, 3, 4, 5, 6, 7, 8, 9 });
        }
    }
}