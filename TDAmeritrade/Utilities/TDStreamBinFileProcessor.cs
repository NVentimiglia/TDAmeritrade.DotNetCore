using FlatSharp;
using System;
using System.Collections.Generic;
using System.IO;

namespace TDAmeritrade
{

    /// <summary>
    /// Helper for reading / wrting binary from / to file
    /// </summary>
    public class TDStreamBinFileProcessor
    {
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

        private Dictionary<SignalTypes, int> _sizes;
        private byte[] _buffer = new byte[1024];

        public TDStreamBinFileProcessor()
        {
            _sizes = new Dictionary<SignalTypes, int>();
            _sizes.Add(SignalTypes.HEARTBEAT, FlatBufferSerializer.Default.GetMaxSize(new TDHeartbeatSignal()));
            _sizes.Add(SignalTypes.CHART, FlatBufferSerializer.Default.GetMaxSize(new TDChartSignal()));
            _sizes.Add(SignalTypes.QUOTE, FlatBufferSerializer.Default.GetMaxSize(new TDQuoteSignal()));
            _sizes.Add(SignalTypes.TIMESALE, FlatBufferSerializer.Default.GetMaxSize(new TDTimeSaleSignal()));
            _sizes.Add(SignalTypes.BOOK, FlatBufferSerializer.Default.GetMaxSize(new TDHeartbeatSignal()));
        }

        public void ReadFile(string path)
        {
            lock (path)
            {
                using (var stream = new FileStream(path, FileMode.Open))
                {
                    while (stream.Position < stream.Length)
                    {
                        var head = stream.ReadByte();
                        if (head == -1)
                        {
                            return;
                        }

                        var header = (SignalTypes)head;

                        switch (header)
                        {
                            case SignalTypes.HEARTBEAT:
                                stream.Read(_buffer, (int)stream.Position, _sizes[SignalTypes.HEARTBEAT]);
                                OnHeartbeatSignal(FlatBufferSerializer.Default.Parse<TDHeartbeatSignal>(_buffer));
                                break;
                            case SignalTypes.BOOK:
                                stream.Read(_buffer, (int)stream.Position, _sizes[SignalTypes.BOOK]);
                                OnBookSignal(FlatBufferSerializer.Default.Parse<TDBookSignal>(_buffer));
                                break;
                            case SignalTypes.QUOTE:
                                stream.Read(_buffer, (int)stream.Position, _sizes[SignalTypes.QUOTE]);
                                OnQuoteSignal(FlatBufferSerializer.Default.Parse<TDQuoteSignal>(_buffer));
                                break;
                            case SignalTypes.TIMESALE:
                                stream.Read(_buffer, (int)stream.Position, _sizes[SignalTypes.TIMESALE]);
                                OnTimeSaleSignal(FlatBufferSerializer.Default.Parse<TDTimeSaleSignal>(_buffer));
                                break;
                            case SignalTypes.CHART:
                                stream.Read(_buffer, (int)stream.Position, _sizes[SignalTypes.CHART]);
                                OnChartSignal(FlatBufferSerializer.Default.Parse<TDChartSignal>(_buffer));
                                break;
                            default:
                                break;
                        }
                    }
                }
            }
        }

        public byte[] Serialize<TModel>(TModel model) where TModel : class
        {

            int maxBytesNeeded = FlatBufferSerializer.Default.GetMaxSize(model);
            byte[] buffer = new byte[maxBytesNeeded];
            int bytesWritten = FlatBufferSerializer.Default.Serialize(model, buffer);
            return buffer;
        }
    }
}