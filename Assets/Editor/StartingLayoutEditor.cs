using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(StartingBoardLayout))]
public class StartingLayoutEditor : Editor
{
    SerializedProperty displayName;
    SerializedProperty forwardLeft;
    SerializedProperty forwardRight;
    SerializedProperty forward;
    SerializedProperty backLeft;
    SerializedProperty back;
    SerializedProperty backRight;
    SerializedProperty right;
    SerializedProperty left;

    private void OnEnable()
    {
        displayName = serializedObject.FindProperty("displayName");
        forwardLeft = serializedObject.FindProperty("forwardLeft");
        forwardRight = serializedObject.FindProperty("forwardRight");
        forward = serializedObject.FindProperty("forward");
        backLeft = serializedObject.FindProperty("backLeft");
        back = serializedObject.FindProperty("back");
        backRight = serializedObject.FindProperty("backRight");
        right = serializedObject.FindProperty("right");
        left = serializedObject.FindProperty("left");
    }

    public override void OnInspectorGUI()
    {
        //base.OnInspectorGUI();
        serializedObject.Update();
        GUI.enabled = false;
        EditorGUIUtility.labelWidth = 90;
        EditorGUILayout.ObjectField("Script:", MonoScript.FromScriptableObject((ScriptableObject)target), typeof(ScriptableObject), false);
        GUI.enabled = true;
        EditorGUILayout.PropertyField(displayName);
        setupProperty(forwardLeft, "Forward Left");
        setupProperty(forward, "Forward");
        setupProperty(forwardRight, "Forward Right");
        setupProperty(left, "Left");
        setupProperty(right, "Right");
        setupProperty(backLeft, "Back Left");
        setupProperty(back, "Back");
        setupProperty(backRight, "Back Right");
        serializedObject.ApplyModifiedProperties();
    }

    private void setupProperty(SerializedProperty prop, string propName)
    {
        EditorGUIUtility.labelWidth = 90;
        EditorGUILayout.BeginHorizontal();
        prop.FindPropertyRelative("layout").objectReferenceValue = (TileLayout) EditorGUILayout.ObjectField(propName, prop.FindPropertyRelative("layout").objectReferenceValue, typeof(TileLayout), true);
        EditorGUIUtility.labelWidth = 50;
        prop.FindPropertyRelative("rotations").intValue = EditorGUILayout.IntField("Rot", prop.FindPropertyRelative("rotations").intValue, GUILayout.ExpandWidth(false));
        EditorGUILayout.EndHorizontal();
    }
}
