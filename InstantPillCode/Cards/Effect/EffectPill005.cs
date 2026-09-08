using BaseLib.Utils.Attributes;

namespace InstantPill.InstantPillCode.Cards.Effect;

[CustomID("INSTANTPILL-EFFECT_PILL_005")]
public sealed class EffectPill005 : BaseEffectPillCard
{
    public EffectPill005() : base(showInCardLibrary: false) { }

    protected override int StrengthAmount => 5;

    public override EffectPillGrade Grade => EffectPillGrade.Excluded;
}
