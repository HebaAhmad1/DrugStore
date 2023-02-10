using DrugStore.Data;
using DrugStoreData.Models;
using DrugStoreInfrastructure.Services.Authenticate;
using DrugStoreInfrastructure.Services.DisplayOrders;
using DrugStoreInfrastructure.Services.Drugs;
using DrugStoreInfrastructure.Services.Orders;
using DrugStoreInfrastructure.Services.Maps;
using MAFInfrastructure.AutoMapper;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DrugStoreInfrastructure.Extensions
{
    public static class ServicesInjection
    {
        public static void AddDrugStoreDependancy(this IServiceCollection services, IConfiguration Configuration)
        {
            services.AddDbContext<DrugStoreDbContext>(options =>
                options.UseSqlServer(Configuration.GetConnectionString("DefaultConnection")));
            services.AddIdentity<Pharmacy, IdentityRole>(options => options.SignIn.RequireConfirmedAccount = false)
                .AddEntityFrameworkStores<DrugStoreDbContext>()
                .AddDefaultUI().AddDefaultTokenProviders();//This Line To Go Screen Login When Not Authorize
            services.AddAutoMapper(typeof(AutomapperProfile).Assembly);

            //Change Identity Setting To Accept Any Password
            services.Configure<IdentityOptions>(opts => {
                opts.Password.RequiredLength = 3;
                opts.Password.RequireNonAlphanumeric = false;
                //opts.Password.RequiredUniqueChars = 0;
                opts.Password.RequireLowercase = false;
                opts.Password.RequireUppercase = false;
                opts.Password.RequireDigit = false;
            });
            //services.AddAutoMapper(typeof(Startup));//Anthor Way To Access AutoMapper Profile
            services.AddScoped<IOrderService, OrderService>();
            services.AddScoped<IDrugService, DrugService>();
            services.AddScoped<IAuthenticationService, AuthenticationService>();
            services.AddScoped<IDisplayOrdersService, DisplayOrdersService>();
            services.AddScoped<IMapsService, MapsService>();
        }
    }
}
