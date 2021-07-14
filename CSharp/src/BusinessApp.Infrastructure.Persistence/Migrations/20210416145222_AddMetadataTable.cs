using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace BusinessApp.Infrastructure.Persistence.Migrations
{
    public partial class AddMetadataTable : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "dbo");

            migrationBuilder.CreateTable(
                name: "Metadata",
                schema: "dbo",
                columns: table => new
                {
                    MetadataId = table.Column<long>(type: "bigint", nullable: false),
                    Username = table.Column<string>(type: "varchar(100)", nullable: false),
                    DataSetName = table.Column<string>(type: "varchar(100)", nullable: false),
                    TypeName = table.Column<string>(type: "varchar(100)", nullable: false),
                    OccurredUtc = table.Column<DateTimeOffset>(type: "datetimeoffset(0)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Metadata", x => x.MetadataId);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Metadata",
                schema: "dbo");
        }
    }
}
