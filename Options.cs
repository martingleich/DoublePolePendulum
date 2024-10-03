using CommandLine;
using CommandLine.Text;

namespace DoublePolePendulum;

class Options
{
    [Option('o', "output", HelpText = "The path of the output file")]
    public string? FileName {get; set;} = null;

    [Option("size", HelpText = "The size of the result image", Required = true)]
    public int Size {get; set;}
    [Option("samples", HelpText = "The numer of samples per pixel", Default = 10)]
    public int Samples {get; set;}

    [Option("distance", HelpText = "The distance between the poles", Default = 0.2)]
    public double Distance {get; set;}

    [Option("attraction", HelpText = "The attraction of the poles", Default = 0.1)]
    public double Attraction {get; set;}

    [Option("friction", HelpText = "The friction coefficent", Default = 0.1)]
    public double Friction {get; set;}

    [Option("pendulum", HelpText = "The coefficent of the pendulum", Default = 1)]
    public double Pendulum {get; set;}
    [Option("heigth", HelpText = "The heigth of the pendulum above the ground", Default = 0.05)]
    public double Heigth {get; set;}

    [Option("seed", HelpText = "The seed used for randomization")]
    public int? Seed {get; set;}

    [Option("cores", HelpText="The number of process cores to use", Default =null)]
    public int? Cores {get;set;}
    [Option("timeStep", HelpText="The timestemp of the simulation", Default =0.05)]
    public double TimeStep {get;set;}
    [Option("maxCountSteps", HelpText="The maximal number of steps of the simulation", Default = 5000)]
    public int MaxCountSteps {get;set;}
    [Option("requiredVelocity", HelpText="The velocity below which the pendulum counts as stoped", Default = 0.1)]
    public double RequiredVelocity {get;set;}
    [Option("requiredDistance", HelpText="The distance below which the pendulum counts as having reached the pole", Default = 0.01)]
    public double RequiredDistance {get;set;}

    [Usage(ApplicationAlias = "DoublePolePendulum.exe")]
    public static IEnumerable<Example> Examples
    {
        get{
            yield return new Example("normal", new Options{Size=512, FileName="image.png"});
        }
    }
    public Vec2 Pole1 => Vec2.UNIT_X * -Distance;
    public Vec2 Pole2 => Vec2.UNIT_X * Distance;

}
