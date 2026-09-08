using BaseLib.Utils.Attributes;

namespace InstantPill.InstantPillCode.Cards.Effect;

[CustomID("INSTANTPILL-EFFECT_PILL_001")]
public sealed class EffectPill001 : BaseEffectPillCard
{
    public EffectPill001() : base(showInCardLibrary: false) { }

    protected override int StrengthAmount => 1;

    public override EffectPillGrade Grade => EffectPillGrade.Excluded;
}
