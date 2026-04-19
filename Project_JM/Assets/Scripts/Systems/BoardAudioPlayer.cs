// SPDX-License-Identifier: LicenseRef-Proprietary
// Copyright (c) 17/04/2026 Sinil Kang. All Rights Reserved.
// Project: Project JM - https://github.com/IDokan/Project_JM
// File: BoardAudioPlayer.cs
// Summary: Plays audio cues for board events such as gem hint shaking and none-gem matches.
// Unauthorized copying, distribution, or modification of this file is strictly prohibited.

using UnityEngine;

public class BoardAudioPlayer : MonoBehaviour
{
    [SerializeField] private AudioCueSO gemHintCue;
    [SerializeField] private AudioCueSO noneMatchCue;

    public void PlayGemHint()
    {
        AudioManager.Instance.PlayPuzzleSFX(gemHintCue);
    }

    public void PlayNoneMatch()
    {
        AudioManager.Instance.PlayPuzzleSFX(noneMatchCue);
    }
}
