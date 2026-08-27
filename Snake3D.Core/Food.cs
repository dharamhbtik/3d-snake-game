namespace Snake3D.Core;

public enum FoodType
{
    Ladybug,      // Small beetle / insect (5 pts)
    Grasshopper,  // Medium insect / cricket (15 pts)
    Frog,         // Large meadow frog (25 pts)
    GoldenFrog,   // Rare golden amphibian (50 pts)
    Dragonfly,    // Rare flying insect (40 pts)

    // Backward-compatibility aliases
    Apple = Frog,
    GoldenApple = GoldenFrog,
    SpeedFruit = Dragonfly
}

public sealed record Food(
    GridPoint Position,
    FoodType Type = FoodType.Frog,
    int Points = 25,
    double LifetimeSeconds = 0.0)
{
    public bool IsSpecial => Type is FoodType.GoldenFrog or FoodType.Dragonfly;

    public string DisplayName => Type switch
    {
        FoodType.Ladybug => "🐞 LADYBUG (+5)",
        FoodType.Grasshopper => "🦗 GRASSHOPPER (+15)",
        FoodType.Frog => "🐸 FROG (+25)",
        FoodType.GoldenFrog => "✨ GOLDEN FROG (+50)",
        FoodType.Dragonfly => "🦋 DRAGONFLY (+40)",
        _ => "🐸 PREY (+25)"
    };
}
