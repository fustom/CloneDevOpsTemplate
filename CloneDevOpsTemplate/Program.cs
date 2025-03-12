using CloneDevOpsTemplate.Middlewares;
using CloneDevOpsTemplate.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddMvc();
builder.Services.AddHttpContextAccessor();
builder.Services.AddTransient<DevOpsAuthorizationHandler>();
builder.Services.AddHttpClient("DevOpsServer", client => {
    client.BaseAddress = new Uri(Const.ServiceRootUrl);
}).AddHttpMessageHandler<DevOpsAuthorizationHandler>();
builder.Services.AddScoped<IProjectService, ProjectService>();
builder.Services.AddScoped<IIterationService, IterationService>();
builder.Services.AddScoped<ITeamsService, TeamsService>();

builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(10);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.UseSession();

app.UseMiddleware<DevOpsAuthorizationMiddleware>();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
