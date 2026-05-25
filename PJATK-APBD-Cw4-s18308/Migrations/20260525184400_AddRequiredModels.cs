using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace PJATK_APBD_Cw4_s18308.Migrations
{
    /// <inheritdoc />
    public partial class AddRequiredModels : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "test-example");

            migrationBuilder.CreateTable(
                name: "ComponentManufacturers",
                schema: "test-example",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Abbreviation = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    FullName = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false),
                    FoundationDate = table.Column<DateOnly>(type: "date", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ComponentManufacturers", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ComponentTypes",
                schema: "test-example",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Abbreviation = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ComponentTypes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PCs",
                schema: "test-example",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Weight = table.Column<double>(type: "float(5)", nullable: false),
                    Warranty = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime", nullable: false),
                    Stock = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PCs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Components",
                schema: "test-example",
                columns: table => new
                {
                    Code = table.Column<string>(type: "char(10)", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ComponentManufacturersId = table.Column<int>(type: "int", nullable: false),
                    ComponentTypesId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Components", x => x.Code);
                    table.ForeignKey(
                        name: "FK_Components_ComponentManufacturers_ComponentManufacturersId",
                        column: x => x.ComponentManufacturersId,
                        principalSchema: "test-example",
                        principalTable: "ComponentManufacturers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Components_ComponentTypes_ComponentTypesId",
                        column: x => x.ComponentTypesId,
                        principalSchema: "test-example",
                        principalTable: "ComponentTypes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PCComponents",
                schema: "test-example",
                columns: table => new
                {
                    PCId = table.Column<int>(type: "int", nullable: false),
                    ComponentCode = table.Column<string>(type: "char(10)", nullable: false),
                    Amount = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PCComponents", x => new { x.PCId, x.ComponentCode });
                    table.ForeignKey(
                        name: "FK_PCComponents_Components_ComponentCode",
                        column: x => x.ComponentCode,
                        principalSchema: "test-example",
                        principalTable: "Components",
                        principalColumn: "Code",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PCComponents_PCs_PCId",
                        column: x => x.PCId,
                        principalSchema: "test-example",
                        principalTable: "PCs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                schema: "test-example",
                table: "ComponentManufacturers",
                columns: new[] { "Id", "Abbreviation", "FoundationDate", "FullName" },
                values: new object[,]
                {
                    { 1, "AMD", new DateOnly(1969, 5, 1), "Advanced Micro Devices" },
                    { 2, "NV", new DateOnly(1993, 4, 5), "NVIDIA Corporation" },
                    { 3, "COR", new DateOnly(1994, 1, 1), "Corsair Gaming Inc." }
                });

            migrationBuilder.InsertData(
                schema: "test-example",
                table: "ComponentTypes",
                columns: new[] { "Id", "Abbreviation", "Name" },
                values: new object[,]
                {
                    { 1, "CPU", "Processor" },
                    { 2, "GPU", "Graphics Card" },
                    { 3, "RAM", "Memory" }
                });

            migrationBuilder.InsertData(
                schema: "test-example",
                table: "PCs",
                columns: new[] { "Id", "CreatedAt", "Name", "Stock", "Warranty", "Weight" },
                values: new object[,]
                {
                    { 1, new DateTime(2026, 5, 8, 9, 0, 0, 0, DateTimeKind.Unspecified), "Gaming Beast X", 5, 36, 12.5 },
                    { 2, new DateTime(2026, 4, 15, 13, 30, 0, 0, DateTimeKind.Unspecified), "Office Mini Pro", 12, 24, 4.2000000000000002 },
                    { 3, new DateTime(2026, 3, 1, 10, 0, 0, 0, DateTimeKind.Unspecified), "Workstation Z", 3, 48, 18.0 }
                });

            migrationBuilder.InsertData(
                schema: "test-example",
                table: "Components",
                columns: new[] { "Code", "ComponentManufacturersId", "ComponentTypesId", "Description", "Name" },
                values: new object[,]
                {
                    { "CPU0000001", 1, 1, "8-core gaming processor", "Ryzen 7 7800X3D" },
                    { "GPU0000001", 2, 2, "High-end gaming graphics card", "RTX 4080 Super" },
                    { "RAM0000001", 3, 3, "DDR5 RAM module 16GB", "Corsair Vengeance DDR5 16GB" }
                });

            migrationBuilder.InsertData(
                schema: "test-example",
                table: "PCComponents",
                columns: new[] { "ComponentCode", "PCId", "Amount" },
                values: new object[,]
                {
                    { "CPU0000001", 1, 1 },
                    { "GPU0000001", 1, 1 },
                    { "RAM0000001", 1, 2 },
                    { "CPU0000001", 2, 1 },
                    { "RAM0000001", 2, 1 },
                    { "CPU0000001", 3, 2 },
                    { "GPU0000001", 3, 2 }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Components_ComponentManufacturersId",
                schema: "test-example",
                table: "Components",
                column: "ComponentManufacturersId");

            migrationBuilder.CreateIndex(
                name: "IX_Components_ComponentTypesId",
                schema: "test-example",
                table: "Components",
                column: "ComponentTypesId");

            migrationBuilder.CreateIndex(
                name: "IX_PCComponents_ComponentCode",
                schema: "test-example",
                table: "PCComponents",
                column: "ComponentCode");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PCComponents",
                schema: "test-example");

            migrationBuilder.DropTable(
                name: "Components",
                schema: "test-example");

            migrationBuilder.DropTable(
                name: "PCs",
                schema: "test-example");

            migrationBuilder.DropTable(
                name: "ComponentManufacturers",
                schema: "test-example");

            migrationBuilder.DropTable(
                name: "ComponentTypes",
                schema: "test-example");
        }
    }
}
