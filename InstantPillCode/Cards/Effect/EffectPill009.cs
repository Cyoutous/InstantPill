using BaseLib.Utils.Attributes;

namespace InstantPill.InstantPillCode.Cards.Effect;

[CustomID("INSTANTPILL-EFFECT_PILL_009")]
public sealed class EffectPill009 : BaseEffectPillCard
{
    public EffectPill009() : base(showInCardLibrary: false) { }

    protected override int StrengthAmount => 9;

    public override EffectPillGrade Grade => EffectPillGrade.Excluded;
}
