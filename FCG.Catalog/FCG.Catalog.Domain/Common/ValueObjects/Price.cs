namespace FCG.Catalog.Domain.Common.ValueObjects;

public class Price
{
    public decimal Value { get; private set; }

    private Price() { }

    public Price(decimal value)
    {
        if (value < 0)
            throw new ArgumentException("O preço não pode ser negativo");

        Value = value;
    }

    public static Price Create(decimal rawInput)
    {
        return new Price(rawInput);
    }
}