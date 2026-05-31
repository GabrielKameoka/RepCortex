using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RepCortex.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AdicionarTenantId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "TenantId",
                table: "Avaliacoes",
                type: "text",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "Avaliacoes");
        }
    }
}
