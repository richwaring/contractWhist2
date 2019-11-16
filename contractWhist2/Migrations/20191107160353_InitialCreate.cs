using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace contractWhist2.Migrations
{
    public partial class InitialCreate : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "card",
                columns: table => new
                {
                    id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    name = table.Column<string>(nullable: true),
                    fullname = table.Column<string>(nullable: true),
                    valueInSuit = table.Column<int>(nullable: false),
                    suit = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_card", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "game",
                columns: table => new
                {
                    id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    crdate = table.Column<DateTime>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_game", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "playerRound",
                columns: table => new
                {
                    id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    gameId = table.Column<int>(nullable: false),
                    roundId = table.Column<int>(nullable: false),
                    playerId = table.Column<int>(nullable: false),
                    target = table.Column<int>(nullable: false),
                    tricksWon = table.Column<int>(nullable: false),
                    roundPoints = table.Column<int>(nullable: false),
                    gamePoints = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_playerRound", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "gameRound",
                columns: table => new
                {
                    id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    gameid = table.Column<int>(nullable: false),
                    roundNumber = table.Column<int>(nullable: false),
                    numCards = table.Column<int>(nullable: false),
                    trumpCardsid = table.Column<int>(nullable: true),
                    trumpSuit = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_gameRound", x => x.id);
                    table.ForeignKey(
                        name: "FK_gameRound_game_gameid",
                        column: x => x.gameid,
                        principalTable: "game",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_gameRound_card_trumpCardsid",
                        column: x => x.trumpCardsid,
                        principalTable: "card",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "playerRoundCard",
                columns: table => new
                {
                    id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    prID = table.Column<int>(nullable: false),
                    playerId = table.Column<int>(nullable: false),
                    roundid = table.Column<int>(nullable: false),
                    thisCardid = table.Column<int>(nullable: true),
                    cardPlayed = table.Column<string>(nullable: true),
                    playerRoundid = table.Column<int>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_playerRoundCard", x => x.id);
                    table.ForeignKey(
                        name: "FK_playerRoundCard_playerRound_playerRoundid",
                        column: x => x.playerRoundid,
                        principalTable: "playerRound",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_playerRoundCard_card_thisCardid",
                        column: x => x.thisCardid,
                        principalTable: "card",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_gameRound_gameid",
                table: "gameRound",
                column: "gameid",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_gameRound_trumpCardsid",
                table: "gameRound",
                column: "trumpCardsid");

            migrationBuilder.CreateIndex(
                name: "IX_playerRoundCard_playerRoundid",
                table: "playerRoundCard",
                column: "playerRoundid");

            migrationBuilder.CreateIndex(
                name: "IX_playerRoundCard_thisCardid",
                table: "playerRoundCard",
                column: "thisCardid");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "gameRound");

            migrationBuilder.DropTable(
                name: "playerRoundCard");

            migrationBuilder.DropTable(
                name: "game");

            migrationBuilder.DropTable(
                name: "playerRound");

            migrationBuilder.DropTable(
                name: "card");
        }
    }
}
