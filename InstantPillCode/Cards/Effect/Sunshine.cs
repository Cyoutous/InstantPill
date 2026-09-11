using System.Collections.Generic;
using System.Threading.Tasks;
using BaseLib.Utils.Attributes;
using InstantPill.InstantPillCode.Audio;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Context;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.Nodes.Rooms;
using MegaCrit.Sts2.Core.Nodes.Vfx;

namespace InstantPill.InstantPillCode.Cards.Effect;

/// <summary>
/// A grade-3 pill that gives its owner permanent combat stats and several turns of
/// the original Clarity and Radiance buffs.
/// </summary>
[CustomID("INSTANTPILL-SUNSHINE")]
public sealed class Sunshine : BaseEffectPillCard
{
    private const string SoundPath = "res://audio/sunshine 1.wav";
    private const float SoundVolume = 1f;
    private const decimal PowerAmount = 1m;
    private const decimal TemporaryBuffAmount = 3m;

    protected override int StrengthAmount => 0;

    public override EffectPillGrade Grade => EffectPillGrade.Grade3;

    public override string? FalsePhdReplacementCardId => "INSTANTPILL-RETRO_VISION";

    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
    [
        HoverTipFactory.FromPower<StrengthPower>(),
        HoverTipFactory.FromPower<DexterityPower>(),
        HoverTipFactory.FromPower<FocusPower>(),
        HoverTipFactory.FromPower<ClarityPower>(),
        HoverTipFactory.FromPower<RadiancePower>()
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (LocalContext.IsMine(this))
        {
            PillAudio.PlayOneShot(SoundPath, SoundVolume);
        }

        var grandFinaleVfx = NGrandFinaleVfx.Create(Owner.Creature);
        if (grandFinaleVfx != null)
        {
            NCombatRoom.Instance?.CombatVfxContainer.AddChildSafely(grandFinaleVfx);
            await Cmd.Wait(NGrandFinaleVfx.totalAnticipationDuration);
        }

        await PowerCmd.Apply<StrengthPower>(
            choiceContext, Owner.Creature, PowerAmount, Owner.Creature, this);
        await PowerCmd.Apply<DexterityPower>(
            choiceContext, Owner.Creature, PowerAmount, Owner.Creature, this);
        await PowerCmd.Apply<FocusPower>(
            choiceContext, Owner.Creature, PowerAmount, Owner.Creature, this);
        await PowerCmd.Apply<ClarityPower>(
            choiceContext, Owner.Creature, TemporaryBuffAmount, Owner.Creature, this);
        await PowerCmd.Apply<RadiancePower>(
            choiceContext, Owner.Creature, TemporaryBuffAmount, Owner.Creature, this);
    }
}
