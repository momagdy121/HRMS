using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace HRSystem.Data.Migrations
{
    /// <inheritdoc />
    public partial class SeedLeaveBalances : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "LeaveBalances",
                columns: new[] { "Id", "EmployeeId", "LeaveType", "TotalDays", "Year" },
                values: new object[,]
                {
                    { 1, 1, (byte)0, 20, 2025 },
                    { 2, 1, (byte)1, 10, 2025 },
                    { 3, 2, (byte)0, 20, 2025 },
                    { 4, 2, (byte)1, 10, 2025 },
                    { 5, 3, (byte)0, 20, 2025 },
                    { 6, 3, (byte)1, 10, 2025 },
                    { 7, 4, (byte)0, 20, 2025 },
                    { 8, 4, (byte)1, 10, 2025 },
                    { 9, 5, (byte)0, 20, 2025 },
                    { 10, 5, (byte)1, 10, 2025 },
                    { 11, 6, (byte)0, 20, 2025 },
                    { 12, 6, (byte)1, 10, 2025 },
                    { 13, 7, (byte)0, 20, 2025 },
                    { 14, 7, (byte)1, 10, 2025 },
                    { 15, 8, (byte)0, 20, 2025 },
                    { 16, 8, (byte)1, 10, 2025 },
                    { 17, 9, (byte)0, 20, 2025 },
                    { 18, 9, (byte)1, 10, 2025 },
                    { 19, 10, (byte)0, 20, 2025 },
                    { 20, 10, (byte)1, 10, 2025 },
                    { 21, 11, (byte)0, 20, 2025 },
                    { 22, 11, (byte)1, 10, 2025 },
                    { 23, 12, (byte)0, 20, 2025 },
                    { 24, 12, (byte)1, 10, 2025 },
                    { 25, 13, (byte)0, 20, 2025 },
                    { 26, 13, (byte)1, 10, 2025 },
                    { 27, 14, (byte)0, 20, 2025 },
                    { 28, 14, (byte)1, 10, 2025 },
                    { 29, 15, (byte)0, 20, 2025 },
                    { 30, 15, (byte)1, 10, 2025 }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "LeaveBalances",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "LeaveBalances",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "LeaveBalances",
                keyColumn: "Id",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "LeaveBalances",
                keyColumn: "Id",
                keyValue: 4);

            migrationBuilder.DeleteData(
                table: "LeaveBalances",
                keyColumn: "Id",
                keyValue: 5);

            migrationBuilder.DeleteData(
                table: "LeaveBalances",
                keyColumn: "Id",
                keyValue: 6);

            migrationBuilder.DeleteData(
                table: "LeaveBalances",
                keyColumn: "Id",
                keyValue: 7);

            migrationBuilder.DeleteData(
                table: "LeaveBalances",
                keyColumn: "Id",
                keyValue: 8);

            migrationBuilder.DeleteData(
                table: "LeaveBalances",
                keyColumn: "Id",
                keyValue: 9);

            migrationBuilder.DeleteData(
                table: "LeaveBalances",
                keyColumn: "Id",
                keyValue: 10);

            migrationBuilder.DeleteData(
                table: "LeaveBalances",
                keyColumn: "Id",
                keyValue: 11);

            migrationBuilder.DeleteData(
                table: "LeaveBalances",
                keyColumn: "Id",
                keyValue: 12);

            migrationBuilder.DeleteData(
                table: "LeaveBalances",
                keyColumn: "Id",
                keyValue: 13);

            migrationBuilder.DeleteData(
                table: "LeaveBalances",
                keyColumn: "Id",
                keyValue: 14);

            migrationBuilder.DeleteData(
                table: "LeaveBalances",
                keyColumn: "Id",
                keyValue: 15);

            migrationBuilder.DeleteData(
                table: "LeaveBalances",
                keyColumn: "Id",
                keyValue: 16);

            migrationBuilder.DeleteData(
                table: "LeaveBalances",
                keyColumn: "Id",
                keyValue: 17);

            migrationBuilder.DeleteData(
                table: "LeaveBalances",
                keyColumn: "Id",
                keyValue: 18);

            migrationBuilder.DeleteData(
                table: "LeaveBalances",
                keyColumn: "Id",
                keyValue: 19);

            migrationBuilder.DeleteData(
                table: "LeaveBalances",
                keyColumn: "Id",
                keyValue: 20);

            migrationBuilder.DeleteData(
                table: "LeaveBalances",
                keyColumn: "Id",
                keyValue: 21);

            migrationBuilder.DeleteData(
                table: "LeaveBalances",
                keyColumn: "Id",
                keyValue: 22);

            migrationBuilder.DeleteData(
                table: "LeaveBalances",
                keyColumn: "Id",
                keyValue: 23);

            migrationBuilder.DeleteData(
                table: "LeaveBalances",
                keyColumn: "Id",
                keyValue: 24);

            migrationBuilder.DeleteData(
                table: "LeaveBalances",
                keyColumn: "Id",
                keyValue: 25);

            migrationBuilder.DeleteData(
                table: "LeaveBalances",
                keyColumn: "Id",
                keyValue: 26);

            migrationBuilder.DeleteData(
                table: "LeaveBalances",
                keyColumn: "Id",
                keyValue: 27);

            migrationBuilder.DeleteData(
                table: "LeaveBalances",
                keyColumn: "Id",
                keyValue: 28);

            migrationBuilder.DeleteData(
                table: "LeaveBalances",
                keyColumn: "Id",
                keyValue: 29);

            migrationBuilder.DeleteData(
                table: "LeaveBalances",
                keyColumn: "Id",
                keyValue: 30);
        }
    }
}
