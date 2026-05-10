using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HRSystem.Data.Migrations;

/// <inheritdoc />
public partial class SetManagerIds : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql(
            """
            UPDATE [Departments] SET [ManagerId] = 2 WHERE [Id] = 1;
            UPDATE [Departments] SET [ManagerId] = 3 WHERE [Id] = 2;
            """);
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql(
            """
            UPDATE [Departments] SET [ManagerId] = NULL WHERE [Id] = 1;
            UPDATE [Departments] SET [ManagerId] = NULL WHERE [Id] = 2;
            """);
    }
}
