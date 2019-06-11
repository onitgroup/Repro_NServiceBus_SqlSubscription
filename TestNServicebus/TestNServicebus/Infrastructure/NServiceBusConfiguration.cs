using NServiceBus;
using NServiceBus.Logging;
using NServiceBus.Pipeline;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using NServiceBus.Persistence.Sql;
using System.Data.SqlClient;

using Microsoft.Extensions.DependencyInjection;
using NServiceBus.Features;
using System.Text.RegularExpressions;
using System.Text;
using System.Security.Cryptography;
using NServiceBus.Routing;
using NServiceBus.Transport;
using TestNServicebus.Infrastructure.Handlers;

namespace TestNServicebus.Infrastructure
{
    public class NServiceBusConfiguration
    {
        IEndpointInstance _endpoint;
        string endpointName = "testnservicebus";
        string connectionString = "Server=NBK2015004\\SQLEXPRESS01;Database=NServiceBus;Trusted_Connection=True;MultipleActiveResultSets=true";

        public NServiceBusConfiguration()
        {

        }

        public void Configure(IServiceCollection services)
        {

            var configuration = new EndpointConfiguration(endpointName);
            configuration.SendFailedMessagesTo(endpointName + ".error");

            var asms = new[]
            {
                    typeof(InseritaFirmaBolla).Assembly,
                    typeof(NServiceBus.SqlServerTransport).Assembly, GetType().Assembly,
                    typeof(EndpointConfiguration).Assembly, GetType().Assembly,
                };

            string path = System.AppDomain.CurrentDomain.BaseDirectory;
            var allFiles = Directory.EnumerateFiles(path, "*.dll")
                .Select(Path.GetFileNameWithoutExtension).Distinct().ToArray();
            var assemblyToExclude = allFiles
                .Except(asms.Select(s => Path.GetFileNameWithoutExtension(s.Location)))
                .ToArray();

            configuration.AssemblyScanner().ExcludeAssemblies(assemblyToExclude);

            configuration.Conventions().DefiningEventsAs(t => typeof(Handlers.IEvent).IsAssignableFrom(t));

            configuration.DisableFeature<NServiceBus.Features.DataBus>();
            configuration.DisableFeature<NServiceBus.Features.TimeoutManager>();

            // https://docs.particular.net/persistence/sql/dialect-mssql

            var persistence = configuration.UsePersistence<SqlPersistence>();
            var subscriptions = persistence.SubscriptionSettings();
            subscriptions.CacheFor(TimeSpan.FromMinutes(1));
            persistence.SqlDialect<SqlDialect.MsSqlServer>();
            persistence.ConnectionBuilder(connectionBuilder: () =>
            {
                return new SqlConnection(connectionString);
            });


            // https://docs.particular.net/transports/sql/

            var transportSql = configuration.UseTransport<SqlServerTransport>();
            transportSql.ConnectionString(connectionString);

            var routingSql = transportSql.Routing();

            RegistraPublisher(routingSql);

            configuration.EnableInstallers();

#if DEBUG
            var defaultFactory = LogManager.Use<NServiceBus.Logging.DefaultFactory>();
            defaultFactory.Level(NServiceBus.Logging.LogLevel.Debug);
#endif

            var endpoint = Endpoint.Start(configuration).GetAwaiter().GetResult();

            endpoint.Subscribe<InseritaFirmaBolla>().GetAwaiter().GetResult();
            _endpoint = endpoint;

        }

        private void RegistraPublisher<T>(RoutingSettings<T> routingSettings) where T : TransportDefinition, IMessageDrivenSubscriptionTransport
        {
            routingSettings.RegisterPublisher(typeof(InseritaFirmaBolla).Assembly, endpointName);
        }
    }
}
