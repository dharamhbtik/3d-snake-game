namespace Snake3D.Core;

public enum FoodType
{
    Apple,
    GoldenApple,
    SpeedFruit
}

public sealed record Food(
    GridPoint Position,
    FoodType Type = FoodType.Apple,
    int Points = 10,
    double LifetimeSeconds = 0.0)
{
    public bool IsSpecial => Type != FoodType.Apple;
}
