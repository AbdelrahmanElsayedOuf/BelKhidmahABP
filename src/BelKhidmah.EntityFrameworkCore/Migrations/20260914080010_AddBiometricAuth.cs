using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BelKhidmah.Migrations
{
    /// <inheritdoc />
    public partial class AddBiometricAuth : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "BiometricChallenges",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    KeyId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Nonce = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    ExpiresAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsUsed = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BiometricChallenges", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "UserBiometricKeys",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId = table.Column<long>(type: "bigint", nullable: false),
                    DeviceId = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    DeviceName = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: true),
                    PublicKey = table.Column<string>(type: "nvarchar(2048)", maxLength: 2048, nullable: false),
                    KeyAlgorithm = table.Column<string>(type: "nvarchar(16)", maxLength: 16, nullable: false),
                    CreationTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LastUsedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserBiometricKeys", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_BiometricChallenges_KeyId_IsUsed_ExpiresAt",
                table: "BiometricChallenges",
                columns: new[] { "KeyId", "IsUsed", "ExpiresAt" });

            migrationBuilder.CreateIndex(
                name: "IX_UserBiometricKeys_UserId_DeviceId",
                table: "UserBiometricKeys",
                columns: new[] { "UserId", "DeviceId" },
                unique: true,
                filter: "[IsDeleted] = 0");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BiometricChallenges");

            migrationBuilder.DropTable(
                name: "UserBiometricKeys");
        }
    }
}
