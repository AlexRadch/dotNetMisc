// See https://habr.com/ru/articles/782250/ for more information

using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Running;

var config = ManualConfig.Create(DefaultConfig.Instance)
//var config = new DebugInProcessConfig()
    //.WithOption(ConfigOptions.JoinSummary, true)
    .WithOption(ConfigOptions.StopOnFirstError, true)
    ;

BenchmarkRunner.Run(typeof(Program).Assembly, config);
