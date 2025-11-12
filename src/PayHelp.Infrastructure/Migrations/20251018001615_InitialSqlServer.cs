using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PayHelp.Infrastructure.Migrations
{

    public partial class InitialSqlServer : Migration
    {

        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CannedMessages",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Titulo = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Conteudo = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                    GatilhoPalavraChave = table.Column<string>(type: "nvarchar(400)", maxLength: 400, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CannedMessages", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "FaqEntries",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TituloProblema = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false),
                    DescricaoProblema = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: false),
                    Solucao = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: false),
                    DataCriacao = table.Column<DateTime>(type: "datetime2", nullable: false),
                    TicketId = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FaqEntries", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Reports",
                columns: table => new
                {
                    TicketId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SolicitanteEmail = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    SolicitanteRole = table.Column<int>(type: "int", nullable: false),
                    StatusFinal = table.Column<int>(type: "int", nullable: false),
                    Duracao = table.Column<TimeSpan>(type: "time", nullable: true),
                    CriadoEmUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EncerradoEmUtc = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Reports", x => x.TicketId);
                });

            migrationBuilder.CreateTable(
                name: "Tickets",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SupportUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Titulo = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Descricao = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    CriadoEmUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EncerradoEmUtc = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tickets", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    NumeroInscricao = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Nome = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Email = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    SenhaHash = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Role = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "TicketMessages",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TicketId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    RemetenteUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Conteudo = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: false),
                    EnviadoEmUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Automatica = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TicketMessages", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TicketMessages_Tickets_TicketId",
                        column: x => x.TicketId,
                        principalTable: "Tickets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_TicketMessages_TicketId",
                table: "TicketMessages",
                column: "TicketId");

            migrationBuilder.CreateIndex(
                name: "IX_Users_Email",
                table: "Users",
                column: "Email",
                unique: true);
        }


        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CannedMessages");

            migrationBuilder.DropTable(
                name: "FaqEntries");

            migrationBuilder.DropTable(
                name: "Reports");

            migrationBuilder.DropTable(
                name: "TicketMessages");

            migrationBuilder.DropTable(
                name: "Users");

            migrationBuilder.DropTable(
                name: "Tickets");
        }
    }
}
