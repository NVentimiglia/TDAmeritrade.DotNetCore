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
            await client.PostRefreshToken();
            Assert.IsTrue(client.IsSignedIn);
        }

        [Test]
        public async Task TestTDQuoteClient_Equity()
        {
            var data = await client.GetQuote<TDEquityQuote>("MSFT");
            Assert.IsTrue(data.symbol == "MSFT");
        }

        [Test]
        public async Task TestTDQuoteClient_Index()
        {
            var data = await client.GetQuote<TDIndexQuote>("$SPX");
            Assert.IsTrue(data.symbol == "$SPX");
        }

        [Test]
        public async Task TestTDQuoteClient_Future()
        {
            var data = await client.GetQuote<TDFundQuote>("/ES");
            Assert.IsTrue(data.symbol == "ES");
        }

        [Test]
        public async Task TestTDQuoteClient_Option()
        {
            var data = await client.GetQuote<TDOptionQuote>("SPY_231215C500");
            Assert.IsTrue(data.symbol == "SPY_231215C500");
        }

        [Test]
        public async Task TestPriceHistory()
        {
            var data = await client.GetPriceHistory(new TDPriceHistoryRequest
            {
                symbol = "SPY",
                frequencyType = TDPriceHistoryRequest.FrequencyType.minute,
                frequency = 5,
                periodType = TDPriceHistoryRequest.PeriodTypes.day,
                period = 2,
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
        public async Task TestRealtimeStream()
        {
            await client.PostRefreshToken();
            using (var socket = new TDAmeritradeStreamClient(client))
            {
                await socket.Connect();
                var symbol = "SPY";
                await socket.SubscribeQuote(symbol);
                await socket.SubscribeChart(symbol, TDAmeritradeClient.IsFutureSymbol(symbol) ? TDChartSubs.CHART_FUTURES : TDChartSubs.CHART_EQUITY);
                await socket.SubscribeTimeSale(symbol, TDAmeritradeClient.IsFutureSymbol(symbol) ? TDTimeSaleServices.TIMESALE_FUTURES : TDTimeSaleServices.TIMESALE_EQUITY);
                await Task.Delay(1000);
                Assert.IsTrue(socket.IsConnected);
                await socket.Disconnect();
            }
        }

        [Test]
        public async Task TestParser()
        {
            var reader = new TDAmeritradeJsonParser();

            int counter = 3;
            reader.OnHeartbeat += (t) => { counter--; };
            reader.OnQuote += (quote) => { counter--; };
            reader.OnTimeSaleEquity += (sale) => { counter--; };

            reader.Parse("{\"notify\":[{\"heartbeat\":\"1620306966752\"}]}");
            reader.Parse("{\"data\":[{\"service\":\"QUOTE\", \"timestamp\":1620306967787,\"command\":\"SUBS\",\"content\":[{\"key\":\"QQQ\",\"2\":328.75,\"4\":33,\"5\":5,\"6\":\"Q\",\"7\":\"P\",\"11\":33367}]}]}");
            reader.Parse("{ \"data\":[{ \"service\":\"TIMESALE_EQUITY\", \"timestamp\":1620331268678,\"command\":\"SUBS\",\"content\":[{ \"seq\":206718,\"key\":\"QQQ\",\"1\":1620331267917,\"2\":331.57,\"3\":57.0,\"4\":220028},{ \"seq\":206719,\"key\":\"QQQ\",\"1\":1620331267917,\"2\":331.57,\"3\":188.0,\"4\":220029},{ \"seq\":206720,\"key\":\"QQQ\",\"1\":1620331267920,\"2\":331.57,\"3\":55.0,\"4\":220030},{ \"seq\":206721,\"key\":\"QQQ\",\"1\":1620331268378,\"2\":331.57,\"3\":200.0,\"4\":220031}]},{ \"service\":\"QUOTE\", \"timestamp\":1620331268678,\"command\":\"SUBS\",\"content\":[{ \"key\":\"QQQ\",\"2\":331.58,\"3\":331.57,\"4\":4,\"5\":3,\"6\":\"Q\",\"7\":\"Q\",\"8\":44788535,\"9\":2,\"10\":57668,\"11\":57668}]}]}");

            Assert.IsTrue(counter == 0);
        }
    }
}