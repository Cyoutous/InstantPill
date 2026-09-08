using BaseLib.Utils.Attributes;

namespace InstantPill.InstantPillCode.Cards.Effect;

[CustomID("INSTANTPILL-EFFECT_PILL_010")]
public sealed class EffectPill010 : BaseEffectPillCard
{
    public EffectPill010() : base(showInCardLibrary: false) { }

    protected override int StrengthAmount => 10;

    public override EffectPillGrade Grade => EffectPillGrade.Excluded;
}
