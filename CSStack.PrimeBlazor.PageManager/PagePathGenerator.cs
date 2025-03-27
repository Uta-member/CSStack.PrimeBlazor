using Microsoft.CodeAnalysis;

namespace CSStack.PrimeBlazor.PageManager
{
    [Generator(LanguageNames.CSharp)]
    public sealed class PagePathGenerator : IIncrementalGenerator
    {
        public void Initialize(IncrementalGeneratorInitializationContext context)
        {
            context.RegisterPostInitializationOutput(
                static x =>
                {
                    x.AddSource(
                        "GeneratedBlazorCode.cs",
                        """
                        namespace CSStack.PrimeBlazor.PageManager;
                        using System;

                        [AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
                        internal sealed class PagePathAttribute : Attribute
                        {
                            public string Path { get; }
                            public PagePathAttribute(string path)
                            {
                                Path = path;
                            }
                        }
                        """);
                });
        }
    }
}
