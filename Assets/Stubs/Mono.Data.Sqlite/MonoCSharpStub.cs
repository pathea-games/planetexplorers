// Stub for Mono.CSharp runtime C# evaluator.
// Used by SkExpEvaluator.cs to compile skill expression strings at runtime.
// This API is not available in .NET Standard — stub exists only so the
// Roslyn analyzer project compiles.
// THESE TYPES MUST NOT BE USED IN RUNTIME CODE.

namespace Mono.CSharp
{
    // CompiledMethod: delegate type returned by Evaluator.Compile.
    // Called as method(ref object result) in SkExpEvaluator.cs.
    public delegate void CompiledMethod(ref object result);

    public static class Evaluator
    {
        public static void Init(string[] args) { }
        public static void ReferenceAssembly(System.Reflection.Assembly assembly) { }
        public static object Evaluate(string expression) => null;
        public static bool Run(string statement) => false;
        // Compile: returns a delegate that can be invoked with (ref object result).
        public static CompiledMethod Compile(string expression) => null;
    }
}
