using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VgcCollege.Web.Migrations
{
    public partial class MakeIdentityUserIdNullable : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("PRAGMA foreign_keys=0;");
            
            migrationBuilder.Sql("CREATE TEMPORARY TABLE __OldStudentProfiles AS SELECT * FROM StudentProfiles;");
            
            migrationBuilder.DropTable(name: "StudentProfiles");
            
            migrationBuilder.CreateTable(
                name: "StudentProfiles",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    IdentityUserId = table.Column<string>(type: "TEXT", nullable: true),
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    Email = table.Column<string>(type: "TEXT", nullable: false),
                    Phone = table.Column<string>(type: "TEXT", nullable: false),
                    Address = table.Column<string>(type: "TEXT", nullable: false),
                    DateOfBirth = table.Column<DateTime>(type: "TEXT", nullable: false),
                    StudentNumber = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StudentProfiles", x => x.Id);
                    table.ForeignKey(
                        name: "FK_StudentProfiles_AspNetUsers_IdentityUserId",
                        column: x => x.IdentityUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                });
            
            migrationBuilder.Sql("INSERT INTO StudentProfiles (Id, IdentityUserId, Name, Email, Phone, Address, DateOfBirth, StudentNumber) SELECT Id, IdentityUserId, Name, Email, Phone, Address, DateOfBirth, StudentNumber FROM __OldStudentProfiles;");
            
            migrationBuilder.Sql("DROP TABLE __OldStudentProfiles;");
            
            migrationBuilder.Sql("PRAGMA foreign_keys=1;");
            
            migrationBuilder.CreateIndex(
                name: "IX_StudentProfiles_IdentityUserId",
                table: "StudentProfiles",
                column: "IdentityUserId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("PRAGMA foreign_keys=0;");
            
            migrationBuilder.Sql("CREATE TEMPORARY TABLE __OldStudentProfiles AS SELECT * FROM StudentProfiles;");
            
            migrationBuilder.DropTable(name: "StudentProfiles");
            
            migrationBuilder.CreateTable(
                name: "StudentProfiles",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    IdentityUserId = table.Column<string>(type: "TEXT", nullable: false),
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    Email = table.Column<string>(type: "TEXT", nullable: false),
                    Phone = table.Column<string>(type: "TEXT", nullable: false),
                    Address = table.Column<string>(type: "TEXT", nullable: false),
                    DateOfBirth = table.Column<DateTime>(type: "TEXT", nullable: false),
                    StudentNumber = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StudentProfiles", x => x.Id);
                    table.ForeignKey(
                        name: "FK_StudentProfiles_AspNetUsers_IdentityUserId",
                        column: x => x.IdentityUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });
            
            migrationBuilder.Sql("INSERT INTO StudentProfiles (Id, IdentityUserId, Name, Email, Phone, Address, DateOfBirth, StudentNumber) SELECT Id, IdentityUserId, Name, Email, Phone, Address, DateOfBirth, StudentNumber FROM __OldStudentProfiles;");
            
            migrationBuilder.Sql("DROP TABLE __OldStudentProfiles;");
            
            migrationBuilder.Sql("PRAGMA foreign_keys=1;");
            
            migrationBuilder.CreateIndex(
                name: "IX_StudentProfiles_IdentityUserId",
                table: "StudentProfiles",
                column: "IdentityUserId");
        }
    }
}