using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RepCortex.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AdicionarApiKeyETenantDomains : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ApiKey",
                table: "Tenants",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "DominiosAutorizados",
                table: "Tenants",
                type: "character varying(500)",
                maxLength: 500,
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_Tenants_ApiKey",
                table: "Tenants",
                column: "ApiKey",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Tenants_ApiKey",
                table: "Tenants");

            migrationBuilder.DropColumn(
                name: "ApiKey",
                table: "Tenants");

            migrationBuilder.DropColumn(
                name: "DominiosAutorizados",
                table: "Tenants");
        }
    }
}
