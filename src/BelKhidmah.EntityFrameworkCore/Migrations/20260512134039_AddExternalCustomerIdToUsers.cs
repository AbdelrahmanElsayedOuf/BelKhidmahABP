using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BelKhidmah.Migrations
{
    /// <inheritdoc />
    public partial class AddExternalCustomerIdToUsers : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "ExternalCustomerId",
                table: "AbpUsers",
                type: "uniqueidentifier",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ExternalCustomerId",
                table: "AbpUsers");
        }
    }
}
