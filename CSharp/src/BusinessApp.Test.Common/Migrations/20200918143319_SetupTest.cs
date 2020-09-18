using Microsoft.EntityFrameworkCore.Migrations;

namespace BusinessApp.Test.Common.Migrations
{
    public partial class SetupTest : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "DomainEventStub",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DomainEventStub", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ResponseStub",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ResponseStubId = table.Column<int>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ResponseStub", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ResponseStub_ResponseStub_ResponseStubId",
                        column: x => x.ResponseStubId,
                        principalTable: "ResponseStub",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ChildResponseStub",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ResponseStubId = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChildResponseStub", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ChildResponseStub_ResponseStub_ResponseStubId",
                        column: x => x.ResponseStubId,
                        principalTable: "ResponseStub",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ChildResponseStub_ResponseStubId",
                table: "ChildResponseStub",
                column: "ResponseStubId");

            migrationBuilder.CreateIndex(
                name: "IX_ResponseStub_ResponseStubId",
                table: "ResponseStub",
                column: "ResponseStubId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ChildResponseStub");

            migrationBuilder.DropTable(
                name: "DomainEventStub");

            migrationBuilder.DropTable(
                name: "ResponseStub");
        }
    }
}
