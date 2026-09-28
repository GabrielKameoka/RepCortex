using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RepCortex.Infrastructure.Migrations;

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
