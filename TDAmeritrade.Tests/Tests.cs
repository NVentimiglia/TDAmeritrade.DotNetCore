using NUnit.Framework;
using System;
using System.Threading.Tasks;

namespace TDAmeritrade.Tests
{
    public class Tests
    {
        // Please sign in first, following services uses the auth file
       // [SetUp]
        public async Task Init()
        {
            TDAuthClient auth = new TDAuthClient(new TDUnprotectedCache());
            var key = Console.ReadLine();
            auth.RequestAccessToken(key);
            var code = Console.ReadLine();
            await auth.PostAccessToken(key, code);
        }

        [Test]
        public async Task TestTDQuoteClient_Equity()
        {
            TDAuthClient auth = new TDAuthClient(new TDUnprotectedCache());
            await auth.PostRefreshToken();
            Assert.IsTrue(auth.IsSignedIn);
            var client = new TDQuoteClient(auth);
            var data = await client.GetQuote<EquityQuote>("MSFT");
            Assert.IsTrue(data.symbol == "MSFT");
        }

        /// SPY,$SPX.X, QQQ,$NDX.X, IWM,$RUT.X, IYY,$DJI2MN Vol indexes $VIX.X,$VXX.X,$VXN.X,$RVX.X
        [Test]
        public async Task TestTDQuoteClient_Index()
        {
            TDAuthClient auth = new TDAuthClient(new TDUnprotectedCache());
            await auth.PostRefreshToken();
            Assert.IsTrue(auth.IsSignedIn);
            var client = new TDQuoteClient(auth);
            var data = await client.GetQuote<IndexQuote>("SPY");
            Assert.IsTrue(data.symbol == "SPY");
        }

        [Test]
        public async Task TestTDQuoteClient_ETF()
        {
            TDAuthClient auth = new TDAuthClient(new TDUnprotectedCache());
            await auth.PostRefreshToken();
            Assert.IsTrue(auth.IsSignedIn);
            var client = new TDQuoteClient(auth);
            var data = await client.GetQuote<ETFQuote>("XLK");
            Assert.IsTrue(data.symbol == "XLK");
        }

        [Test]
        public async Task TestTDQuoteClient_Future()
        {
            TDAuthClient auth = new TDAuthClient(new TDUnprotectedCache());
            await auth.PostRefreshToken();
            Assert.IsTrue(auth.IsSignedIn);
            var client = new TDQuoteClient(auth);
            var data = await client.GetQuote<FundQuote>("/ES");
            Assert.IsTrue(data.symbol == "ES");
        }

        [Test]
        public async Task TestTDQuoteClient_Option()
        {
            TDAuthClient auth = new TDAuthClient(new TDUnprotectedCache());
            await auth.PostRefreshToken();
            Assert.IsTrue(auth.IsSignedIn);
            var client = new TDQuoteClient(auth);
            var data = await client.GetQuote<OptionQuote>("SPY_231215C500");
            Assert.IsTrue(data.symbol == "SPY_231215C500");
        }

        [Test]
        public async Task TestPriceHistory()
        {
            TDAuthClient auth = new TDAuthClient(new TDUnprotectedCache());
            await auth.PostRefreshToken();
            Assert.IsTrue(auth.IsSignedIn);
            var client = new TDPriceHistoryClient(auth);
            var data = await client.Get(new TDPriceHistoryRequest
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
            TDAuthClient auth = new TDAuthClient(new TDUnprotectedCache());
            await auth.PostRefreshToken();
            Assert.IsTrue(auth.IsSignedIn);
            var client = new TDOptionChainClient(auth);
            var data = await client.Get(new TDOptionChainRequest
            {
                symbol = "SPY",
            });
        }

        [Test]
        public async Task TestTDPrincipalClient()
        {
            TDAuthClient auth = new TDAuthClient(new TDUnprotectedCache());
            await auth.PostRefreshToken();
            Assert.IsTrue(auth.IsSignedIn);
            var client = new TDUserInfoClient(auth);
            var data = await client.GetPrincipals(TDUserInfoClient.PrincipalsFields.preferences, TDUserInfoClient.PrincipalsFields.streamerConnectionInfo, TDUserInfoClient.PrincipalsFields.streamerSubscriptionKeys);
            Assert.IsTrue(!string.IsNullOrEmpty(data.accessLevel));
        }
    }
}