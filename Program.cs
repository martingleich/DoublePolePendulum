using System.Collections.Immutable;
using System.Diagnostics;
using System.Drawing;
using System.Threading.Tasks.Dataflow;


namespace DoublePolePendulum;
public sealed class Program
{
    public static void Main(string[] args)
    {
        var watch = new Stopwatch();
        watch.Start();
        CommandLine.ParserResultExtensions.WithParsed(CommandLine.Parser.Default.ParseArguments<Options>(args), RealMain);
        watch.Stop();
        Console.WriteLine($"Completed in {watch.Elapsed}");
    }


    private static void RealMain(Options o)
    {
        var rnd = o.Seed is {} seed ? new Random(seed) : new Random();

        var sampler = Sampler(o);
        Vec2 to_vec(int x, int y) => new((double)x / o.Size, (double)y / o.Size);

        ImmutableArray<Vec2> create_simple_image(Random rnd, int samples, Action<int> progress)
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
                    progress(samples);
                    builder.Add(px / samples);
                }
            }
            return builder.ToImmutable();
        }
        var cores = o.Cores ?? Environment.ProcessorCount;
        var real_samples = (o.Samples + cores - 1) / cores * cores; // Round to to nearest multiple of cores, so that each core can do the same amount of work.

        var layers = WaitWithProgress(progress => Task.WhenAll(Split(real_samples, cores).Select(s =>
            {
                var rnd2 = new Random(rnd.Next());
                return Task.Run(() => create_simple_image(rnd2, s, progress));
            })), real_samples * o.Size * o.Size);
        var final_image = SumLayers(layers);

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

    public static T WaitWithProgress<T>(Func<Action<int>, Task<T>> taskFactory, int total, string doneText = "")
    {
        var start = DateTime.Now;
        int done = 0;
        var task = taskFactory(c => Interlocked.Add(ref done, c));
        var lastText = "";
        do 
        {
            var progress = (double)done/total;
            var passed_time = DateTime.Now-start;
            TimeSpan? total_time;
            try
            {
                total_time = passed_time/progress;
            } catch(OverflowException)
            {
                total_time = null;
            }
            CleanWrite($"{progress*100:0.00}% | {total_time-passed_time} -> {passed_time}");
        } while(!task.Wait(TimeSpan.FromSeconds(1)));
        CleanWrite(doneText);
        return task.Result;

        void CleanWrite(string text)
        {
            string clean = "";
            if(lastText.Length > doneText.Length)
            {
                var toClean = lastText.Length - doneText.Length;
                clean = new string(' ', toClean) + new string('\b', toClean);
            }
            
            Console.Write(new string('\b', lastText.Length) + text + clean);
            lastText = text;
        }
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
            return Enumerable.Repeat(1, total);
        int one = total / count;
        if(total % count == 0)
            return Enumerable.Repeat(one, count);
        else
            return Enumerable.Repeat(one, count - 1).Prepend(total - one*count);
    }
    private static Func<Vec2, bool?> Sampler(Options o) => p =>
    {
        var t = o.TimeStep;
        var v = Vec2.ZERO;
        int count = o.MaxCountSteps;
        while(count-- > 0)
        {
            var d1 = o.Pole1 - p;
            var d2 = o.Pole2 - p;
            var ap = p * -o.Pendulum;
            var vl = v.Length;
            var d1l = d1.Length;
            var d2l = d2.Length;
            if (vl < o.RequiredVelocity)
            {
                if (d1l < o.RequiredDistance)
                    return true;
                if (d2l < o.RequiredDistance)
                    return false;
            }
            var af = v * (vl * o.Friction);
            var a1 = d1/(d1l * (d1l*d1l + o.Heigth));
            var a2 = d2/(d2l * (d2l*d2l + o.Heigth));
            var a = (a1 + a2) * o.Attraction + ap - af;
            var nv = v + t*a;
            var np = p + t*v + t*t*a;
            v = nv;
            p = np;
        }
        return null;
    };
}
