using Random = System.Random;

namespace Enamel.Utils;

public static class RandomUtils
{
    private static Random Random { get; } = new();

    public static int RandomInt(int max)
    {
        return Random.Next(max);
    }
}