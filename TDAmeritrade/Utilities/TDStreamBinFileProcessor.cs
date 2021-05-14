//using BinaryPack;
using System;
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

        private BitSerializer _reader = new BitSerializer();
        private BitSerializer _writer = new BitSerializer();

        public TDStreamBinFileProcessor()
        {
            _reader.IsWriting = false;
            _writer.IsWriting = true;
        }

        public void ReadFile(string path)
        {
            lock (path)
            {
                _reader.Data = File.ReadAllBytes(path);

                while (_reader.Data.Length > _reader.Index)
                {
                    byte head = 0;
                    _reader.Parse(ref head);
                    if (head ==  0)
                    {
                        return;
                    }
                    var header = (TDSignalTypes)head;
                    switch (header)
                    {
                        case TDSignalTypes.HEARTBEAT:
                            OnHeartbeatSignal(_reader.Read<TDHeartbeatSignal>());
                            break;
                        case TDSignalTypes.BOOK:
                            OnBookSignal(_reader.Read<TDBookSignal>());
                            break;
                        case TDSignalTypes.QUOTE:
                            OnQuoteSignal(_reader.Read<TDQuoteSignal>());
                            break;
                        case TDSignalTypes.TIMESALE:
                            OnTimeSaleSignal(_reader.Read<TDTimeSaleSignal>());
                            break;
                        case TDSignalTypes.CHART:
                            OnChartSignal(_reader.Read<TDChartSignal>());
                            break;
                        default:
                            break;
                    }
                }
            }

        }


        public byte[] Serialize<TModel>(TModel model) where TModel : IBitModel, new()
        {
            _writer.Reset();

            // header
            // (byte) payload type
            // payload
            if (model is TDHeartbeatSignal)
            {
                _writer.Parse((byte)TDSignalTypes.HEARTBEAT);
            }
            else if (model is TDBookSignal)
            {
                _writer.Parse((byte)TDSignalTypes.BOOK);
            }
            else if (model is TDTimeSaleSignal)
            {
                _writer.Parse((byte)TDSignalTypes.TIMESALE);
            }
            else if (model is TDQuoteSignal)
            {
                _writer.Parse((byte)TDSignalTypes.QUOTE);
            }
            else if (model is TDChartSignal)
            {
                _writer.Parse((byte)TDSignalTypes.CHART);
            }
            _writer.Parse(model);
            return _writer.Copy();
        }
    }
}