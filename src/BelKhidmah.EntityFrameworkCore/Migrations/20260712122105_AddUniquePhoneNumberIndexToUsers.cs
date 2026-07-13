using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BelKhidmah.Migrations
{
    /// <inheritdoc />
    public partial class AddUniquePhoneNumberIndexToUsers : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
CREATE UNIQUE NONCLUSTERED INDEX [IX_AbpUsers_TenantId_PhoneNumber_Unique]
ON [AbpUsers]([TenantId], [PhoneNumber])
WHERE [PhoneNumber] IS NOT NULL AND [IsDeleted] = 0;");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP INDEX [IX_AbpUsers_TenantId_PhoneNumber_Unique] ON [AbpUsers];");
        }
    }
}
