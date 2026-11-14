namespace HomeRadar.Domain.ValueObjects;

public readonly record struct Area(decimal SquareMetres)
{
    public static Area? FromNullable(decimal? squareMetres) =>
        squareMetres is > 0 ? new Area(Math.Round(squareMetres.Value, 2, MidpointRounding.ToEven)) : null;

    public PricePerSquareMetre PriceFor(Money price) =>
        SquareMetres <= 0 ? throw new InvalidOperationException("Area must be positive.") : new PricePerSquareMetre(Math.Round(price.Amount / SquareMetres, 2, MidpointRounding.ToEven));
}

public readonly record struct PricePerSquareMetre(decimal EurosPerSquareMetre);
