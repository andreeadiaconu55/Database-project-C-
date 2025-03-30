var builder = WebApplication.CreateBuilder(args);

// Configure distributed memory cache și sesiunea
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30); // Timpul de expirare al sesiunii
    options.Cookie.HttpOnly = true; // Crește securitatea
    options.Cookie.IsEssential = true; // Necesită cookie-uri pentru funcționare
});

// Adaugă serviciile pentru Razor Pages
builder.Services.AddRazorPages();

var app = builder.Build();

// Configure the HTTP request pipeline
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseSession(); // Asigură-te că este apelat înainte de MapRazorPages

app.UseAuthorization();

app.MapRazorPages();

app.Run();
