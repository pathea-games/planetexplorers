using UnityEngine;

/// <summary>
/// NGUI stub: sprite widget that displays an image from an atlas.
/// Provides spriteName, atlas reference, and pixel-perfect rendering.
/// Will be replaced with functional uGUI equivalent in Phase 8.
/// </summary>
public class UISprite : UIBasicSprite
{
    public string spriteName = "";
    public UIAtlas atlas;
    // Returns the UISpriteData for the current sprite from the atlas
    public UISpriteData sprite => GetAtlasSprite() ?? new UISpriteData();

    public override void MakePixelPerfect() { }

    public UISpriteData GetAtlasSprite() { return null; }

    public bool isValid { get { return false; } }
}

/// <summary>
/// NGUI stub: sprite data descriptor returned by atlas lookups.
/// </summary>
public class UISpriteData
{
    public string name = "";
    public int x;
    public int y;
    public int width;
    public int height;
    public int borderLeft;
    public int borderRight;
    public int borderTop;
    public int borderBottom;
    public int paddingLeft;
    public int paddingRight;
    public int paddingTop;
    public int paddingBottom;
}
