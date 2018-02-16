using Microsoft.Extensions.DependencyInjection;

namespace Djohnnie.HomeAutomation.DataAccess.Lifx
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddDataAccessLifx(this IServiceCollection serviceCollection)
        {
            serviceCollection.AddSingleton<ILifxRepository, LifxRepository>();
            return serviceCollection;
        }
    }
}