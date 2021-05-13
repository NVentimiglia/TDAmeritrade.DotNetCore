using BinaryPack;
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
        private enum SignalTypes
        {
            NA,
            HEARTBEAT,
            CHART,
            QUOTE,
            TIMESALE,
            BOOK
        }

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
                                OnHeartbeatSignal(Deserialize<TDHeartbeatSignal>(stream));
                                break;
                            case SignalTypes.BOOK:
                                OnBookSignal(Deserialize<TDBookSignal>(stream));
                                break;
                            case SignalTypes.QUOTE:
                                OnQuoteSignal(Deserialize<TDQuoteSignal>(stream));
                                break;
                            case SignalTypes.TIMESALE:
                                OnTimeSaleSignal(Deserialize<TDTimeSaleSignal>(stream));
                                break;
                            case SignalTypes.CHART:
                                OnChartSignal(Deserialize<TDChartSignal>(stream));
                                break;
                            default:
                                break;
                        }
                    }
                }
            }
        }

        public TModel Deserialize<TModel>(byte[] stream) where TModel : new()
        {
            var length = ReadInt(stream, 0);
            var payload = new byte[length];
            Buffer.BlockCopy(stream, sizeof(int), payload, 0, length);
            return BinaryConverter.Deserialize<TModel>(payload);
        }

        public TModel Deserialize<TModel>(Stream stream) where TModel : new()
        {
            var header = new byte[sizeof(int)];
            stream.Read(header, 0, sizeof(int));
            var length = ReadInt(header, 0);
            var payload = new byte[length];
            stream.Read(payload, 0, length);
            return BinaryConverter.Deserialize<TModel>(payload);
        }


        public byte[] Serialize<TModel>(TModel model) where TModel : new()
        {
            // header
            // (byte) payload type
            // (int)  payload length
            //
            // payload
            const int headsize = sizeof(int) + sizeof(byte);
            byte[] buffer = BinaryConverter.Serialize(model);
            byte[] header = new byte[buffer.Length + headsize];
            Buffer.BlockCopy(buffer, 0, header, headsize, buffer.Length);
            WriteInt(buffer.Length, header, 1);
            if (model is TDHeartbeatSignal)
            {
                header[0] = (byte)SignalTypes.HEARTBEAT;
            }
            else if (model is TDBookSignal)
            {
                header[0] = (byte)SignalTypes.BOOK;
            }
            else if (model is TDTimeSaleSignal)
            {
                header[0] = (byte)SignalTypes.TIMESALE;
            }
            else if (model is TDQuoteSignal)
            {
                header[0] = (byte)SignalTypes.QUOTE;
            }
            else if (model is TDChartSignal)
            {
                header[0] = (byte)SignalTypes.CHART;
            }
            return header;
        }

        unsafe void WriteInt(int value, byte[] buffer, int offset)
        {
            fixed (byte* ptr = buffer)
            {
                *(int*)(ptr + offset) = value;
            }
        }
        unsafe int ReadInt(byte[] buffer, int offset)
        {
            int value;
            fixed (byte* ptr = buffer)
            {
                value = *(int*)(ptr + offset);
            }
            return value;
        }
    }
}