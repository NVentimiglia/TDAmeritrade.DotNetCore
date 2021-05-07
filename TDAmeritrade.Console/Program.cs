using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
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

            Console.WriteLine("1 to sign in fresh, 2 to refresh signin, 3 streaming test, 4 streaming to file");
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
                case ConsoleKey.D3:
                    await client.PostRefreshToken();
                    Console.WriteLine($"Authenticated {client.IsSignedIn}.");
                    using (var socket = new TDAmeritradeStreamClient(client))
                    {
                        socket.OnMessage += (m) =>
                        {
                            Console.WriteLine(m);
                        };
                        await socket.Connect();
                        Console.WriteLine("Type symbol(s)");
                        var symbols2 = Console.ReadLine();
                        await socket.SubscribeQuote(symbols2);
                        await socket.SubscribeChart(symbols2, TDAmeritradeClient.IsFutureSymbol(symbols2) ? TDChartSubs.CHART_FUTURES : TDChartSubs.CHART_EQUITY);
                        await socket.SubscribeTimeSale(symbols2, TDAmeritradeClient.IsFutureSymbol(symbols2) ? TDTimeSaleServices.TIMESALE_FUTURES : TDTimeSaleServices.TIMESALE_EQUITY);
                        Console.WriteLine("Type any key to quit");
                        Console.ReadLine();
                        await socket.Disconnect();
                    }
                    break;
                case ConsoleKey.D4:
                    await client.PostRefreshToken();
                    Console.WriteLine($"Authenticated {client.IsSignedIn}.");

                    Console.WriteLine("Type symbol(s)");
                    var symbols = Console.ReadLine();

                    var path = $"../../../Records/{DateTime.UtcNow.ToString("yyyy-MM-dd")}.txt";

                    var ch_path = $"../../../Records/{DateTime.UtcNow.ToString("yyyy-MM-dd")}.ch.dat";
                    var ts_path = $"../../../Records/{DateTime.UtcNow.ToString("yyyy-MM-dd")}.ts.dat";
                    var qu_path = $"../../../Records/{DateTime.UtcNow.ToString("yyyy-MM-dd")}.qu.dat";

                    if (!Directory.Exists("../../../Records/")) { Directory.CreateDirectory("../../../Records/"); }

                    if (!File.Exists(path)) { using (var s = File.Create(path)) { } }
                    if (!File.Exists(ch_path)) { using (var s = File.Create(ch_path)) { } }
                    if (!File.Exists(ts_path)) { using (var s = File.Create(ts_path)) { } }
                    if (!File.Exists(qu_path)) { using (var s = File.Create(qu_path)) { } }

                    var ts_formatter = new BinaryFormatter();
                    var ch_formatter = new BinaryFormatter();
                    var qu_formatter = new BinaryFormatter();

                    var ts_fs = new FileStream(ts_path, FileMode.Append);
                    var ch_fs = new FileStream(ch_path, FileMode.Append);
                    var qu_fs = new FileStream(qu_path, FileMode.Append);

                    using (var socket = new TDAmeritradeStreamClient(client))
                    {
                        socket.OnMessage += (m) =>
                        {
                            lock (path)
                            {
                                using (var s = File.AppendText(path))
                                {
                                    s.WriteLine(m);
                                }
                                Console.WriteLine(m);
                            }
                        };

                        socket.OnTimeSale += o =>
                        {
                            lock (ts_path)
                            {
                                ts_formatter.Serialize(ts_fs, o);
                            }

                        };
                        socket.OnChart += o =>
                        {
                            lock (ch_path)
                            {
                                ch_formatter.Serialize(ch_fs, o);
                            }
                        };
                        socket.OnQuote += o =>
                        {
                            lock (qu_path)
                            {
                                qu_formatter.Serialize(qu_fs, o);
                            }
                        };

                        await socket.Connect();
                        await socket.SubscribeQuote(symbols);
                        await socket.SubscribeChart(symbols, TDAmeritradeClient.IsFutureSymbol(symbols) ? TDChartSubs.CHART_FUTURES : TDChartSubs.CHART_EQUITY);
                        await socket.SubscribeTimeSale(symbols, TDAmeritradeClient.IsFutureSymbol(symbols) ? TDTimeSaleServices.TIMESALE_FUTURES : TDTimeSaleServices.TIMESALE_EQUITY);
                        Console.WriteLine("Type any key to quit");
                        Console.ReadLine();
                        await socket.Disconnect();
                        ts_fs.Dispose();
                        ch_fs.Dispose();
                        qu_fs.Dispose();

                        //var list = new List<TDTimeSaleSignal>();
                        //var bFormatter = new BinaryFormatter();
                        //using (var temp = new FileStream(ts_path, FileMode.Open))
                        //{
                        //    while (temp.Position != temp.Length)
                        //    {
                        //        var t = bFormatter.Deserialize(temp);
                        //        list.Add((TDTimeSaleSignal)t);
                        //    }
                        //}

                    }
                    break;
                default:
                    return;
            }
        }
    }
}
