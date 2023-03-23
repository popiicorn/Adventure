using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(UnityLibrary.OpenAI))]
public class CharacterEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        UnityLibrary.OpenAI myScript = (UnityLibrary.OpenAI)target;
        if (GUILayout.Button("生成"))
        {
            myScript.Execute();
        }
    }
}