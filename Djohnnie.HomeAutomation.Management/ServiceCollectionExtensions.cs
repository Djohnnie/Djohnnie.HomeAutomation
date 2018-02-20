using Djohnnie.HomeAutomation.DataAccess.Lifx;
using Djohnnie.HomeAutomation.DataAccess.Nest;
using Djohnnie.HomeAutomation.DataAccess.Smappee;
using Djohnnie.HomeAutomation.DataAccess.Sql;
using Microsoft.Extensions.DependencyInjection;

namespace Djohnnie.HomeAutomation.Management
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddManagement(this IServiceCollection serviceCollection)
        {
            serviceCollection.AddDataAccessSql();
            serviceCollection.AddDataAccessLifx();
            serviceCollection.AddDataAccessSmappee();
            serviceCollection.AddDataAccessNest();
            return serviceCollection;
        }
    }
}