using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PayHelp.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddUserResolutionFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "DataResolvidoUsuario",
                table: "Tickets",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "FeedbackUsuario",
                table: "Tickets",
                type: "nvarchar(2000)",
                maxLength: 2000,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "NotaUsuario",
                table: "Tickets",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "ResolvidoPeloUsuario",
                table: "Tickets",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "FeedbackUsuario",
                table: "Reports",
                type: "nvarchar(2000)",
                maxLength: 2000,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "NotaUsuario",
                table: "Reports",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "ResolvidoPeloUsuario",
                table: "Reports",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DataResolvidoUsuario",
                table: "Tickets");

            migrationBuilder.DropColumn(
                name: "FeedbackUsuario",
                table: "Tickets");

            migrationBuilder.DropColumn(
                name: "NotaUsuario",
                table: "Tickets");

            migrationBuilder.DropColumn(
                name: "ResolvidoPeloUsuario",
                table: "Tickets");

            migrationBuilder.DropColumn(
                name: "FeedbackUsuario",
                table: "Reports");

            migrationBuilder.DropColumn(
                name: "NotaUsuario",
                table: "Reports");

            migrationBuilder.DropColumn(
                name: "ResolvidoPeloUsuario",
                table: "Reports");
        }
    }
}
