// SPDX-License-Identifier: LicenseRef-Proprietary
// Copyright (c) 10/07/2026 Sinil Kang. All Rights Reserved.
// Project: Project JM - https://github.com/IDokan/Project_JM
// File: TutorialStepSpawner.cs
// Summary: Editor menu commands (JM/Tutorials/Easy|Medium|Hard/StepN) that instantiate a single TutorialStepData asset's sprite entries as real Image GameObjects under Canvas/UIManager/TutorialUIOverlay, for visually authoring tutorial overlays in the open scene.
// Unauthorized copying, distribution, or modification of this file is strictly prohibited.

using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

public static class TutorialStepSpawner
{
    private const string ParentPath = "Canvas/UIManager/TutorialUIOverlay";
    private const string EasyFolder = "Assets/Resources/TutorialSequences/EasySteps/";
    private const string MediumFolder = "Assets/Resources/TutorialSequences/MediumSteps/";
    private const string HardFolder = "Assets/Resources/TutorialSequences/HardSteps/";

    [MenuItem("JM/Tutorials/Easy/Step 00 - Hello")]
    private static void SpawnEasyStep00() => SpawnStepAtPath(EasyFolder + "EasyStep00-Hello.asset");

    [MenuItem("JM/Tutorials/Easy/Step 01 - Village")]
    private static void SpawnEasyStep01() => SpawnStepAtPath(EasyFolder + "EasyStep01-Village.asset");

    [MenuItem("JM/Tutorials/Easy/Step 02 - Enemy")]
    private static void SpawnEasyStep02() => SpawnStepAtPath(EasyFolder + "EasyStep02-Enemy.asset");

    [MenuItem("JM/Tutorials/Easy/Step 03")]
    private static void SpawnEasyStep03() => SpawnStepAtPath(EasyFolder + "EasyStep03.asset");

    [MenuItem("JM/Tutorials/Easy/Step 04 - MatchAction-BlueGemUp")]
    private static void SpawnEasyStep04() => SpawnStepAtPath(EasyFolder + "EasyStep04 MatchAction-BlueGemUp.asset");

    [MenuItem("JM/Tutorials/Easy/Step 05 - Applause")]
    private static void SpawnEasyStep05() => SpawnStepAtPath(EasyFolder + "EasyStep05 Applause.asset");

    [MenuItem("JM/Tutorials/Easy/Step 06 - GemPowerQuestion")]
    private static void SpawnEasyStep06() => SpawnStepAtPath(EasyFolder + "EasyStep06-GemPowerQuestion.asset");

    [MenuItem("JM/Tutorials/Easy/Step 07 - Let's See Gem Board")]
    private static void SpawnEasyStep07() => SpawnStepAtPath(EasyFolder + "EasyStep07-Let'sSeeGemBoard.asset");

    [MenuItem("JM/Tutorials/Easy/Step 08 - SurprisedAfterDelay")]
    private static void SpawnEasyStep08() => SpawnStepAtPath(EasyFolder + "EasyStep08-SurprisedAfterDelay.asset");

    [MenuItem("JM/Tutorials/Easy/Step 09 - GemPowerExplain")]
    private static void SpawnEasyStep09() => SpawnStepAtPath(EasyFolder + "EasyStep09-GemPowerExplain.asset");

    [MenuItem("JM/Tutorials/Easy/Step 10 - BrightJewelAndEnemy")]
    private static void SpawnEasyStep10() => SpawnStepAtPath(EasyFolder + "EasyStep10-BrightJewelAndEnemy.asset");

    [MenuItem("JM/Tutorials/Easy/Step 11 - Let's See Enemy")]
    private static void SpawnEasyStep11() => SpawnStepAtPath(EasyFolder + "EasyStep11-Let'sSeeEnemy.asset");

    [MenuItem("JM/Tutorials/Easy/Step 12 - ApplaudeAfterAttack")]
    private static void SpawnEasyStep12() => SpawnStepAtPath(EasyFolder + "EasyStep12-ApplaudeAfterAttack.asset");

    [MenuItem("JM/Tutorials/Easy/Step 13 - Go Fight!")]
    private static void SpawnEasyStep13() => SpawnStepAtPath(EasyFolder + "EasyStep13-GoFight!.asset");

    [MenuItem("JM/Tutorials/Medium/Step 00 - Hello")]
    private static void SpawnMediumStep00() => SpawnStepAtPath(MediumFolder + "MediumStep00-Hello.asset");

    [MenuItem("JM/Tutorials/Medium/Step 01 - What's Purples")]
    private static void SpawnMediumStep01() => SpawnStepAtPath(MediumFolder + "MediumStep01-What'sPurples.asset");

    [MenuItem("JM/Tutorials/Medium/Step 02 - Let's See Gem Board")]
    private static void SpawnMediumStep02() => SpawnStepAtPath(MediumFolder + "MediumStep02-Let'sSeeGemBoard.asset");

    [MenuItem("JM/Tutorials/Medium/Step 03 - SurprisedByGemDisabled")]
    private static void SpawnMediumStep03() => SpawnStepAtPath(MediumFolder + "MediumStep03-SurprisedByGemDisabled.asset");

    [MenuItem("JM/Tutorials/Medium/Step 04 - DisabledGemsByEnemyAttack")]
    private static void SpawnMediumStep04() => SpawnStepAtPath(MediumFolder + "MediumStep04-DisabledGemsByEnemyAttack.asset");

    [MenuItem("JM/Tutorials/Medium/Step 05 - BoardAction-PurpleDown")]
    private static void SpawnMediumStep05() => SpawnStepAtPath(MediumFolder + "MediumStep05-BoardAction-PurpleDown.asset");

    [MenuItem("JM/Tutorials/Medium/Step 06 - Applause")]
    private static void SpawnMediumStep06() => SpawnStepAtPath(MediumFolder + "MediumStep06-Applause.asset");

    [MenuItem("JM/Tutorials/Medium/Step 07 - BoardExplanation")]
    private static void SpawnMediumStep07() => SpawnStepAtPath(MediumFolder + "MediumStep07-BoardExplanation.asset");

    [MenuItem("JM/Tutorials/Medium/Step 08 - PurpleFirst")]
    private static void SpawnMediumStep08() => SpawnStepAtPath(MediumFolder + "MediumStep08-PurpleFirst.asset");

    [MenuItem("JM/Tutorials/Medium/Step 09 - Attack")]
    private static void SpawnMediumStep09() => SpawnStepAtPath(MediumFolder + "MediumStep09-Attack.asset");

    [MenuItem("JM/Tutorials/Hard/Step 0 - Applause")]
    private static void SpawnHardStep0() => SpawnStepAtPath(HardFolder + "HardStep0 - Applause.asset");

    [MenuItem("JM/Tutorials/Hard/Step 1 - Graduation")]
    private static void SpawnHardStep1() => SpawnStepAtPath(HardFolder + "HardStep1 - Graduation.asset");

    [MenuItem("JM/Tutorials/Hard/Step 2 - StrongEnemy")]
    private static void SpawnHardStep2() => SpawnStepAtPath(HardFolder + "HardStep2 - StrongEnemy.asset");

    [MenuItem("JM/Tutorials/Hard/Step 3 - Go Fight!")]
    private static void SpawnHardStep3() => SpawnStepAtPath(HardFolder + "HardStep3-GoFight!.asset");

    private static void SpawnStepAtPath(string assetPath)
    {
        GameObject parent = GameObject.Find(ParentPath);
        if (parent == null)
        {
            Debug.LogError($"[TutorialStepSpawner] Could not find '{ParentPath}' in the open scene.");
            return;
        }

        TutorialStepData step = AssetDatabase.LoadAssetAtPath<TutorialStepData>(assetPath);
        if (step == null)
        {
            Debug.LogError($"[TutorialStepSpawner] Could not load TutorialStepData at '{assetPath}'.");
            return;
        }

        int imageCount = SpawnStepEntries(parent.transform, step);
        EditorSceneManager.MarkSceneDirty(parent.scene);
        Debug.Log($"[TutorialStepSpawner] Spawned {imageCount} images for '{step.name}' under '{ParentPath}'.");
    }

    private static int SpawnStepEntries(Transform parent, TutorialStepData step)
    {

        foreach (TutorialSpriteEntry entry in step.DialogueSprites)
        {
            SpawnEntry(parent, entry, "TutorialDialogueSprite");
        }


        foreach (TutorialSpriteEntry entry in step.Sprites)
        {
            SpawnEntry(parent, entry, "TutorialSprite");
        }

        return step.Sprites.Count + step.DialogueSprites.Count;
    }

    private static void SpawnEntry(Transform parent, TutorialSpriteEntry entry, string name)
    {
        GameObject go = new GameObject(name);
        Undo.RegisterCreatedObjectUndo(go, "Spawn Tutorial Sprite");
        go.transform.SetParent(parent, false);

        Image img = go.AddComponent<Image>();
        img.sprite = entry.sprite;
        img.SetNativeSize();
        img.raycastTarget = false;

        RectTransform rt = img.rectTransform;
        rt.anchorMin = entry.AnchorMin;
        rt.anchorMax = entry.AnchorMax;
        rt.anchoredPosition = entry.AnchoredPosition;
        rt.localScale = new Vector3(entry.Scale.x, entry.Scale.y, 1f);
    }
}
