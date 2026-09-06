using System.Threading.Tasks;
using BaseLib.Utils.Attributes;
using InstantPill.InstantPillCode.Audio;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Context;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models.Powers;

namespace InstantPill.InstantPillCode.Cards.Effect;

/// <summary>
/// A grade-2 pill that plants five independent, one-turn original Bomb powers.
/// </summary>
[CustomID("INSTANTPILL-EXPLOSIVE_DIARRHEA")]
public sealed class ExplosiveDiarrhea : BaseEffectPillCard
{
    private const string SoundPath = "res://audio/explosive diahhrea 2.wav";
    private const float SoundVolume = 1f;
    private const int BombCount = 5;
    private const decimal BombDamage = 4m;
    private const decimal TurnsUntilExplosion = 1m;

    protected override int StrengthAmount => 0;

    public override EffectPillGrade Grade => EffectPillGrade.Grade2;

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (LocalContext.IsMine(this))
        {
            PillAudio.PlayOneShot(SoundPath, SoundVolume);
        }

        // TheBombPower is instanced, so every application becomes a separately visible
        // one-turn bomb. Original logic supplies the all-enemy explosion VFX and removes it.
        for (int bombIndex = 0; bombIndex < BombCount; bombIndex++)
        {
            TheBombPower? bomb = await PowerCmd.Apply<TheBombPower>(
                choiceContext,
                Owner.Creature,
                TurnsUntilExplosion,
                Owner.Creature,
                this);
            bomb?.SetDamage(BombDamage);
        }
    }
}
