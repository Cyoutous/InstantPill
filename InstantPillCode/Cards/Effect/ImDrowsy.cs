using System.Collections.Generic;
using System.Threading.Tasks;
using BaseLib.Utils.Attributes;
using InstantPill.InstantPillCode.Audio;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Context;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models.Powers;

namespace InstantPill.InstantPillCode.Cards.Effect;

/// <summary>
/// Applies the original Dark Shackles temporary Strength loss to every player and enemy.
/// Pets are intentionally not included.
/// </summary>
[CustomID("INSTANTPILL-IM_DROWSY")]
public sealed class ImDrowsy : BaseEffectPillCard
{
    private const string SoundPath = "res://audio/drowsy 1.wav";
    private const float SoundVolume = 1f;
    private const decimal StrengthLoss = 5m;

    protected override int StrengthAmount => 0;

    public override EffectPillGrade Grade => EffectPillGrade.Grade2;

    public override string? PhdReplacementCardId => "INSTANTPILL-PHD_IM_DROWSY";

    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
    [
        HoverTipFactory.FromPower<StrengthPower>()
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (LocalContext.IsMine(this))
        {
            PillAudio.PlayOneShot(SoundPath, SoundVolume);
        }

        var combatState = CombatState;
        if (combatState == null)
        {
            return;
        }

        // PlayerCreatures contains only players, while Enemies contains only enemies;
        // this deliberately leaves pets outside the effect.
        List<Creature> targets =
        [
            .. combatState.PlayerCreatures,
            .. combatState.Enemies
        ];

        await PowerCmd.Apply<DarkShacklesPower>(
            choiceContext,
            targets,
            StrengthLoss,
            Owner.Creature,
            this);
    }
}
