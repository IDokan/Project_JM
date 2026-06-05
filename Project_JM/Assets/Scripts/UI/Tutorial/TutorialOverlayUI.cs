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
    [SerializeField] private RectTransform brightZone;
    [SerializeField] private List<RectTransform> brightZones;
    [SerializeField] private Color backdropColor = new Color(0f, 0f, 0f, 0.5f);
    [SerializeField] private float backdropFadeDuration = 0.3f;

    private readonly List<Image> _activeImages = new();
    private readonly List<Tween> _idleTweens = new();
    private readonly List<Image> _backdropPanels = new();
    private TutorialStepData _currentStep;
    private bool _confirmed;
    private bool _waitingForConfirm;

    public bool IsWaitingForConfirm => _waitingForConfirm;
    public bool IsSequenceActive { get; private set; }

    public void SetSequenceActive(bool active) => IsSequenceActive = active;

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

        Sequence showSeq = DOTween.Sequence().SetUpdate(true);
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

    public IEnumerator TransitionStep(TutorialStepData nextStep)
    {
        KillIdleTweens();

        int currentCount = _currentStep != null ? _currentStep.Sprites.Count : 0;
        int nextCount = nextStep.Sprites.Count;
        int sharedCount = Mathf.Min(currentCount, nextCount);

        // Hide sprites that are being replaced or removed
        List<Image> toDestroy = new List<Image>();
        Sequence hideSeq = DOTween.Sequence().SetUpdate(true);
        bool hasHide = false;

        for (int i = 0; i < sharedCount; i++)
        {
            if (_currentStep.Sprites[i].sprite != nextStep.Sprites[i].sprite)
            {
                Tween hide = BuildHideTween(_activeImages[i], _currentStep.Sprites[i]);
                if (hide != null) { hideSeq.Join(hide); hasHide = true; }
                toDestroy.Add(_activeImages[i]);
            }
        }
        for (int i = sharedCount; i < currentCount; i++)
        {
            Tween hide = BuildHideTween(_activeImages[i], _currentStep.Sprites[i]);
            if (hide != null) { hideSeq.Join(hide); hasHide = true; }
            toDestroy.Add(_activeImages[i]);
        }

        if (hasHide) { yield return hideSeq.WaitForCompletion(); }
        else { hideSeq.Kill(); }

        foreach (Image img in toDestroy)
        {
            if (img != null) { Destroy(img.gameObject); }
        }

        // Build next image list; create and show new sprites
        List<Image> nextImages = new List<Image>();
        Sequence showSeq = DOTween.Sequence().SetUpdate(true);
        bool hasShow = false;

        for (int i = 0; i < sharedCount; i++)
        {
            if (_currentStep.Sprites[i].sprite == nextStep.Sprites[i].sprite)
            {
                ResetImageToEntry(_activeImages[i], nextStep.Sprites[i]);
                nextImages.Add(_activeImages[i]);
            }
            else
            {
                Image newImg = CreateSpriteImage(nextStep.Sprites[i]);
                nextImages.Add(newImg);
                Tween show = BuildShowTween(newImg, nextStep.Sprites[i]);
                if (show != null) { showSeq.Join(show); hasShow = true; }
            }
        }
        for (int i = sharedCount; i < nextCount; i++)
        {
            Image newImg = CreateSpriteImage(nextStep.Sprites[i]);
            nextImages.Add(newImg);
            Tween show = BuildShowTween(newImg, nextStep.Sprites[i]);
            if (show != null) { showSeq.Join(show); hasShow = true; }
        }

        if (hasShow) { yield return showSeq.WaitForCompletion(); }
        else { showSeq.Kill(); }

        _activeImages.Clear();
        _activeImages.AddRange(nextImages);
        _currentStep = nextStep;

        for (int i = 0; i < _activeImages.Count; i++)
        {
            Tween idle = BuildIdleTween(_activeImages[i], nextStep.Sprites[i]);
            if (idle != null) { _idleTweens.Add(idle); }
        }
    }

    public IEnumerator HideStep()
    {
        KillIdleTweens();

        if (_currentStep == null || _currentStep.Sprites.Count == 0 || _activeImages.Count == 0)
        {
            ClearAll();
            _currentStep = null;
            yield break;
        }

        Sequence hideSeq = DOTween.Sequence().SetUpdate(true);
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
        _currentStep = null;
    }

    public void SetBrightZoneImmediate(int index)
    {
        RectTransform zone = (index >= 0 && index < brightZones.Count) ? brightZones[index] : null;
        if (zone == brightZone)
        {
            return;
        }

        brightZone = zone;
        if (_backdropPanels.Count == 0)
        {
            return;
        }

        foreach (Image panel in _backdropPanels)
        {
            if (panel != null)
            {
                Destroy(panel.gameObject);
            }
        }
        _backdropPanels.Clear();
        BuildBackdropPanels(backdropColor.a);
    }

    public IEnumerator ShowBackdrop()
    {
        BuildBackdropPanels(0f);

        if (_backdropPanels.Count == 0)
        {
            yield break;
        }

        Sequence seq = DOTween.Sequence().SetUpdate(true);
        foreach (Image panel in _backdropPanels)
        {
            seq.Join(panel.DOFade(backdropColor.a, backdropFadeDuration));
        }
        yield return seq.WaitForCompletion();
    }

    public IEnumerator HideBackdrop()
    {
        if (_backdropPanels.Count == 0)
        {
            yield break;
        }

        Sequence seq = DOTween.Sequence().SetUpdate(true);
        foreach (Image panel in _backdropPanels)
        {
            seq.Join(panel.DOFade(0f, backdropFadeDuration));
        }
        yield return seq.WaitForCompletion();

        foreach (Image panel in _backdropPanels)
        {
            if (panel != null)
            {
                Destroy(panel.gameObject);
            }
        }
        _backdropPanels.Clear();
    }

    private void BuildBackdropPanels(float alpha)
    {
        foreach (Image panel in _backdropPanels)
        {
            if (panel != null)
            {
                Destroy(panel.gameObject);
            }
        }
        _backdropPanels.Clear();

        Color color = new Color(backdropColor.r, backdropColor.g, backdropColor.b, alpha);

        if (brightZone == null)
        {
            Image panel = CreateBackdropPanel(color);
            panel.rectTransform.anchorMin = Vector2.zero;
            panel.rectTransform.anchorMax = Vector2.one;
            panel.rectTransform.offsetMin = Vector2.zero;
            panel.rectTransform.offsetMax = Vector2.zero;
            _backdropPanels.Add(panel);
            return;
        }

        GetNormalizedBoundsInParent(brightZone, container, out Vector2 nMin, out Vector2 nMax);

        TryAddBackdropPanel(color, new Vector2(0f, nMax.y), new Vector2(1f, 1f));          // top
        TryAddBackdropPanel(color, new Vector2(0f, 0f),     new Vector2(1f, nMin.y));       // bottom
        TryAddBackdropPanel(color, new Vector2(0f, nMin.y), new Vector2(nMin.x, nMax.y));   // left
        TryAddBackdropPanel(color, new Vector2(nMax.x, nMin.y), new Vector2(1f, nMax.y));   // right
    }

    private void TryAddBackdropPanel(Color color, Vector2 anchorMin, Vector2 anchorMax)
    {
        if (anchorMax.x <= anchorMin.x || anchorMax.y <= anchorMin.y)
        {
            return;
        }

        Image panel = CreateBackdropPanel(color);
        panel.rectTransform.anchorMin = anchorMin;
        panel.rectTransform.anchorMax = anchorMax;
        panel.rectTransform.offsetMin = Vector2.zero;
        panel.rectTransform.offsetMax = Vector2.zero;
        _backdropPanels.Add(panel);
    }

    private Image CreateBackdropPanel(Color color)
    {
        GameObject go = new GameObject("BackdropPanel");
        go.transform.SetParent(container, false);
        go.transform.SetAsFirstSibling();
        Image img = go.AddComponent<Image>();
        img.color = color;
        img.raycastTarget = true;
        return img;
    }

    private void GetNormalizedBoundsInParent(RectTransform rt, RectTransform parent,
        out Vector2 normMin, out Vector2 normMax)
    {
        Vector3[] rtCorners = new Vector3[4];
        rt.GetWorldCorners(rtCorners);

        Vector3[] parentCorners = new Vector3[4];
        parent.GetWorldCorners(parentCorners);

        float parentW = parentCorners[2].x - parentCorners[0].x;
        float parentH = parentCorners[2].y - parentCorners[0].y;

        normMin = new Vector2(
            (rtCorners[0].x - parentCorners[0].x) / parentW,
            (rtCorners[0].y - parentCorners[0].y) / parentH
        );
        normMax = new Vector2(
            (rtCorners[2].x - parentCorners[0].x) / parentW,
            (rtCorners[2].y - parentCorners[0].y) / parentH
        );
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
                    .SetLoops(-1, LoopType.Yoyo)
                    .SetUpdate(true);

            case TutorialAnimType.PingPongFade:
                return img.DOFade(anim.targetValue, anim.duration)
                    .SetEase(anim.ease)
                    .SetLoops(-1, LoopType.Yoyo)
                    .SetUpdate(true);

            case TutorialAnimType.Rotate:
                return img.rectTransform
                    .DOLocalRotate(new Vector3(0f, 0f, anim.targetValue), anim.duration)
                    .SetEase(anim.ease)
                    .SetLoops(-1, LoopType.Yoyo)
                    .SetUpdate(true);

            case TutorialAnimType.PingPongSlide:
                return img.rectTransform
                    .DOAnchorPos(entry.anchoredPosition + anim.offset, anim.duration)
                    .SetEase(anim.ease)
                    .SetLoops(-1, LoopType.Yoyo)
                    .SetUpdate(true);

            default:
                return null;
        }
    }

    private void ResetImageToEntry(Image img, TutorialSpriteEntry entry)
    {
        img.rectTransform.anchoredPosition = entry.anchoredPosition;
        img.rectTransform.localScale = new Vector3(entry.scale.x, entry.scale.y, 1f);
        img.rectTransform.localRotation = Quaternion.identity;
        img.color = new Color(img.color.r, img.color.g, img.color.b, 1f);
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
