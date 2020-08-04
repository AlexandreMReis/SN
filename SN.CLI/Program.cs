using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using NLog.Extensions.Logging;
using SN.DAL.ADO;
using SN.DAL.Interfaces;
using SN.Logger;
using System;
using System.IO;

namespace SN.CLI
{
    class Program
    {
        public static IConfigurationRoot _configuration;

        static void Main()
        {
            // Create service collection
            LogEngine.DILogger.WriteToLog(LogLevels.Debug, "Creating service collection");
            ServiceCollection serviceCollection = new ServiceCollection();
            ConfigureServices(serviceCollection);


            // Create service provider
            LogEngine.DILogger.WriteToLog(LogLevels.Debug, "Building service provider");
            IServiceProvider serviceProvider = serviceCollection.BuildServiceProvider();

            //Config Nlog
            NLog.LogManager.Configuration = new NLogLoggingConfiguration(_configuration.GetSection("NLog"));

            LogEngine.DILogger.WriteToLog(LogLevels.Debug, $"ConnectionString={(_configuration.GetConnectionString("DbConnection"))}");

            try
            {
                LogEngine.DILogger.WriteToLog(LogLevels.Debug, "Starting service");
                serviceProvider.GetService<ISocialNetwork>().StartAsync().Wait();
                LogEngine.DILogger.WriteToLog(LogLevels.Debug, "Ending service");
            }
            catch (Exception ex)
            {
                LogEngine.Logger.WriteToLog(LogLevels.Debug, JsonConvert.SerializeObject(ex));
            }

            return;
        }

        private static void ConfigureServices(IServiceCollection services)
        {
            _configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetParent(AppContext.BaseDirectory).FullName)
                .AddJsonFile("appsettings.json", false)
                .Build();

            services.AddSingleton(_configuration);
            services.AddTransient<IDbContext, DbContext>();
            services.AddTransient<IMembersRepository, MembersRepository>();
            services.AddTransient<IBooksRepository, BooksRepository>();
            services.AddTransient<ISocialNetwork, SocialNetwork>();
        }
    }
}
