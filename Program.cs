using System.Security.Claims;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

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


app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.UseMiddleware<ClaimsMiddleware>();
app.Run();

public class ClaimsMiddleware
{
    private readonly RequestDelegate _next;

    public ClaimsMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        context.User.AddIdentity(new CustomClaimsIdentity());
       
        await _next(context);
    }
}

public class CustomClaimsIdentity : ClaimsIdentity
{
    private List<Claim> _claims;

    public CustomClaimsIdentity()
    {
        _claims = new List<Claim>();
        _claims.Add(new Claim("userx", "xxxxxxxx"));

        // Toggle
        var issueReplicationEnabled = true;

        if (issueReplicationEnabled)
        {
            // Fails anti forger token validation - no single user claim pulled in DefaultClaimUidExtractor
            // so all claims are used when generating token, but these can change between requests expecially iat
            _claims.Add(new Claim("iat", DateTime.UtcNow.Ticks.ToString()));
        }
        else 
        {
            // Works, supported claim 'sub' is used when generating token so iat is not at play
            _claims.Add(new Claim("sub", "xxxxxxxx"));
            _claims.Add(new Claim("iat", DateTime.UtcNow.Ticks.ToString()));
        }
    }

    public override bool IsAuthenticated => true;

    public override IEnumerable<Claim> Claims => _claims;

    public override string? Name => "Bob";
}