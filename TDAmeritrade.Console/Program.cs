using System;
using System.Threading.Tasks;
using TDAmeritrade;

namespace TDConsole
{

    class Program
    {
        static async Task Main(string[] args)
        {
            Console.WriteLine("Starting Client");
            Console.WriteLine("...");

            //var serviceCollection = new ServiceCollection();
            //serviceCollection.AddDataProtection();
            //var services = serviceCollection.BuildServiceProvider();
            //var protection = services.GetService<IDataProtectionProvider>();
            //var cache = new TDProtectedCache(protection);
            //var client = new TDAmeritradeClient(cache);

            var cache = new TDUnprotectedCache();
            var client = new TDAmeritradeClient(cache);

            Console.WriteLine("1 to sign in fresh, 2 to refresh signin");
            var option = Console.ReadKey();
            switch (option.Key)
            {
                case ConsoleKey.D1:
                    Console.WriteLine("Paste consumer key :");
                    var key = Console.ReadLine();
                    Console.WriteLine("Opening Browser. Please sign in.");
                    client.RequestAccessToken(key);

                    Console.WriteLine("Paste the code. This is in the browser url bar 'code={code}'.");
                    var code = Console.ReadLine();
                    await client.PostAccessToken(key, code);
                    Console.WriteLine($"Authenticated {client.IsSignedIn}.");
                    Console.ReadLine();
                    break;
                case ConsoleKey.D2:
                    await client.PostRefreshToken();
                    Console.WriteLine($"Authenticated {client.IsSignedIn}.");
                    Console.ReadLine();
                    break;
                default:
                    return;
            }
        }
    }
}
