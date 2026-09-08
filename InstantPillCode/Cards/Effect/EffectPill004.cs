using BaseLib.Utils.Attributes;

namespace InstantPill.InstantPillCode.Cards.Effect;

[CustomID("INSTANTPILL-EFFECT_PILL_004")]
public sealed class EffectPill004 : BaseEffectPillCard
{
    public EffectPill004() : base(showInCardLibrary: false) { }

    protected override int StrengthAmount => 4;

    public override EffectPillGrade Grade => EffectPillGrade.Excluded;
}
