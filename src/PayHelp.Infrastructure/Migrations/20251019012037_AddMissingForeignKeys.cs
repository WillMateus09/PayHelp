using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PayHelp.Infrastructure.Migrations
{

    public partial class AddMissingForeignKeys : Migration
    {

        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_Tickets_SupportUserId",
                table: "Tickets",
                column: "SupportUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Tickets_UserId",
                table: "Tickets",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_TicketMessages_RemetenteUserId",
                table: "TicketMessages",
                column: "RemetenteUserId");

            migrationBuilder.CreateIndex(
                name: "IX_FaqEntries_TicketId",
                table: "FaqEntries",
                column: "TicketId");

            migrationBuilder.AddForeignKey(
                name: "FK_FaqEntries_Tickets_TicketId",
                table: "FaqEntries",
                column: "TicketId",
                principalTable: "Tickets",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_Reports_Tickets_TicketId",
                table: "Reports",
                column: "TicketId",
                principalTable: "Tickets",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_TicketMessages_Users_RemetenteUserId",
                table: "TicketMessages",
                column: "RemetenteUserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Tickets_Users_SupportUserId",
                table: "Tickets",
                column: "SupportUserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_Tickets_Users_UserId",
                table: "Tickets",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }


        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_FaqEntries_Tickets_TicketId",
                table: "FaqEntries");

            migrationBuilder.DropForeignKey(
                name: "FK_Reports_Tickets_TicketId",
                table: "Reports");

            migrationBuilder.DropForeignKey(
                name: "FK_TicketMessages_Users_RemetenteUserId",
                table: "TicketMessages");

            migrationBuilder.DropForeignKey(
                name: "FK_Tickets_Users_SupportUserId",
                table: "Tickets");

            migrationBuilder.DropForeignKey(
                name: "FK_Tickets_Users_UserId",
                table: "Tickets");

            migrationBuilder.DropIndex(
                name: "IX_Tickets_SupportUserId",
                table: "Tickets");

            migrationBuilder.DropIndex(
                name: "IX_Tickets_UserId",
                table: "Tickets");

            migrationBuilder.DropIndex(
                name: "IX_TicketMessages_RemetenteUserId",
                table: "TicketMessages");

            migrationBuilder.DropIndex(
                name: "IX_FaqEntries_TicketId",
                table: "FaqEntries");
        }
    }
}
