using UnityEngine;
using UnityEditor;
using System.Collections;

[CustomEditor(typeof(DBK_ColorThemeSelect))]
[CanEditMultipleObjects]
public class DBK_ColorThemeSelectEditor : Editor
{
    SerializedProperty colortheme, ornamenttheme;

    public void OnEnable()
    {
        colortheme = serializedObject.FindProperty("iColorTheme");
        ornamenttheme = serializedObject.FindProperty("iOrnamentTheme");        
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        EditorGUI.BeginChangeCheck();
        EditorGUI.showMixedValue = colortheme.hasMultipleDifferentValues;
        var colorValue = EditorGUILayout.IntSlider("Color Theme", colortheme.intValue, 0, 64);        
        EditorGUI.showMixedValue = false;
        if (EditorGUI.EndChangeCheck())
        {
            colortheme.intValue = colorValue;
        }

        EditorGUI.BeginChangeCheck();
        EditorGUI.showMixedValue = ornamenttheme.hasMultipleDifferentValues;
        var wallValue = EditorGUILayout.IntSlider("Wall/Wallpaper Theme", ornamenttheme.intValue, 1, 16);
        EditorGUI.showMixedValue = false;
        if (EditorGUI.EndChangeCheck())
        {
            ornamenttheme.intValue = wallValue;
        }

        serializedObject.ApplyModifiedProperties();
    }
}
