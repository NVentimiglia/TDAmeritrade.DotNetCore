using Newtonsoft.Json;
using System;
using System.Collections.Specialized;
using System.IO;
using System.Net.WebSockets;
using System.Text;

using System.Threading;
using System.Threading.Tasks;
using System.Web;

namespace TDAmeritrade
{
    /// <summary>
    /// Make a request to the User Info & Preferences API's Get User Principals method to retrieve the information found in the javascript example login request below. This can be run directly in the browser console.
    /// https://developer.tdameritrade.com/content/streaming-data
    /// </summary>
    public class TDAmeritradeStreamClient : IDisposable
    {
        TDAmeritradeClient _client;
        ClientWebSocket _socket;
        TDPrincipal _prince;
        TDAccount _account;
        TDStreamJsonProcessor _parser;
        JsonSerializerSettings _settings = new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore };
        int _counter;
        bool _connected;
        private SemaphoreSlim _slim = new SemaphoreSlim(1);

        /// <summary>
        /// Is stream connected
        /// </summary>
        public bool IsConnected
        {
            get
            {
                return _connected;
            }
            private set
            {
                if (_connected == value)
                    return;
                _connected = value;
                OnConnect(value);
            }
        }

        /// <summary>Client sent errors</summary>
        public event Action<Exception> OnException = delegate { };
        /// <summary> Server Sent Events </summary>
        public event Action<bool> OnConnect = delegate { };
        /// <summary> Server Sent Events as raw json </summary>
        public event Action<string> OnJsonSignal = delegate { };
        /// <summary> Server Sent Events </summary>
        public event Action<TDHeartbeatSignal> OnHeartbeatSignal = delegate { };
        /// <summary> Server Sent Events </summary>
        public event Action<TDChartSignal> OnChartSignal = delegate { };
        /// <summary> Server Sent Events </summary>
        public event Action<TDQuoteSignal> OnQuoteSignal = delegate { };
        /// <summary> Server Sent Events </summary>
        public event Action<TDTimeSaleSignal> OnTimeSaleSignal = delegate { };
        /// <summary> Server Sent Events </summary>
        public event Action<TDBookSignal> OnBookSignal = delegate { };

        public TDAmeritradeStreamClient(TDAmeritradeClient client)
        {
            _client = client;
            _parser = new TDStreamJsonProcessor();
            _parser.OnHeartbeatSignal += o => { OnHeartbeatSignal(o); };
            _parser.OnChartSignal += o => { OnChartSignal(o); };
            _parser.OnQuoteSignal += o => { OnQuoteSignal(o); };
            _parser.OnTimeSaleSignal += o => { OnTimeSaleSignal(o); };
            _parser.OnBookSignal += o => { OnBookSignal(o); };
        }

        /// <summary>
        /// Connects to the live stream service
        /// </summary>
        /// <returns></returns>
        public async Task Connect()
        {
            try
            {
                if (!_client.IsSignedIn)
                {
                    throw new Exception("Unauthorized");
                }

                if (_socket != null && _socket.State != WebSocketState.Closed)
                {
                    throw new Exception("Busy");
                }


                _prince = await _client.GetPrincipals(TDPrincipalsFields.streamerConnectionInfo, TDPrincipalsFields.streamerSubscriptionKeys, TDPrincipalsFields.preferences);
                _account = _prince.accounts.Find(o => o.accountId == _prince.primaryAccountId);

                var path = new Uri("wss://" + _prince.streamerInfo.streamerSocketUrl + "/ws");
                _socket = new ClientWebSocket();

                await _socket.ConnectAsync(path, CancellationToken.None);

                if (_socket.State == WebSocketState.Open)
                {
                    await Login();
                    Receive();
                    IsConnected = true;
                }
            }
            catch (Exception ex)
            {
                OnException(ex);
                Cleanup();
            }
        }

        /// <summary>
        /// Disconnects from the live stream service and logs out
        /// </summary>
        /// <returns></returns>
        public async Task Disconnect()
        {
            if (_socket != null)
            {
                if (_socket.State == WebSocketState.Open)
                {
                    await LogOut();
                    await _socket.CloseAsync(WebSocketCloseStatus.NormalClosure, "NormalClosure", CancellationToken.None);
                    OnConnect(IsConnected);
                }
                if (_socket != null)
                {
                    _socket.Dispose();
                }
                _socket = null;
            }
            IsConnected = false;
        }

        public void Dispose()
        {
            Cleanup();
        }

        /// <summary>
        /// Subscribed to the chart event service
        /// </summary>
        /// <param name="symbols">spy,qqq,amd</param>
        /// <param name="isFutureSymbol">true if symbols are for futures</param>
        /// <returns></returns>
        public Task SubscribeChart(string symbols, TDChartSubs service)
        {
            var request = new TDRealtimeRequestContainer
            {
                requests = new TDRealtimeRequest[]
                    {
                    new TDRealtimeRequest
                    {
                        service = service.ToString(),
                        command = "SUBS",
                        requestid = Interlocked.Increment(ref _counter),
                        account = _account.accountId,
                        source = _prince.streamerInfo.appId,
                        parameters = new
                        {
                            keys = symbols,
                            fields = "0,1,2,3,4,5,6,7,8"
                        }
                    }
                    }
            };
            var data = JsonConvert.SerializeObject(request, _settings);
            return SendToServer(data);
        }

        /// <summary>
        /// Unsubscribed to the chart event service
        /// </summary>
        /// <param name="symbols">spy,qqq,amd</param>
        /// <param name="isFutureSymbol">true if symbols are for futures</param>
        /// <returns></returns>
        public Task UnsubscribeChart(string symbols, TDChartSubs service)
        {
            var request = new TDRealtimeRequestContainer
            {
                requests = new TDRealtimeRequest[]
                    {
                    new TDRealtimeRequest
                    {
                        service = service.ToString(),
                        command = "UNSUBS",
                        requestid = Interlocked.Increment(ref _counter),
                        account = _account.accountId,
                        source = _prince.streamerInfo.appId,
                        parameters = new
                        {
                            keys = symbols,
                        }
                    }
                    }
            };
            var data = JsonConvert.SerializeObject(request, _settings);
            return SendToServer(data);
        }

        /// <summary>
        /// Subscribeds to the level one quote event service
        /// </summary>
        /// <param name="symbols"></param>
        /// <returns></returns>
        public Task SubscribeQuote(string symbols)
        {
            var request = new TDRealtimeRequestContainer
            {
                requests = new TDRealtimeRequest[]
                {
                    new TDRealtimeRequest
                    {
                        service = "QUOTE",
                        command = "SUBS",
                        requestid = Interlocked.Increment(ref _counter),
                        account = _account.accountId,
                        source = _prince.streamerInfo.appId,
                        parameters = new
                        {
                            keys = symbols,
                            fields = "0,1,2,3,4,5,8,9,10,11,12,13,14,15,24,28"
                        }
                    }
                }
            };

            var data = JsonConvert.SerializeObject(request, _settings);
            return SendToServer(data);
        }

        /// <summary>
        /// Unsubscribeds to the level one quote event service
        /// </summary>
        /// <param name="symbols"></param>
        /// <returns></returns>
        public Task UnsubscribeQuote(string symbols)
        {
            var request = new TDRealtimeRequestContainer
            {
                requests = new TDRealtimeRequest[]
                {
                    new TDRealtimeRequest
                    {
                        service = "QUOTE",
                        command = "UNSUBS",
                        requestid = Interlocked.Increment(ref _counter),
                        account = _account.accountId,
                        source = _prince.streamerInfo.appId,
                        parameters = new
                        {
                            keys = symbols,
                        }
                    }
                }
            };

            var data = JsonConvert.SerializeObject(request, _settings);
            return SendToServer(data);
        }

        /// <summary>
        /// Subscribed to the time&sales event service
        /// </summary>
        /// <param name="symbols">spy,qqq,amd</param>
        /// <param name="service">data service to subscribe to</param>
        /// <returns></returns>
        public Task SubscribeTimeSale(string symbols, TDTimeSaleServices service)
        {
            var request = new TDRealtimeRequestContainer
            {
                requests = new TDRealtimeRequest[]
                {
                    new TDRealtimeRequest
                    {
                        service = service.ToString(),
                        command = "SUBS",
                        requestid = Interlocked.Increment(ref _counter),
                        account = _account.accountId,
                        source = _prince.streamerInfo.appId,
                        parameters = new
                        {
                            keys = symbols,
                            fields = "0,1,2,3,4"
                        }
                    }
                }
            };

            var data = JsonConvert.SerializeObject(request, _settings);
            return SendToServer(data);
        }

        /// <summary>
        /// Unsubscribed to the time&sales event service
        /// </summary>
        /// <param name="symbols">spy,qqq,amd</param>
        /// <param name="service">data service to subscribe to</param>
        /// <returns></returns>
        public Task UnsubscribeTimeSale(string symbols, TDTimeSaleServices service)
        {
            var request = new TDRealtimeRequestContainer
            {
                requests = new TDRealtimeRequest[]
                {
                    new TDRealtimeRequest
                    {
                        service = service.ToString(),
                        command = "UNSUBS",
                        requestid = Interlocked.Increment(ref _counter),
                        account = _account.accountId,
                        source = _prince.streamerInfo.appId,
                        parameters = new
                        {
                            keys = symbols,
                        }
                    }
                }
            };

            var data = JsonConvert.SerializeObject(request, _settings);
            return SendToServer(data);
        }

        /// <summary>
        /// Subscribe to the level two order book. Note this stream has no official documentation, and it's not entirely clear what exchange it corresponds to.Use at your own risk.
        /// </summary>
        /// <returns></returns>
        public Task SubscribeBook(string symbols, TDBookOptions option)
        {
            var request = new TDRealtimeRequestContainer
            {
                requests = new TDRealtimeRequest[]
                {
                    new TDRealtimeRequest
                    {
                        service = option.ToString(),
                        command = "SUBS",
                        requestid = Interlocked.Increment(ref _counter),
                        account = _account.accountId,
                        source = _prince.streamerInfo.appId,
                        parameters = new
                        {
                            keys = symbols,
                            fields = "0,1,2,3"
                        }
                    }
                }
            };

            var data = JsonConvert.SerializeObject(request, _settings);
            return SendToServer(data);
        }

        /// <summary>
        /// Unsubscribe to the level two order book. Note this stream has no official documentation, and it's not entirely clear what exchange it corresponds to.Use at your own risk.
        /// </summary>
        /// <returns></returns>
        public Task UnsubscribeBook(string symbols, TDBookOptions option)
        {
            var request = new TDRealtimeRequestContainer
            {
                requests = new TDRealtimeRequest[]
                {
                    new TDRealtimeRequest
                    {
                        service = option.ToString(),
                        command = "UNSUBS",
                        requestid = Interlocked.Increment(ref _counter),
                        account = _account.accountId,
                        source = _prince.streamerInfo.appId,
                        parameters = new
                        {
                            keys = symbols,
                        }
                    }
                }
            };

            var data = JsonConvert.SerializeObject(request, _settings);
            return SendToServer(data);
        }

        /// <summary>
        /// Quality of Service provides the different rates of data updates per protocol (binary, websocket etc), or per user based.
        /// </summary>
        /// <param name="quality">Quality of Service, or the rate the data will be sent to the client.</param>
        /// <returns></returns>
        public Task RequestQOS(TDQOSLevels quality)
        {
            var request = new TDRealtimeRequestContainer
            {
                requests = new TDRealtimeRequest[]
                {
                    new TDRealtimeRequest
                    {
                        service = "ADMIN",
                        command = "QOS",
                        requestid = Interlocked.Increment(ref _counter),
                        account = _account.accountId,
                        source = _prince.streamerInfo.appId,
                        parameters = new
                        {
                            qoslevel = ((int)quality)
                        }
                    }
                }
            };

            var data = JsonConvert.SerializeObject(request, _settings);
            return SendToServer(data);
        }

        //

        /// <summary>
        /// Sends a request to the server
        public async Task SendToServer(string data)
        {
            await _slim.WaitAsync();
            try
            {
                if (_socket != null)
                {
                    var encoded = Encoding.UTF8.GetBytes(data);
                    var buffer = new ArraySegment<byte>(encoded, 0, encoded.Length);
                    await _socket.SendAsync(buffer, WebSocketMessageType.Text, true, CancellationToken.None);
                }
            }
            catch (Exception ex)
            {
                OnException(ex);
                Cleanup();
            }
            finally
            {
                _slim.Release();
            }
        }

        //

        private async void Receive()
        {
            var buffer = new ArraySegment<byte>(new byte[2048]);
            try
            {
                do
                {
                    WebSocketReceiveResult result;
                    using (var ms = new MemoryStream())
                    {
                        do
                        {
                            result = await _socket.ReceiveAsync(buffer, CancellationToken.None);
                            ms.Write(buffer.Array, buffer.Offset, result.Count);
                        } while (!result.EndOfMessage);

                        if (result.MessageType == WebSocketMessageType.Close)
                        {
                            throw new Exception("WebSocketMessageType.Close");
                        }

                        ms.Seek(0, SeekOrigin.Begin);

                        using (var reader = new StreamReader(ms, Encoding.UTF8))
                        {
                            var msg = await reader.ReadToEndAsync();
                            HandleMessage(msg);
                        }
                    }
                } while (_socket != null && _socket.State == WebSocketState.Open);
            }
            catch (Exception ex)
            {
                OnException(ex);
                Cleanup();
            }
        }

        private Task Login()
        {
            //Converts ISO-8601 response in snapshot to ms since epoch accepted by Streamer
            var tokenTimeStampAsDateObj = DateTime.Parse(_prince.streamerInfo.tokenTimestamp);
            var tokenTimeStampAsMs = tokenTimeStampAsDateObj.ToUniversalTime().ToUnixTimeMilliseconds();

            NameValueCollection queryString = HttpUtility.ParseQueryString(string.Empty);

            queryString.Add("userid", _account.accountId);
            queryString.Add("company", _account.company);
            queryString.Add("segment", _account.segment);
            queryString.Add("cddomain", _account.accountCdDomainId);

            queryString.Add("token", _prince.streamerInfo.token);
            queryString.Add("usergroup", _prince.streamerInfo.userGroup);
            queryString.Add("accessLevel", _prince.streamerInfo.accessLevel);
            queryString.Add("appId", _prince.streamerInfo.appId);
            queryString.Add("acl", _prince.streamerInfo.acl);

            queryString.Add("timestamp", tokenTimeStampAsMs.ToString());
            queryString.Add("authorized", "Y");

            var credits = queryString.ToString();
            var encoded = HttpUtility.UrlEncode(credits);

            var request = new TDRealtimeRequestContainer
            {
                requests = new TDRealtimeRequest[]
                {
                    new TDRealtimeRequest
                    {
                        service = "ADMIN",
                        command = "LOGIN",
                        requestid = Interlocked.Increment(ref _counter),
                        account = _account.accountId,
                        source = _prince.streamerInfo.appId,
                        parameters = new
                        {
                            token = _prince.streamerInfo.token,
                            version = "1.0",
                            credential = encoded,
                        }
                    }
                }
            };
            var data = JsonConvert.SerializeObject(request);
            return SendToServer(data);
        }

        private Task LogOut()
        {
            var request = new TDRealtimeRequestContainer
            {
                requests = new TDRealtimeRequest[]
                {
                    new TDRealtimeRequest
                    {
                        service = "ADMIN",
                        command = "LOGOUT",
                        requestid = Interlocked.Increment(ref _counter),
                        account = _account.accountId,
                        source = _prince.streamerInfo.appId,
                        parameters = new { }
                    }
                }
            };
            var data = JsonConvert.SerializeObject(request);
            return SendToServer(data);
        }

        private void HandleMessage(string msg)
        {
            try
            {
                OnJsonSignal(msg);
                _parser.Parse(msg);
            }
            catch (Exception ex)
            {
                OnException(ex);
                //Do not cleanup, this is a user code issue
            }
        }

        private async void Cleanup()
        {
            if (_socket != null)
            {
                if (_socket.State == WebSocketState.Open)
                {
                    await LogOut();
                    if (_socket != null)
                    {
                        await _socket.CloseAsync(WebSocketCloseStatus.NormalClosure, "NormalClosure", CancellationToken.None);
                    }
                }
                if (_socket != null)
                {
                    _socket.Dispose();
                }
                _socket = null;
            }
            IsConnected = false;
        }
    }
}
