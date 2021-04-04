using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Loader;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Text;

namespace Archon
{
    public static class Compiler
    {
        public static byte[] Compile(string code, out Diagnostic[] diagnostics, out bool success)
        {
            diagnostics = null;

            using var peStream = new MemoryStream();
            var result = GenerateCode(code).Emit(peStream);
            success = result.Success;
            diagnostics = result.Diagnostics.ToArray();

            if (!result.Success)
            {
                return null;
            }

            peStream.Seek(0, SeekOrigin.Begin);

            return peStream.ToArray();
        }

        private static CSharpCompilation GenerateCode(string sourceCode)
        {
            var codeString = SourceText.From(sourceCode);
            var options = CSharpParseOptions.Default.WithLanguageVersion(LanguageVersion.CSharp7_3);

            var parsedSyntaxTree = SyntaxFactory.ParseSyntaxTree(codeString, options);

            var references = AppDomain.CurrentDomain.GetAssemblies()
                .Where(a => !a.IsDynamic)
                .Select(a => MetadataReference.CreateFromFile(a.Location));

            return CSharpCompilation.Create("compiledEvaluation.dll", new[] { parsedSyntaxTree },
                references: references,
                options: new CSharpCompilationOptions(OutputKind.ConsoleApplication,
                optimizationLevel: OptimizationLevel.Release,
                assemblyIdentityComparer: DesktopAssemblyIdentityComparer.Default));
        }

        public static Assembly GetAssembly(byte[] compiledAssembly)
        {
            using var asm = new MemoryStream(compiledAssembly);
            var assemblyLoadContext = new AssemblyLoadContext("compiledEvaluation.dll", true);
            return assemblyLoadContext.LoadFromStream(asm);
        }
    }
}
