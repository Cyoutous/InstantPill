using BaseLib.Utils.Attributes;

namespace InstantPill.InstantPillCode.Cards.Effect;

[CustomID("INSTANTPILL-EFFECT_PILL_003")]
public sealed class EffectPill003 : BaseEffectPillCard
{
    public EffectPill003() : base(showInCardLibrary: false) { }

    protected override int StrengthAmount => 3;

    public override EffectPillGrade Grade => EffectPillGrade.Excluded;
}
