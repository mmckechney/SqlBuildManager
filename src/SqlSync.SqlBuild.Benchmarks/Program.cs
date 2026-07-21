using BenchmarkDotNet.Running;

BenchmarkSwitcher.FromAssembly(typeof(Program).Assembly).Run(args);

// Make the entry-point type visible for BenchmarkSwitcher reflection.
public partial class Program { }
