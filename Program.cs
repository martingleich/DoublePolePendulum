using System.Collections.Immutable;
using System.Drawing;


namespace DoublePolePendulum;
public sealed class Program
{
    public static void Main(string[] args)
    {
        CommandLine.ParserResultExtensions.WithParsed(CommandLine.Parser.Default.ParseArguments<Options>(args), RealMain);
    }

    private static void RealMain(Options o)
    {
        var rnd = o.Seed is {} seed ? new Random(seed) : new Random();

        var sampler = Sampler(o);
        Vec2 to_vec(int x, int y) => new((double)x / o.Size, (double)y / o.Size);

        ImmutableArray<Vec2> create_simple_image(Random rnd, int samples)
        {
            var builder = ImmutableArray.CreateBuilder<Vec2>();
            for (int x = 0; x < o.Size; ++x)
            {
                for (int y = 0; y < o.Size; ++y)
                {
                    var center = to_vec(x, y);
                    var px = Vec2.ZERO;
                    for (int s = 0; s < samples; ++s)
                    {
                        var offset = rnd.NextVec() / o.Size;
                        if (sampler(center + offset) is {} result)
                            px += result ? Vec2.UNIT_X : Vec2.UNIT_Y;
                    }
                    builder.Add(px / samples);
                }
            }
            return builder.ToImmutable();
        }

        var final_image = SumLayers(Task.WhenAll(Split(o.Samples, o.Cores ?? Environment.ProcessorCount).Select(s =>
        {
            var rnd2 = new Random(rnd.Next());
            return Task.Run(() => create_simple_image(rnd2, s));
        })).Result);

        var bitmap = new Bitmap(2*o.Size, 2*o.Size);
        for(int x = 0; x < o.Size; ++x)
        {
            for(int y = 0; y < o.Size; ++y)
            {
                var px = final_image[x*o.Size + y];
                bitmap.SetPixel(o.Size + x, o.Size + y, ToColor(px));
                bitmap.SetPixel(o.Size - x, o.Size + y, ToColor(px.Transpose()));
                bitmap.SetPixel(o.Size + x, o.Size - y, ToColor(px));
                bitmap.SetPixel(o.Size - x, o.Size - y, ToColor(px.Transpose()));
            }
        }

        if(o.FileName != null)
            bitmap.Save(o.FileName);
        else
            bitmap.Save(Console.OpenStandardOutput(), System.Drawing.Imaging.ImageFormat.Png);
    }
    private static Color ToColor(Vec2 v) => Color.FromArgb((int)(v.X * 255), 0, (int)(v.Y*255));
    private static ImmutableArray<Vec2> SumLayers(IReadOnlyCollection<ImmutableArray<Vec2>> layers)
    {
        return layers.Aggregate((a, b) => a.Zip(b, Vec2.Sum).ToImmutableArray()).Select(v => v / layers.Count).ToImmutableArray();
    }
    private static IEnumerable<int> Split(int total, int count)
    {
        if(total <= 0)
            return [];
        if(count > total)
            return [total];
        int one = total / count;
        if(total % count == 0)
            return Enumerable.Repeat(one, count);
        else
            return Enumerable.Repeat(one, count - 1).Prepend(total - one*count);
    }
    private static IEnumerable<Vec2> Points(Vec2 p, Options o)
    {
        var t = o.TimeStep;
        var v = Vec2.ZERO;
        int count = o.MaxCountSteps;
        while(count-- > 0)
        {
            yield return p;
            var d1 = o.Pole1 - p;
            var d2 = o.Pole2 - p;
            var ap = p * -o.Pendulum;
            var af = v * v.Length * o.Friction;
            var a1 = d1.Norm()/(d1.LengthSq + o.Heigth) * o.Attraction;
            var a2 = d2.Norm()/(d2.LengthSq + o.Heigth) * o.Attraction;
            var a = a1 + a2 + ap - af;
            var nv = v + t*a;
            var np = p + t*v + t*t*a;
            v = nv;
            p = np;
            if(v.Length < o.RequiredVelocity && (d1.Length < o.RequiredDistance || d2.Length < o.RequiredDistance))
                yield break;
        }
    }
    private static Func<Vec2, bool?> Sampler(Options o) => p =>
    {
        p = Points(p, o).Last();
        if (o.Pole1.DistanceTo(p) < o.RequiredDistance)
            return true;
        if(o.Pole2.DistanceTo(p) < o.RequiredDistance)
            return false;
        return null;
    };
}
