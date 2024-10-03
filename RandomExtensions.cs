namespace DoublePolePendulum;

public static class RandomExtensions
{
    public static Vec2 NextVec(this Random rnd) => new (rnd.NextDouble(), rnd.NextDouble());
}
