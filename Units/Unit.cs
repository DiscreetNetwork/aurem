using NUlid;
using Aurem.ECC;
using Aurem.Common.Serialize;

/// <summary>
/// Aurem.Units contains code related to chDAG units, which are data structures
/// that represent nodes in a DAG. Units hold transactions data.
/// </summary>
namespace Aurem.Units
{
    public class Unit
    {
        // TODO We need to calculate a hash to use as ID.
        public Ulid Id { get; set; }
        public long CreatorId { get; set; }
        public int Round { get; set; }
        public int ParentsCount { get; set; }
        public List<Ulid> Parents { get; set; }
        public AltBn128G1 Share;
        public byte[]? Data { get; set; }

        public Unit(long creatorId, byte[] data, AltBn128G1 share)
        {
            CreatorId = creatorId;
            Id = Ulid.NewUlid();
            Data = data;
            Round = 0;
            Parents = new();
            Share = share;
        }

        // Used for deserializing a unit.
        public Unit()
        {
            Parents = new();
        }

        public void Serialize(BEBinaryWriter writer)
        {
            writer.Write(Id.ToByteArray());
            writer.Write(CreatorId);
            writer.Write(Round);
            writer.Write(ParentsCount);
            // Parents.
            if (Parents == null)
                throw new Exception("Parents should never be empty.");
            foreach(Ulid parent in Parents)
                writer.Write(parent.ToByteArray());
            // Share.
            for (int c = 0; c < Share.X.Words.Length; c++)
                writer.Write(Share.X.Words[c]);
            for (int c = 0; c < Share.Y.Words.Length; c++)
                writer.Write(Share.Y.Words[c]);
            for (int c = 0; c < Share.Z.Words.Length; c++)
                writer.Write(Share.Z.Words[c]);
            // Data needs to be added to the end.
            if (Data == null)
                throw new Exception("Data should never be empty.");
            writer.Write(Data);
        }

        public void Deserialize(ref MemoryReader reader)
        {
            Id = reader.ReadUlid();
            CreatorId = reader.ReadInt64();
            Round = (int) reader.ReadInt64();
            ParentsCount = (int) reader.ReadInt64();
            if (Parents == null)
                throw new Exception("Parents not initialized.");
            for (int c = 0; c < ParentsCount; c++)
                Parents.Add(reader.ReadUlid());
            for (int c = 0; c < 4; c++)
                Share.X.Words[c] = reader.ReadUInt64();
            for (int c = 0; c < 4; c++)
                Share.Y.Words[c] = reader.ReadUInt64();
            for (int c = 0; c < 4; c++)
                Share.Z.Words[c] = reader.ReadUInt64();
            Data = reader.ReadToEnd().ToArray();
        }

        private void TransactionDeserialize(ref MemoryReader reader)
        {
            byte Version = reader.ReadUInt8();
            byte NumInputs = reader.ReadUInt8();
            byte NumOutputs = reader.ReadUInt8();
            byte NumSigs = reader.ReadUInt8();

            // if (Version > 0) Fee = reader.ReadUInt64();
            // TransactionKey = reader.ReadKey();

            // if (Version > 0) Inputs = reader.ReadSerializableArray<TXInput>(NumInputs);
            // Outputs = reader.ReadSerializableArray<TXOutput>(NumOutputs, (x) => x.TXUnmarshal);

            // if (Version > 0)
            // {
            //     if (Version == 2) RangeProofPlus = reader.ReadSerializable<BulletproofPlus>();
            //     else RangeProof = reader.ReadSerializable<Bulletproof>();

            //     Signatures = reader.ReadSerializableArray<Triptych>(NumSigs);
            //     PseudoOutputs = reader.ReadKeyArray(NumInputs);
            // }
        }
    }
}
