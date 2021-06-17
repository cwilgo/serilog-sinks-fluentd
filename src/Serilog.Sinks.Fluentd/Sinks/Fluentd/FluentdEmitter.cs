using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using MsgPack;
using MsgPack.Serialization;

namespace Serilog.Sinks.Fluentd
{
    internal class FluentdEmitter
    {
        private static readonly DateTime UnixEpoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        private readonly Packer _packer;
        private readonly SerializationContext _serializationContext;

        public FluentdEmitter(Stream stream)
        {
            _serializationContext = new SerializationContext(PackerCompatibilityOptions.PackBinaryAsRaw);
            _serializationContext.Serializers.Register(new OrdinaryDictionarySerializer());
            _packer = Packer.Create(stream, PackerCompatibilityOptions.PackBinaryAsRaw);
        }

        public void Emit(DateTime timestamp, string tag, IDictionary<string, object> data)
        {
            _packer.PackArrayHeader(3);
            _packer.PackString(tag, Encoding.UTF8);
            _packer.PackExtendedTypeValue(0x00, ConvertToFluentEventTime(timestamp));
            _packer.Pack(data, _serializationContext);
        }

        private byte[] ConvertToFluentEventTime(DateTime timestamp)
        {
            var sec = (uint) (timestamp.ToUniversalTime().Subtract(UnixEpoch).Ticks / 10000000);
            var nsec = (uint) ((timestamp.ToUniversalTime().Subtract(UnixEpoch).Ticks - sec * 10000000) * 100);
            
            // Format according to Fluentd documentation:
            // https://github.com/fluent/fluentd/wiki/Forward-Protocol-Specification-v0#eventtime-ext-format
            var secBytes = BitConverter.GetBytes(sec);
            var nsecBytes = BitConverter.GetBytes(nsec);
            if (BitConverter.IsLittleEndian)
            {
                // Convert from little-endian to big-endian
                Array.Reverse(secBytes);
                Array.Reverse(nsecBytes);
            }
            var data = new byte[8];
            Buffer.BlockCopy(secBytes, 0, data, 0, 4);
            Buffer.BlockCopy(nsecBytes, 0, data, 4, 4);
            
            return data;
        }
    }
}

