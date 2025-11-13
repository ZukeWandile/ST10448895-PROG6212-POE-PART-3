using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ST10448895_CMCS_PROG.Migrations
{
    /// <inheritdoc />
    public partial class UpdateClaimVerification : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Claims",
                keyColumn: "Id",
                keyValue: 1,
                column: "SubmitDate",
                value: new DateTime(2025, 10, 16, 11, 21, 42, 158, DateTimeKind.Local).AddTicks(3037));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Claims",
                keyColumn: "Id",
                keyValue: 1,
                column: "SubmitDate",
                value: new DateTime(2025, 10, 15, 21, 37, 53, 449, DateTimeKind.Local).AddTicks(5281));
        }
    }
}
