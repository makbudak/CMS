using CMS.Data.Context;
using CMS.Data.Repository;
using CMS.Model.Model;
using CMS.Service;
using CMS.Service.Helper;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Newtonsoft.Json;
using System;
using System.Linq;

namespace CMS.Api
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers();
            services.AddDbContext<CMSContext>(options => options.UseSqlite("FileName=database.db"));

            services.TryAddSingleton<IHttpContextAccessor, HttpContextAccessor>();

            services.AddScoped(typeof(IUnitOfWork<>), typeof(UnitOfWork<>));
            services.AddScoped(typeof(IRepository<>), typeof(Repository<>));

            services.AddScoped<IUserService, UserService>();
            services.AddScoped<IJwtHelper, JwtHelper>();
            services.AddScoped<IAccessRightService, AccessRightService>();
            services.AddScoped<IContactCategoryService, ContactCategoryService>();
            services.AddScoped<IContactService, ContactService>();
            services.AddScoped<IChatService, ChatService>();
            services.AddScoped<IChatMessageService, ChatMessageService>();
            services.AddScoped<IUserAccessRightService, UserAccessRightService>();
            services.AddScoped<IAccessRightService, AccessRightService>();
            services.AddScoped<IPageService, PageService>();
            services.AddScoped<IBlogCategoryService, BlogCategoryService>();
            services.AddScoped<IBlogService, BlogService>();
            services.AddScoped<ITodoCategoryService, TodoCategoryService>();
            services.AddScoped<ITodoStatusService, TodoStatusService>();
            services.AddScoped<ITodoService, TodoService>();
            services.AddScoped<IMenuService, MenuService>();


            services.ConfigureApplicationCookie(s =>
            {
                s.LoginPath = new PathString("/login");
                s.Cookie = new CookieBuilder
                {
                    Name = "cms", //Olu�turulacak Cookie'yi isimlendiriyoruz.
                    HttpOnly = false, //K�t� niyetli insanlar�n client-side taraf�ndan Cookie'ye eri�mesini engelliyoruz.
                    Expiration = TimeSpan.FromMinutes(2), //Olu�turulacak Cookie'nin vadesini belirliyoruz.
                    SameSite = SameSiteMode.Lax, //Top level navigasyonlara sebep olmayan requestlere Cookie'nin g�nderilmemesini belirtiyoruz.
                    SecurePolicy = CookieSecurePolicy.Always //HTTPS �zerinden eri�ilebilir yap�yoruz.                    
                };
                s.SlidingExpiration = true; //Expiration s�resinin yar�s� kadar s�re zarf�nda istekte bulunulursa e�er geri kalan yar�s�n� tekrar s�f�rlayarak ilk ayarlanan s�reyi tazeleyecektir.
                s.ExpireTimeSpan = TimeSpan.FromMinutes(2); //CookieBuilder nesnesinde tan�mlanan Expiration de�erinin varsay�lan de�erlerle ezilme ihtimaline kar��n tekrardan Cookie vadesi burada da belirtiliyor.
            });

            services.AddSession();
            services.AddSignalR();
            services.AddCors();

            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Latest)
                 .AddNewtonsoftJson(opt => opt.SerializerSettings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore)
                .ConfigureApiBehaviorOptions(options =>
                {
                    options.InvalidModelStateResponseFactory = (context) =>
                    {
                        var errors = context.ModelState.Values
                        .SelectMany(x => x.Errors
                        .Select(p => p.ErrorMessage)).ToList();

                        return new BadRequestObjectResult(
                            new ServiceResult
                            {
                                Message = errors.First()
                            });
                    };
                }).AddJsonOptions(options =>
                {
                    options.JsonSerializerOptions.PropertyNameCaseInsensitive = true;
                });

            services.AddMemoryCache();

            services.AddSwaggerGen();

        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseRouting();

            app.UseAuthorization();

            app.UseCors(builder => builder
            .AllowAnyOrigin()
            .AllowAnyMethod()
            .AllowAnyHeader());

            Global.Initialize(Configuration);

            app.UseEndpoints(endpoints => endpoints.MapControllers());

            app.UseSwagger();

            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "My API V1");
            });
        }
    }
}
