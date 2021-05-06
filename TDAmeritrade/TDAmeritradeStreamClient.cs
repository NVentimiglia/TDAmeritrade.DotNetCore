using Newtonsoft.Json.Linq;
using System;
using System.Collections.Specialized;
using System.Dynamic;
using System.IO;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
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
        int _counter;
        
        /// <summary>
        /// Is stream connected
        /// </summary>
        public bool IsConnected
        {
            get
            {
                return _socket != null && _socket.State == WebSocketState.Open;
            }
        }
        
        /// <summary>
        /// Raw json messages
        /// </summary>
        public event Action<string> OnMessage = delegate { };

        /// <summary>
        /// On Heartbeat
        /// </summary>
        public event Action<long> OnHeartbeat = delegate { };

        public TDAmeritradeStreamClient(TDAmeritradeClient client)
        {
            _client = client;
        }

        public async Task Connect()
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

            try
            {
                Console.WriteLine($"TDAmeritradeStreamClient Connect");

                var path = new Uri("wss://" + _prince.streamerInfo.streamerSocketUrl + "/ws");
                _socket = new ClientWebSocket();

                await _socket.ConnectAsync(path, CancellationToken.None);

                if (_socket.State == WebSocketState.Open)
                {
                    await Login();
                    Receive();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ERROR - {ex.Message}");
                Cleanup();
            }
        }

        async void Receive()
        {
            Console.WriteLine($"TDAmeritradeStreamClient Receive");
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
                            Console.WriteLine($"WebSocketReceiveResult Close");
                            break;
                        }

                        ms.Seek(0, SeekOrigin.Begin);

                        using (var reader = new StreamReader(ms, Encoding.UTF8))
                        {
                            var msg = await reader.ReadToEndAsync();
                            try
                            {
                                HandleMessage(msg);
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine($"ERROR - {ex.Message}");
                                Cleanup();
                            }
                        }
                    }
                } while (_socket != null && _socket.State == WebSocketState.Open);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ERROR - {ex.Message}");
                Cleanup();
            }
        }

        void HandleMessage(string msg)
        {
            OnMessage(msg);
        }

        async void Cleanup()
        {
            if (_socket != null)
            {
                if (_socket.State == WebSocketState.Open)
                {
                    await LogOut();
                    await _socket.CloseAsync(WebSocketCloseStatus.NormalClosure, "NormalClosure", CancellationToken.None);
                }
                _socket.Dispose();
                _socket = null;
            }
        }

        public async Task SendString(string data)
        {
            if (_socket != null)
            {
                var encoded = Encoding.UTF8.GetBytes(data);
                var buffer = new ArraySegment<byte>(encoded, 0, encoded.Length);
                await _socket.SendAsync(buffer, WebSocketMessageType.Text, true, CancellationToken.None);
            }
        }

        public async Task Disconnect()
        {
            if (_socket != null)
            {
                Console.WriteLine($"TDAmeritradeStreamClient Disconnect");
                if (_socket.State == WebSocketState.Open)
                {
                    await LogOut();
                    await _socket.CloseAsync(WebSocketCloseStatus.NormalClosure, "NormalClosure", CancellationToken.None);
                }
                _socket.Dispose();
                _socket = null;
            }
        }

        public void Dispose()
        {
            Cleanup();
        }

        Task Login()
        {
            Console.WriteLine($"TDAmeritradeStreamClient Login");
            //Converts ISO-8601 response in snapshot to ms since epoch accepted by Streamer
            var tokenTimeStampAsDateObj = DateTime.Parse(_prince.streamerInfo.tokenTimestamp);
            var tokenTimeStampAsMs = tokenTimeStampAsDateObj.ToUniversalTime().Subtract(new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).TotalMilliseconds;

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
                        parameters = new TDRealtimeParams
                        {
                            token = _prince.streamerInfo.token,
                            version = "1.0",
                            credential = encoded,
                        }
                    }
                }
            };
            var data = JsonSerializer.Serialize(request);
            return SendString(data);
        }
        Task LogOut()
        {
            Console.WriteLine($"TDAmeritradeStreamClient LogOut");
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
                        parameters = new TDRealtimeParams { }
                    }
                }
            };
            var data = JsonSerializer.Serialize(request);
            return SendString(data);
        }

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
                        parameters = new TDRealtimeParams
                        {
                            keys = symbols,
                            fields = "0,1,2,3,4,5,6,7,8"
                        }
                    }
                }
            };
            var data = JsonSerializer.Serialize(request);
            return SendString(data);
        }

        public Task SubscribeQuote(string symbol)
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
                        parameters = new TDRealtimeParams
                        {
                            keys = symbol,
                            fields = "0,1,2,3,4,5,6,7,8,9,10,11,12,13,14,15"
                        }
                    }
                   }
            };

            var data = JsonSerializer.Serialize(request);
            return SendString(data);
        }

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
                        parameters = new TDRealtimeParams
                        {
                            keys = symbols,
                            fields = "0,1,2,3,4"
                        }
                    }
                }
            };

            var data = JsonSerializer.Serialize(request);
            return SendString(data);
        }
    }
}
