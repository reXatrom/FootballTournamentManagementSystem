using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FootballTournamentManagementSystem.Migrations
{
    /// <inheritdoc />
    public partial class AddRecordOwnership : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                IF NOT EXISTS (SELECT 1 FROM [AspNetUsers] WHERE [Id] = N'00000000-0000-0000-0000-000000000001')
                BEGIN
                    INSERT INTO [AspNetUsers]
                        ([Id], [FullName], [UserName], [NormalizedUserName], [Email], [NormalizedEmail], [EmailConfirmed], [PasswordHash], [SecurityStamp], [ConcurrencyStamp], [PhoneNumber], [PhoneNumberConfirmed], [TwoFactorEnabled], [LockoutEnd], [LockoutEnabled], [AccessFailedCount])
                    VALUES
                        (N'00000000-0000-0000-0000-000000000001', N'Legacy Data', N'legacy-data', N'LEGACY-DATA', N'legacy-data@ftms.local', N'LEGACY-DATA@FTMS.LOCAL', 1, NULL, NEWID(), NEWID(), NULL, 0, 0, NULL, 0, 0);
                END
                """);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "Tournaments",
                type: "datetime2",
                nullable: false,
                defaultValueSql: "SYSUTCDATETIME()");

            migrationBuilder.AddColumn<string>(
                name: "CreatedByUserId",
                table: "Tournaments",
                type: "nvarchar(450)",
                nullable: false,
                defaultValue: "00000000-0000-0000-0000-000000000001");

            migrationBuilder.AddColumn<byte[]>(
                name: "RowVersion",
                table: "Tournaments",
                type: "rowversion",
                rowVersion: true,
                nullable: false,
                defaultValue: new byte[0]);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                table: "Tournaments",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "UpdatedByUserId",
                table: "Tournaments",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "Teams",
                type: "datetime2",
                nullable: false,
                defaultValueSql: "SYSUTCDATETIME()");

            migrationBuilder.AddColumn<string>(
                name: "CreatedByUserId",
                table: "Teams",
                type: "nvarchar(450)",
                nullable: false,
                defaultValue: "00000000-0000-0000-0000-000000000001");

            migrationBuilder.AddColumn<byte[]>(
                name: "RowVersion",
                table: "Teams",
                type: "rowversion",
                rowVersion: true,
                nullable: false,
                defaultValue: new byte[0]);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                table: "Teams",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "UpdatedByUserId",
                table: "Teams",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "Players",
                type: "datetime2",
                nullable: false,
                defaultValueSql: "SYSUTCDATETIME()");

            migrationBuilder.AddColumn<string>(
                name: "CreatedByUserId",
                table: "Players",
                type: "nvarchar(450)",
                nullable: false,
                defaultValue: "00000000-0000-0000-0000-000000000001");

            migrationBuilder.AddColumn<byte[]>(
                name: "RowVersion",
                table: "Players",
                type: "rowversion",
                rowVersion: true,
                nullable: false,
                defaultValue: new byte[0]);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                table: "Players",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "UpdatedByUserId",
                table: "Players",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "MatchEvents",
                type: "datetime2",
                nullable: false,
                defaultValueSql: "SYSUTCDATETIME()");

            migrationBuilder.AddColumn<string>(
                name: "CreatedByUserId",
                table: "MatchEvents",
                type: "nvarchar(450)",
                nullable: false,
                defaultValue: "00000000-0000-0000-0000-000000000001");

            migrationBuilder.AddColumn<byte[]>(
                name: "RowVersion",
                table: "MatchEvents",
                type: "rowversion",
                rowVersion: true,
                nullable: false,
                defaultValue: new byte[0]);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                table: "MatchEvents",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "UpdatedByUserId",
                table: "MatchEvents",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "Matches",
                type: "datetime2",
                nullable: false,
                defaultValueSql: "SYSUTCDATETIME()");

            migrationBuilder.AddColumn<string>(
                name: "CreatedByUserId",
                table: "Matches",
                type: "nvarchar(450)",
                nullable: false,
                defaultValue: "00000000-0000-0000-0000-000000000001");

            migrationBuilder.AddColumn<byte[]>(
                name: "RowVersion",
                table: "Matches",
                type: "rowversion",
                rowVersion: true,
                nullable: false,
                defaultValue: new byte[0]);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                table: "Matches",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "UpdatedByUserId",
                table: "Matches",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.Sql("""
                DECLARE @dropOwnerDefaults nvarchar(max) = N'';
                SELECT @dropOwnerDefaults = @dropOwnerDefaults
                    + N'ALTER TABLE ' + QUOTENAME(SCHEMA_NAME(t.schema_id)) + N'.' + QUOTENAME(t.name)
                    + N' DROP CONSTRAINT ' + QUOTENAME(dc.name) + N';'
                FROM sys.default_constraints dc
                INNER JOIN sys.tables t ON t.object_id = dc.parent_object_id
                INNER JOIN sys.columns c ON c.object_id = t.object_id AND c.column_id = dc.parent_column_id
                WHERE c.name = N'CreatedByUserId'
                    AND t.name IN (N'Tournaments', N'Teams', N'Players', N'Matches', N'MatchEvents');
                EXEC sys.sp_executesql @dropOwnerDefaults;
                """);

            migrationBuilder.CreateIndex(
                name: "IX_Tournaments_CreatedByUserId",
                table: "Tournaments",
                column: "CreatedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Tournaments_UpdatedByUserId",
                table: "Tournaments",
                column: "UpdatedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Teams_CreatedByUserId",
                table: "Teams",
                column: "CreatedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Teams_UpdatedByUserId",
                table: "Teams",
                column: "UpdatedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Players_CreatedByUserId",
                table: "Players",
                column: "CreatedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Players_UpdatedByUserId",
                table: "Players",
                column: "UpdatedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_MatchEvents_CreatedByUserId",
                table: "MatchEvents",
                column: "CreatedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_MatchEvents_UpdatedByUserId",
                table: "MatchEvents",
                column: "UpdatedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Matches_CreatedByUserId",
                table: "Matches",
                column: "CreatedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Matches_UpdatedByUserId",
                table: "Matches",
                column: "UpdatedByUserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Matches_AspNetUsers_CreatedByUserId",
                table: "Matches",
                column: "CreatedByUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Matches_AspNetUsers_UpdatedByUserId",
                table: "Matches",
                column: "UpdatedByUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_MatchEvents_AspNetUsers_CreatedByUserId",
                table: "MatchEvents",
                column: "CreatedByUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_MatchEvents_AspNetUsers_UpdatedByUserId",
                table: "MatchEvents",
                column: "UpdatedByUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Players_AspNetUsers_CreatedByUserId",
                table: "Players",
                column: "CreatedByUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Players_AspNetUsers_UpdatedByUserId",
                table: "Players",
                column: "UpdatedByUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Teams_AspNetUsers_CreatedByUserId",
                table: "Teams",
                column: "CreatedByUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Teams_AspNetUsers_UpdatedByUserId",
                table: "Teams",
                column: "UpdatedByUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Tournaments_AspNetUsers_CreatedByUserId",
                table: "Tournaments",
                column: "CreatedByUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Tournaments_AspNetUsers_UpdatedByUserId",
                table: "Tournaments",
                column: "UpdatedByUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Matches_AspNetUsers_CreatedByUserId",
                table: "Matches");

            migrationBuilder.DropForeignKey(
                name: "FK_Matches_AspNetUsers_UpdatedByUserId",
                table: "Matches");

            migrationBuilder.DropForeignKey(
                name: "FK_MatchEvents_AspNetUsers_CreatedByUserId",
                table: "MatchEvents");

            migrationBuilder.DropForeignKey(
                name: "FK_MatchEvents_AspNetUsers_UpdatedByUserId",
                table: "MatchEvents");

            migrationBuilder.DropForeignKey(
                name: "FK_Players_AspNetUsers_CreatedByUserId",
                table: "Players");

            migrationBuilder.DropForeignKey(
                name: "FK_Players_AspNetUsers_UpdatedByUserId",
                table: "Players");

            migrationBuilder.DropForeignKey(
                name: "FK_Teams_AspNetUsers_CreatedByUserId",
                table: "Teams");

            migrationBuilder.DropForeignKey(
                name: "FK_Teams_AspNetUsers_UpdatedByUserId",
                table: "Teams");

            migrationBuilder.DropForeignKey(
                name: "FK_Tournaments_AspNetUsers_CreatedByUserId",
                table: "Tournaments");

            migrationBuilder.DropForeignKey(
                name: "FK_Tournaments_AspNetUsers_UpdatedByUserId",
                table: "Tournaments");

            migrationBuilder.DropIndex(
                name: "IX_Tournaments_CreatedByUserId",
                table: "Tournaments");

            migrationBuilder.DropIndex(
                name: "IX_Tournaments_UpdatedByUserId",
                table: "Tournaments");

            migrationBuilder.DropIndex(
                name: "IX_Teams_CreatedByUserId",
                table: "Teams");

            migrationBuilder.DropIndex(
                name: "IX_Teams_UpdatedByUserId",
                table: "Teams");

            migrationBuilder.DropIndex(
                name: "IX_Players_CreatedByUserId",
                table: "Players");

            migrationBuilder.DropIndex(
                name: "IX_Players_UpdatedByUserId",
                table: "Players");

            migrationBuilder.DropIndex(
                name: "IX_MatchEvents_CreatedByUserId",
                table: "MatchEvents");

            migrationBuilder.DropIndex(
                name: "IX_MatchEvents_UpdatedByUserId",
                table: "MatchEvents");

            migrationBuilder.DropIndex(
                name: "IX_Matches_CreatedByUserId",
                table: "Matches");

            migrationBuilder.DropIndex(
                name: "IX_Matches_UpdatedByUserId",
                table: "Matches");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "Tournaments");

            migrationBuilder.DropColumn(
                name: "CreatedByUserId",
                table: "Tournaments");

            migrationBuilder.DropColumn(
                name: "RowVersion",
                table: "Tournaments");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "Tournaments");

            migrationBuilder.DropColumn(
                name: "UpdatedByUserId",
                table: "Tournaments");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "Teams");

            migrationBuilder.DropColumn(
                name: "CreatedByUserId",
                table: "Teams");

            migrationBuilder.DropColumn(
                name: "RowVersion",
                table: "Teams");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "Teams");

            migrationBuilder.DropColumn(
                name: "UpdatedByUserId",
                table: "Teams");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "Players");

            migrationBuilder.DropColumn(
                name: "CreatedByUserId",
                table: "Players");

            migrationBuilder.DropColumn(
                name: "RowVersion",
                table: "Players");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "Players");

            migrationBuilder.DropColumn(
                name: "UpdatedByUserId",
                table: "Players");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "MatchEvents");

            migrationBuilder.DropColumn(
                name: "CreatedByUserId",
                table: "MatchEvents");

            migrationBuilder.DropColumn(
                name: "RowVersion",
                table: "MatchEvents");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "MatchEvents");

            migrationBuilder.DropColumn(
                name: "UpdatedByUserId",
                table: "MatchEvents");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "Matches");

            migrationBuilder.DropColumn(
                name: "CreatedByUserId",
                table: "Matches");

            migrationBuilder.DropColumn(
                name: "RowVersion",
                table: "Matches");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "Matches");

            migrationBuilder.DropColumn(
                name: "UpdatedByUserId",
                table: "Matches");
        }
    }
}
