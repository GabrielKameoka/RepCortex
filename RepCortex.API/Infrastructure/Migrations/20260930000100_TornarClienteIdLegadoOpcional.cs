using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using RepCortex.Infrastructure.Data;

#nullable disable

namespace RepCortex.Infrastructure.Migrations;

[DbContext(typeof(AppDbContext))]
[Migration("20260930000100_TornarClienteIdLegadoOpcional")]
public partial class TornarClienteIdLegadoOpcional : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AlterColumn<string>(
            name: "ClienteId",
            table: "Avaliacoes",
            type: "character varying(100)",
            maxLength: 100,
            nullable: true,
            oldClrType: typeof(string),
            oldType: "character varying(100)",
            oldMaxLength: 100);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql("UPDATE \"Avaliacoes\" SET \"ClienteId\" = \"UsuarioIdExterno\" WHERE \"ClienteId\" IS NULL");
        migrationBuilder.AlterColumn<string>(
            name: "ClienteId",
            table: "Avaliacoes",
            type: "character varying(100)",
            maxLength: 100,
            nullable: false,
            oldClrType: typeof(string),
            oldType: "character varying(100)",
            oldMaxLength: 100,
            oldNullable: true);
    }
}
