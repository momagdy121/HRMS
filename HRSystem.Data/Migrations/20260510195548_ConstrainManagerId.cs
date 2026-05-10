using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HRSystem.Data.Migrations;

/// <inheritdoc />
public partial class ConstrainManagerId : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        // Ensure manager values exist before NOT NULL (idempotent if SetManagerIds already ran).
        migrationBuilder.Sql(
            """
            UPDATE [Departments] SET [ManagerId] = 2 WHERE [Id] = 1;
            UPDATE [Departments] SET [ManagerId] = 3 WHERE [Id] = 2;
            """);

        migrationBuilder.AlterColumn<int>(
            name: "ManagerId",
            table: "Departments",
            type: "int",
            nullable: false,
            oldClrType: typeof(int),
            oldType: "int",
            oldNullable: true);

        migrationBuilder.CreateIndex(
            name: "IX_Departments_ManagerId",
            table: "Departments",
            column: "ManagerId");

        migrationBuilder.AddForeignKey(
            name: "FK_Departments_Employees_ManagerId",
            table: "Departments",
            column: "ManagerId",
            principalTable: "Employees",
            principalColumn: "Id",
            onDelete: ReferentialAction.Restrict);
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropForeignKey(
            name: "FK_Departments_Employees_ManagerId",
            table: "Departments");

        migrationBuilder.DropIndex(
            name: "IX_Departments_ManagerId",
            table: "Departments");

        migrationBuilder.AlterColumn<int>(
            name: "ManagerId",
            table: "Departments",
            type: "int",
            nullable: true,
            oldClrType: typeof(int),
            oldType: "int");

        migrationBuilder.Sql(
            """
            UPDATE [Departments] SET [ManagerId] = NULL WHERE [Id] = 1;
            UPDATE [Departments] SET [ManagerId] = NULL WHERE [Id] = 2;
            """);
    }
}
