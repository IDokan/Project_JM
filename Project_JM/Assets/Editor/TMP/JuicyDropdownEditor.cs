// SPDX-License-Identifier: LicenseRef-Proprietary
// Copyright (c) 07/04/2026 Sinil Kang. All Rights Reserved.
// Project: Project JM - https://github.com/IDokan/Project_JM
// File: JuicyDropdownEditor.cs
// Summary: Custom editor for JuicyDropdown; appends Juicy fields below TMP Dropdown's built-in inspector.
// Unauthorized copying, distribution, or modification of this file is strictly prohibited.

using TMPro.EditorUtilities;
using UnityEditor;

[CustomEditor(typeof(JuicyDropdown), true)]
[CanEditMultipleObjects]
public class JuicyDropdownEditor : DropdownEditor
{
    private SerializedProperty _background;
    private SerializedProperty _style;

    protected override void OnEnable()
    {
        base.OnEnable();
        _background = serializedObject.FindProperty("background");
        _style      = serializedObject.FindProperty("style");
    }

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Juicy", EditorStyles.boldLabel);

        serializedObject.Update();
        EditorGUILayout.PropertyField(_background);
        EditorGUILayout.PropertyField(_style);
        serializedObject.ApplyModifiedProperties();
    }
}
