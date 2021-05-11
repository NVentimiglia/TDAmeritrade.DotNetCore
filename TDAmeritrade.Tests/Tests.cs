using NUnit.Framework;
using System.Threading.Tasks;

namespace TDAmeritrade.Tests
{
    public class Tests
    {
        TDAmeritradeClient client;

        [SetUp]
        public async Task Init()
        {
            // Please sign in first, following services uses the client file
            var cache = new TDUnprotectedCache();
            client = new TDAmeritradeClient(cache);
            await client.SignIn();
            Assert.IsTrue(client.IsSignedIn);
        }

        [Test]
        public async Task TestTDQuoteClient_Equity()
        {
            var data = await client.GetQuote_Equity("MSFT");
            Assert.IsTrue(data.symbol == "MSFT");
        }

        [Test]
        public async Task TestTDQuoteClient_Index()
        {
            var data = await client.GetQuote_Index("$SPX.X");
            Assert.IsTrue(data.symbol == "$SPX.X");
        }

        [Test]
        public async Task TestTDQuoteClient_Future()
        {
            var data = await client.GetQuote_Future("/ES");
            Assert.IsTrue(data.symbol == "ES");
        }

        [Test]
        public async Task TestTDQuoteClient_Option()
        {
            var data = await client.GetQuote_Option("SPY_231215C500");
            Assert.IsTrue(data.symbol == "SPY_231215C500");
        }

        [Test]
        public async Task TestPriceHistory_NoAuth()
        {
            client.SignOut(true, false);
            var data = await client.GetPriceHistory(new TDPriceHistoryRequest
            {
                symbol = "MSFT",
                frequencyType = TDPriceHistoryRequest.FrequencyType.minute,
                frequency = 5,
                periodType = TDPriceHistoryRequest.PeriodTypes.day,
                period = 5,
            });
            Assert.IsTrue(data.Length > 0);
        }

        [Test]
        public async Task TestPriceHistory()
        {
            var data = await client.GetPriceHistory(new TDPriceHistoryRequest
            {
                symbol = "MSFT",
                frequencyType = TDPriceHistoryRequest.FrequencyType.minute,
                frequency = 5,
                periodType = TDPriceHistoryRequest.PeriodTypes.day,
                period = 5,
            });
            Assert.IsTrue(data.Length > 0);
        }

        [Test]
        public async Task TestOptionChain()
        {
            var data = await client.GetOptionsChain(new TDOptionChainRequest
            {
                symbol = "SPY",
            });
        }

        [Test]
        public async Task TestTDPrincipalClient()
        {
            var data = await client.GetPrincipals(TDPrincipalsFields.preferences, TDPrincipalsFields.streamerConnectionInfo, TDPrincipalsFields.streamerSubscriptionKeys);
            Assert.IsTrue(!string.IsNullOrEmpty(data.accessLevel));
        }

        [Test]
        public async Task TestQOSRequest()
        {
            await client.SignIn();
            using (var socket = new TDAmeritradeStreamClient(client))
            {
                await socket.Connect();
                await socket.RequestQOS(TDQOSLevels.FAST);
            }
        }

        [Test]
        public async Task TestRealtimeStream()
        {
            await client.SignIn();
            using (var socket = new TDAmeritradeStreamClient(client))
            {
                var symbol = "SPY";
                socket.OnHeartbeatSignal += o => { };
                socket.OnQuoteSignal += o => { };
                socket.OnTimeSaleSignal += o => { };
                socket.OnChartSignal += o => { };
                socket.OnBookSignal += o => { };
                await socket.Connect();
                await socket.SubscribeQuote(symbol);
                await socket.SubscribeChart(symbol, TDChartSubs.CHART_EQUITY);
                await socket.SubscribeTimeSale(symbol, TDTimeSaleServices.TIMESALE_EQUITY);
                await socket.SubscribeBook(symbol, TDBookOptions.LISTED_BOOK);
                await socket.SubscribeBook(symbol, TDBookOptions.NASDAQ_BOOK);
                for (int i = 0; i < 10; i++)
                {
                    await Task.Delay(1000);
                    if (socket.IsConnected)
                        break;
                }
                Assert.IsTrue(socket.IsConnected);
                await socket.Disconnect();
            }
        }

        [Test]
        public async Task TestRealtimeStreamFuture()
        {
            await client.SignIn();
            using (var socket = new TDAmeritradeStreamClient(client))
            {
                var symbol = "/NQ";

                socket.OnHeartbeatSignal += o => { };
                socket.OnQuoteSignal += o => { };
                socket.OnTimeSaleSignal += o => { };
                socket.OnChartSignal += o => { };
                socket.OnBookSignal += o => { };

                await socket.Connect();
                await socket.SubscribeQuote(symbol);
                await socket.SubscribeChart(symbol, TDChartSubs.CHART_FUTURES);
                await socket.SubscribeTimeSale(symbol, TDTimeSaleServices.TIMESALE_FUTURES);

                for (int i = 0; i < 10; i++)
                {
                    await Task.Delay(1000);
                    if (socket.IsConnected)
                        break;
                }

                Assert.IsTrue(socket.IsConnected);
                await socket.Disconnect();
            }
        }

        [Test]
        public void TestParser()
        {
            var reader = new TDStreamJsonProcessor();

            int counter = 5;
            reader.OnHeartbeatSignal += (t) =>
            {
                counter--;
            };
            reader.OnQuoteSignal += (quote) =>
            {
                counter--;
            };
            reader.OnTimeSaleSignal += (sale) =>
            {
                counter--;
            };
            reader.OnChartSignal += (sale) =>
            {
                counter--;
            };
            reader.OnBookSignal += (sale) =>
            {
                counter--;
            };

            reader.Parse("{\"notify\":[{\"heartbeat\":\"1620306966752\"}]}");
            reader.Parse("{\"data\":[{\"service\":\"QUOTE\", \"timestamp\":1620306967787,\"command\":\"SUBS\",\"content\":[{\"key\":\"QQQ\",\"2\":328.75,\"4\":33,\"5\":5,\"6\":\"Q\",\"7\":\"P\",\"11\":33367}]}]}");
            reader.Parse("{ \"data\":[{ \"service\":\"TIMESALE_EQUITY\", \"timestamp\":1620331268678,\"command\":\"SUBS\",\"content\":[{ \"seq\":206718,\"key\":\"QQQ\",\"1\":1620331267917,\"2\":331.57,\"3\":57.0,\"4\":220028},{ \"seq\":206719,\"key\":\"QQQ\",\"1\":1620331267917,\"2\":331.57,\"3\":188.0,\"4\":220029},{ \"seq\":206720,\"key\":\"QQQ\",\"1\":1620331267920,\"2\":331.57,\"3\":55.0,\"4\":220030},{ \"seq\":206721,\"key\":\"QQQ\",\"1\":1620331268378,\"2\":331.57,\"3\":200.0,\"4\":220031}]},{ \"service\":\"QUOTE\", \"timestamp\":1620331268678,\"command\":\"SUBS\",\"content\":[{ \"key\":\"QQQ\",\"2\":331.58,\"3\":331.57,\"4\":4,\"5\":3,\"6\":\"Q\",\"7\":\"Q\",\"8\":44788535,\"9\":2,\"10\":57668,\"11\":57668}]}]}");
            reader.Parse("{ \"data\":[{ \"service\":\"CHART_FUTURES\", \"timestamp\":1620348064760,\"command\":\"SUBS\",\"content\":[{ \"seq\":522,\"key\":\"/NQ\",\"1\":1620348000000,\"2\":13633.75,\"3\":13634.25,\"4\":13633.0,\"5\":13633.5,\"6\":38.0}]}]}");
            reader.Parse("{ \"data\":[{ \"service\":\"NASDAQ_BOOK\", \"timestamp\":1620658957880,\"command\":\"SUBS\", \"content\": [{\"key\":\"QQQ\",\"1\":1620658957722,\"2\": [{\"0\":328.47,\"1\":535,\"2\":3,\"3\":[{\"0\":\"NSDQ\",\"1\":335,\"2\":36155235}, {\"0\":\"phlx\",\"1\":100,\"2\":36157556},{\"0\":\"arcx\",\"1\":100,\"2\":36157656}]}, {\"0\":328.46,\"1\":2800,\"2\":3,\"3\":[{\"0\":\"batx\",\"1\":1000,\"2\":36157696}, {\"0\":\"nyse\",\"1\":1000,\"2\":36157697},{\"0\":\"edgx\",\"1\":800,\"2\":36157694}]}, {\"0\":328.45,\"1\":200,\"2\":2,\"3\":[{\"0\":\"MEMX\",\"1\":100,\"2\":36157694}, {\"0\":\"bosx\",\"1\":100,\"2\":36157696}]},{\"0\":328.44,\"1\":1200,\"2\":4,\"3\":[{\"0\":\"cinn\",\"1\":300,\"2\":36157339},{\"0\":\"edga\",\"1\":300,\"2\":36157555}, {\"0\":\"baty\",\"1\":300,\"2\":36157592},{\"0\":\"MIAX\",\"1\":300,\"2\":36157694}]}, {\"0\":328.42,\"1\":200,\"2\":1,\"3\":[{\"0\":\"iexg\",\"1\":200,\"2\":36157695}]}, {\"0\":327.34,\"1\":100,\"2\":1,\"3\":[{\"0\":\"mwse\",\"1\":100,\"2\":36126129}]}, {\"0\":326.77,\"1\":100,\"2\":1,\"3\":[{\"0\":\"amex\",\"1\":100,\"2\":36146149}]}], \"3\":[{\"0\":328.48,\"1\":1200,\"2\":4,\"3\":[{\"0\":\"NSDQ\",\"1\":300,\"2\":36157695}, {\"0\":\"phlx\",\"1\":300,\"2\":36157695},{\"0\":\"arcx\",\"1\":300,\"2\":36157696}, {\"0\":\"nyse\",\"1\":300,\"2\":36157696}]},{\"0\":328.49,\"1\":2800,\"2\":4,\"3\":[{\"0\":\"batx\",\"1\":1400,\"2\":36157337}, {\"0\":\"edgx\",\"1\":900,\"2\":36157695},{\"0\":\"MIAX\",\"1\":300,\"2\":36157694},{\"0\":\"MEMX\",\"1\":200,\"2\":36157694}]}, {\"0\":328.5,\"1\":300,\"2\":1,\"3\":[{\"0\":\"bosx\",\"1\":300,\"2\":36157695}]}, {\"0\":328.51,\"1\":1500,\"2\":3,\"3\":[{\"0\":\"baty\",\"1\":600,\"2\":36157337},{\"0\":\"edga\",\"1\":600,\"2\":36157695}, {\"0\":\"cinn\",\"1\":300,\"2\":36157656}]},{\"0\":328.59,\"1\":200,\"2\":1,\"3\":[{\"0\":\"iexg\",\"1\":200,\"2\":36157696}]}, {\"0\":329.55,\"1\":100,\"2\":1,\"3\":[{\"0\":\"mwse\",\"1\":100,\"2\":35431559}]}, {\"0\":336.64,\"1\":200,\"2\":1,\"3\":[{\"0\":\"GSCO\",\"1\":200,\"2\":30661019}]} ]}]}]}");
            Assert.IsTrue(counter == 0);
        }
    }
}