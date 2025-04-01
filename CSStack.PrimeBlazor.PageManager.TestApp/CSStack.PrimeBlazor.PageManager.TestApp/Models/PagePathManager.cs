using CSStack.PrimeBlazor.PageManager.TestApp.Components.Pages;

namespace CSStack.PrimeBlazor.PageManager.TestApp.Models
{
    [BlazorPathManager]
    public class PagePathManager
    {
        [BlazorPage<Home>]
        public const string Home = "/";
        [BlazorPage<TestPag>]
        public const string TestPag = "/test";
    }
}
