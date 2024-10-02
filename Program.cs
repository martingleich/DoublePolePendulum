using System.Drawing;
using System.Reflection;
using System.Runtime.Versioning;
using CommandLine;
using CommandLine.Text;

[assembly:SupportedOSPlatform("windows")]
namespace DoublePolePendulum;

class Options
{
    [Option('o', "output", HelpText = "The path of the output file")]
    public string? FileName {get; set;} = null;

    [Option('s', "size", HelpText = "The size of the result image", Required = true)]
    public int Size {get; set;}

    [Option('d', "distance", HelpText = "The distance between the poles", Default = 0.1)]
    public double Distance {get; set;}

    [Option('a', "attraction", HelpText = "The attraction of the poles", Default = 1)]
    public double Attraction {get; set;}

    [Option('f', "friction", HelpText = "The friction coefficent", Default = 0.5)]
    public double Friction {get; set;}
    [Option("seed", HelpText = "The seed used for randomization")]
    public int? Seed {get; set;}

    [Usage(ApplicationAlias = "DoublePolePendulum.exe")]
    public static IEnumerable<Example> Examples
    {
        get{
            yield return new Example("normal", new Options{Size=512, FileName="image.png"});
        }
    }

}
public sealed class Program
{
    public static void Main(string[] args)
    {
        Parser.Default.ParseArguments<Options>(args)
            .WithParsed(Main);
    }

    private static void Main(Options o)
    {
        var rnd = o.Seed is {} value ? new Random(value) : new Random();
        Func<Vec2, bool> sampler = v => Math.Sin(v.X * 100 + v.Y * 100) > 0;
        var bitmap = new Bitmap(o.Size, o.Size);
        for(int x = 0; x < o.Size; ++x)
            for(int y = 0; y < o.Size; ++y)
                bitmap.SetPixel(x, y, sampler(new Vec2(((double)x / o.Size) - 0.5, ((double)y / o.Size) - 0.5)) ? Color.Black : Color.White);

        if(o.FileName != null)
            bitmap.Save(o.FileName);
        else
            bitmap.Save(Console.OpenStandardOutput(), System.Drawing.Imaging.ImageFormat.Png);
    }
}

public readonly record struct Vec2(double X, double Y)
{

}
