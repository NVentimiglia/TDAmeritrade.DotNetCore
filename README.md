## TD Ameritrade Client Library for .NET

Free, open-source .NET Client for the [TD Ameritrade Trading Platform](https://www.tdameritrade.com/api.page).
Helps developers integrate TD Ameritrade API into custom trading solutions.

### Features

- Authentication Flow
- Principal
- Quotes
- Historical Charts
- Option Chain
- Streaming QOS
- Streaming Charts
- Streaming Level 1 Quotes
- Streaming Level 2 Quotes
- Streaming Time & Sales

### Sample

```csharp
// Credentials are saved, please use the protected cache
var cache = TDProtectedCache();
var client = new TDAmeritradeClient(cache);

// Sign in first time
var url = client.GetSignInUrl("consumerkey");
client.SignIn("consumerkey", "codefromloginurl");
Assert.IsTrue(client.IsSignedIn);

// Sign in second time
client.SignIn();
Assert.IsTrue(client.IsSignedIn);

//Use!
var data = await client.GetQuote_Equity("SPY");
var data = await client.GetQuote_Future("/NQ");
var data = await client.GetQuote_Option("SPY_231215C500");
var data = await client.GetPriceHistory(new TDPriceHistoryRequest
{
    symbol = "SPY",
    frequencyType = TDPriceHistoryRequest.FrequencyType.minute,
    frequency = 5,
    periodType = TDPriceHistoryRequest.PeriodTypes.day,
    period = 2,
});
var data = await client.GetOptionsChain(new TDOptionChainRequest
{
    symbol = "SPY",
});
var data = await client.GetPrincipals(TDPrincipalsFields.preferences);
using (var socket = new TDAmeritradeStreamClient(client))
{
    socket.OnHeartbeatSignal += o => { };
    socket.OnQuoteSignal += o => { };
    socket.OnTimeSaleSignal += o => { };
    socket.OnChartSignal += o => { };
    socket.OnBookSignal += o => { };
    await socket.Connect();
    await socket.SubscribeQuote("QQQ");
    await socket.SubscribeChart("QQQ", TDChartSubs.CHART_EQUITY);
    await socket.SubscribeTimeSale("QQQ", TDTimeSaleServices.TIMESALE_EQUITY);
    await socket.SubscribeBook("QQQ", TDBookOptions.NASDAQ_BOOK);
}
```
### Console/UnitTest Initialization

Setup is multi-step process. To use this SDK, you must run the console app and set up your security file.

1) Run the console app, this will prompt you for a consumer key. 
2) Consumer key is located under [MyApps](https://developer.tdameritrade.com/user/me/apps)
3) Sign in using the opened web browser. 
4) Copy the code (?code={code}) from the returned website upon completion.
5) Input the code into the console app.
6) This will write a security file (TDAmeritradeKey). Place it in the runtime root when needed.
7) Copy this key to the test bin/debug folder next to the exe and dll

### AspNetCore Initialization

I have included a AspNetCore example with Dataprotection implemented using a web page. 

1) Run the web application
2) Start authentication by submitting your consumer key.
3) Sign in using the opened web browser. 
4) Copy the code (?code={code}) from the returned website upon completion.
5) Input the code into the 'PostAccessToken' field
7) This will write a security file (TDAmeritradeKey).
8) Enjoy

### Credits

Copyright (c) 2021 [Nicholas Ventimiglia](https://www.nicholasventimiglia.com)

Logo image is a trademark of TD Ameritrade, Inc.
