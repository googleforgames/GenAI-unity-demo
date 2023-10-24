using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]

public class DBK_ColorThemeSelect : MonoBehaviour {

    [SerializeField]
    public int iColorTheme = 1;
    [SerializeField]
    public int iOrnamentTheme = 1;

    private MaterialPropertyBlock propertyBlock;
    private Renderer renderer_r;
    private int ornamentRow;
    private int ornamentColumn;

    void OnEnable()
    {
        ApplyProperties();
    }

    void ApplyProperties()
    {
        if (propertyBlock == null)
            propertyBlock = new MaterialPropertyBlock();

        renderer_r = GetComponentInChildren<Renderer>();
        propertyBlock.SetInt("_Color", iColorTheme);
        propertyBlock.SetInt("_WallpaperNumber", ReturnOrnamentRow(iOrnamentTheme));
        propertyBlock.SetInt("_WallpaperRow", ReturnOrnamentColumn(iOrnamentTheme));
        renderer_r.SetPropertyBlock(propertyBlock);
    }

    void OnValidate()
    {
        ApplyProperties();
    }

    int ReturnOrnamentRow(int index)
    {
        int row = Mathf.CeilToInt(index / 4) + 1;
        return row;
    }

    int ReturnOrnamentColumn(int index)
    {
        int column = index % 4;
        return column;
    }
}
