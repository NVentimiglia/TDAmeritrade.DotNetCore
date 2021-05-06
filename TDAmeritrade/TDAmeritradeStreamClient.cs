using System;
using System.Collections.Specialized;
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
        CancellationTokenSource _cancel;
        TDPrincipal _prince;
        TDAccount _account;
        private bool _firstMessage = false;

        public event Action<string> OnMessage;

        public TDAmeritradeStreamClient(TDAmeritradeClient client)
        {
            _client = client;
            _cancel = new CancellationTokenSource();
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

            if (_socket != null)
            {
                _socket.Dispose();
                _socket = null;
            }

            try
            {
                Console.WriteLine($"TDAmeritradeStreamClient Connect");

                _firstMessage = false;
                var path = new Uri("wss://" + _prince.streamerInfo.streamerSocketUrl + "/ws");
                _socket = new ClientWebSocket();

                await _socket.ConnectAsync(path, _cancel.Token);

                if (_socket.State == WebSocketState.Open)
                {
                    await Login();

                    Receive();

                }

                await TaskEx.WaitUntil(() => _firstMessage, timeout: 15);

                if (!_firstMessage)
                {
                    throw new Exception("Timeout");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ERROR - {ex.Message}");

                if (_socket != null)
                {
                    _socket.Dispose();
                    _socket = null;
                }
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
                            result = await _socket.ReceiveAsync(buffer, _cancel.Token);
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
                                Disconnect();
                            }
                        }
                    }
                } while (_socket != null && _socket.State == WebSocketState.Open);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ERROR - {ex.Message}");
                Dispose();
            }
        }

        void HandleMessage(string msg)
        {
            _firstMessage = true;
            OnMessage(msg);
        }
        public async Task SendString(string data, CancellationToken cancellation)
        {
            if (_socket != null)
            {
                var encoded = Encoding.UTF8.GetBytes(data);
                var buffer = new ArraySegment<Byte>(encoded, 0, encoded.Length);
                await _socket.SendAsync(buffer, WebSocketMessageType.Text, true, cancellation);
            }
        }

        public async void Disconnect()
        {
            Console.WriteLine($"TDAmeritradeStreamClient Disconnect");
            _cancel.Cancel();
            if (_socket != null)
            {
                await _socket.CloseAsync(WebSocketCloseStatus.NormalClosure, "NormalClosure", _cancel.Token);
                _socket.Dispose();
                _socket = null;
            }
        }

        public void Dispose()
        {
            _cancel.Cancel();
            if (_socket != null)
            {
                _socket.Dispose();
                _socket = null;
            }
        }

        Task Login()
        {
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
                        requestid = 0,
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
            return SendString(data, _cancel.Token);
        }

        public Task Subscribe_Chart(string symbol)
        {
            var request = new TDRealtimeRequestContainer
            {
                requests = new TDRealtimeRequest[]
                {
                    new TDRealtimeRequest
                    {
                        service = "CHART_EQUITY",
                        command = "SUBS",
                        requestid = 2,
                        account = _account.accountId,
                        source = _prince.streamerInfo.appId,
                        parameters = new TDRealtimeParams
                        {
                            keys = symbol,
                            fields = "0,1,2,3,4,5,6,7,8"
                        }
                    }
                }
            };
            var data = JsonSerializer.Serialize(request);
            return SendString(data, _cancel.Token);
        }
    }
}
