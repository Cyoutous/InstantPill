using System;
using System.Linq;
using System.Text;
using InstantPill.InstantPillCode.Content;
using InstantPill.InstantPillCode.Gameplay.Pools;
using MegaCrit.Sts2.Core.DevConsole;
using MegaCrit.Sts2.Core.DevConsole.ConsoleCommands;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Runs;

namespace InstantPill.InstantPillCode.Debug;

/// <summary>
/// Text-console equivalent of the temporary BaseLib settings buttons.
/// ReflectionHelper discovers public AbstractConsoleCmd subclasses in loaded mods.
/// </summary>
public sealed class InstantPillConsoleCmd : AbstractConsoleCmd
{
    public override string CmdName => "instantpill";

    public override string Args => "<grant|clear|pools>";

    public override string Description => "InstantPill test command: grant/clear Capsule, or print this player's pill pools.";

    public override bool IsNetworked => false;

    // This is a narrowly scoped test command, so it remains available whenever the game's console is open.
    public override bool DebugOnly => false;

    public override CmdResult Process(Player? issuingPlayer, string[] args)
    {
        if (!RunManager.Instance.IsInProgress || issuingPlayer == null)
        {
            return new CmdResult(success: false, "Start a single-player run before using InstantPill test commands.");
        }

        if (args.Length != 1)
        {
            return new CmdResult(success: false, "Usage: instantpill <grant|clear|pools>");
        }

        return args[0].ToLowerInvariant() switch
        {
            "grant" => Grant(issuingPlayer),
            "clear" => Clear(issuingPlayer),
            "pools" => PrintPools(issuingPlayer),
            _ => new CmdResult(success: false, "Usage: instantpill <grant|clear|pools>")
        };
    }

    private static CmdResult Grant(Player player)
    {
        CardModel? card = player.Deck.Cards.FirstOrDefault(candidate => !candidate.Keywords.Contains(InstantPillKeywords.Capsule));
        if (card == null)
        {
            return new CmdResult(success: false, "Every card in this deck already has Capsule. Use 'instantpill clear' first.");
        }

        card.AddKeyword(InstantPillKeywords.Capsule);
        MainFile.Logger.Info($"Console test command granted Capsule to {card.Id}.", 1);
        return new CmdResult(success: true, $"Granted Capsule to {card.Id.Entry}. Start the next combat to test the opening-hand effect.");
    }

    private static CmdResult Clear(Player player)
    {
        int cleared = 0;
        foreach (CardModel card in player.Deck.Cards.Where(candidate => candidate.Keywords.Contains(InstantPillKeywords.Capsule)))
        {
            card.RemoveKeyword(InstantPillKeywords.Capsule);
            cleared++;
        }

        MainFile.Logger.Info($"Console test command removed Capsule from {cleared} deck card(s).", 1);
        return new CmdResult(success: true, $"Removed Capsule from {cleared} deck card(s).");
    }

    private static CmdResult PrintPools(Player player)
    {
        // RunStarted normally creates this state. EnsureInitialized is retained as a safe fallback
        // for older saves and developer-created runs that did not dispatch that event.
        PillPoolState state = PillPoolService.EnsureInitialized(player);
        StringBuilder output = new();

        output.AppendLine($"InstantPill pools for player {player.NetId}");
        output.AppendLine($"Candidate effect pool ({state.RemainingEffectIds.Count} remaining):");
        foreach (string effectId in state.RemainingEffectIds)
        {
            output.AppendLine($"  {effectId}");
        }

        output.AppendLine($"Capsule pool ({state.CapsuleSlots.Count} slots):");
        foreach (PillPoolSlotState slot in state.CapsuleSlots)
        {
            string mapping = slot.RevealedEffectPillId ?? "unrevealed";
            output.AppendLine($"  {slot.MysteryPillId} -> {mapping}");
        }

        string message = output.ToString().TrimEnd();
        MainFile.Logger.Info(message, 1);
        return new CmdResult(success: true, message);
    }
}
