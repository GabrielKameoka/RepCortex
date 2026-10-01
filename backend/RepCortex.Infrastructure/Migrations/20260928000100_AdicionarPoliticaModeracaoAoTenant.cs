using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using RepCortex.Infrastructure.Data;

#nullable disable

namespace RepCortex.Infrastructure.Migrations;

[DbContext(typeof(AppDbContext))]
[Migration("20260928000100_AdicionarPoliticaModeracaoAoTenant")]
public partial class AdicionarPoliticaModeracaoAoTenant : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<string>(
            name: "PoliticaModeracao",
            table: "Tenants",
            type: "character varying(30)",
            maxLength: 30,
            nullable: false,
            defaultValue: "Automatica");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(
            name: "PoliticaModeracao",
            table: "Tenants");
    }
}
