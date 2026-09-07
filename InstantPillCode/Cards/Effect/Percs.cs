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

/// <summary>A grade-2 pill that grants its owner the original Hard to Kill power.</summary>
[CustomID("INSTANTPILL-PERCS")]
public sealed class Percs : BaseEffectPillCard
{
    private const string SoundPath = "res://audio/percs1.wav";
    private const float SoundVolume = 1f;
    private const decimal HardToKillAmount = 12m;

    protected override int StrengthAmount => 0;

    public override EffectPillGrade Grade => EffectPillGrade.Grade2;

    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
    [
        new HoverTip(
            ModelDb.Power<HardToKillPower>(),
            new LocString("cards", "INSTANTPILL-PERCS-HARD_TO_KILL.description").GetFormattedText(),
            isSmart: false)
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (LocalContext.IsMine(this))
        {
            PillAudio.PlayOneShot(SoundPath, SoundVolume);
        }

        HardToKillPower? existingHardToKill = Owner.Creature.GetPower<HardToKillPower>();
        if (existingHardToKill != null)
        {
            // Percs can never remove the final stack of Hard to Kill.
            if (existingHardToKill.Amount > 1m)
            {
                await PowerCmd.ModifyAmount(
                    choiceContext,
                    existingHardToKill,
                    -1m,
                    Owner.Creature,
                    this);
            }

            return;
        }

        await PowerCmd.Apply<HardToKillPower>(
            choiceContext,
            Owner.Creature,
            HardToKillAmount,
            Owner.Creature,
            this);
    }
}
