using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using RepCortex.Infrastructure.Data;

#nullable disable

namespace RepCortex.Infrastructure.Migrations
{
    [DbContext(typeof(AppDbContext))]
    [Migration("20260618080000_SepararPublishableKeyESecretKey")]
    public partial class SepararPublishableKeyESecretKey : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "ApiKey",
                table: "Tenants",
                newName: "PublishableKey");

            migrationBuilder.RenameIndex(
                name: "IX_Tenants_ApiKey",
                table: "Tenants",
                newName: "IX_Tenants_PublishableKey");

            migrationBuilder.AddColumn<string>(
                name: "SecretKey",
                table: "Tenants",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.Sql("""
                UPDATE "Tenants"
                SET "SecretKey" = 'rc_sec_' || md5(random()::text || clock_timestamp()::text || "Id")
                WHERE "SecretKey" = '';
                """);

            migrationBuilder.CreateIndex(
                name: "IX_Tenants_SecretKey",
                table: "Tenants",
                column: "SecretKey",
                unique: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Tenants_SecretKey",
                table: "Tenants");

            migrationBuilder.DropColumn(
                name: "SecretKey",
                table: "Tenants");

            migrationBuilder.RenameColumn(
                name: "PublishableKey",
                table: "Tenants",
                newName: "ApiKey");

            migrationBuilder.RenameIndex(
                name: "IX_Tenants_PublishableKey",
                table: "Tenants",
                newName: "IX_Tenants_ApiKey");
        }
    }
}
