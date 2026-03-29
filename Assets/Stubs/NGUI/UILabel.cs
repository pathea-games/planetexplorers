using UnityEngine;

/// <summary>
/// NGUI stub: text rendering widget.
/// Provides text, font, overflow, effect, and sizing properties.
/// Will be replaced with functional uGUI equivalent in Phase 8.
/// </summary>
public class UILabel : UIWidget
{
    public enum Overflow
    {
        ShrinkContent,
        ClampContent,
        ResizeFreely,
        ResizeHeight,
    }

    public enum Effect
    {
        None,
        Shadow,
        Outline,
        Outline8,
    }

    public enum Crispness
    {
        Never,
        OnDesktop,
        Always,
    }

    public string text = "";
    public int fontSize = 16;
    public UIFont font;
    public Overflow overflowMethod = Overflow.ShrinkContent;
    public Effect effectStyle = Effect.None;
    public Color effectColor = Color.black;
    public Vector2 effectDistance = Vector2.one;
    public int spacingX;
    public int spacingY;
    public int maxLineCount;
    public bool supportEncoding = true;
    public UIFont.SymbolStyle symbolStyle = UIFont.SymbolStyle.Normal;
    public NGUIText.Alignment alignment = NGUIText.Alignment.Automatic;
    public bool gradientTop;
    public bool gradientBottom;
    public Color gradientTopColor = Color.white;
    public Color gradientBottomColor = Color.white;
    public bool shrinkToFit;
    public bool keepCrispWhenShrunk;
    public bool multiLine = true;

    public Vector2 printedSize { get { return Vector2.zero; } }

    public string processedText { get { return text ?? ""; } }

    public int lineCount { get { return 1; } }

    public int lineWidth { get; set; }

    public int lineHeight { get { return fontSize; } }

    public bool isValid { get { return true; } }

    public void SetCurrentPercent() { }

    public void SetCurrentProgress() { }

    public void SetCurrentSelection() { }

    public void AssumeNaturalSize() { }
}

/// <summary>
/// NGUI stub: text utility class for alignment and wrapping.
/// </summary>
public static class NGUIText
{
    public enum Alignment
    {
        Automatic,
        Left,
        Center,
        Right,
        Justified,
    }

    public enum SymbolStyle
    {
        None,
        Normal,
        Colored,
    }

    public static int fontSize = 16;
    public static float fontScale = 1f;
    public static int regionWidth = 1000000;
    public static int regionHeight = 1000000;
    public static int maxLines = 0;
    public static bool encoding = true;
    public static Alignment alignment = Alignment.Automatic;

    public static string StripSymbols(string text) { return text ?? ""; }

    public static void Update() { }

    public static void Update(bool request) { }

    public static bool WrapText(string text, out string final) { final = text; return true; }

    public static bool WrapText(string text, out string final, bool keepCharCount) { final = text; return true; }

    public static Vector2 CalculatePrintedSize(string text) { return Vector2.zero; }
}
