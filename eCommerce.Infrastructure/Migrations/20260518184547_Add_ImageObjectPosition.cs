using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace eCommerce.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Add_ImageObjectPosition : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                DO $$
                BEGIN
                    IF NOT EXISTS (
                        SELECT 1 FROM information_schema.columns
                        WHERE table_name = 'ProductImages' AND column_name = 'ObjectPosition'
                    ) THEN
                        ALTER TABLE ""ProductImages"" ADD ""ObjectPosition"" text;
                    END IF;
                END $$;
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                DO $$
                BEGIN
                    IF EXISTS (
                        SELECT 1 FROM information_schema.columns
                        WHERE table_name = 'ProductImages' AND column_name = 'ObjectPosition'
                    ) THEN
                        ALTER TABLE ""ProductImages"" DROP COLUMN ""ObjectPosition"";
                    END IF;
                END $$;
            ");
        }
    }
}
