using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace HRSystem.Data.Migrations
{
    /// <inheritdoc />
    public partial class SeedDepartmentsAndEmployees : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "Departments",
                columns: new[] { "Id", "CreatedAt", "ManagerId", "Name" },
                values: new object[,]
                {
                    { 1, new DateTime(2025, 1, 1, 8, 0, 0, 0, DateTimeKind.Utc), null, "Information Technology" },
                    { 2, new DateTime(2025, 1, 1, 8, 0, 0, 0, DateTimeKind.Utc), null, "Human Resources" }
                });

            migrationBuilder.InsertData(
                table: "Employees",
                columns: new[] { "Id", "CreatedAt", "DepartmentId", "Email", "FirstName", "HireDate", "IsActive", "IsHR", "LastName", "Salary" },
                values: new object[] { 1, new DateTime(2025, 1, 1, 8, 0, 0, 0, DateTimeKind.Utc), 2, "admin@hr.com", "System", new DateOnly(2024, 6, 1), true, true, "Administrator", 95000m });

            migrationBuilder.InsertData(
                table: "Employees",
                columns: new[] { "Id", "CreatedAt", "DepartmentId", "Email", "FirstName", "HireDate", "IsActive", "LastName", "Salary" },
                values: new object[,]
                {
                    { 2, new DateTime(2025, 1, 1, 8, 0, 0, 0, DateTimeKind.Utc), 1, "jane@hr.com", "Jane", new DateOnly(2024, 6, 1), true, "Smith", 82000m },
                    { 3, new DateTime(2025, 1, 1, 8, 0, 0, 0, DateTimeKind.Utc), 2, "bob@hr.com", "Bob", new DateOnly(2024, 6, 1), true, "Jones", 83000m },
                    { 4, new DateTime(2025, 1, 1, 8, 0, 0, 0, DateTimeKind.Utc), 1, "alice@it.com", "Alice", new DateOnly(2024, 6, 1), true, "Brown", 62000m },
                    { 5, new DateTime(2025, 1, 1, 8, 0, 0, 0, DateTimeKind.Utc), 2, "charlie@hr.com", "Charlie", new DateOnly(2024, 6, 1), true, "Wilson", 61000m },
                    { 6, new DateTime(2025, 1, 1, 8, 0, 0, 0, DateTimeKind.Utc), 1, "diana@it.com", "Diana", new DateOnly(2024, 6, 1), true, "Lee", 63000m },
                    { 7, new DateTime(2025, 1, 1, 8, 0, 0, 0, DateTimeKind.Utc), 2, "evan@hr.com", "Evan", new DateOnly(2024, 6, 1), true, "Clark", 64000m },
                    { 8, new DateTime(2025, 1, 1, 8, 0, 0, 0, DateTimeKind.Utc), 1, "fiona@it.com", "Fiona", new DateOnly(2024, 6, 1), true, "Hall", 65000m },
                    { 9, new DateTime(2025, 1, 1, 8, 0, 0, 0, DateTimeKind.Utc), 2, "george@hr.com", "George", new DateOnly(2024, 6, 1), true, "Young", 66000m },
                    { 10, new DateTime(2025, 1, 1, 8, 0, 0, 0, DateTimeKind.Utc), 1, "hannah@it.com", "Hannah", new DateOnly(2024, 6, 1), true, "King", 67000m },
                    { 11, new DateTime(2025, 1, 1, 8, 0, 0, 0, DateTimeKind.Utc), 2, "ian@hr.com", "Ian", new DateOnly(2024, 6, 1), true, "Wright", 68000m },
                    { 12, new DateTime(2025, 1, 1, 8, 0, 0, 0, DateTimeKind.Utc), 1, "julia@it.com", "Julia", new DateOnly(2024, 6, 1), true, "Scott", 69000m },
                    { 13, new DateTime(2025, 1, 1, 8, 0, 0, 0, DateTimeKind.Utc), 2, "kevin@hr.com", "Kevin", new DateOnly(2024, 6, 1), true, "Green", 70000m },
                    { 14, new DateTime(2025, 1, 1, 8, 0, 0, 0, DateTimeKind.Utc), 1, "laura@it.com", "Laura", new DateOnly(2024, 6, 1), true, "Adams", 71000m },
                    { 15, new DateTime(2025, 1, 1, 8, 0, 0, 0, DateTimeKind.Utc), 2, "michael@hr.com", "Michael", new DateOnly(2024, 6, 1), true, "Baker", 72000m }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Employees",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "Employees",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "Employees",
                keyColumn: "Id",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "Employees",
                keyColumn: "Id",
                keyValue: 4);

            migrationBuilder.DeleteData(
                table: "Employees",
                keyColumn: "Id",
                keyValue: 5);

            migrationBuilder.DeleteData(
                table: "Employees",
                keyColumn: "Id",
                keyValue: 6);

            migrationBuilder.DeleteData(
                table: "Employees",
                keyColumn: "Id",
                keyValue: 7);

            migrationBuilder.DeleteData(
                table: "Employees",
                keyColumn: "Id",
                keyValue: 8);

            migrationBuilder.DeleteData(
                table: "Employees",
                keyColumn: "Id",
                keyValue: 9);

            migrationBuilder.DeleteData(
                table: "Employees",
                keyColumn: "Id",
                keyValue: 10);

            migrationBuilder.DeleteData(
                table: "Employees",
                keyColumn: "Id",
                keyValue: 11);

            migrationBuilder.DeleteData(
                table: "Employees",
                keyColumn: "Id",
                keyValue: 12);

            migrationBuilder.DeleteData(
                table: "Employees",
                keyColumn: "Id",
                keyValue: 13);

            migrationBuilder.DeleteData(
                table: "Employees",
                keyColumn: "Id",
                keyValue: 14);

            migrationBuilder.DeleteData(
                table: "Employees",
                keyColumn: "Id",
                keyValue: 15);

            migrationBuilder.DeleteData(
                table: "Departments",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "Departments",
                keyColumn: "Id",
                keyValue: 2);
        }
    }
}
