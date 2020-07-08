using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using WellEquipment.Models;

namespace WellEquipment
{

    public class Program
    {
        public static readonly bool Change_Base = false;
        public static void Main(string[] args)
        {
            var host = CreateHostBuilder(args).Build();
            using (var scope = host.Services.CreateScope())
            {
                var services = scope.ServiceProvider;
                SaveEquip.CreateValuesList();

                //List<Equipment> Equipments = new List<Equipment>();
                //Equipments.Add(new Equipment());
                //Equipments.Add(new Equipment());
                //SaveEquip.SaveEquipments(Equipments, "Data2.txt");
                SaveEquip.ReadAccounts(SaveEquip.AccountsFile);
                SaveEquip.ReadEquipments(SaveEquip.OldData);
                if (Program.Change_Base)
                {
                    //ÄËß ÏÅÐÅÑÎÕÐÀÍÅÍÈß ÁÀÇÛ
                    SaveEquip.SaveEquipments();
                }
                Filters.StartFilters = new Filters();
            }
            host.Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                });
    }
}
