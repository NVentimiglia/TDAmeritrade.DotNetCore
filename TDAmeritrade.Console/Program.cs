using FlatSharp;
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
    class Program : IDisposable
    {
        static async Task Main(string[] args)
        {
            using (var instance = new Program())
            {
                await instance.Run();
            }
        }

        TDUnprotectedCache cache;
        TDAmeritradeClient client;
        FileStream stream;
        bool terminated;

        public Program()
        {
            cache = new TDUnprotectedCache();
            client = new TDAmeritradeClient(cache);
        }

        public void Dispose()
        {
            terminated = true;
            if (stream != null)
            {
                stream.Dispose();
            }
        }

        public async Task Run()
        {
            Console.WriteLine("Choose a flow");
            Console.WriteLine("1) SignIn first time");
            Console.WriteLine("2) Sign in refresh");
            Console.WriteLine("3) Record Stream data");

            var option = Console.ReadKey();
            Console.WriteLine();
            switch (option.Key)
            {
                case ConsoleKey.NumPad1:
                case ConsoleKey.D1:
                    await SignIn();
                    break;
                case ConsoleKey.NumPad2:
                case ConsoleKey.D2:
                    await SignInRefresh();
                    break;
                case ConsoleKey.NumPad3:
                case ConsoleKey.D3:
                    await RecordStream();
                    break;
                case ConsoleKey.NumPad4:
                case ConsoleKey.D4:
                    await BinStream();
                    break;
            }

            Console.WriteLine("Type any key to exit");
            Console.ReadLine();
        }


        public async Task SignIn()
        {
            Console.WriteLine("Paste consumer key : (https://developer.tdameritrade.com/user/me/apps)");
            var consumerKey = Console.ReadLine();
            Console.WriteLine("Opening Browser. Please sign in.");
            var uri = client.GetSignInUrl(consumerKey);
            OpenBrowser(uri);
            Console.WriteLine("When complete,please input the code (code={code}) query paramater. Located inside your browser url bar.");
            var code = Console.ReadLine();
            await client.SignIn(consumerKey, code);
            Console.WriteLine($"IsSignedIn : {client.IsSignedIn}");
        }


        public async Task SignInRefresh()
        {
            await client.SignIn();
            Console.WriteLine($"IsSignedIn : {client.IsSignedIn}");
        }

        public async Task BinStream()
        {
            var txt_path = $"../../../Records/Copy.txt";
            var bin_path = $"../../../Records/Copy.bin";

            if (!File.Exists(bin_path)) { using (var s = File.Create(bin_path)) { } }

            var jsonParser = new TDStreamJsonProcessor();
            var processor = new TDStreamBinFileProcessor();

            using (var writer = new StreamWriter(bin_path))
            {
                jsonParser.OnBookSignal += (o) => writer.Write(processor.Serialize(o));
                jsonParser.OnChartSignal += (o) => writer.Write(processor.Serialize(o));
                jsonParser.OnHeartbeatSignal += (o) => writer.Write(processor.Serialize(o));
                jsonParser.OnQuoteSignal += (o) => writer.Write(processor.Serialize(o));
                jsonParser.OnTimeSaleSignal += (o) => writer.Write(processor.Serialize(o));

                using (var file = new StreamReader(txt_path))
                {
                    var _file = new StreamReader(txt_path);
                    string json = null;
                    while ((json = _file.ReadLine()) != null)
                    {
                        jsonParser.Parse(json);
                    }
                }
            }
        }

        public async Task RecordStream()
        {
            await client.SignIn();
            Console.WriteLine($"IsSignedIn : {client.IsSignedIn}");

            Console.WriteLine("Input Symbol :");
            var symbols = Console.ReadLine();

            Console.WriteLine("Input QOS : 0-5 (500, 750, 1000, 1500, 3000, 5000)ms");
            var qos = Console.ReadLine();
            int qosInt = 0;
            int.TryParse(qos, out qosInt);


            if (!Directory.Exists("../../../Records/")) { Directory.CreateDirectory("../../../Records/"); }

            var txt_path = $"../../../Records/{DateTime.UtcNow.ToString("yyyy-MM-dd")}.txt";
            var bin_path = $"../../../Records/{DateTime.UtcNow.ToString("yyyy-MM-dd")}.bin";

            if (!File.Exists(txt_path)) { using (var s = File.Create(txt_path)) { } }
            if (!File.Exists(bin_path)) { using (var s = File.Create(bin_path)) { } }
            var processor = new TDStreamBinFileProcessor();

            using (var socket = new TDAmeritradeStreamClient(client))
            {
                async void Retry()
                {
                    if (!terminated)
                    {
                        Console.WriteLine("Retrying...");
                        await Task.Delay(5000);
                        Connect();
                    }
                }

                async void Connect()
                {
                    Console.WriteLine("Connecting...");
                    try
                    {
                        await socket.Connect();

                        if (socket.IsConnected)
                        {
                            await socket.RequestQOS((TDQOSLevels)qosInt);
                            await socket.SubscribeQuote(symbols);
                            await socket.SubscribeChart(symbols, IsFutureSymbol(symbols) ? TDChartSubs.CHART_FUTURES : TDChartSubs.CHART_EQUITY);
                            await socket.SubscribeTimeSale(symbols, IsFutureSymbol(symbols) ? TDTimeSaleServices.TIMESALE_FUTURES : TDTimeSaleServices.TIMESALE_EQUITY);
                            await socket.SubscribeBook(symbols, TDBookOptions.LISTED_BOOK);
                            await socket.SubscribeBook(symbols, TDBookOptions.NASDAQ_BOOK);
                            await socket.SubscribeBook(symbols, TDBookOptions.FUTURES_BOOK);
                        }
                        else if (!terminated)
                        {
                            Retry();
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("Error " + ex);
                        Retry();
                    }
                }

                socket.OnJsonSignal += (m) =>
                {
                    lock (txt_path)
                    {
                        Console.WriteLine(m);
                        using (var s = File.AppendText(txt_path))
                        {
                            s.WriteLine(m);
                        }
                    }
                };

                Action<byte[]> writeBin = (buff) =>
               {
                   lock (bin_path)
                   {
                       using (var s = File.AppendText(bin_path))
                       {
                           s.Write(buff);
                       }
                   }
               };

                socket.OnBookSignal += o => writeBin(processor.Serialize(o));
                socket.OnChartSignal += o => writeBin(processor.Serialize(o));
                socket.OnTimeSaleSignal += o => writeBin(processor.Serialize(o));
                socket.OnQuoteSignal += o => writeBin(processor.Serialize(o));
                socket.OnHeartbeatSignal += o => writeBin(processor.Serialize(o));

                socket.OnConnect += (s) =>
                {
                    if (!s)
                    {
                        Connect();
                    }
                };

                Connect();
                Console.WriteLine("Type any key to quit");
                Console.ReadLine();
                terminated = true;
                await socket.Disconnect();

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
        }

        void WriteBin<TModel>(TModel model) where TModel : class
        {

        }


        bool IsFutureSymbol(string s)
        {
            return s.StartsWith("/");
        }

        void OpenBrowser(string url)
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