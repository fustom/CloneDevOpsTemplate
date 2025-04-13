using CloneDevOpsTemplate;
using CloneDevOpsTemplate.IServices;
using CloneDevOpsTemplate.Managers;
using CloneDevOpsTemplate.MessageHandlers;
using CloneDevOpsTemplate.Middlewares;
using CloneDevOpsTemplate.Services;
using Microsoft.AspNetCore.Authorization;

var builder = WebApplication.CreateBuilder(args);

var apiConfigurationSection = builder.Configuration.GetSection(nameof(ApiSettings));
builder.Services.Configure<ApiSettings>(apiConfigurationSection);

// Add services to the container.
builder.Services.AddMvc();
builder.Services.AddHttpContextAccessor();
builder.Services.AddTransient<DevOpsAuthorizationHandler>();
builder.Services.AddHttpClient("DevOpsServer", client =>
{
    var baseAddress = apiConfigurationSection.GetValue<string>(nameof(ApiSettings.ServiceRootUrl));
    if (string.IsNullOrEmpty(baseAddress))
    {
        throw new ArgumentException("ServiceRootUrl is not configured.");
    }
    client.BaseAddress = new Uri(baseAddress);
}).AddHttpMessageHandler<DevOpsAuthorizationHandler>();
builder.Services.AddScoped<IProjectService, ProjectService>();
builder.Services.AddScoped<IIterationService, IterationService>();
builder.Services.AddScoped<ITeamsService, TeamsService>();
builder.Services.AddScoped<IBoardService, BoardService>();
builder.Services.AddScoped<ITeamSettingsService, TeamSettingsService>();
builder.Services.AddScoped<IRepositoryService, RepositoryService>();
builder.Services.AddScoped<IServiceService, ServiceService>();
builder.Services.AddScoped<IWorkItemService, WorkItemService>();
builder.Services.AddScoped<ITestService, TestService>();
builder.Services.AddScoped<ICloneManager, CloneManager>();
builder.Services.AddSingleton<IAuthorizationMiddlewareResultHandler, DevOpsAuthorizationMiddleware>();


builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(10);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});
builder.Services.AddAuthorization();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.MapStaticAssets();

app.UseRouting();

app.UseSession();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

await app.RunAsync();
