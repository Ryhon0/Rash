using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Rash.Data;
using Rash;
using SharpItch;
using System.IO;
using System.Text.Json;


/*
var okr = await RashClient.Itch.GetOwnedKeys();
var key = okr.OwnedKeys[1];

var ups = await RashClient.Itch.ListUploads(key.GameID, key.ID);
foreach(var u in ups.Uploads)
{
	Console.WriteLine(u.DisplayName);
	
	var sa = await RashClient.Itch.GetUploadScannedArchive(u.ID, key.ID);
	foreach(var s in sa.ScannedArchive.LaunchTargets)
	{
		Console.WriteLine(s);
	}
	Console.WriteLine(sa.ScannedArchive.ExctractedSize);
	Console.WriteLine(sa.ScannedArchive.Manifest);
	
	var dlsr = await RashClient.Itch.NewDownloadSession(key.GameID, key.ID);
	var uuid = dlsr.UUID;

	Console.WriteLine(RashClient.Itch.CreateDownloadURL(u.ID, uuid, key.ID));
}
return;
*/
RashClient.Config = Config.Load();
RashClient.Config.Setup();

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();
builder.Services.AddSingleton<WeatherForecastService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseStaticFiles();

app.UseRouting();

app.MapBlazorHub();
app.MapFallbackToPage("/_Host");

app.Run();
