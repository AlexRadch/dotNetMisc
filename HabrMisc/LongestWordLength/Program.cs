// See https://habr.com/ru/articles/782250/ for more information

using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Jobs;
using BenchmarkDotNet.Running;
using Perfolizer.Horology;

var config = ManualConfig.Create(DefaultConfig.Instance)
//var config = new DebugInProcessConfig()
    //.WithOption(ConfigOptions.JoinSummary, true)
    .WithOption(ConfigOptions.StopOnFirstError, true)
    .AddJob(
        Job.ShortRun.WithIterationTime(TimeInterval.Millisecond * 100)
        )
    ;

BenchmarkRunner.Run(typeof(Program).Assembly, config);
