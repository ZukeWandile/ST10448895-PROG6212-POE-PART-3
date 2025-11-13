using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace ST10448895_CMCS_PROG.Migrations
{
    /// <inheritdoc />
    public partial class SeedInitialData : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Approvals_CoordinatorModel_CoordinatorId",
                table: "Approvals");

            migrationBuilder.DropForeignKey(
                name: "FK_Approvals_ManagerModel_ManagerId",
                table: "Approvals");

            migrationBuilder.DropForeignKey(
                name: "FK_Claims_CoordinatorModel_CoordinatorModelId",
                table: "Claims");

            migrationBuilder.DropForeignKey(
                name: "FK_Claims_ManagerModel_ManagerModelId",
                table: "Claims");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ManagerModel",
                table: "ManagerModel");

            migrationBuilder.DropPrimaryKey(
                name: "PK_CoordinatorModel",
                table: "CoordinatorModel");

            migrationBuilder.RenameTable(
                name: "ManagerModel",
                newName: "Managers");

            migrationBuilder.RenameTable(
                name: "CoordinatorModel",
                newName: "Coordinators");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Managers",
                table: "Managers",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Coordinators",
                table: "Coordinators",
                column: "Id");

            migrationBuilder.InsertData(
                table: "Coordinators",
                columns: new[] { "Id", "Name" },
                values: new object[] { 1, "Dr. Linda Brown" });

            migrationBuilder.InsertData(
                table: "Lecturers",
                columns: new[] { "Id", "Email", "Name" },
                values: new object[,]
                {
                    { 1, "john.smith@college.edu", "John Smith" },
                    { 2, "mary.johnson@college.edu", "Mary Johnson" }
                });

            migrationBuilder.InsertData(
                table: "Managers",
                columns: new[] { "Id", "Name" },
                values: new object[] { 1, "Prof. Alan White" });

            migrationBuilder.InsertData(
                table: "Claims",
                columns: new[] { "Id", "Approved", "CoordinatorModelId", "Description", "Document", "HourlyRate", "HoursWorked", "LecturerId", "ManagerModelId", "Status", "SubmitDate", "Verified" },
                values: new object[] { 1, false, null, "Cybersecurity workshop claim", null, 300m, 5, 1, null, "Pending", new DateTime(2025, 10, 15, 21, 37, 53, 449, DateTimeKind.Local).AddTicks(5281), false });

            migrationBuilder.AddForeignKey(
                name: "FK_Approvals_Coordinators_CoordinatorId",
                table: "Approvals",
                column: "CoordinatorId",
                principalTable: "Coordinators",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Approvals_Managers_ManagerId",
                table: "Approvals",
                column: "ManagerId",
                principalTable: "Managers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Claims_Coordinators_CoordinatorModelId",
                table: "Claims",
                column: "CoordinatorModelId",
                principalTable: "Coordinators",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Claims_Managers_ManagerModelId",
                table: "Claims",
                column: "ManagerModelId",
                principalTable: "Managers",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Approvals_Coordinators_CoordinatorId",
                table: "Approvals");

            migrationBuilder.DropForeignKey(
                name: "FK_Approvals_Managers_ManagerId",
                table: "Approvals");

            migrationBuilder.DropForeignKey(
                name: "FK_Claims_Coordinators_CoordinatorModelId",
                table: "Claims");

            migrationBuilder.DropForeignKey(
                name: "FK_Claims_Managers_ManagerModelId",
                table: "Claims");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Managers",
                table: "Managers");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Coordinators",
                table: "Coordinators");

            migrationBuilder.DeleteData(
                table: "Claims",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "Coordinators",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "Lecturers",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "Managers",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "Lecturers",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.RenameTable(
                name: "Managers",
                newName: "ManagerModel");

            migrationBuilder.RenameTable(
                name: "Coordinators",
                newName: "CoordinatorModel");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ManagerModel",
                table: "ManagerModel",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_CoordinatorModel",
                table: "CoordinatorModel",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Approvals_CoordinatorModel_CoordinatorId",
                table: "Approvals",
                column: "CoordinatorId",
                principalTable: "CoordinatorModel",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Approvals_ManagerModel_ManagerId",
                table: "Approvals",
                column: "ManagerId",
                principalTable: "ManagerModel",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Claims_CoordinatorModel_CoordinatorModelId",
                table: "Claims",
                column: "CoordinatorModelId",
                principalTable: "CoordinatorModel",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Claims_ManagerModel_ManagerModelId",
                table: "Claims",
                column: "ManagerModelId",
                principalTable: "ManagerModel",
                principalColumn: "Id");
        }
    }
}
