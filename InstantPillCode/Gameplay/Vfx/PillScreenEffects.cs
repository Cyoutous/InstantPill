using Godot;
using MegaCrit.Sts2.Core.Context;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.Rooms;

namespace InstantPill.InstantPillCode.Gameplay.Vfx;

/// <summary>
/// Owns short-lived, local-only full-screen effects used by pill cards. The overlay lives in a
/// high CanvasLayer so a screen-reading shader processes the combat scene and ordinary combat UI.
/// </summary>
public static class PillScreenEffects
{
    private static PillPixelationOverlay? activePixelation;

    /// <summary>
    /// Plays the pixelation transition for the local owner of <paramref name="source"/> only.
    /// A new invocation replaces a currently active transition rather than stacking filters.
    /// </summary>
    public static void PlayPixelation(CardModel source)
    {
        NCombatRoom? combatRoom = NCombatRoom.Instance;
        if (!LocalContext.IsMine(source) || combatRoom is null || !combatRoom.IsInsideTree())
        {
            return;
        }

        activePixelation?.StopImmediately();

        PillPixelationOverlay? overlay = null;
        overlay = new(() =>
        {
            if (ReferenceEquals(activePixelation, overlay))
            {
                activePixelation = null;
            }
        });
        activePixelation = overlay;
        combatRoom.AddChild(overlay);
        overlay.Play();
    }
}

/// <summary>
/// A self-cleaning, non-interactive post-process canvas. It intentionally has no gameplay state.
/// </summary>
internal sealed partial class PillPixelationOverlay : CanvasLayer
{
    private const string ShaderPath = "res://InstantPill/shaders/pill_pixelate.gdshader";
    private const int OverlayLayer = 100;
    private const float PeakPixelSize = 16f;
    private const float FadeInSeconds = 1.75f;
    private const float HoldSeconds = 9.50f;
    private const float FadeOutSeconds = 1.75f;

    private readonly Action onFinished;
    private readonly ColorRect screen = new();
    private ShaderMaterial? shaderMaterial;
    private Tween? tween;
    private bool isFinished;

    public PillPixelationOverlay(Action onFinished)
    {
        this.onFinished = onFinished;
        Layer = OverlayLayer;
        Name = nameof(PillPixelationOverlay);

        screen.SetAnchorsAndOffsetsPreset(Control.LayoutPreset.FullRect);
        screen.MouseFilter = Control.MouseFilterEnum.Ignore;
        screen.Color = Colors.White;
        AddChild(screen);
    }

    public void Play()
    {
        Shader? shader = GD.Load<Shader>(ShaderPath);
        if (shader is null)
        {
            Finish();
            return;
        }

        shaderMaterial = new ShaderMaterial { Shader = shader };
        screen.Material = shaderMaterial;
        SetPixelSize(1f);
        SetIntensity(0f);

        tween = CreateTween();
        tween.TweenMethod(Callable.From<float>(SetPixelSize), 1f, PeakPixelSize, FadeInSeconds)
            .SetEase(Tween.EaseType.Out)
            .SetTrans(Tween.TransitionType.Quad);
        tween.Parallel().TweenMethod(Callable.From<float>(SetIntensity), 0f, 1f, FadeInSeconds)
            .SetEase(Tween.EaseType.Out)
            .SetTrans(Tween.TransitionType.Quad);
        tween.TweenInterval(HoldSeconds);
        tween.TweenMethod(Callable.From<float>(SetPixelSize), PeakPixelSize, 1f, FadeOutSeconds)
            .SetEase(Tween.EaseType.In)
            .SetTrans(Tween.TransitionType.Quad);
        tween.Parallel().TweenMethod(Callable.From<float>(SetIntensity), 1f, 0f, FadeOutSeconds)
            .SetEase(Tween.EaseType.In)
            .SetTrans(Tween.TransitionType.Quad);
        tween.TweenCallback(Callable.From(Finish));
    }

    public void StopImmediately()
    {
        tween?.Kill();
        Finish();
    }

    public override void _ExitTree()
    {
        tween?.Kill();
        NotifyFinished();
        base._ExitTree();
    }

    private void Finish()
    {
        NotifyFinished();
        QueueFree();
    }

    private void SetPixelSize(float value) => shaderMaterial?.SetShaderParameter("pixel_size", value);

    private void SetIntensity(float value) => shaderMaterial?.SetShaderParameter("intensity", value);

    private void NotifyFinished()
    {
        if (isFinished)
        {
            return;
        }

        isFinished = true;
        onFinished();
    }
}
