using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading.Tasks;
using TDAmeritrade;

namespace TDConsole
{
    class Program
    {
        static TDUnprotectedCache cache;
        static TDAmeritradeClient client;

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

            cache = new TDUnprotectedCache();
            client = new TDAmeritradeClient(cache);

            Console.WriteLine("Choose a flow :");
            Console.WriteLine("1) Fresh SignIn.");
            Console.WriteLine("2) Refresh SignIn.");
            Console.WriteLine("3) Record Stream");

            var option = Console.ReadKey();
            Console.WriteLine("-");
            switch (option.Key)
            {
                case ConsoleKey.NumPad1:
                case ConsoleKey.D1:
                    await RegistrationFlow();
                    break;
                case ConsoleKey.NumPad2:
                case ConsoleKey.D2:
                    await SignInFlow();
                    break;
                case ConsoleKey.NumPad3:
                case ConsoleKey.D3:
                    await RecordFlow();
                    break;
                default:
                    return;
            }
        }

        static async Task RegistrationFlow()
        {
            Console.WriteLine("Paste consumer key :");
            var key = Console.ReadLine();
            Console.WriteLine("Opening Browser. Please sign in.");
            var uri = client.GetSignInUrl(key);
            OpenBrowser(uri);
            Console.WriteLine("Paste the code. This is in the browser url bar 'code={code}'.");
            var code = Console.ReadLine();
            await client.SignIn(key, code);
            Console.WriteLine($"Authenticated {client.IsSignedIn}.");
            Console.ReadLine();
        }

        static async Task SignInFlow()
        {
            await client.SignIn();
            Console.WriteLine($"Authenticated {client.IsSignedIn}.");
            Console.ReadLine();
        }

        static async Task RecordFlow()
        {
            await client.SignIn();

            if (!client.IsSignedIn)
            {
                throw new Exception("Not signed in");
            }

            Console.WriteLine("Choose a QOS 0-5 : 5000, 3000, 1500, 1000, 750, 500");
            var option = Console.ReadKey();
            Console.WriteLine("");
            int qos = 0;
            int.TryParse(option.KeyChar.ToString(), out qos);

            Console.WriteLine("Type symbol(s)");
            var symbols = Console.ReadLine();

            if (!Directory.Exists("../../../Records/")) { Directory.CreateDirectory("../../../Records/"); }

            var path = $"../../../Records/{DateTime.UtcNow.ToString("yyyy-MM-dd")}.txt";
            var pathbin = $"../../../Records/{DateTime.UtcNow.ToString("yyyy-MM-dd")}.dat";
            var stream = new FileStream(pathbin, FileMode.Append);
            var formatter = new BinaryFormatter();

            using (var socket = new TDAmeritradeStreamClient(client))
            {
                socket.OnException += (e) =>
                {
                    throw e;
                };
                socket.OnJsonSignal += (m) =>
                {
                    lock (path)
                    {
                        Console.WriteLine(m);
                        using (var s = File.AppendText(path))
                        {
                            s.WriteLine(m);
                        }
                        formatter.Serialize(stream, m);
                    }
                };

                await socket.Connect();
                await socket.RequestQOS((TDQOSLevels)qos);
                await socket.SubscribeQuote(symbols);
                await socket.SubscribeChart(symbols, IsFuture(symbols) ? TDChartSubs.CHART_FUTURES : TDChartSubs.CHART_EQUITY);
                await socket.SubscribeTimeSale(symbols, IsFuture(symbols) ? TDTimeSaleServices.TIMESALE_FUTURES : TDTimeSaleServices.TIMESALE_EQUITY);
                Console.WriteLine("Type any key to quit");
                Console.ReadLine();
                await socket.Disconnect();
                stream.Dispose();
            }
        }

        static bool IsFuture(string s)
        {
            return s.StartsWith("/");
        }

        static void OpenBrowser(string url)
        {
            try
            {
                Process.Start(url);
            }
            catch
            {
                // hack because of this: https://github.com/dotnet/corefx/issues/10361
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                {
                    url = url.Replace("&", "^&");
                    Process.Start(new ProcessStartInfo("cmd", $"/c start {url}") { CreateNoWindow = true });
                }
                else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                {
                    Process.Start("xdg-open", url);
                }
                else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
                {
                    Process.Start("open", url);
                }
                else
                {
                    throw;
                }
            }
        }
    }
}
