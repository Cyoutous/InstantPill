using BaseLib.Utils.Attributes;

namespace InstantPill.InstantPillCode.Cards.Effect;

[CustomID("INSTANTPILL-EFFECT_PILL_007")]
public sealed class EffectPill007 : BaseEffectPillCard
{
    protected override int StrengthAmount => 7;

    public override EffectPillGrade Grade => EffectPillGrade.Excluded;
}
