using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BaseLib.Utils.Attributes;
using InstantPill.InstantPillCode.Audio;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Helpers;

namespace InstantPill.InstantPillCode.Cards.Effect;

/// <summary>
/// A grade-3 pill that reduces the player's max HP by 6%, rounded up. The loss amount is capped
/// before calling the native command, which preserves the game's usual max-HP-loss behavior while
/// guaranteeing that max HP cannot fall below one.
/// </summary>
[CustomID("INSTANTPILL-HEALTH_DOWN")]
public sealed class HealthDown : BaseEffectPillCard
{
    private const string SoundPath = "res://audio/health down 2.wav";
    private const float SoundVolume = 1f;
    private const decimal MaxHpLossPercent = 0.06m;

    protected override int StrengthAmount => 0;

    public override EffectPillGrade Grade => EffectPillGrade.Grade3;

    // Blood Wall explicitly preloads this scene before it plays the same VFX.
    protected override IEnumerable<string> ExtraRunAssetPaths =>
        [SceneHelper.GetScenePath("vfx/vfx_blood_wall")];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (MegaCrit.Sts2.Core.Context.LocalContext.IsMine(this))
        {
            PillAudio.PlayOneShot(SoundPath, SoundVolume);
        }

        VfxCmd.PlayOnCreatureCenter(Owner.Creature, "vfx/vfx_bloody_impact");

        int currentMaxHp = Owner.Creature.MaxHp;
        decimal requestedLoss = Math.Ceiling(currentMaxHp * MaxHpLossPercent);
        decimal allowedLoss = Math.Max(0m, currentMaxHp - 1m);
        decimal actualLoss = Math.Min(requestedLoss, allowedLoss);
        if (actualLoss > 0m)
        {
            // This uses the game's native max-HP-loss handling, including clamping current HP
            // to the new maximum when necessary.
            await CreatureCmd.LoseMaxHp(choiceContext, Owner.Creature, actualLoss, isFromCard: true);
        }

        // Intentionally visual-only: no Blood Wall cast animation or SFX is played here.
        VfxCmd.PlayOnCreature(Owner.Creature, "vfx/vfx_blood_wall");
    }
}
