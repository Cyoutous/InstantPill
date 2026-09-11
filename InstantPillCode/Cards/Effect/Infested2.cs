using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BaseLib.Utils.Attributes;
using InstantPill.InstantPillCode.Audio;
using InstantPill.InstantPillCode.Cards.Generated;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Context;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;

namespace InstantPill.InstantPillCode.Cards.Effect;

/// <summary>
/// A grade-1 pill that creates one Blue Fly for every currently hittable enemy. The native
/// generated-card command naturally redirects additions from a full hand to the discard pile.
/// </summary>
[CustomID("INSTANTPILL-INFESTED_2")]
public sealed class Infested2 : BaseEffectPillCard
{
    private const string SoundPath = "res://audio/infested_08.wav";
    private const float SoundVolume = 1f;

    protected override int StrengthAmount => 0;

    public override EffectPillGrade Grade => EffectPillGrade.Grade1;

    public override string? FalsePhdReplacementCardId => "INSTANTPILL-LUCK_DOWN";

    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
    [
        HoverTipFactory.FromCard<BlueFly>()
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (LocalContext.IsMine(this))
        {
            PillAudio.PlayOneShot(SoundPath, SoundVolume);
        }

        int enemyCount = CombatState?.HittableEnemies.Count() ?? 0;
        for (int index = 0; index < enemyCount; index++)
        {
            BlueFly fly = CombatState!.CreateCard<BlueFly>(Owner);
            await CardPileCmd.AddGeneratedCardToCombat(fly, PileType.Hand, Owner);
        }
    }
}
