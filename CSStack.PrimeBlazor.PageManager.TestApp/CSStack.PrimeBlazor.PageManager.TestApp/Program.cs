using CSStack.PrimeBlazor.PageManager.TestApp.Components;
using CSStack.PrimeBlazor.PageManager.TestApp.Models;

var builder = WebApplication.CreateBuilder(args);

var mc = new MyClass() { Hoge = 10, Bar = "tako" };
Console.WriteLine(mc);

// Add services to the container.
builder.Services.AddRazorComponents().AddInteractiveServerComponents().AddInteractiveWebAssemblyComponents();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseWebAssemblyDebugging();
}
else
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseStaticFiles();
app.UseAntiforgery();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode()
    .AddInteractiveWebAssemblyRenderMode()
    .AddAdditionalAssemblies(typeof(CSStack.PrimeBlazor.PageManager.TestApp.Client._Imports).Assembly);

app.Run();
