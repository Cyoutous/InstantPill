using BaseLib.Utils.Attributes;

namespace InstantPill.InstantPillCode.Cards.Effect;

[CustomID("INSTANTPILL-EFFECT_PILL_002")]
public sealed class EffectPill002 : BaseEffectPillCard
{
    public EffectPill002() : base(showInCardLibrary: false) { }

    protected override int StrengthAmount => 2;

    public override EffectPillGrade Grade => EffectPillGrade.Excluded;
}
