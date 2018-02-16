using Microsoft.Extensions.DependencyInjection;

namespace Djohnnie.HomeAutomation.DataAccess.Sql
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddDataAccessSql(this IServiceCollection serviceCollection)
        {
            serviceCollection.AddSingleton<ILightRepository, LightRepository>();
            return serviceCollection;
        }
    }
}