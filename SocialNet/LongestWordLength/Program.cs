// See https://habr.com/ru/articles/782250/ for more information

using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Jobs;
using BenchmarkDotNet.Running;
using BenchmarkDotNet.Toolchains.InProcess.Emit;
using LongestWordLength;
using Perfolizer.Horology;

BenchmarkRunner.Run<LongestWordLengthBench>(
    DefaultConfig.Instance
        .WithOption(ConfigOptions.StopOnFirstError, true)
        .AddJob(
            Job.ShortRun
                .WithToolchain(new InProcessEmitToolchain(true))
                .WithIterationTime(TimeInterval.FromMilliseconds(150))
        )
);
