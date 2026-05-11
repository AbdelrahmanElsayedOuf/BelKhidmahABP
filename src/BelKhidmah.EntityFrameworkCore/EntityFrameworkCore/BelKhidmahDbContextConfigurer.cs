using System.Data.Common;
using Microsoft.EntityFrameworkCore;

namespace BelKhidmah.EntityFrameworkCore
{
    public static class BelKhidmahDbContextConfigurer
    {
        public static void Configure(DbContextOptionsBuilder<BelKhidmahDbContext> builder, string connectionString)
        {
            builder.UseSqlServer(connectionString);
        }

        public static void Configure(DbContextOptionsBuilder<BelKhidmahDbContext> builder, DbConnection connection)
        {
            builder.UseSqlServer(connection);
        }
    }
}
