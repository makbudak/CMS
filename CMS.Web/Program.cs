using CMS.Data.Context;
using CMS.Data.Repository;
using CMS.Storage.Model;
using CMS.Service;
using CMS.Service.Helper;
using CMS.Service.Infrastructure;
using CMS.Service.Middlewares;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Newtonsoft.Json;
using System.Globalization;
using System.Net;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorPages().AddRazorRuntimeCompilation();

builder.Services.AddControllers();

builder.Services.AddDbContext<CMSContext>(options =>
       options.UseSqlServer(builder.Configuration.GetConnectionString("AppConnectionString"), options => options.EnableRetryOnFailure()));

builder.Services.ConfigureApplicationCookie(s =>
{
    s.LoginPath = new PathString("/login");
    s.Cookie = new CookieBuilder
    {
        Name = "cms",
        HttpOnly = false,
        Expiration = TimeSpan.FromMinutes(2),
        SameSite = SameSiteMode.Lax,
        SecurePolicy = CookieSecurePolicy.Always
    };
    s.SlidingExpiration = true;
    s.ExpireTimeSpan = TimeSpan.FromMinutes(2);
});

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme).AddCookie();
builder.Services.TryAddSingleton<IHttpContextAccessor, HttpContextAccessor>();
builder.Services.AddScoped(typeof(IUnitOfWork<>), typeof(UnitOfWork<>));
builder.Services.AddScoped(typeof(IRepository<>), typeof(Repository<>));

builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IJwtHelper, JwtHelper>();
builder.Services.AddScoped<IAccessRightService, AccessRightService>();
builder.Services.AddScoped<IContactCategoryService, ContactCategoryService>();
builder.Services.AddScoped<IContactService, ContactService>();
builder.Services.AddScoped<IChatService, ChatService>();
builder.Services.AddScoped<IChatMessageService, ChatMessageService>();
builder.Services.AddScoped<IUserAccessRightService, UserAccessRightService>();
builder.Services.AddScoped<IAccessRightService, AccessRightService>();
builder.Services.AddScoped<IPageService, PageService>();
builder.Services.AddScoped<IBlogCategoryService, BlogCategoryService>();
builder.Services.AddScoped<IBlogService, BlogService>();
builder.Services.AddScoped<ITodoCategoryService, TodoCategoryService>();
builder.Services.AddScoped<ITodoStatusService, TodoStatusService>();
builder.Services.AddScoped<ITodoService, TodoService>();
builder.Services.AddScoped<IMenuService, MenuService>();
builder.Services.AddScoped<IAuthorService, AuthorService>();
builder.Services.AddScoped<IMailHelper, MailHelper>();
builder.Services.AddScoped<IWebsiteParameterService, WebsiteParameterService>();
builder.Services.AddScoped<IMailTemplateService, MailTemplateService>();
builder.Services.AddScoped<ICommentService, CommentService>();
builder.Services.AddScoped<ILookupService, LookupService>();
builder.Services.AddScoped<ITeamService, TeamService>();
builder.Services.AddScoped<ITestimonialService, TestimonialService>();
builder.Services.AddScoped<IService_Service, Service_Service>();
builder.Services.AddScoped<IClientService, ClientService>();
builder.Services.AddScoped<IHomepageSliderService, HomepageSliderService>();
builder.Services.AddScoped<ITagService, TagService>();

builder.Services.AddMvc().AddNewtonsoftJson(opt => opt.SerializerSettings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore)
    .ConfigureApiBehaviorOptions(options =>
    {
        options.InvalidModelStateResponseFactory = (context) =>
        {
            var errors = context.ModelState.Values
                                .SelectMany(x => x.Errors
                                .Select(p => p.ErrorMessage))
                                .ToList();

            return new BadRequestObjectResult(
                new BaseResult
                {
                    Message = errors.First(),
                    StatusCode = HttpStatusCode.BadRequest
                });
        };
    }).AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNameCaseInsensitive = true;
    });

builder.Services.AddControllersWithViews().AddViewLocalization();

builder.Services.AddLocalization(options =>
{
    options.ResourcesPath = "Resources";
});

builder.Services.Configure<RequestLocalizationOptions>(options =>
{
    options.DefaultRequestCulture = new("tr-TR");

    CultureInfo[] cultures = new CultureInfo[]
    {
        new("tr-TR"),
        new("en-US"),
        new("fr-FR")
    };

    options.SupportedCultures = cultures;
    options.SupportedUICultures = cultures;
});

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
}
app.UseStaticFiles();

app.UseRouting();

app.ErrorHandler();

app.UseAuthentication();
app.UseAuthorization();

app.UseRequestLocalization();

Global.Initialize(builder.Configuration);

app.UseEndpoints(endpoints =>
{
    endpoints.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

    endpoints.MapControllerRoute(
      name: "areas",
      pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}"
    );
});

app.Run();
