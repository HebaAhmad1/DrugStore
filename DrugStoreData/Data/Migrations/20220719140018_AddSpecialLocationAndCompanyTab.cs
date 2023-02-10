using Microsoft.EntityFrameworkCore.Migrations;

namespace DrugStore.Data.Migrations
{
    public partial class AddSpecialLocationAndCompanyTab : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "NewConacts",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Email = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Message = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NewConacts", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SpecialCompanies",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CID = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SpecialCompanies", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SpecialLocations",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Username = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LocationName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Latitude = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Longitude = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CID = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SpecialCompanyId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SpecialLocations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SpecialLocations_SpecialCompanies_SpecialCompanyId",
                        column: x => x.SpecialCompanyId,
                        principalTable: "SpecialCompanies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_SpecialCompanies_CID",
                table: "SpecialCompanies",
                column: "CID",
                unique: true,
                filter: "[CID] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_SpecialLocations_SpecialCompanyId",
                table: "SpecialLocations",
                column: "SpecialCompanyId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "NewConacts");

            migrationBuilder.DropTable(
                name: "SpecialLocations");

            migrationBuilder.DropTable(
                name: "SpecialCompanies");
        }
    }
}
