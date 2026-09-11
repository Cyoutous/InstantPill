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
/// A grade-3 pill that normally reduces the player's max HP by 10. As a hidden recovery rule,
/// it instead sets a critically low maximum HP to 95.
/// </summary>
[CustomID("INSTANTPILL-HEALTH_DOWN")]
public sealed class HealthDown : BaseEffectPillCard
{
    private const string SoundPath = "res://audio/health down 2.wav";
    private const float SoundVolume = 1f;
    private const decimal MaxHpLoss = 10m;
    private const int CriticalMaxHpThreshold = 35;
    private const int CriticalMaxHpReset = 95;

    protected override int StrengthAmount => 0;

    public override EffectPillGrade Grade => EffectPillGrade.Grade3;

    public override string? PhdReplacementCardId => "INSTANTPILL-HEALTH_UP";

    public override bool IsExtremelyPowerfulEffect => true;

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

        if (Owner.Creature.MaxHp < CriticalMaxHpThreshold)
        {
            // SetMaxHp preserves current HP where possible and clamps it to the new maximum.
            await CreatureCmd.SetMaxHp(Owner.Creature, CriticalMaxHpReset);
        }
        else
        {
            decimal allowedLoss = Math.Max(0m, Owner.Creature.MaxHp - 1m);
            decimal actualLoss = Math.Min(MaxHpLoss, allowedLoss);
            if (actualLoss > 0m)
            {
                // Preserve the native max-HP-loss behavior, including card-loss hooks.
                await CreatureCmd.LoseMaxHp(choiceContext, Owner.Creature, actualLoss, isFromCard: true);
            }
        }

        // Intentionally visual-only: no Blood Wall cast animation or SFX is played here.
        VfxCmd.PlayOnCreature(Owner.Creature, "vfx/vfx_blood_wall");
    }
}
