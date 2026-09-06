using System.Collections.Generic;
using System.Threading.Tasks;
using BaseLib.Utils.Attributes;
using InstantPill.InstantPillCode.Audio;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Context;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;

namespace InstantPill.InstantPillCode.Cards.Effect;

/// <summary>A grade-2 pill that grants Hardened Shell and Vulnerable.</summary>
[CustomID("INSTANTPILL-ADDICTED")]
public sealed class Addicted : BaseEffectPillCard
{
    private const string SoundPath = "res://audio/addicted_01.wav";
    private const float SoundVolume = 1f;
    private const decimal HardenedShellAmount = 20m;
    private const decimal VulnerableAmount = 3m;

    protected override int StrengthAmount => 0;

    public override EffectPillGrade Grade => EffectPillGrade.Grade2;

    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
    [
        new HoverTip(
            ModelDb.Power<HardenedShellPower>(),
            new LocString("cards", "INSTANTPILL-ADDICTED-HARDENED_SHELL.description").GetFormattedText(),
            isSmart: false),
        HoverTipFactory.FromPower<VulnerablePower>()
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (LocalContext.IsMine(this))
        {
            PillAudio.PlayOneShot(SoundPath, SoundVolume);
        }

        await PowerCmd.Apply<HardenedShellPower>(
            choiceContext,
            Owner.Creature,
            HardenedShellAmount,
            Owner.Creature,
            this);
        await PowerCmd.Apply<VulnerablePower>(
            choiceContext,
            Owner.Creature,
            VulnerableAmount,
            Owner.Creature,
            this);
    }
}
