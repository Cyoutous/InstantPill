using BaseLib.Utils.Attributes;

namespace InstantPill.InstantPillCode.Cards.Effect;

[CustomID("INSTANTPILL-EFFECT_PILL_006")]
public sealed class EffectPill006 : BaseEffectPillCard
{
    protected override int StrengthAmount => 6;

    public override EffectPillGrade Grade => EffectPillGrade.Excluded;
}
