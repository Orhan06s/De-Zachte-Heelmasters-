using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace De_zachte_heelmeester.Migrations
{
    /// <inheritdoc />
    public partial class LOG01Registratie : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "EmailIndex",
                table: "AspNetUsers");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Patients_BSN",
                table: "Patients",
                sql: "[BSN] <> '000000000'\nAND CASE\n    WHEN LEN([BSN]) = 9 AND [BSN] NOT LIKE '%[^0-9]%' THEN\n        ( CAST(SUBSTRING([BSN], 1, 1) AS INT) * 9\n        + CAST(SUBSTRING([BSN], 2, 1) AS INT) * 8\n        + CAST(SUBSTRING([BSN], 3, 1) AS INT) * 7\n        + CAST(SUBSTRING([BSN], 4, 1) AS INT) * 6\n        + CAST(SUBSTRING([BSN], 5, 1) AS INT) * 5\n        + CAST(SUBSTRING([BSN], 6, 1) AS INT) * 4\n        + CAST(SUBSTRING([BSN], 7, 1) AS INT) * 3\n        + CAST(SUBSTRING([BSN], 8, 1) AS INT) * 2\n        - CAST(SUBSTRING([BSN], 9, 1) AS INT) ) % 11\n    ELSE 1\nEND = 0");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Patients_Email",
                table: "Patients",
                sql: "[Email] IS NULL OR [Email] LIKE '%_@_%._%'");

            migrationBuilder.CreateIndex(
                name: "EmailIndex",
                table: "AspNetUsers",
                column: "NormalizedEmail",
                unique: true,
                filter: "[NormalizedEmail] IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "CK_Patients_BSN",
                table: "Patients");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Patients_Email",
                table: "Patients");

            migrationBuilder.DropIndex(
                name: "EmailIndex",
                table: "AspNetUsers");

            migrationBuilder.CreateIndex(
                name: "EmailIndex",
                table: "AspNetUsers",
                column: "NormalizedEmail");
        }
    }
}
