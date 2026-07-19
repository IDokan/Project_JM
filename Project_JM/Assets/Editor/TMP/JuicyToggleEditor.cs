// SPDX-License-Identifier: LicenseRef-Proprietary
// Copyright (c) 07/04/2026 Sinil Kang. All Rights Reserved.
// Project: Project JM - https://github.com/IDokan/Project_JM
// File: JuicyToggleEditor.cs
// Summary: Custom editor for JuicyToggle; appends Juicy fields below Toggle's built-in inspector.
// Unauthorized copying, distribution, or modification of this file is strictly prohibited.

using UnityEditor;
using UnityEditor.UI;

[CustomEditor(typeof(JuicyToggle), true)]
[CanEditMultipleObjects]
public class JuicyToggleEditor : ToggleEditor
{
    private SerializedProperty _checkmark;
    private SerializedProperty _background;
    private SerializedProperty _style;

    protected override void OnEnable()
    {
        base.OnEnable();
        _checkmark  = serializedObject.FindProperty("checkmark");
        _background = serializedObject.FindProperty("background");
        _style      = serializedObject.FindProperty("style");
    }

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Juicy", EditorStyles.boldLabel);

        serializedObject.Update();
        EditorGUILayout.PropertyField(_checkmark);
        EditorGUILayout.PropertyField(_background);
        EditorGUILayout.PropertyField(_style);
        serializedObject.ApplyModifiedProperties();
    }
}
