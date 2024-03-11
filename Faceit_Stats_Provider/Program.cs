using Faceit_Stats_Provider.Classes;
using Faceit_Stats_Provider.Models;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Net;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();
builder.Services.AddHttpClient();

builder.Services.AddHttpClient("Faceit", httpClient =>
{
    httpClient.BaseAddress = new Uri("https://open.faceit.com/data/");
    httpClient.DefaultRequestHeaders.Add("Authorization", builder.Configuration.GetValue<string>("FaceitAPI"));
});

builder.Services.AddHttpClient("FaceitV1", httpClient =>
{
    httpClient.BaseAddress = new Uri("https://api.faceit.com/stats/");
});

builder.Services.AddMemoryCache();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
