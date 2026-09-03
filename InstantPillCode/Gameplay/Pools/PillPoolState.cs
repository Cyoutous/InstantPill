using System;
using System.Collections.Generic;
using MegaCrit.Sts2.Core.Multiplayer.Serialization;

namespace InstantPill.InstantPillCode.Gameplay.Pools;

/// <summary>
/// Per-player run state for the candidate-effect pool and capsule pool.
/// This object is persisted through a SavedSpireField; it is not a static card pool.
/// </summary>
public sealed class PillPoolState : IPacketSerializable
{
    public int SchemaVersion { get; set; } = PillPoolRules.SchemaVersion;

    // Advances whenever this service makes a random pool decision after initialization.
    // It is persisted so save/load cannot reroll future rewards or revelations.
    public int RandomRollCounter { get; set; }

    public List<string> RemainingEffectIds { get; set; } = [];

    public List<PillPoolSlotState> CapsuleSlots { get; set; } = [];

    public void Serialize(PacketWriter writer)
    {
        writer.WriteInt(SchemaVersion);
        writer.WriteInt(RandomRollCounter);
        WriteStringList(writer, RemainingEffectIds);

        writer.WriteInt(CapsuleSlots.Count);
        foreach (PillPoolSlotState slot in CapsuleSlots)
        {
            slot.Serialize(writer);
        }
    }

    public void Deserialize(PacketReader reader)
    {
        SchemaVersion = reader.ReadInt();
        RandomRollCounter = reader.ReadInt();
        RemainingEffectIds = ReadStringList(reader);

        int slotCount = ReadCount(reader, "capsule slot");
        CapsuleSlots = new List<PillPoolSlotState>(slotCount);
        for (int index = 0; index < slotCount; index++)
        {
            PillPoolSlotState slot = new();
            slot.Deserialize(reader);
            CapsuleSlots.Add(slot);
        }
    }

    private static void WriteStringList(PacketWriter writer, IReadOnlyList<string> values)
    {
        writer.WriteInt(values.Count);
        foreach (string value in values)
        {
            writer.WriteString(value);
        }
    }

    private static List<string> ReadStringList(PacketReader reader)
    {
        int count = ReadCount(reader, "effect");
        List<string> values = new(count);
        for (int index = 0; index < count; index++)
        {
            values.Add(reader.ReadString());
        }

        return values;
    }

    private static int ReadCount(PacketReader reader, string label)
    {
        const int maximumPersistedEntries = 1000;

        int count = reader.ReadInt();
        if (count < 0 || count > maximumPersistedEntries)
        {
            throw new InvalidOperationException($"Invalid InstantPill {label} count in saved pool state: {count}.");
        }

        return count;
    }
}
