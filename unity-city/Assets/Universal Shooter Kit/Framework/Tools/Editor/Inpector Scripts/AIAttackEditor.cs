using UnityEngine;
using UnityEditor;

namespace GercStudio.USK.Scripts
{

    [CustomEditor(typeof(AIAttack))]
    public class AIAttackEditor : Editor
    {

        public AIAttack script;

        public void Awake()
        {
            script = (AIAttack) target;
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            EditorGUILayout.HelpBox("You can adjust all parameters in the AI Controller script.", MessageType.Info);
            serializedObject.ApplyModifiedProperties();
       
        }
    }

}


