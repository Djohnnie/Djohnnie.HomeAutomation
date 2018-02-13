using Microsoft.Extensions.DependencyInjection;

namespace Djohnnie.HomeAutomation.DataAccess.Lifx
{
    public static class ServiceCollectionExtensions
    {
        public static void AddDataAccessLifx(this IServiceCollection serviceCollection)
        {
            serviceCollection.AddSingleton<ILightingRepository, LightingRepository>();
        }
    }
}