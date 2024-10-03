namespace DoublePolePendulum;

public readonly record struct Vec2(double X, double Y)
{
    public static Vec2 operator*(Vec2 v, double d) => new (v.X * d, v.Y * d);
    public static Vec2 operator/(Vec2 v, double d) => new (v.X / d, v.Y / d);
    public static Vec2 operator*(double d, Vec2 v) => v * d;
    public static Vec2 operator+(Vec2 a, Vec2 b) => new (a.X + b.X, a.Y + b.Y);
    public static Vec2 operator-(Vec2 a, Vec2 b) => new (a.X - b.X, a.Y - b.Y);
    public static double Dot(Vec2 a, Vec2 b) => a.X * b.X + a.Y * b.Y;
    public double Length => Math.Sqrt(LengthSq);
    public double LengthSq => Dot(this, this);
    public Vec2 Norm() => this/Length;
    public Vec2 Transpose() => new(Y, X);

    public override string ToString() => $"{X}, {Y}";
    public static Vec2 Sum(Vec2 a, Vec2 b) => a + b;
    public static readonly Vec2 ZERO = default;
    public static readonly Vec2 UNIT_X = new (1,0);
    public static readonly Vec2 UNIT_Y = new (0,1);
    public double DistanceTo(Vec2 other)  => (other-this).Length;
}
