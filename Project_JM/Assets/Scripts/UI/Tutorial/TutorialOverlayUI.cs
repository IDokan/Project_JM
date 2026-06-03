// SPDX-License-Identifier: LicenseRef-Proprietary
// Copyright (c) 03/06/2026 Sinil Kang. All Rights Reserved.
// Project: Project JM - https://github.com/IDokan/Project_JM
// File: TutorialOverlayUI.cs
// Summary: Renders tutorial overlay sprites with per-entry DOTween animations; captures confirm input for input steps.
// Unauthorized copying, distribution, or modification of this file is strictly prohibited.

using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class TutorialOverlayUI : MonoBehaviour
{
    [SerializeField] private RectTransform container;

    private readonly List<Image> _activeImages = new();
    private readonly List<Tween> _idleTweens = new();
    private TutorialStepData _currentStep;
    private bool _confirmed;
    private bool _waitingForConfirm;

    public bool TryConfirm()
    {
        if (!_waitingForConfirm)
        {
            return false;
        }

        _confirmed = true;
        return true;
    }

    public IEnumerator WaitForConfirm()
    {
        _confirmed = false;
        _waitingForConfirm = true;
        yield return new WaitUntil(() => _confirmed);
        _waitingForConfirm = false;
    }

    public IEnumerator ShowStep(TutorialStepData step)
    {
        _currentStep = step;
        ClearAll();

        if (step.Sprites.Count == 0)
        {
            yield break;
        }

        Sequence showSeq = DOTween.Sequence();
        bool hasAnyTween = false;

        foreach (TutorialSpriteEntry entry in step.Sprites)
        {
            Image img = CreateSpriteImage(entry);
            _activeImages.Add(img);

            Tween t = BuildShowTween(img, entry);
            if (t != null)
            {
                showSeq.Join(t);
                hasAnyTween = true;
            }
        }

        if (hasAnyTween)
        {
            yield return showSeq.WaitForCompletion();
        }
        else
        {
            showSeq.Kill();
        }

        for (int i = 0; i < _activeImages.Count; i++)
        {
            Tween idle = BuildIdleTween(_activeImages[i], step.Sprites[i]);
            if (idle != null)
            {
                _idleTweens.Add(idle);
            }
        }
    }

    public IEnumerator HideStep()
    {
        KillIdleTweens();

        if (_currentStep == null || _currentStep.Sprites.Count == 0 || _activeImages.Count == 0)
        {
            ClearAll();
            yield break;
        }

        Sequence hideSeq = DOTween.Sequence();
        bool hasAnyTween = false;

        for (int i = 0; i < _activeImages.Count; i++)
        {
            if (i >= _currentStep.Sprites.Count)
            {
                break;
            }

            Tween t = BuildHideTween(_activeImages[i], _currentStep.Sprites[i]);
            if (t != null)
            {
                hideSeq.Join(t);
                hasAnyTween = true;
            }
        }

        if (hasAnyTween)
        {
            yield return hideSeq.WaitForCompletion();
        }
        else
        {
            hideSeq.Kill();
        }

        ClearAll();
    }

    private Image CreateSpriteImage(TutorialSpriteEntry entry)
    {
        GameObject go = new GameObject("TutorialSprite");
        go.transform.SetParent(container, false);

        Image img = go.AddComponent<Image>();
        img.sprite = entry.sprite;
        img.SetNativeSize();
        img.raycastTarget = false;

        RectTransform rt = img.rectTransform;
        rt.anchoredPosition = entry.anchoredPosition;
        rt.localScale = new Vector3(entry.scale.x, entry.scale.y, 1f);

        return img;
    }

    private Tween BuildShowTween(Image img, TutorialSpriteEntry entry)
    {
        TutorialSpriteAnim anim = entry.showAnim;
        switch (anim.type)
        {
            case TutorialAnimType.Fade:
                img.color = new Color(1f, 1f, 1f, 0f);
                return img.DOFade(1f, anim.duration).SetEase(anim.ease);

            case TutorialAnimType.SlideFrom:
                img.rectTransform.anchoredPosition = entry.anchoredPosition + anim.offset;
                return img.rectTransform.DOAnchorPos(entry.anchoredPosition, anim.duration).SetEase(anim.ease);

            case TutorialAnimType.Scale:
                img.rectTransform.localScale = new Vector3(anim.targetValue, anim.targetValue, 1f);
                return img.rectTransform.DOScale(new Vector3(entry.scale.x, entry.scale.y, 1f), anim.duration).SetEase(anim.ease);

            default:
                return null;
        }
    }

    private Tween BuildHideTween(Image img, TutorialSpriteEntry entry)
    {
        TutorialSpriteAnim anim = entry.hideAnim;
        switch (anim.type)
        {
            case TutorialAnimType.Fade:
                return img.DOFade(0f, anim.duration).SetEase(anim.ease);

            case TutorialAnimType.SlideFrom:
                return img.rectTransform.DOAnchorPos(entry.anchoredPosition + anim.offset, anim.duration).SetEase(anim.ease);

            case TutorialAnimType.Scale:
                return img.rectTransform.DOScale(anim.targetValue, anim.duration).SetEase(anim.ease);

            default:
                return null;
        }
    }

    private Tween BuildIdleTween(Image img, TutorialSpriteEntry entry)
    {
        TutorialSpriteAnim anim = entry.idleAnim;

        switch (anim.type)
        {
            case TutorialAnimType.PulseScale:
                return img.rectTransform
                    .DOScale(anim.targetValue, anim.duration)
                    .SetEase(anim.ease)
                    .SetLoops(-1, LoopType.Yoyo);

            case TutorialAnimType.PingPongFade:
                return img.DOFade(anim.targetValue, anim.duration)
                    .SetEase(anim.ease)
                    .SetLoops(-1, LoopType.Yoyo);

            default:
                return null;
        }
    }

    private void KillIdleTweens()
    {
        foreach (Tween t in _idleTweens)
        {
            t?.Kill();
        }
        _idleTweens.Clear();
    }

    private void ClearAll()
    {
        KillIdleTweens();
        foreach (Image img in _activeImages)
        {
            if (img != null)
            {
                Destroy(img.gameObject);
            }
        }
        _activeImages.Clear();
    }
}
