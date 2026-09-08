using BaseLib.Utils.Attributes;

namespace InstantPill.InstantPillCode.Cards.Effect;

[CustomID("INSTANTPILL-EFFECT_PILL_008")]
public sealed class EffectPill008 : BaseEffectPillCard
{
    public EffectPill008() : base(showInCardLibrary: false) { }

    protected override int StrengthAmount => 8;

    public override EffectPillGrade Grade => EffectPillGrade.Excluded;
}
