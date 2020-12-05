using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace BusinessApp.Data.Migrations
{
    public partial class CreateEventMetadata : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "evt");

            migrationBuilder.CreateSequence(
                name: "EventIds",
                schema: "evt");

            migrationBuilder.CreateTable(
                name: "EventMetadata",
                schema: "evt",
                columns: table => new
                {
                    EventMetadataId = table.Column<long>(nullable: false),
                    CorrelationId = table.Column<long>(nullable: true, defaultValueSql: "NEXT VALUE FOR evt.EventIds"),
                    EventDisplayText = table.Column<string>(type: "varchar(500)", nullable: false),
                    EventCreator = table.Column<string>(type: "varchar(50)", nullable: false),
                    OccurredUtc = table.Column<DateTimeOffset>(type: "datetimeoffset(0)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EventMetadata", x => x.EventMetadataId);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "EventMetadata",
                schema: "evt");

            migrationBuilder.DropSequence(
                name: "EventIds",
                schema: "evt");
        }
    }
}
