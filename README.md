## TD Ameritrade Client Library for .NET

Free, open-source .NET Client for the [TD Ameritrade Trading Platform](https://www.tdameritrade.com/api.page).
Helps developers integrate TD Ameritrade API into custom trading solutions.

### Download

Get the latest version via [NuGet](https://www.nuget.org/packages/TDAmeritrade.DotNetCore/)

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


### Credits

Copyright (c) 2021 [Nicholas Ventimiglia], (https://www.nicholasventimiglia.com)

This software is released under the Apache License 2.0 (the "License"); you may not use the software
except in compliance with the License. You can find a copy of the License in the file
[LICENSE.txt](https://raw.github.com/kriasoft/tdameritrade/master/LICENSE.txt) accompanying this file. 

Logo image is a trademark of TD Ameritrade, Inc.
