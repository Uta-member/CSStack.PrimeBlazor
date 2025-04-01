using Microsoft.CodeAnalysis;

namespace CSStack.PrimeBlazor.PageManager
{
    internal sealed class DiagnosticDescriptors
    {
        public static readonly DiagnosticDescriptor ExistsOverrideToString = new(
            id: "CSTPBPM:0001",
            title: "ToString override",
            messageFormat: "The GenerateToString class '{0}' has ToString override but it is not allowed",
            category: Category,
            defaultSeverity: DiagnosticSeverity.Error,
            isEnabledByDefault: true);
        private const string Category = "CSStack.PrimeBlazor.PageManager";
    }
}
