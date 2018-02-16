using Djohnnie.HomeAutomation.Management;
using Djohnnie.HomeAutomation.Utils;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using static System.Console;

namespace Djohnnie.HomeAutomation.Deamon
{
    class Program
    {
        private const Int32 DELAY_MS = 10000;

        static void Main(string[] args)
        {
            ServiceProvider serviceProvider = new ServiceCollection()
                .AddManagement().BuildServiceProvider();

            WriteLine("HOME AUTOMATION deamon v1");
            WriteLine("-------------------------");
            WriteLine();
            WriteLine(" -> Running...");
            WriteLine();
            WriteLine("Press any key to quit!");
            CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
            List<Task> tasks = new List<Task>
            {
                WatchLights(cancellationTokenSource.Token),
                WatchPlugs(cancellationTokenSource.Token),
                WatchPowerMeter(cancellationTokenSource.Token),
                WatchThermostat(cancellationTokenSource.Token)
            };
            ReadKey(intercept: true);
            WriteLine(" -> Quitting...");
            cancellationTokenSource.Cancel();
            Task.WaitAll(tasks.ToArray());
            WriteLine("Bye!");
        }

        static Task WatchLights(CancellationToken cancellationToken)
        {
            return Watch(cancellationToken, "HomeAutomation - Watch lights",
                async () => await Task.Delay(new Random().Next(100, 1000), cancellationToken));
        }

        static Task WatchPlugs(CancellationToken cancellationToken)
        {
            return Watch(cancellationToken, "HomeAutomation - Watch plugs",
                async () => await Task.Delay(new Random().Next(100, 1000), cancellationToken));
        }

        static Task WatchPowerMeter(CancellationToken cancellationToken)
        {
            return Watch(cancellationToken, "HomeAutomation - Watch power meter",
                async () => await Task.Delay(new Random().Next(100, 1000), cancellationToken));
        }

        static Task WatchThermostat(CancellationToken cancellationToken)
        {
            return Watch(cancellationToken, "HomeAutomation - Watch thermostat",
                async () => await Task.Delay(new Random().Next(100, 1000), cancellationToken));
        }

        static Task Watch(CancellationToken cancellationToken, String description, Func<Task> actionToExecute)
        {
            return Task.Run(async () =>
            {
                while (!cancellationToken.IsCancellationRequested)
                {
                    var start = DateTime.UtcNow;

                    try
                    {
                        using (var sw = new SimpleStopwatch())
                        {
                            await actionToExecute();
                            WriteLine($"[ {description} - {sw.ElapsedMilliseconds}ms! ]");
                        }
                    }
                    catch (Exception ex)
                    {
                        WriteLine($"[ {description} - EXCEPTION - '{ex.Message}'! ]");
                    }

                    var timeTaken = DateTime.UtcNow - start;
                    var delay = (Int32)(timeTaken.TotalMilliseconds < DELAY_MS ? DELAY_MS - timeTaken.TotalMilliseconds : 0);
                    await Task.Delay(delay, cancellationToken);
                }
            }, cancellationToken);
        }
    }
}