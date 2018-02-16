using Microsoft.Extensions.DependencyInjection;

namespace Djohnnie.HomeAutomation.DataAccess.Smappee
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddDataAccessSmappee(this IServiceCollection serviceCollection)
        {
            serviceCollection.AddSingleton<ISmappeeRepository, SmappeeRepository>();
            return serviceCollection;
        }
    }
}