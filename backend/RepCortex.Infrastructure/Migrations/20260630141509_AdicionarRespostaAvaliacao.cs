using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RepCortex.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AdicionarRespostaAvaliacao : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Resposta",
                table: "Avaliacoes",
                type: "character varying(2000)",
                maxLength: 2000,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Resposta",
                table: "Avaliacoes");
        }
    }
}
