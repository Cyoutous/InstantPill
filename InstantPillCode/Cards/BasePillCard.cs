using BaseLib.Abstracts;
using BaseLib.Utils;
using Godot;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Models.CardPools;

namespace InstantPill.InstantPillCode.Cards;

/// <summary>
/// Shared structural base for InstantPill cards.
/// Individual pill cards opt into their own keywords and play behaviour.
/// </summary>
[Pool(typeof(TokenCardPool))]
public abstract class BasePillCard : CustomCardModel
{
    private const string PillFramePath = "res://InstantPill/images/cards/instant_pill_frame.png";
    private const string PillPortraitBorderPath = "res://InstantPill/images/cards/instant_pill_portrait_border.png";
    private const string PillBannerPath = "res://InstantPill/images/cards/instant_pill_banner.png";
    private const string PillEnergyIconPath = "res://InstantPill/images/cards/instant_pill_energy_icon.png";

    private static Texture2D? _pillFrame;
    private static Texture2D? _pillPortraitBorder;
    private static Texture2D? _pillBanner;
    private static Texture2D? _pillEnergyIcon;
    private static readonly CanvasItemMaterial PillFrameMaterial = new();
    private static readonly CanvasItemMaterial PillBannerMaterial = new();

    // Uses the same mechanism as unupgradable vanilla cards such as Dazed.
    // All standard smithing and CardCmd upgrade paths therefore skip every pill card.
    public override int MaxUpgradeLevel => 0;

    /// <summary>
    /// Shared full-card frame for Mystery and effect pills. Portraits, banners,
    /// energy icons, and their existing gameplay-specific visuals remain separate.
    /// </summary>
    public override Texture2D? CustomFrame => _pillFrame ??= ResourceLoader.Load<Texture2D>(PillFramePath);

    /// <summary>
    /// The Token pool's colourless frame shader would desaturate the custom PNG.
    /// Use a plain UI material so the capsule frame keeps its authored colours.
    /// </summary>
    public override Material? CreateCustomFrameMaterial => PillFrameMaterial;

    /// <summary>
    /// Keeps the custom portrait border and banner from being recoloured by the
    /// Token rarity's standard banner material.
    /// </summary>
    public override Material? CreateCustomBannerMaterial => PillBannerMaterial;

    internal static Texture2D? PillPortraitBorder =>
        _pillPortraitBorder ??= ResourceLoader.Load<Texture2D>(PillPortraitBorderPath);

    internal static Texture2D? PillBanner =>
        _pillBanner ??= ResourceLoader.Load<Texture2D>(PillBannerPath);

    internal static Texture2D? PillEnergyIcon =>
        _pillEnergyIcon ??= ResourceLoader.Load<Texture2D>(PillEnergyIconPath);

    internal static Material PillUiMaterial => PillBannerMaterial;

    protected BasePillCard(int energyCost, bool showInCardLibrary = true)
        : base(energyCost, CardType.Power, CardRarity.Token, TargetType.None, showInCardLibrary)
    {
    }
}
