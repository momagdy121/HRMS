using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace HRSystem.Data.Migrations
{
    /// <inheritdoc />
    public partial class SeedOperationalDomainData : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "Attendances",
                columns: new[] { "Id", "CheckInTime", "CheckOutTime", "Date", "EmployeeId", "Notes" },
                values: new object[,]
                {
                    { 1, new DateTime(2025, 3, 10, 8, 0, 0, 0, DateTimeKind.Utc), new DateTime(2025, 3, 10, 16, 30, 0, 0, DateTimeKind.Utc), new DateOnly(2025, 3, 10), 4, null },
                    { 2, new DateTime(2025, 3, 11, 8, 15, 0, 0, DateTimeKind.Utc), new DateTime(2025, 3, 11, 16, 45, 0, 0, DateTimeKind.Utc), new DateOnly(2025, 3, 11), 5, null },
                    { 3, new DateTime(2025, 3, 12, 7, 55, 0, 0, DateTimeKind.Utc), null, new DateOnly(2025, 3, 12), 6, "Forgot checkout — corrected next day." },
                    { 4, new DateTime(2025, 3, 13, 8, 0, 0, 0, DateTimeKind.Utc), new DateTime(2025, 3, 13, 17, 0, 0, 0, DateTimeKind.Utc), new DateOnly(2025, 3, 13), 2, null },
                    { 5, new DateTime(2025, 3, 14, 8, 30, 0, 0, DateTimeKind.Utc), new DateTime(2025, 3, 14, 16, 0, 0, 0, DateTimeKind.Utc), new DateOnly(2025, 3, 14), 3, null },
                    { 6, new DateTime(2025, 3, 17, 7, 45, 0, 0, DateTimeKind.Utc), new DateTime(2025, 3, 17, 15, 30, 0, 0, DateTimeKind.Utc), new DateOnly(2025, 3, 17), 1, "Half day" }
                });

            migrationBuilder.InsertData(
                table: "EmployeeTasks",
                columns: new[] { "Id", "AssignedById", "AssignedToId", "CreatedAt", "Description", "DueDate", "Status", "Title", "UpdatedAt" },
                values: new object[,]
                {
                    { 1, 2, 4, new DateTime(2025, 2, 10, 9, 0, 0, 0, DateTimeKind.Utc), "Update internal wiki for onboarding.", new DateOnly(2025, 11, 30), (byte)0, "Review API documentation", null },
                    { 2, 2, 6, new DateTime(2025, 2, 11, 9, 30, 0, 0, DateTimeKind.Utc), "Record short walkthrough of dashboard features.", new DateOnly(2025, 10, 15), (byte)1, "Prepare sprint demo", new DateTime(2025, 2, 15, 14, 0, 0, 0, DateTimeKind.Utc) },
                    { 3, 3, 5, new DateTime(2025, 2, 10, 9, 0, 0, 0, DateTimeKind.Utc), null, new DateOnly(2025, 9, 1), (byte)2, "Update HR policy checklist", new DateTime(2025, 2, 15, 14, 0, 0, 0, DateTimeKind.Utc) },
                    { 4, 3, 7, new DateTime(2025, 2, 11, 9, 30, 0, 0, DateTimeKind.Utc), null, null, (byte)0, "Schedule 1:1 meetings", null }
                });

            migrationBuilder.InsertData(
                table: "LeaveRequests",
                columns: new[] { "Id", "ApprovedAt", "ApprovedBy", "EmployeeId", "EndDate", "LeaveType", "Reason", "RejectionReason", "RequestDate", "StartDate", "Status" },
                values: new object[,]
                {
                    { 1, null, null, 4, new DateOnly(2025, 8, 8), (byte)0, "Summer vacation", null, new DateTime(2025, 2, 20, 12, 0, 0, 0, DateTimeKind.Utc), new DateOnly(2025, 8, 4), (byte)0 },
                    { 2, new DateTime(2025, 2, 21, 9, 0, 0, 0, DateTimeKind.Utc), 1, 5, new DateOnly(2025, 7, 3), (byte)1, null, null, new DateTime(2025, 2, 20, 12, 0, 0, 0, DateTimeKind.Utc), new DateOnly(2025, 7, 1), (byte)1 },
                    { 3, null, null, 6, new DateOnly(2025, 9, 12), (byte)2, "Personal errands", "Team coverage not available for those dates.", new DateTime(2025, 2, 20, 12, 0, 0, 0, DateTimeKind.Utc), new DateOnly(2025, 9, 10), (byte)2 },
                    { 4, null, null, 8, new DateOnly(2025, 10, 5), (byte)0, "Conference travel", null, new DateTime(2025, 2, 20, 12, 0, 0, 0, DateTimeKind.Utc), new DateOnly(2025, 10, 1), (byte)0 }
                });

            migrationBuilder.InsertData(
                table: "Payrolls",
                columns: new[] { "Id", "BaseSalary", "CreatedAt", "EmployeeId", "Month", "NetSalary", "ProcessedAt", "ProcessedBy", "Status", "TotalBonus", "TotalDeduction", "Year" },
                values: new object[,]
                {
                    { 1, 62000m, new DateTime(2025, 3, 1, 10, 0, 0, 0, DateTimeKind.Utc), 4, 4, 62800m, null, 1, (byte)0, 1000m, 200m, 2025 },
                    { 2, 61000m, new DateTime(2025, 3, 1, 10, 0, 0, 0, DateTimeKind.Utc), 5, 5, 61350m, new DateTime(2025, 3, 2, 11, 0, 0, 0, DateTimeKind.Utc), 1, (byte)1, 500m, 150m, 2025 }
                });

            migrationBuilder.InsertData(
                table: "PayrollItems",
                columns: new[] { "Id", "Amount", "Description", "ItemType", "PayrollId" },
                values: new object[,]
                {
                    { 1, 1000m, "Q1 performance bonus", (byte)0, 1 },
                    { 2, 200m, "Health insurance contribution", (byte)1, 1 },
                    { 3, 500m, "Team milestone bonus", (byte)0, 2 },
                    { 4, 150m, "Parking pass", (byte)1, 2 }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Attendances",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "Attendances",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "Attendances",
                keyColumn: "Id",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "Attendances",
                keyColumn: "Id",
                keyValue: 4);

            migrationBuilder.DeleteData(
                table: "Attendances",
                keyColumn: "Id",
                keyValue: 5);

            migrationBuilder.DeleteData(
                table: "Attendances",
                keyColumn: "Id",
                keyValue: 6);

            migrationBuilder.DeleteData(
                table: "EmployeeTasks",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "EmployeeTasks",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "EmployeeTasks",
                keyColumn: "Id",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "EmployeeTasks",
                keyColumn: "Id",
                keyValue: 4);

            migrationBuilder.DeleteData(
                table: "LeaveRequests",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "LeaveRequests",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "LeaveRequests",
                keyColumn: "Id",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "LeaveRequests",
                keyColumn: "Id",
                keyValue: 4);

            migrationBuilder.DeleteData(
                table: "PayrollItems",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "PayrollItems",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "PayrollItems",
                keyColumn: "Id",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "PayrollItems",
                keyColumn: "Id",
                keyValue: 4);

            migrationBuilder.DeleteData(
                table: "Payrolls",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "Payrolls",
                keyColumn: "Id",
                keyValue: 2);
        }
    }
}
