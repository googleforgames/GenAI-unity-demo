using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[InitializeOnLoad]
public class InitializeLayersAndTags : MonoBehaviour
{
    static InitializeLayersAndTags()
    {
        CreateTag("Melee Collider");
        CreateTag("Fire");
        CreateTag("Cover");
        
        CreateLayer("Character");
        CreateLayer("Enemy");
        CreateLayer("Grass");
        CreateLayer("Head");
        CreateLayer("Noise Collider");
        CreateLayer("Smoke");
        CreateLayer("MultiplayerCharacter");
        
        // var ps = new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/ProjectSettings.asset"));
        // var inputProps = ps.FindProperty("enableNativePlatformBackendsForNewInputSystem");
        // inputProps.boolValue = true;
        // ps.ApplyModifiedProperties();
    }

    private static void CreateLayer(string name)
    {
        var tagManager = new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);
        var layerProps = tagManager.FindProperty("layers");
        var propCount = layerProps.arraySize;

        SerializedProperty firstEmptyProp = null;
 
        for (var i = 0; i < propCount; i++)
        {
            var layerProp = layerProps.GetArrayElementAtIndex(i);
 
            var stringValue = layerProp.stringValue;
 
            if (stringValue == name) return;
 
            if (i < 8 || stringValue != string.Empty) continue;
 
            if (firstEmptyProp == null)
                firstEmptyProp = layerProp;
        }
 
        if (firstEmptyProp == null)
        {
            Debug.LogError("Maximum limit of " + propCount + " layers exceeded. Layer \"" + name + "\" not created.");
            return;
        }

        firstEmptyProp.stringValue = name;
        tagManager.ApplyModifiedProperties();
    }

    private static void CreateTag(string name)
    {
        var tagManager = new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);
        var tagsProps = tagManager.FindProperty("tags");
        
        var found = false;
        
        for (var i = 0; i < tagsProps.arraySize; i++)
        {
            var property = tagsProps.GetArrayElementAtIndex(i);
            if (property.stringValue.Equals(name))
            {
                found = true;
                break;
            }
        }
        
        if (!found)
        {
            tagsProps.InsertArrayElementAtIndex(tagsProps.arraySize);
            var newTag = tagsProps.GetArrayElementAtIndex(tagsProps.arraySize - 1);
            newTag.stringValue = name;
        }


        tagManager.ApplyModifiedProperties();
    }
}
