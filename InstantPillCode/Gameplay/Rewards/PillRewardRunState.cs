using System;
using InstantPill.InstantPillCode.Configuration;
using MegaCrit.Sts2.Core.Multiplayer.Serialization;

namespace InstantPill.InstantPillCode.Gameplay.Rewards;

/// <summary>Per-player persisted state for InstantPill's two independent reward pity systems.</summary>
public sealed class PillRewardRunState : IPacketSerializable
{
    // Version 6 persists both configurable PHD relic rarities with the per-run configuration.
    // Older saves preserve their original reward rules and use each relic's historic Shop placement.
    public const int CurrentSchemaVersion = 6;

    public int SchemaVersion { get; set; } = CurrentSchemaVersion;

    public float IndependentCurrentOdds { get; set; }

    public float ReplacementCurrentOdds { get; set; }

    public int IndependentRollCounter { get; set; }

    public int ReplacementRollCounter { get; set; }

    public int ChoiceRollCounter { get; set; }

    public PillRewardRulesSnapshot Rules { get; set; } = new();

    public void Serialize(PacketWriter writer)
    {
        writer.WriteInt(SchemaVersion);
        writer.WriteFloat(IndependentCurrentOdds, null);
        writer.WriteFloat(ReplacementCurrentOdds, null);
        writer.WriteInt(IndependentRollCounter);
        writer.WriteInt(ReplacementRollCounter);
        writer.WriteInt(ChoiceRollCounter);
        SerializeRules(writer, Rules);
    }

    public void Deserialize(PacketReader reader)
    {
        int serializedSchemaVersion = reader.ReadInt();
        if (serializedSchemaVersion is not 3 and not 4 and not 5 and not CurrentSchemaVersion)
        {
            throw new InvalidOperationException($"Unsupported InstantPill reward-state schema {serializedSchemaVersion}.");
        }

        IndependentCurrentOdds = reader.ReadFloat(null);
        ReplacementCurrentOdds = reader.ReadFloat(null);
        IndependentRollCounter = reader.ReadInt();
        ReplacementRollCounter = reader.ReadInt();
        ChoiceRollCounter = reader.ReadInt();
        Rules = DeserializeRules(
            reader,
            serializedSchemaVersion >= 4,
            serializedSchemaVersion >= 5,
            serializedSchemaVersion >= 6);
        SchemaVersion = CurrentSchemaVersion;
    }

    private static void SerializeRules(PacketWriter writer, PillRewardRulesSnapshot rules)
    {
        SerializeOdds(writer, rules.IndependentDrop);
        SerializeOdds(writer, rules.PotionReplacement);
        writer.WriteInt(rules.NormalChoiceCount);
        writer.WriteInt(rules.EliteChoiceCount);
        writer.WriteInt(rules.BossChoiceCount);
        writer.WriteInt(rules.NormalGenerationAttempts);
        writer.WriteInt(rules.EliteGenerationAttempts);
        writer.WriteInt(rules.BossGenerationAttempts);
        writer.WriteInt(rules.NormalGuaranteedRewardCount);
        writer.WriteInt(rules.EliteGuaranteedRewardCount);
        writer.WriteInt(rules.BossGuaranteedRewardCount);
        writer.WriteInt(rules.InitialMysteryPillCount);
        writer.WriteString(rules.PhdRelicRarity);
        writer.WriteString(rules.FalsePhdRelicRarity);
        writer.WriteBool(rules.EnableNormalRewards);
        writer.WriteBool(rules.EnableEliteRewards);
        writer.WriteBool(rules.EnableBossRewards);
        writer.WriteBool(rules.PreventDuplicateCapsuleOptionsWithinCombat);
    }

    private static PillRewardRulesSnapshot DeserializeRules(
        PacketReader reader,
        bool hasInitialMysteryPillCount,
        bool hasPhdRelicRarity,
        bool hasFalsePhdRelicRarity) => new()
    {
        IndependentDrop = DeserializeOdds(reader),
        PotionReplacement = DeserializeOdds(reader),
        NormalChoiceCount = reader.ReadInt(),
        EliteChoiceCount = reader.ReadInt(),
        BossChoiceCount = reader.ReadInt(),
        NormalGenerationAttempts = reader.ReadInt(),
        EliteGenerationAttempts = reader.ReadInt(),
        BossGenerationAttempts = reader.ReadInt(),
        NormalGuaranteedRewardCount = reader.ReadInt(),
        EliteGuaranteedRewardCount = reader.ReadInt(),
        BossGuaranteedRewardCount = reader.ReadInt(),
        InitialMysteryPillCount = hasInitialMysteryPillCount ? reader.ReadInt() : 0,
        PhdRelicRarity = hasPhdRelicRarity ? reader.ReadString() : PhdRelicRarityRules.DefaultValue,
        FalsePhdRelicRarity = hasFalsePhdRelicRarity ? reader.ReadString() : PhdRelicRarityRules.DefaultValue,
        EnableNormalRewards = reader.ReadBool(),
        EnableEliteRewards = reader.ReadBool(),
        EnableBossRewards = reader.ReadBool(),
        PreventDuplicateCapsuleOptionsWithinCombat = reader.ReadBool()
    };

    private static void SerializeOdds(PacketWriter writer, PillOddsRules odds)
    {
        writer.WriteFloat(odds.BaseOdds, null);
        writer.WriteFloat(odds.SuccessDecrease, null);
        writer.WriteFloat(odds.FailureIncrease, null);
        writer.WriteFloat(odds.EliteBonus, null);
        writer.WriteFloat(odds.BossBonus, null);
        writer.WriteFloat(odds.MinimumOdds, null);
        writer.WriteFloat(odds.MaximumOdds, null);
    }

    private static PillOddsRules DeserializeOdds(PacketReader reader) => new()
    {
        BaseOdds = reader.ReadFloat(null),
        SuccessDecrease = reader.ReadFloat(null),
        FailureIncrease = reader.ReadFloat(null),
        EliteBonus = reader.ReadFloat(null),
        BossBonus = reader.ReadFloat(null),
        MinimumOdds = reader.ReadFloat(null),
        MaximumOdds = reader.ReadFloat(null)
    };
}
