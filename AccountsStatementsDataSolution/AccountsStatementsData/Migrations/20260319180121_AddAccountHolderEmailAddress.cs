using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AccountsStatementsData.Migrations
{
    /// <inheritdoc />
    public partial class AddAccountHolderEmailAddress : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_Statement",
                table: "Statement");

            migrationBuilder.RenameTable(
                name: "Statement",
                newName: "Statements");

            migrationBuilder.AddColumn<string>(
                name: "AccountHolderEmailAddress",
                table: "Accounts",
                type: "character varying(255)",
                maxLength: 255,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Statements",
                table: "Statements",
                column: "StatementId");

            migrationBuilder.CreateIndex(
                name: "IX_Statements_AccountId",
                table: "Statements",
                column: "AccountId");

            migrationBuilder.AddForeignKey(
                name: "FK_Statements_Accounts_AccountId",
                table: "Statements",
                column: "AccountId",
                principalTable: "Accounts",
                principalColumn: "AccountId",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Statements_Accounts_AccountId",
                table: "Statements");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Statements",
                table: "Statements");

            migrationBuilder.DropIndex(
                name: "IX_Statements_AccountId",
                table: "Statements");

            migrationBuilder.DropColumn(
                name: "AccountHolderEmailAddress",
                table: "Accounts");

            migrationBuilder.RenameTable(
                name: "Statements",
                newName: "Statement");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Statement",
                table: "Statement",
                column: "StatementId");
        }
    }
}
