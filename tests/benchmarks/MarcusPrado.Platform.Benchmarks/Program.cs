using BenchmarkDotNet.Running;

// Run all benchmarks:   dotnet run -c Release
// Run a single class:   dotnet run -c Release --filter *ResultBenchmark*
// Run with memory:      dotnet run -c Release --filter *ResultBenchmark* -m
BenchmarkSwitcher.FromAssembly(typeof(Program).Assembly).Run(args);
