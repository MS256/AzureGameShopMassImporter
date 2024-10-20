using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Server.HttpSys;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
}
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapRazorPages();

app.MapPost("/import", (HttpRequest request) =>
{
    if(request.Form.Files.Count != 1)
    {
        return Results.BadRequest();
    }
    // TODO : Put file into Azure Storage
    return Results.Redirect("/success");
}).Accepts<IFormFile>("multipart/form-data")
.Produces(200);

app.Run();
