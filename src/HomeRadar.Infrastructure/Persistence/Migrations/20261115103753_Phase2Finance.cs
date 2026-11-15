using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HomeRadar.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class Phase2Finance : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_business_config_versions_ValidFrom",
                table: "business_config_versions");

            migrationBuilder.DropColumn(
                name: "ValidTo",
                table: "business_config_versions");

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "LoadedAt",
                table: "business_config_versions",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)));

            migrationBuilder.AddColumn<int>(
                name: "Revision",
                table: "business_config_versions",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "listing_analyses",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ListingId = table.Column<Guid>(type: "uuid", nullable: false),
                    FinancingProfileId = table.Column<Guid>(type: "uuid", nullable: false),
                    InputHash = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    BusinessConfigVersionId = table.Column<Guid>(type: "uuid", nullable: false),
                    TaxRuleSetIds = table.Column<Guid[]>(type: "uuid[]", nullable: false),
                    Verdict = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    PriceAmount = table.Column<decimal>(type: "numeric(14,2)", precision: 14, scale: 2, nullable: false),
                    MaximumAffordablePrice = table.Column<decimal>(type: "numeric(14,2)", precision: 14, scale: 2, nullable: true),
                    InputJson = table.Column<string>(type: "jsonb", nullable: false),
                    ResultJson = table.Column<string>(type: "jsonb", nullable: false),
                    CalculatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_listing_analyses", x => x.Id);
                    table.ForeignKey(
                        name: "FK_listing_analyses_financing_profiles_FinancingProfileId",
                        column: x => x.FinancingProfileId,
                        principalTable: "financing_profiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_listing_analyses_listings_ListingId",
                        column: x => x.ListingId,
                        principalTable: "listings",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "tax_rule_sets",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Jurisdiction = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    ValidFrom = table.Column<DateOnly>(type: "date", nullable: false),
                    Revision = table.Column<int>(type: "integer", nullable: false),
                    VerifiedOn = table.Column<DateOnly>(type: "date", nullable: true),
                    SourceFile = table.Column<string>(type: "character varying(260)", maxLength: 260, nullable: false),
                    LoadedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    DocumentJson = table.Column<string>(type: "jsonb", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tax_rule_sets", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_business_config_versions_ValidFrom_Revision",
                table: "business_config_versions",
                columns: new[] { "ValidFrom", "Revision" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_listing_analyses_FinancingProfileId",
                table: "listing_analyses",
                column: "FinancingProfileId");

            migrationBuilder.CreateIndex(
                name: "IX_listing_analyses_ListingId_FinancingProfileId_CalculatedAt",
                table: "listing_analyses",
                columns: new[] { "ListingId", "FinancingProfileId", "CalculatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_listing_analyses_ListingId_FinancingProfileId_InputHash",
                table: "listing_analyses",
                columns: new[] { "ListingId", "FinancingProfileId", "InputHash" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_tax_rule_sets_Jurisdiction_ValidFrom_Revision",
                table: "tax_rule_sets",
                columns: new[] { "Jurisdiction", "ValidFrom", "Revision" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "listing_analyses");

            migrationBuilder.DropTable(
                name: "tax_rule_sets");

            migrationBuilder.DropIndex(
                name: "IX_business_config_versions_ValidFrom_Revision",
                table: "business_config_versions");

            migrationBuilder.DropColumn(
                name: "LoadedAt",
                table: "business_config_versions");

            migrationBuilder.DropColumn(
                name: "Revision",
                table: "business_config_versions");

            migrationBuilder.AddColumn<DateOnly>(
                name: "ValidTo",
                table: "business_config_versions",
                type: "date",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_business_config_versions_ValidFrom",
                table: "business_config_versions",
                column: "ValidFrom",
                unique: true);
        }
    }
}
