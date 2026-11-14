using CsCheck;
using HomeRadar.Domain.ValueObjects;

namespace HomeRadar.Domain.Tests;

public sealed class MoneyTests
{
    [Fact]
    public void Addition_and_subtraction_are_inverse_and_keep_two_decimals() =>
        Gen.Select(Gen.Decimal[-1_000_000m, 1_000_000m], Gen.Decimal[-1_000_000m, 1_000_000m])
            .Sample((a, b) =>
            {
                var left = Money.Euros(a);
                var right = Money.Euros(b);
                var roundTrip = left + right - right;
                roundTrip.ShouldBe(left);
                (left + right).Amount.ShouldBe(decimal.Round((left + right).Amount, 2));
            });

    [Fact]
    public void Rounding_is_bankers_rounding()
    {
        Money.Euros(2.345m).Amount.ShouldBe(2.34m);
        Money.Euros(2.355m).Amount.ShouldBe(2.36m);
    }

    [Fact]
    public void Cadastral_reference_accepts_spaces_and_lowercase()
    {
        CadastralReference.TryParse("9872023 vh5797s 0001 wx", out var reference).ShouldBeTrue();
        reference.Value.ShouldBe("9872023VH5797S0001WX");
        reference.ParcelPart.ShouldBe("9872023VH5797S");
        CadastralReference.TryParse("not-a-reference", out _).ShouldBeFalse();
    }
}

public sealed class UtmProjectionTests
{
    [Theory]
    [InlineData(39.5674, 2.6480, 469764.60, 4379803.81)]
    [InlineData(39.7, 2.9, 491426.79, 4394465.80)]
    [InlineData(39.9, 3.1, 508548.41, 4416663.08)]
    public void Matches_epsg25831_reference_values_within_ten_centimetres(double latitude, double longitude, double expectedEasting, double expectedNorthing)
    {
        var (easting, northing) = HomeRadar.Domain.ValueObjects.UtmProjection.ToUtm31N(latitude, longitude);

        easting.ShouldBe(expectedEasting, 0.1);
        northing.ShouldBe(expectedNorthing, 0.1);
    }
}
