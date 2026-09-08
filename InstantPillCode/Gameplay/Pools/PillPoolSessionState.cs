using System;
using MegaCrit.Sts2.Core.Multiplayer.Serialization;

namespace InstantPill.InstantPillCode.Gameplay.Pools;

/// <summary>
/// Run-scoped snapshot for the optional multiplayer pool mode. This deliberately lives on the
/// run, rather than on a player, so every player observes the same pool mapping and reveal state.
/// </summary>
public sealed class PillPoolSessionState : IPacketSerializable
{
    public const int CurrentSchemaVersion = 1;

    public int SchemaVersion { get; set; } = CurrentSchemaVersion;

    /// <summary>Whether this run routes all capsule-pool access through <see cref="SharedPool"/>.</summary>
    public bool SharedPoolEnabled { get; set; }

    /// <summary>
    /// Distinguishes a deliberately empty, newly-created session from a constructed shared pool.
    /// A pool is initialized lazily because some run setup paths access the session before decks
    /// and all card scopes have been established.
    /// </summary>
    public bool HasSharedPool { get; set; }

    public PillPoolState SharedPool { get; set; } = new();

    public void Serialize(PacketWriter writer)
    {
        writer.WriteInt(SchemaVersion);
        writer.WriteBool(SharedPoolEnabled);
        writer.WriteBool(HasSharedPool);
        SharedPool.Serialize(writer);
    }

    public void Deserialize(PacketReader reader)
    {
        SchemaVersion = reader.ReadInt();
        if (SchemaVersion != CurrentSchemaVersion)
        {
            throw new InvalidOperationException(
                $"Unsupported InstantPill pool-session schema {SchemaVersion}.");
        }

        SharedPoolEnabled = reader.ReadBool();
        HasSharedPool = reader.ReadBool();
        SharedPool = new PillPoolState();
        SharedPool.Deserialize(reader);
    }
}
