using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace BusinessApp.Test.Shared.Migrations
{
    public partial class CreateTestDb : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "evt");

            migrationBuilder.EnsureSchema(
                name: "dbo");

            migrationBuilder.CreateTable(
                name: "AggregateRootStub",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AggregateRootStub", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "EventStub",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false),
                    OccurredUtc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EventStub", x => x.Id);
                });

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

            migrationBuilder.CreateTable(
                name: "RequestMetadata",
                schema: "dbo",
                columns: table => new
                {
                    RequestMetadataId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RequestType = table.Column<string>(type: "varchar(100)", nullable: false),
                    ResponseType = table.Column<string>(type: "varchar(100)", nullable: false),
                    EventTriggers = table.Column<string>(type: "varchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RequestMetadata", x => x.RequestMetadataId);
                });

            migrationBuilder.CreateTable(
                name: "RequestStub",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RequestStub", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ResponseStub",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ResponseStubId = table.Column<int>(type: "int", nullable: true)
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
                name: "DeleteEvent",
                schema: "evt",
                columns: table => new
                {
                    EventMetadataId = table.Column<long>(type: "bigint", nullable: false),
                    Event_Id = table.Column<int>(type: "int", nullable: true),
                    EventName = table.Column<string>(type: "varchar(500)", nullable: false),
                    CausationId = table.Column<long>(type: "bigint", nullable: false),
                    CorrelationId = table.Column<long>(type: "bigint", nullable: false),
                    OccurredUtc = table.Column<DateTimeOffset>(type: "datetimeoffset(0)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DeleteEvent", x => x.EventMetadataId);
                    table.ForeignKey(
                        name: "FK_DeleteEvent_Metadata_CorrelationId",
                        column: x => x.CorrelationId,
                        principalSchema: "dbo",
                        principalTable: "Metadata",
                        principalColumn: "MetadataId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "DeleteQuery",
                columns: table => new
                {
                    DeleteQueryRequestId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DeleteQueryId = table.Column<int>(type: "int", nullable: true),
                    MetadataId = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DeleteQuery", x => x.DeleteQueryRequestId);
                    table.ForeignKey(
                        name: "FK_DeleteQuery_Metadata_MetadataId",
                        column: x => x.MetadataId,
                        principalSchema: "dbo",
                        principalTable: "Metadata",
                        principalColumn: "MetadataId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PostOrPutBody",
                columns: table => new
                {
                    PostOrPutBodyRequestId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    LongerId = table.Column<long>(type: "bigint", nullable: false),
                    PostOrPutId = table.Column<int>(type: "int", nullable: true),
                    MetadataId = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PostOrPutBody", x => x.PostOrPutBodyRequestId);
                    table.ForeignKey(
                        name: "FK_PostOrPutBody_Metadata_MetadataId",
                        column: x => x.MetadataId,
                        principalSchema: "dbo",
                        principalTable: "Metadata",
                        principalColumn: "MetadataId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ChildResponseStub",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ResponseStubId = table.Column<int>(type: "int", nullable: false)
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
                name: "IX_DeleteEvent_CorrelationId",
                schema: "evt",
                table: "DeleteEvent",
                column: "CorrelationId");

            migrationBuilder.CreateIndex(
                name: "IX_DeleteQuery_MetadataId",
                table: "DeleteQuery",
                column: "MetadataId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PostOrPutBody_MetadataId",
                table: "PostOrPutBody",
                column: "MetadataId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ResponseStub_ResponseStubId",
                table: "ResponseStub",
                column: "ResponseStubId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AggregateRootStub");

            migrationBuilder.DropTable(
                name: "ChildResponseStub");

            migrationBuilder.DropTable(
                name: "DeleteEvent",
                schema: "evt");

            migrationBuilder.DropTable(
                name: "DeleteQuery");

            migrationBuilder.DropTable(
                name: "EventStub");

            migrationBuilder.DropTable(
                name: "PostOrPutBody");

            migrationBuilder.DropTable(
                name: "RequestMetadata",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "RequestStub");

            migrationBuilder.DropTable(
                name: "ResponseStub");

            migrationBuilder.DropTable(
                name: "Metadata",
                schema: "dbo");
        }
    }
}
