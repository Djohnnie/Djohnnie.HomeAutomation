using Microsoft.Extensions.DependencyInjection;

namespace Djohnnie.HomeAutomation.DataAccess.Nest
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddDataAccessNest(this IServiceCollection serviceCollection)
        {
            serviceCollection.AddSingleton<INestRepository, NestRepository>();
            return serviceCollection;
        }
    }
}