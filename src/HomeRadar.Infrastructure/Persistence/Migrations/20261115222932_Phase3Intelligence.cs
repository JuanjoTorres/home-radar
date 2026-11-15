using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HomeRadar.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class Phase3Intelligence : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "CadastralBuiltSquareMetres",
                table: "listings",
                type: "numeric(10,2)",
                precision: 10,
                scale: 2,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CadastralMatch",
                table: "listings",
                type: "character varying(16)",
                maxLength: 16,
                nullable: false,
                defaultValue: "None");

            migrationBuilder.AddColumn<string>(
                name: "CadastralParcelReference",
                table: "listings",
                type: "character varying(14)",
                maxLength: 14,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "CadastralRustic",
                table: "listings",
                type: "boolean",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CadastralUse",
                table: "listings",
                type: "character varying(60)",
                maxLength: 60,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "CadastralYearBuilt",
                table: "listings",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "IntelligenceUpdatedAt",
                table: "listings",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string[]>(
                name: "RedFlagCodes",
                table: "listings",
                type: "text[]",
                nullable: false,
                defaultValue: new string[0]);

            migrationBuilder.AddColumn<decimal>(
                name: "ReferenceValueAmount",
                table: "listings",
                type: "numeric(14,2)",
                precision: 14,
                scale: 2,
                nullable: true);

            migrationBuilder.AddColumn<DateOnly>(
                name: "ReferenceValueDate",
                table: "listings",
                type: "date",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RenovationLevel",
                table: "listings",
                type: "character varying(16)",
                maxLength: 16,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "RenovationManual",
                table: "listings",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<decimal>(
                name: "RenovationProbableCost",
                table: "listings",
                type: "numeric(14,2)",
                precision: 14,
                scale: 2,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Score",
                table: "listings",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "cadastral_parcels",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ParcelReference = table.Column<string>(type: "character varying(14)", maxLength: 14, nullable: false),
                    Status = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: false),
                    Units = table.Column<string>(type: "jsonb", nullable: false),
                    Address = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: true),
                    IsRustic = table.Column<bool>(type: "boolean", nullable: true),
                    FetchedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_cadastral_parcels", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "reference_rates",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Code = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    Month = table.Column<DateOnly>(type: "date", nullable: false),
                    Percent = table.Column<decimal>(type: "numeric(8,4)", precision: 8, scale: 4, nullable: false),
                    Source = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    FetchedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_reference_rates", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "renovation_overrides",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ListingId = table.Column<Guid>(type: "uuid", nullable: false),
                    Level = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: true),
                    Items = table.Column<string>(type: "jsonb", nullable: false),
                    TotalBudget = table.Column<decimal>(type: "numeric(14,2)", precision: 14, scale: 2, nullable: true),
                    Notes = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_renovation_overrides", x => x.Id);
                    table.ForeignKey(
                        name: "FK_renovation_overrides_listings_ListingId",
                        column: x => x.ListingId,
                        principalTable: "listings",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_listings_Score",
                table: "listings",
                column: "Score");

            migrationBuilder.CreateIndex(
                name: "IX_cadastral_parcels_ParcelReference",
                table: "cadastral_parcels",
                column: "ParcelReference",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_reference_rates_Code_Month",
                table: "reference_rates",
                columns: new[] { "Code", "Month" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_renovation_overrides_ListingId",
                table: "renovation_overrides",
                column: "ListingId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "cadastral_parcels");

            migrationBuilder.DropTable(
                name: "reference_rates");

            migrationBuilder.DropTable(
                name: "renovation_overrides");

            migrationBuilder.DropIndex(
                name: "IX_listings_Score",
                table: "listings");

            migrationBuilder.DropColumn(
                name: "CadastralBuiltSquareMetres",
                table: "listings");

            migrationBuilder.DropColumn(
                name: "CadastralMatch",
                table: "listings");

            migrationBuilder.DropColumn(
                name: "CadastralParcelReference",
                table: "listings");

            migrationBuilder.DropColumn(
                name: "CadastralRustic",
                table: "listings");

            migrationBuilder.DropColumn(
                name: "CadastralUse",
                table: "listings");

            migrationBuilder.DropColumn(
                name: "CadastralYearBuilt",
                table: "listings");

            migrationBuilder.DropColumn(
                name: "IntelligenceUpdatedAt",
                table: "listings");

            migrationBuilder.DropColumn(
                name: "RedFlagCodes",
                table: "listings");

            migrationBuilder.DropColumn(
                name: "ReferenceValueAmount",
                table: "listings");

            migrationBuilder.DropColumn(
                name: "ReferenceValueDate",
                table: "listings");

            migrationBuilder.DropColumn(
                name: "RenovationLevel",
                table: "listings");

            migrationBuilder.DropColumn(
                name: "RenovationManual",
                table: "listings");

            migrationBuilder.DropColumn(
                name: "RenovationProbableCost",
                table: "listings");

            migrationBuilder.DropColumn(
                name: "Score",
                table: "listings");
        }
    }
}
