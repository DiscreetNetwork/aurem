using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;
using System.IO;
using Aurem.Common.Serialize;
using System.Text.Json;

namespace Discreet.Coin.Models
{
    public class Unit : ISerializable
    {
        // public uint[] Offsets { get; set; }

        // public Key KeyImage { get; set; }

        public void Deserialize(ref MemoryReader reader)
        {
            // Offsets = reader.ReadUInt32Array(len: 64);
            // KeyImage = reader.ReadKey();
        }

        public void Serialize(BEBinaryWriter writer)
        {
            // writer.WriteUInt32Array(Offsets, false);
            // writer.WriteKey(KeyImage);
        }

        // public string Readable()
        // {
        //     var options = new JsonSerializerOptions();
        //     options.Converters.Add(new Converters.TXInputConverter());

        //     return JsonSerializer.Serialize(this, typeof(TXInput), options);
        // }

        public static uint GetSize()
        {
            return 4 * 64 + 32;
        }

        public int Size => (int)GetSize();
    }
}
