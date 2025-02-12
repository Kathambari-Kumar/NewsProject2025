using NewsProject.Models.DB;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.EntityFrameworkCore;
using NewsProject.Data;
using NewsProject.Services;
using Stripe;
using Azure.Data.Tables;

namespace NewsProject
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
            builder.Services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(connectionString));
            builder.Services.AddDatabaseDeveloperPageExceptionFilter();

            builder.Services.AddSession();
            builder.Services.AddDefaultIdentity<User>(options => options.SignIn.RequireConfirmedAccount = true)
                .AddRoles<IdentityRole>()
                .AddEntityFrameworkStores<ApplicationDbContext>();
           

            builder.Services.Configure<IdentityOptions>(options =>
            {
                options.SignIn.RequireConfirmedEmail = true; // Ensures email confirmation is required
                options.Password.RequireDigit = true;       // Example: Enforce strong password policies
                options.Lockout.MaxFailedAccessAttempts = 5; // Example: Lock account after 5 failed attempts
            });

            builder.Services.AddTransient<IEmailSender, EmailSender>();
            builder.Services.AddScoped<IAdminService, AdminService>();
            builder.Services.AddScoped<ISubscriptionService,UserSubscriptionService>();
            builder.Services.AddScoped<IQuizService, QuizService>();
            builder.Services.AddScoped<INewsArticleService, NewsArticleService>();
            builder.Services.AddScoped<IWeatherService,WeatherService>();
            builder.Services.AddScoped<IProductService, NotebookProductService>();
            builder.Services.AddScoped<IElectrictyService,ElectricityService>();
           


            

            builder.Services.AddControllersWithViews();

            //builder.Services.AddHttpClient("v4/weather/realtime", config =>

            builder.Services.AddHttpClient("forecast", config =>
            {
                config.BaseAddress = new(builder.Configuration["MyWeatherAPIAddress"]);
            });
            builder.Services.AddHttpClient("espot", config =>
            {
                config.BaseAddress = new(builder.Configuration["MyElectricityAddress"]);
            });
            

            builder.Services.AddHttpClient("v6/latest/USD", config =>
            {
                config.BaseAddress = new(builder.Configuration["MyExchangeRateAPIAddress"]);
            });

           
            StripeConfiguration.ApiKey = builder.Configuration.GetSection("Stripe:SecretKey").Get<string>();

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseMigrationsEndPoint();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseSession();
            app.UseRouting();

            app.UseAuthorization();

            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}");
            app.MapRazorPages();

            app.Run();
        }
    }
}
