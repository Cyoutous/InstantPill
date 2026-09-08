using System;
using MegaCrit.Sts2.Core.Multiplayer.Serialization;

namespace InstantPill.InstantPillCode.Gameplay.Rewards;

/// <summary>
/// The complete InstantPill configuration chosen by the multiplayer host before a run starts.
/// It travels once in the base game's lobby-begin message, then is retained in the run save for
/// reloads and reconnects.
/// </summary>
public sealed class PillMultiplayerRulesSnapshot
{
    public const int CurrentSchemaVersion = 1;

    public int SchemaVersion { get; set; } = CurrentSchemaVersion;

    /// <summary>Only the host's value is meaningful; clients never author this decision.</summary>
    public bool HostSynchronizesParameters { get; set; }

    /// <summary>
    /// The host's shared-pool preference. It is honored only while
    /// <see cref="HostSynchronizesParameters"/> is true.
    /// </summary>
    public bool HostEnablesSharedPillPool { get; set; }

    public PillRewardRulesSnapshot RewardRules { get; set; } = new();

    public void Serialize(PacketWriter writer)
    {
        writer.WriteInt(SchemaVersion);
        writer.WriteBool(HostSynchronizesParameters);
        writer.WriteBool(HostEnablesSharedPillPool);
        SerializeRewardRules(writer, RewardRules);
    }

    public static PillMultiplayerRulesSnapshot Deserialize(PacketReader reader)
    {
        PillMultiplayerRulesSnapshot snapshot = new()
        {
            SchemaVersion = reader.ReadInt()
        };
        if (snapshot.SchemaVersion != CurrentSchemaVersion)
        {
            throw new InvalidOperationException(
                $"Unsupported InstantPill multiplayer-rules schema {snapshot.SchemaVersion}.");
        }

        snapshot.HostSynchronizesParameters = reader.ReadBool();
        snapshot.HostEnablesSharedPillPool = reader.ReadBool();
        snapshot.RewardRules = DeserializeRewardRules(reader);
        return snapshot;
    }

    private static void SerializeRewardRules(PacketWriter writer, PillRewardRulesSnapshot rules)
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
        writer.WriteBool(rules.EnableNormalRewards);
        writer.WriteBool(rules.EnableEliteRewards);
        writer.WriteBool(rules.EnableBossRewards);
        writer.WriteBool(rules.PreventDuplicateCapsuleOptionsWithinCombat);
    }

    private static PillRewardRulesSnapshot DeserializeRewardRules(PacketReader reader) => new()
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
        InitialMysteryPillCount = reader.ReadInt(),
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

/// <summary>Persisted run holder for the host's one-time multiplayer settings decision.</summary>
public sealed class PillMultiplayerRulesSessionState : IPacketSerializable
{
    public const int CurrentSchemaVersion = 1;

    public int SchemaVersion { get; set; } = CurrentSchemaVersion;

    public PillMultiplayerRulesSnapshot Snapshot { get; set; } = new();

    public void Serialize(PacketWriter writer)
    {
        writer.WriteInt(SchemaVersion);
        Snapshot.Serialize(writer);
    }

    public void Deserialize(PacketReader reader)
    {
        SchemaVersion = reader.ReadInt();
        if (SchemaVersion != CurrentSchemaVersion)
        {
            throw new InvalidOperationException(
                $"Unsupported InstantPill multiplayer-rules session schema {SchemaVersion}.");
        }

        Snapshot = PillMultiplayerRulesSnapshot.Deserialize(reader);
    }
}
