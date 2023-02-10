using DrugStore.Data;
using DrugStoreData.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HiringWeb.DataBase
{
    public static class DBSeeder
    {

        public static IHost SeedDb(this IHost webHost)
        {
            using var scope = webHost.Services.CreateScope();
            try
            {
                var context = scope.ServiceProvider.GetRequiredService<DrugStoreDbContext>();
                try
                {
                    context.Database.Migrate();
                }
                catch
                {
                    // ignore
                }

                //Add Roles If Not Found Any Roles In DB
                var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
                roleManager.SeedRoles().Wait();

                //Add Admin User If Not Found Any Usres In DB
                var userManager = scope.ServiceProvider.GetRequiredService<UserManager<Pharmacy>>();
                userManager.SeedAdmin().Wait();

                //Add Suppliers If Not Found Any Supplier In DB
                context.SeedSupplier().Wait();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                throw;
            }

            return webHost;
        }

        //Add Roles If Not Found Any Roles In DB
        public static async Task SeedRoles(this RoleManager<IdentityRole> roleManager)
        {
            //If Found Any Roles Break And Not Add Any Roles
            if (await roleManager.Roles.AnyAsync()) return;
            var roleNames = new List<string>();
            roleNames.Add("Pharmacy");
            roleNames.Add("Admin");

            foreach (var role in roleNames)
            {
                await roleManager.CreateAsync(new IdentityRole() { Name = role });
            }
        }

        //Add Admin User If Not Found Any Usres In DB
        public static async Task SeedAdmin(this UserManager<Pharmacy> userManager)
        {
            //If Found Any User Break And Not Add Any Users
            if (await userManager.Users.AnyAsync()) return;

            var pharmacy = new Pharmacy();
            pharmacy.UserName = "Admin@Admin.com";
            pharmacy.Email = "Admin@Admin.com";
            pharmacy.PharmacyName = "Admin";
            pharmacy.PharmacyRole = "Admin";

            await userManager.CreateAsync(pharmacy, "Aa123##");
            await userManager.AddToRoleAsync(pharmacy, pharmacy.PharmacyRole);
        }

        //Add Suppliers If Not Found Any Supplier In DB
        public static async Task SeedSupplier(this DrugStoreDbContext supplier)
        {
            //If Found Any Suppliers Break And Not Add Any Supplier
            if (await supplier.Suppliers.AnyAsync()) return;

            var supplierDb = new List<Supplier>();
            supplierDb.Add(new Supplier { SupplierId = 4112, Name = "عاشور أبو عصر" });
            supplierDb.Add(new Supplier { SupplierId = 4113, Name = "ابراهيم لولو أبو السعيد" });
            supplierDb.Add(new Supplier { SupplierId = 4114, Name = "Mahmoud Othman" });


            supplier.Suppliers.AddRange(supplierDb);
            await supplier.SaveChangesAsync();
        }

    }
}
