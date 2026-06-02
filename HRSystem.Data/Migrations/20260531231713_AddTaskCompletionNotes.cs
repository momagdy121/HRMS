using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HRSystem.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddTaskCompletionNotes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CompletionNotes",
                table: "EmployeeTasks",
                type: "nvarchar(2000)",
                maxLength: 2000,
                nullable: true);

            migrationBuilder.UpdateData(
                table: "EmployeeTasks",
                keyColumn: "Id",
                keyValue: 1,
                column: "CompletionNotes",
                value: null);

            migrationBuilder.UpdateData(
                table: "EmployeeTasks",
                keyColumn: "Id",
                keyValue: 2,
                column: "CompletionNotes",
                value: null);

            migrationBuilder.UpdateData(
                table: "EmployeeTasks",
                keyColumn: "Id",
                keyValue: 3,
                column: "CompletionNotes",
                value: "Updated checklist published at https://intranet/hr/policy-checklist");

            migrationBuilder.UpdateData(
                table: "EmployeeTasks",
                keyColumn: "Id",
                keyValue: 4,
                column: "CompletionNotes",
                value: null);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CompletionNotes",
                table: "EmployeeTasks");
        }
    }
}
