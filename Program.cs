using kv_be_csharp_dotnet_dataapi_collections.Repositories;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

// register repository and services as singletons
builder.Services.AddSingleton<ICassandraConnection, CassandraConnection>();
builder.Services.AddSingleton<IVideoDAL, VideoDAL>();
builder.Services.AddSingleton<ILatestVideosDAL, LatestVideosDAL>();
builder.Services.AddSingleton<ICommentDAL, CommentDAL>();

// Register HttpClient for Astra DB service
//builder.Services.AddHttpClient<IAstraHelperService, AstraHelperService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();
app.UseCors("AllowAll");
app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

app.Run();
