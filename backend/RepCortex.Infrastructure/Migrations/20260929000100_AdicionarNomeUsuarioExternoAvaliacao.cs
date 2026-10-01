using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using RepCortex.Infrastructure.Data;

#nullable disable

namespace RepCortex.Infrastructure.Migrations;

[DbContext(typeof(AppDbContext))]
[Migration("20260929000100_AdicionarNomeUsuarioExternoAvaliacao")]
public partial class AdicionarNomeUsuarioExternoAvaliacao : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<string>(
            name: "NomeUsuarioExterno",
            table: "Avaliacoes",
            type: "character varying(100)",
            maxLength: 100,
            nullable: true);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(
            name: "NomeUsuarioExterno",
            table: "Avaliacoes");
    }
}
