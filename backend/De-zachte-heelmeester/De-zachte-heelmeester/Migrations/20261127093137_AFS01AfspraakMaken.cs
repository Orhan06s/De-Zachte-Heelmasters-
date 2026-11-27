using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace De_zachte_heelmeester.Migrations
{
    /// <inheritdoc />
    public partial class AFS01AfspraakMaken : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Existing specialists work Monday to Friday (31). The generated value 0 would mean they never work.
            migrationBuilder.AddColumn<int>(
                name: "WorkDays",
                table: "Employees",
                type: "int",
                nullable: false,
                defaultValue: 31);

            migrationBuilder.AddColumn<int>(
                name: "ReferralId",
                table: "Appointments",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "CareTreatments",
                columns: table => new
                {
                    CareCode = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Specialization = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DurationQuarters = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CareTreatments", x => x.CareCode);
                });

            migrationBuilder.CreateTable(
                name: "Referrals",
                columns: table => new
                {
                    ReferralId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PatientBSN = table.Column<string>(type: "nvarchar(9)", maxLength: 9, nullable: false),
                    CareCode = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Referrals", x => x.ReferralId);
                    table.ForeignKey(
                        name: "FK_Referrals_CareTreatments_CareCode",
                        column: x => x.CareCode,
                        principalTable: "CareTreatments",
                        principalColumn: "CareCode",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Referrals_Patients_PatientBSN",
                        column: x => x.PatientBSN,
                        principalTable: "Patients",
                        principalColumn: "BSN",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.InsertData(
                table: "CareTreatments",
                columns: new[] { "CareCode", "Description", "DurationQuarters", "Specialization" },
                values: new object[,]
                {
                    { "Z0001", "Consult hart", 3, "Cardiologie" },
                    { "Z0002", "Consult huid", 3, "Dermatologie" },
                    { "Z0003", "Consult longen", 2, "Longgeneeskunde" },
                    { "Z0004", "Consult ogen", 3, "Oogheelkunde" },
                    { "Z0005", "Consult oren", 3, "KNO" },
                    { "Z0006", "Operatie knie", 2, "Orthopedie" },
                    { "Z0007", "Operatie heup", 3, "Orthopedie" },
                    { "Z0008", "Operatie hart", 4, "Cardiologie" },
                    { "Z0009", "Operatie huid", 4, "Chirurgie" },
                    { "Z0010", "Operatie rug", 3, "Orthopedie" },
                    { "Z0011", "Controle hart", 3, "Cardiologie" },
                    { "Z0012", "Controle huid", 4, "Dermatologie" },
                    { "Z0013", "Controle longen", 3, "Longgeneeskunde" },
                    { "Z0014", "Controle ogen", 3, "Oogheelkunde" },
                    { "Z0015", "Controle oren", 4, "KNO" },
                    { "Z0016", "MRI-scan hoofd", 2, "Radiologie" },
                    { "Z0017", "MRI-scan knie", 3, "Radiologie" },
                    { "Z0018", "MRI-scan rug", 3, "Radiologie" },
                    { "Z0019", "MRI-scan hart", 2, "Radiologie" },
                    { "Z0020", "MRI-scan buik", 2, "Radiologie" },
                    { "Z0021", "CT-scan hoofd", 3, "Radiologie" },
                    { "Z0022", "CT-scan knie", 3, "Radiologie" },
                    { "Z0023", "CT-scan rug", 3, "Radiologie" },
                    { "Z0024", "CT-scan hart", 3, "Radiologie" },
                    { "Z0025", "CT-scan buik", 4, "Radiologie" },
                    { "Z0026", "Bloedonderzoek algemeen", 1, "Laboratoriumgeneeskunde" },
                    { "Z0027", "Bloedonderzoek hormonen", 2, "Laboratoriumgeneeskunde" },
                    { "Z0028", "Bloedonderzoek infecties", 1, "Laboratoriumgeneeskunde" },
                    { "Z0029", "Bloedonderzoek suiker", 4, "Laboratoriumgeneeskunde" },
                    { "Z0030", "Bloedonderzoek cholesterol", 3, "Laboratoriumgeneeskunde" },
                    { "Z0031", "Fysiotherapie knie", 1, "Revalidatiegeneeskunde" },
                    { "Z0032", "Fysiotherapie rug", 3, "Revalidatiegeneeskunde" },
                    { "Z0033", "Fysiotherapie schouder", 2, "Revalidatiegeneeskunde" },
                    { "Z0034", "Fysiotherapie heup", 4, "Revalidatiegeneeskunde" },
                    { "Z0035", "Fysiotherapie nek", 1, "Revalidatiegeneeskunde" },
                    { "Z0036", "Psychologisch gesprek depressie", 4, "Psychiatrie" },
                    { "Z0037", "Psychologisch gesprek angst", 3, "Psychiatrie" },
                    { "Z0038", "Psychologisch gesprek trauma", 1, "Psychiatrie" },
                    { "Z0039", "Psychologisch gesprek burn-out", 3, "Psychiatrie" },
                    { "Z0040", "Psychologisch gesprek stress", 4, "Psychiatrie" },
                    { "Z0041", "Vaccinatie griep", 3, "Algemeen" },
                    { "Z0042", "Vaccinatie hepatitis", 2, "Algemeen" },
                    { "Z0043", "Vaccinatie tetanus", 1, "Algemeen" },
                    { "Z0044", "Vaccinatie covid", 2, "Algemeen" },
                    { "Z0045", "Vaccinatie HPV", 4, "Algemeen" },
                    { "Z0046", "Wondverzorging arm", 2, "Chirurgie" },
                    { "Z0047", "Wondverzorging been", 2, "Chirurgie" },
                    { "Z0048", "Wondverzorging hoofd", 2, "Chirurgie" },
                    { "Z0049", "Wondverzorging buik", 4, "Chirurgie" },
                    { "Z0050", "Wondverzorging rug", 4, "Chirurgie" },
                    { "Z0051", "Fysiotherapie rug", 1, "Revalidatiegeneeskunde" },
                    { "Z0052", "Fysiotherapie schouder", 4, "Revalidatiegeneeskunde" },
                    { "Z0053", "Wondverzorging rug", 4, "Chirurgie" },
                    { "Z0054", "Vaccinatie HPV", 4, "Algemeen" },
                    { "Z0055", "Vaccinatie hepatitis", 1, "Algemeen" },
                    { "Z0056", "Psychologisch gesprek stress", 1, "Psychiatrie" },
                    { "Z0057", "Consult ogen", 1, "Oogheelkunde" },
                    { "Z0058", "Operatie rug", 1, "Orthopedie" },
                    { "Z0059", "MRI-scan buik", 2, "Radiologie" },
                    { "Z0060", "Controle huid", 3, "Dermatologie" },
                    { "Z0061", "MRI-scan hoofd", 1, "Radiologie" },
                    { "Z0062", "Controle longen", 4, "Longgeneeskunde" },
                    { "Z0063", "Bloedonderzoek algemeen", 4, "Laboratoriumgeneeskunde" },
                    { "Z0064", "Wondverzorging arm", 2, "Chirurgie" },
                    { "Z0065", "Operatie hart", 4, "Cardiologie" },
                    { "Z0066", "Consult huid", 2, "Dermatologie" },
                    { "Z0067", "Psychologisch gesprek angst", 4, "Psychiatrie" },
                    { "Z0068", "CT-scan knie", 1, "Radiologie" },
                    { "Z0069", "Wondverzorging been", 1, "Chirurgie" },
                    { "Z0070", "Operatie huid", 4, "Chirurgie" },
                    { "Z0071", "Consult hart", 2, "Cardiologie" },
                    { "Z0072", "Consult oren", 4, "KNO" },
                    { "Z0073", "Bloedonderzoek cholesterol", 3, "Laboratoriumgeneeskunde" },
                    { "Z0074", "Controle hart", 4, "Cardiologie" },
                    { "Z0075", "Vaccinatie griep", 4, "Algemeen" },
                    { "Z0076", "CT-scan rug", 4, "Radiologie" },
                    { "Z0077", "Bloedonderzoek hormonen", 2, "Laboratoriumgeneeskunde" },
                    { "Z0078", "Vaccinatie covid", 2, "Algemeen" },
                    { "Z0079", "Psychologisch gesprek trauma", 1, "Psychiatrie" },
                    { "Z0080", "MRI-scan hart", 2, "Radiologie" },
                    { "Z0081", "Fysiotherapie heup", 2, "Revalidatiegeneeskunde" },
                    { "Z0082", "MRI-scan knie", 1, "Radiologie" },
                    { "Z0083", "Psychologisch gesprek burn-out", 4, "Psychiatrie" },
                    { "Z0084", "CT-scan buik", 2, "Radiologie" },
                    { "Z0085", "Consult longen", 2, "Longgeneeskunde" },
                    { "Z0086", "CT-scan hoofd", 4, "Radiologie" },
                    { "Z0087", "Fysiotherapie knie", 1, "Revalidatiegeneeskunde" },
                    { "Z0088", "Wondverzorging buik", 4, "Chirurgie" },
                    { "Z0089", "MRI-scan rug", 4, "Radiologie" },
                    { "Z0090", "Psychologisch gesprek depressie", 2, "Psychiatrie" },
                    { "Z0091", "Operatie knie", 3, "Orthopedie" },
                    { "Z0092", "Fysiotherapie nek", 4, "Revalidatiegeneeskunde" },
                    { "Z0093", "CT-scan hart", 1, "Radiologie" },
                    { "Z0094", "Controle ogen", 2, "Oogheelkunde" },
                    { "Z0095", "Bloedonderzoek suiker", 1, "Laboratoriumgeneeskunde" },
                    { "Z0096", "Controle oren", 1, "KNO" },
                    { "Z0097", "Wondverzorging hoofd", 1, "Chirurgie" },
                    { "Z0098", "Vaccinatie tetanus", 1, "Algemeen" },
                    { "Z0099", "Operatie heup", 1, "Orthopedie" },
                    { "Z0100", "Bloedonderzoek infecties", 4, "Laboratoriumgeneeskunde" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Appointments_ReferralId",
                table: "Appointments",
                column: "ReferralId");

            migrationBuilder.CreateIndex(
                name: "IX_Referrals_CareCode",
                table: "Referrals",
                column: "CareCode");

            migrationBuilder.CreateIndex(
                name: "IX_Referrals_PatientBSN",
                table: "Referrals",
                column: "PatientBSN");

            migrationBuilder.AddForeignKey(
                name: "FK_Appointments_Referrals_ReferralId",
                table: "Appointments",
                column: "ReferralId",
                principalTable: "Referrals",
                principalColumn: "ReferralId",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Appointments_Referrals_ReferralId",
                table: "Appointments");

            migrationBuilder.DropTable(
                name: "Referrals");

            migrationBuilder.DropTable(
                name: "CareTreatments");

            migrationBuilder.DropIndex(
                name: "IX_Appointments_ReferralId",
                table: "Appointments");

            migrationBuilder.DropColumn(
                name: "WorkDays",
                table: "Employees");

            migrationBuilder.DropColumn(
                name: "ReferralId",
                table: "Appointments");
        }
    }
}
