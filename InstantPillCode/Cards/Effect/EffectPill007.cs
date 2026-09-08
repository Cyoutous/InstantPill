using BaseLib.Utils.Attributes;

namespace InstantPill.InstantPillCode.Cards.Effect;

[CustomID("INSTANTPILL-EFFECT_PILL_007")]
public sealed class EffectPill007 : BaseEffectPillCard
{
    public EffectPill007() : base(showInCardLibrary: false) { }

    protected override int StrengthAmount => 7;

    public override EffectPillGrade Grade => EffectPillGrade.Excluded;
}
