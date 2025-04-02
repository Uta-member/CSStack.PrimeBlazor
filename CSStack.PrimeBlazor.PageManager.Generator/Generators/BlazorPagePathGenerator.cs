using CSStack.PrimeBlazor.PageManager.Generator;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using System.Collections.Immutable;
using System.Linq;
using System.Text;

namespace CSStack.PrimeBlazor.PageManager
{
    [Generator(LanguageNames.CSharp)]
    public class BlazorPagePathGenerator : IIncrementalGenerator
    {
        /// <inheritdoc/>
        public void Initialize(IncrementalGeneratorInitializationContext context)
        {
            // 属性を使えるように登録
            RegisterAttributes(context);

            // PageManager クラスを探して、ページとパスのマッピング情報を取得
            var source = context.SyntaxProvider
                .ForAttributeWithMetadataName(
                    "CSStack.PrimeBlazor.PageManager.BlazorPathManagerAttribute",
                    static(node, token) => node is ClassDeclarationSyntax,
                    static(context, token) => context)
                .Where(static context => context.TargetSymbol is INamedTypeSymbol)
                .Select((context, token) => ExtractPathMappings(context));

            // マッピング情報を使ってコードを生成
            // 第1引数の型が第2引数のコールバックの第2引数の型になる
            context.RegisterSourceOutput(source, Emit);
        }

        /// <summary>
        /// `Route` 属性を持たせるコードを生成
        /// </summary>
        private static void Emit(SourceProductionContext context, ImmutableList<RazorPageContext> mappings)
        {
            var sb = new StringBuilder();
            sb.AppendLine("using Microsoft.AspNetCore.Components;");
            // sb.AppendLine("namespace CSStack.PrimeBlazor.GeneratedRoutes");
            sb.AppendLine("namespace CSStack.PrimeBlazor.PageManager.TestApp.Components.Pages");
            sb.AppendLine("{");

            foreach (var mapping in mappings)
            {
                sb.AppendLine(
                    $$"""
                        [Route("{{mapping.Path}}")]
                        public partial class {{mapping.PageName}}
                        {
                        }
                    """);
            }

            sb.AppendLine("}");
            var sourceText = SourceText.From(sb.ToString(), Encoding.UTF8);
            context.AddSource("BlazorGeneratedRoutes.g.cs", sourceText);
        }

        /// <summary>
        /// PagePathManagerクラスの情報を解析し、ページとパスのマッピング情報を取得
        /// </summary>
        private static ImmutableList<RazorPageContext> ExtractPathMappings(GeneratorAttributeSyntaxContext context)
        {
            var classSymbol = (INamedTypeSymbol)context.TargetSymbol;
            var mappings = ImmutableList.CreateBuilder<RazorPageContext>();

            foreach (var member in classSymbol.GetMembers())
            {
                // フィールドまたはプロパティで、BlazorPageAttribute<T> が付いているものを探す
                ITypeSymbol? memberType = null;
                string? path = null;
                INamedTypeSymbol? pageType = null;

                if (member is IFieldSymbol fieldSymbol && fieldSymbol.Type.SpecialType == SpecialType.System_String)
                {
                    memberType = fieldSymbol.Type;
                    path = fieldSymbol.ConstantValue as string;
                    pageType = GetBlazorPageType(fieldSymbol);
                }
                else if (member is IPropertySymbol propertySymbol
                    && propertySymbol.Type.SpecialType == SpecialType.System_String)
                {
                    memberType = propertySymbol.Type;
                    path = GetConstantValue(propertySymbol) as string;
                    pageType = GetBlazorPageType(propertySymbol);
                }

                if (path != null && pageType != null)
                {
                    mappings.Add(new RazorPageContext() { Path = path, PageName = pageType.Name });
                }
            }

            return mappings.ToImmutableList();
        }

        /// <summary>
        /// BlazorPageAttribute<T> の T を取得
        /// </summary>
        private static INamedTypeSymbol? GetBlazorPageType(ISymbol symbol)
        {
            foreach (var attribute in symbol.GetAttributes())
            {
                if (attribute.AttributeClass is { Name: "BlazorPageAttribute", TypeArguments.Length: 1 })
                {
                    return attribute.AttributeClass.TypeArguments[0] as INamedTypeSymbol;
                }
            }
            return null;
        }

        /// <summary>
        /// BlazorPageAttributeの付いた静的プロパティを取得
        /// </summary>
        /// <param name="propertySymbol"></param>
        /// <returns></returns>
        private static object? GetConstantValue(IPropertySymbol propertySymbol)
        {
            // TODO: ここで名前空間も確認した方がいいかも
            var constantValue = propertySymbol.GetAttributes()
                .FirstOrDefault(attr => attr.AttributeClass?.Name == "BlazorPageAttribute")
                ?.ConstructorArguments.FirstOrDefault().Value;

            return constantValue;
        }

        /// <summary>
        /// 使用する属性を登録
        /// </summary>
        /// <param name="context"></param>
        private void RegisterAttributes(IncrementalGeneratorInitializationContext context)
        {
            // これをやらないと、使う側で属性が見つからないと言われる
            context.RegisterPostInitializationOutput(
                static context =>
                {
                    context.AddSource(
                        "BlazorPathManagerAttribute.cs",
                        """
                        namespace CSStack.PrimeBlazor.PageManager
                        {
                            using System;

                            [AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
                            internal sealed class BlazorPathManagerAttribute : Attribute
                            {
                            }
                        }
                        """);

                    context.AddSource(
                        "BlazorPageAttribute.cs",
                        """
                        namespace CSStack.PrimeBlazor.PageManager
                        {
                            using System;

                            [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, Inherited = false, AllowMultiple = false)]
                            internal sealed class BlazorPageAttribute<T> : Attribute
                            {
                            }
                        }
                        """);
                });
        }
    }
}
