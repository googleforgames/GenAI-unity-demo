using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace GercStudio.USK.Scripts
{
    [CustomEditor(typeof(InteractionWithUSKCharcaters))]
    public class InteractionWithUSKCharactersEditor : Editor
    {
        public InteractionWithUSKCharcaters script;
        
        private GUIStyle grayBackground;
        private GUIStyle style;
        
        public void Awake()
        {
            script = (InteractionWithUSKCharcaters) target;
        }

        public override void OnInspectorGUI()
        {
            // DrawDefaultInspector();
        }
    }
}
