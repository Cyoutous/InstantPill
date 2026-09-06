using System.Collections.Generic;
using System.Threading.Tasks;
using BaseLib.Utils.Attributes;
using InstantPill.InstantPillCode.Audio;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Context;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;

namespace InstantPill.InstantPillCode.Cards.Effect;

/// <summary>
/// Draws and grants energy to its owner, then grants every enemy the original
/// Feeding Frenzy temporary Strength buff for the current turn.
/// </summary>
[CustomID("INSTANTPILL-IM_EXCITED")]
public sealed class ImExcited : BaseEffectPillCard
{
    private const string SoundPath = "res://audio/excited 1.wav";
    private const float SoundVolume = 1f;
    private const decimal CardsToDraw = 5m;
    private const int EnergyToGain = 2;
    private const decimal EnemyStrength = 3m;

    protected override int StrengthAmount => 0;

    public override EffectPillGrade Grade => EffectPillGrade.Grade2;

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new EnergyVar(EnergyToGain)
    ];

    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
    [
        EnergyHoverTip,
        HoverTipFactory.FromPower<StrengthPower>()
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (LocalContext.IsMine(this))
        {
            PillAudio.PlayOneShot(SoundPath, SoundVolume);
        }

        await CardPileCmd.Draw(choiceContext, CardsToDraw, Owner);
        await PlayerCmd.GainEnergy(DynamicVars.Energy.IntValue, Owner);

        var combatState = CombatState;
        if (combatState == null)
        {
            return;
        }

        await PowerCmd.Apply<FeedingFrenzyPower>(
            choiceContext,
            combatState.Enemies,
            EnemyStrength,
            Owner.Creature,
            this);
    }
}
