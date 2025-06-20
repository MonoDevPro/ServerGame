using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Game.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class Initial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "gameserver");

            migrationBuilder.CreateSequence(
                name: "BaseEntitySequence",
                schema: "gameserver");

            migrationBuilder.CreateTable(
                name: "Accounts",
                schema: "gameserver",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false, defaultValueSql: "nextval('gameserver.\"BaseEntitySequence\"')"),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    Created = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    LastModified = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    LastModifiedBy = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    AccountType = table.Column<int>(type: "integer", nullable: false),
                    BanStatus = table.Column<string>(type: "text", nullable: true),
                    BanExpiresAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    BanReason = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    BannedById = table.Column<long>(type: "bigint", nullable: true),
                    LastLoginInfo = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Accounts", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Accounts",
                schema: "gameserver");

            migrationBuilder.DropSequence(
                name: "BaseEntitySequence",
                schema: "gameserver");
        }
    }
}
