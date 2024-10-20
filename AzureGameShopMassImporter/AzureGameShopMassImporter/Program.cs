using Azure.Storage;
using Azure.Storage.Files.Shares;
using Azure.Storage.Files.Shares.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding.Binders;
using Microsoft.AspNetCore.Server.HttpSys;
using Microsoft.Extensions.Azure;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.AddAzureClients(clientBuilder =>
{
    clientBuilder.AddBlobServiceClient(builder.Configuration["AzureGameShopStorage:blob"]!, preferMsi: true);
    clientBuilder.AddQueueServiceClient(builder.Configuration["AzureGameShopStorage:queue"]!, preferMsi: true);
});

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

app.MapPost("/import", async (HttpRequest request) =>
{
    if(request.Form.Files.Count != 1)
    {
        return Results.BadRequest();
    }

    var fileToStore = request.Form.Files[0];

    var storageConnectionString = builder.Configuration["ConnectionStrings:AzureGameShopStorage"];
    if (storageConnectionString == null)
    {
        // TODO : Log error about proper setup
        throw new Exception("This service is not properly configured.");
    }

    using var uploadFileStream = fileToStore.OpenReadStream();

    var storageClient = new ShareClient(storageConnectionString, "importer-csvs");

    var storageDirectory = storageClient.GetDirectoryClient("CSVS");
    storageDirectory.CreateIfNotExists();

    var storageFile = storageDirectory.GetFileClient(fileToStore.FileName);
    storageFile.DeleteIfExists();
    storageFile.Create(fileToStore.Length);
    storageFile.Upload(uploadFileStream);
/*    using var storageStream = storageFile.OpenWrite(true, 0, new ShareFileOpenWriteOptions { MaxSize = 100000 });
    byte[] buffer = new byte[1024];
    int bytesRead = uploadFileStream.Read(buffer, 0, buffer.Length);
    while (bytesRead > 0)
    {
        storageStream.Write(buffer, 0, bytesRead);
        bytesRead = uploadFileStream.Read(buffer, 0, buffer.Length);
    }
    storageFile.
*/
    return Results.Redirect("/success");
}).Accepts<IFormFile>("multipart/form-data")
.Produces(200);

app.Run();
