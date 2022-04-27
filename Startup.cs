using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Logging;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Newtonsoft.Json.Serialization;
using PhotoGalleryAPI.Data;
using PhotoGalleryAPI.Services;
using System;
using System.Runtime.InteropServices;

namespace PhotoGalleryAPI
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }
        public object OpenIdConnectDefaults { get; private set; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            IdentityModelEventSource.ShowPII = true;

            services.AddControllers()
                .AddNewtonsoftJson(o=>o.SerializerSettings.ContractResolver =  new CamelCasePropertyNamesContractResolver());
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "PhotoGalleryAPI", Version = "v1" });
            });

            services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());

            services.AddDbContext<ApplicationDbContext>(options =>
            {
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                {
                    options.UseMySql(Configuration.GetValue<string>("Data:ConnectionStrings:WindowsMariaDB"));
                    //options.UseMySql(Configuration.GetValue<string>("Data:ConnectionStrings:LinuxMariaDB")); 
                }
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                {
                    options.UseMySql(Configuration.GetValue<string>("Data:ConnectionStrings:LinuxMariaDBDocker"));
                }
            });

            services.AddTransient<IRepository, DbRepository>();
            services.AddTransient<IRepositoryAdmin, DbRepository>();
            services.AddTransient<EmailSender>();
           

            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer(JwtBearerDefaults.AuthenticationScheme, options =>
            {
#if DEBUG
                options.MetadataAddress = "https://localhost:4001/.well-known/openid-configuration";
                options.Authority = "https://localhost:4001";
                
#else           

                options.MetadataAddress = "https://accounts.PhotoGallery.photography/.well-known/openid-configuration";
                options.Authority = "https://account.PhotoGallery.photography";
#endif

                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateAudience = false,
                    //SaveSigninToken = true,
                   
                    RoleClaimType = "role",
                    NameClaimType = "name",
                 };
            
             });
            services.AddCors(options =>
            {
                options.AddPolicy("Policy",
                      builder =>
                      {
                          builder
                                 .WithOrigins("https://localhost:5001")

                                 .AllowAnyHeader().
                                  AllowAnyMethod().
                                  AllowCredentials();

                          //builder.AllowAnyMethod()
                          //       .AllowAnyHeader()
                          //      .SetIsOriginAllowed(origin => true) // allow any origin
                          //      .AllowCredentials();

                      });
            });
            services.Configure<ForwardedHeadersOptions>(options =>
            {
                options.ForwardedHeaders =
                    ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
            });

        }

        //update DB if new migrations are created
        private static void UpdateDatabase(IApplicationBuilder app)
        {
            using var serviceScope = app.ApplicationServices
                .GetRequiredService<IServiceScopeFactory>()
                .CreateScope();
            using var context = serviceScope.ServiceProvider.GetService<ApplicationDbContext>();
            context.Database.Migrate();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "PhotoGalleryAPI v1"));
            }

            UpdateDatabase(app);

           
            app.UseForwardedHeaders();

            app.UseCors("Policy");

            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseRouting();

            //middleware for reverse proxing with NGINX
           
            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
                
            });
        }
    }
}
