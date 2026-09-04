using MegaCrit.Sts2.Core.Multiplayer.Serialization;

namespace InstantPill.InstantPillCode.Gameplay.Pools;

/// <summary>
/// One persistent capsule-pool slot. The effect assignment is fixed at run start;
/// revealing a pill only activates that pre-existing mapping.
/// </summary>
public sealed class PillPoolSlotState : IPacketSerializable
{
    public string MysteryPillId { get; set; } = string.Empty;

    public string AssignedEffectPillId { get; set; } = string.Empty;

    public bool IsRevealed { get; set; }

    public string CurrentCardId => IsRevealed ? AssignedEffectPillId : MysteryPillId;

    public void Serialize(PacketWriter writer)
    {
        writer.WriteString(MysteryPillId);
        writer.WriteString(AssignedEffectPillId);
        writer.WriteBool(IsRevealed);
    }

    public void Deserialize(PacketReader reader)
    {
        MysteryPillId = reader.ReadString();
        AssignedEffectPillId = reader.ReadString();
        IsRevealed = reader.ReadBool();
    }

    internal void DeserializeSchemaV1(PacketReader reader)
    {
        MysteryPillId = reader.ReadString();
        string? previouslyRevealedEffect = reader.ReadBool() ? reader.ReadString() : null;
        AssignedEffectPillId = previouslyRevealedEffect ?? string.Empty;
        IsRevealed = previouslyRevealedEffect != null;
    }
}
