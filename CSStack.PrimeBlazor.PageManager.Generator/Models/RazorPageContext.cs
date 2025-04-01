namespace CSStack.PrimeBlazor.PageManager.Generator
{
    internal sealed record RazorPageContext
    {
        public string PageName { get; set; } = string.Empty;

        public string Path { get; set; } = string.Empty;
    }
}
