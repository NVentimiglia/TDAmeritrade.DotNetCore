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
### Initialization

Setup is multi-step process. To use this SDK, you must run the console app and set up your security file.

1) Update UserSecrets with a entropy key. This ensure the saved security token is secure. [UserSecrets](https://docs.microsoft.com/en-us/aspnet/core/security/app-secrets?view=aspnetcore-5.0&tabs=windows)
3) Run the console app, this will prompt you for a consumer key. This is located under [MyApps](https://developer.tdameritrade.com/user/me/apps)
4) Sign in using the web browser. Copy the code (?code={code}) from the returned website upon completion.
5) Input the code into the console app.
6) This will write a security file (TDAmeritradeKey). Place it in the runtime root when needed.

### Credits

Copyright (c) 2021 [Nicholas Ventimiglia](https://www.nicholasventimiglia.com)

This software is released under the Apache License 2.0 (the "License"); you may not use the software
except in compliance with the License. You can find a copy of the License in the file
[LICENSE.txt](https://raw.github.com/kriasoft/tdameritrade/master/LICENSE.txt) accompanying this file. 

Logo image is a trademark of TD Ameritrade, Inc.
