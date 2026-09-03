using MegaCrit.Sts2.Core.Multiplayer.Serialization;

namespace InstantPill.InstantPillCode.Gameplay.Pools;

/// <summary>
/// One persistent capsule-pool slot. The mystery card identity is its visual identity;
/// once revealed, every copy of that mystery card maps to the same effect card entry.
/// </summary>
public sealed class PillPoolSlotState : IPacketSerializable
{
    public string MysteryPillId { get; set; } = string.Empty;

    public string? RevealedEffectPillId { get; set; }

    public string CurrentCardId => RevealedEffectPillId ?? MysteryPillId;

    public void Serialize(PacketWriter writer)
    {
        writer.WriteString(MysteryPillId);
        writer.WriteBool(RevealedEffectPillId != null);
        if (RevealedEffectPillId != null)
        {
            writer.WriteString(RevealedEffectPillId);
        }
    }

    public void Deserialize(PacketReader reader)
    {
        MysteryPillId = reader.ReadString();
        RevealedEffectPillId = reader.ReadBool() ? reader.ReadString() : null;
    }
}
