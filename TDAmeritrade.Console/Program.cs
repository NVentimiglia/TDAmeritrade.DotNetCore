using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.Json;
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
            var client = new TDAuthClient(new TDUnprotectedCache());


            Console.WriteLine("1 to sign in fresh, 2 to refresh signin, 3 for magic");
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
                    break;
                case ConsoleKey.D2:
                    await client.PostRefreshToken();
                    break;
                //case ConsoleKey.D3:

                //    var common = new List<PropertyInfo>();
                //    var counts = new List<string>();
                //    var types = new List<Type>();
                //    types.Add(typeof(EquityQuote));
                //    types.Add(typeof(ETFQuote));
                //    types.Add(typeof(OptionQuote));

                //    foreach (var t in types)
                //    {
                //        var props = t.GetProperties();
                //        foreach (var p in props)
                //        {
                //            counts.Add(p.Name);
                //        }
                //    }

                //    var dictionary = new Dictionary<string, string>();
                //    var distinct = counts.Distinct();
                //    foreach (var p in distinct)
                //    {
                //        var good = counts.Count(o => o == p) == types.Count;

                //        if (good)
                //        {
                //            dictionary.Add(p, "");
                //        }
                //    }

                //    var json = JsonSerializer.Serialize(dictionary);

                //    break;
                default:
                    return;
            }
        }
    }
}
