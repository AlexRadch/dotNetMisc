using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Running;

namespace BagQueueStackBench;

public class Program
{
    public static void Main()
    {
        var config = ManualConfig.Create(DefaultConfig.Instance)
            .WithOption(ConfigOptions.JoinSummary, true);

        //BenchmarkRunner.Run(typeof(Program).Assembly, config);

        BenchmarkRunner.Run(new[] {
        BenchmarkConverter.TypeToBenchmarks( typeof(PccBenchmark_Int), config),
        //BenchmarkConverter.TypeToBenchmarks( typeof(PccBenchmark_String), config),
    });
    }
}
