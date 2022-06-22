using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using KyoshinMonitorLib;
using MessagePack;

using static MessagePack.MessagePackSerializer;
using static MessagePack.MessagePackSerializerOptions;

namespace EarthquakeMap2.Utilities
{
    internal static class KyoshinPlaceUtil
    {
        internal static ObservationPoint[] LoadFromMpkBinary(byte[] binary)
        {
            var stream = new ReadOnlyMemory<byte>(binary);
            return Deserialize<ObservationPoint[]>(stream, Standard.WithCompression(MessagePackCompression.Lz4Block));
        }
    }
}
