## TD Ameritrade Client Library for .NET

Free, open-source .NET Client for the [TD Ameritrade Trading Platform](https://www.tdameritrade.com/api.page).
Helps developers integrate TD Ameritrade API into custom trading solutions.

### Sample

```csharp
// Credentials are saved, please use the protected cache
var cache = TDProtectedCache();
var client = new TDAmeritradeClient(cache);

// Confirm the credentials
client.PostRefreshToken();
Assert.IsTrue(client.IsSignedIn);

//Use!
var data = await client.GetQuote<TDIndexQuote>("SPY");
var data = await client.GetQuote<TDOptionQuote>("SPY_231215C500");
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
var data = await client.GetPrincipals(TDPrincipalsFields.preferences, TDPrincipalsFields.streamerConnectionInfo, TDPrincipalsFields.streamerSubscriptionKeys);
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

This software is released under the Apache License 2.0 (the "License"); you may not use the software
except in compliance with the License. You can find a copy of the License in the file
[LICENSE.txt](https://raw.github.com/kriasoft/tdameritrade/master/LICENSE.txt) accompanying this file. 

Logo image is a trademark of TD Ameritrade, Inc.
