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
    [SerializeField] private RectTransform fullScreenRoot;
    private RectTransform _brightZone;
    [SerializeField] private List<RectTransform> brightZones;
    [SerializeField] private Image highlightImage;
    [SerializeField] private Color backdropColor = new Color(0f, 0f, 0f, 0.5f);
    [SerializeField] private float backdropFadeDuration = 0.3f;

    private readonly List<Image> _activeImages = new();
    private readonly List<Tween> _idleTweens = new();
    private readonly List<Image> _activeDialogueImages = new();
    private readonly List<Tween> _dialogueIdleTweens = new();
    private readonly List<TutorialSpriteEntry> _currentDialogueEntries = new();
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

        if (step.Sprites.Count == 0 && step.DialogueSprites.Count == 0)
        {
            yield break;
        }

        if (step.DialogueSprites.Count > 0)
        {
            Sequence dialogueShowSeq = DOTween.Sequence().SetUpdate(true);
            bool hasDialogueShow = false;

            foreach (TutorialSpriteEntry entry in step.DialogueSprites)
            {
                Image img = CreateSpriteImage(entry);
                _activeDialogueImages.Add(img);
                _currentDialogueEntries.Add(entry);

                Tween t = BuildShowTween(img, entry);
                if (t != null) { dialogueShowSeq.Join(t); hasDialogueShow = true; }
            }

            if (hasDialogueShow) { yield return dialogueShowSeq.WaitForCompletion(); }
            else { dialogueShowSeq.Kill(); }

            for (int i = 0; i < _activeDialogueImages.Count; i++)
            {
                _dialogueIdleTweens.Add(BuildIdleTween(_activeDialogueImages[i], _currentDialogueEntries[i]));
            }
        }

        if (step.Sprites.Count > 0)
        {
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
                _idleTweens.Add(BuildIdleTween(_activeImages[i], step.Sprites[i]));
            }
        }
    }

    public IEnumerator TransitionStep(TutorialStepData nextStep)
    {
        int currentCount = _currentStep != null ? _currentStep.Sprites.Count : 0;
        int nextCount = nextStep.Sprites.Count;
        int sharedCount = Mathf.Min(currentCount, nextCount);

        List<Image> toDestroy = new List<Image>();
        Sequence hideSeq = DOTween.Sequence().SetUpdate(true);
        bool hasHide = false;

        // Regular sprites: kill idle tweens and hide only changed/removed; kept sprites' idle tweens stay running
        for (int i = 0; i < sharedCount; i++)
        {
            if (_currentStep.Sprites[i].sprite != nextStep.Sprites[i].sprite)
            {
                if (i < _idleTweens.Count) { _idleTweens[i]?.Kill(); }
                Tween hide = BuildHideTween(_activeImages[i], _currentStep.Sprites[i]);
                if (hide != null) { hideSeq.Join(hide); hasHide = true; }
                toDestroy.Add(_activeImages[i]);
            }
        }
        for (int i = sharedCount; i < currentCount; i++)
        {
            // No need to check they are the same sprites because index is greater than one of them.
            if (i < _idleTweens.Count) { _idleTweens[i]?.Kill(); }
            Tween hide = BuildHideTween(_activeImages[i], _currentStep.Sprites[i]);
            if (hide != null) { hideSeq.Join(hide); hasHide = true; }
            toDestroy.Add(_activeImages[i]);
        }

        // Dialogue sprites: hide those not present in next step
        IReadOnlyList<TutorialSpriteEntry> nextDialogue = nextStep.DialogueSprites;
        List<int> dialogueRemove = new List<int>();
        for (int i = 0; i < _currentDialogueEntries.Count; i++)
        {
            bool foundInNext = false;
            // Is current entry in among the next step's entries?
            foreach (TutorialSpriteEntry e in nextDialogue)
            {
                if (e.sprite == _currentDialogueEntries[i].sprite) { foundInNext = true; break; }
            }
            if (!foundInNext)
            {
                dialogueRemove.Add(i);
                _dialogueIdleTweens[i]?.Kill();
                Tween hide = BuildHideTween(_activeDialogueImages[i], _currentDialogueEntries[i]);
                if (hide != null) { hideSeq.Join(hide); hasHide = true; }
                toDestroy.Add(_activeDialogueImages[i]);
            }
        }

        // hasHide ==> Any item includes sprites and dialogue sprites needs Hide Tween?
        if (hasHide) { yield return hideSeq.WaitForCompletion(); }
        else { hideSeq.Kill(); }

        // Delayed destroy
        foreach (Image img in toDestroy)
        {
            if (img != null) { Destroy(img.gameObject); }
        }

        // Regular sprites: build next list, show new/changed; carry kept images and their idle tweens
        List<Image> nextImages = new List<Image>();
        List<Tween> nextIdleTweens = new List<Tween>();
        Sequence showSeq = DOTween.Sequence().SetUpdate(true);
        bool hasShow = false;

        for (int i = 0; i < sharedCount; i++)
        {
            if (_currentStep.Sprites[i].sprite == nextStep.Sprites[i].sprite)
            {
                nextImages.Add(_activeImages[i]);
                if (_currentStep.Sprites[i].idleAnim.type != nextStep.Sprites[i].idleAnim.type)
                {
                    if (i < _idleTweens.Count) { _idleTweens[i]?.Kill(); }
                    ResetImageToEntry(_activeImages[i], nextStep.Sprites[i]);
                    nextIdleTweens.Add(null);
                }
                else
                {
                    nextIdleTweens.Add(i < _idleTweens.Count ? _idleTweens[i] : null);
                }
            }
            else
            {
                Image newImg = CreateSpriteImage(nextStep.Sprites[i]);
                nextImages.Add(newImg);
                nextIdleTweens.Add(null);
                Tween show = BuildShowTween(newImg, nextStep.Sprites[i]);
                if (show != null) { showSeq.Join(show); hasShow = true; }
            }
        }
        for (int i = sharedCount; i < nextCount; i++)
        {
            Image newImg = CreateSpriteImage(nextStep.Sprites[i]);
            nextImages.Add(newImg);
            nextIdleTweens.Add(null);
            Tween show = BuildShowTween(newImg, nextStep.Sprites[i]);
            if (show != null) { showSeq.Join(show); hasShow = true; }
        }

        // Dialogue sprites: create those not already showing
        HashSet<int> removedSet = new HashSet<int>(dialogueRemove);
        Dictionary<int, Image> createdDialogue = new Dictionary<int, Image>();
        for (int j = 0; j < nextDialogue.Count; j++)
        {
            bool alreadyShowing = false;
            foreach (TutorialSpriteEntry e in _currentDialogueEntries)
            {
                if (e.sprite == nextDialogue[j].sprite) { alreadyShowing = true; break; }
            }
            if (!alreadyShowing)
            {
                Image newImg = CreateSpriteImage(nextDialogue[j]);
                createdDialogue[j] = newImg;
                Tween show = BuildShowTween(newImg, nextDialogue[j]);
                if (show != null) { showSeq.Join(show); hasShow = true; }
            }
        }

        if (hasShow) { yield return showSeq.WaitForCompletion(); }
        else { showSeq.Kill(); }

        // Regular sprites: finalize; build idle tweens for new sprites, carry kept ones
        _activeImages.Clear();
        _activeImages.AddRange(nextImages);
        _idleTweens.Clear();
        _currentStep = nextStep;

        for (int i = 0; i < _activeImages.Count; i++)
        {
            // ?? is the null-coalescing operator - it returns the left side if it's not null, otherwise the right side.
            _idleTweens.Add(nextIdleTweens[i] ?? BuildIdleTween(_activeImages[i], nextStep.Sprites[i]));
        }

        // Dialogue sprites: rebuild parallel lists in next-step order
        List<Image> nextDialogueImages = new List<Image>();
        List<Tween> nextDialogueTweens = new List<Tween>();
        List<TutorialSpriteEntry> nextDialogueEntries = new List<TutorialSpriteEntry>();

        for (int j = 0; j < nextDialogue.Count; j++)
        {
            // index that represents where nextDialogue[j] is in _currentDialogueEntries.
            // if it is -1, there is no duplicated entry in next dialogue.
            int keptOldIdx = -1;
            for (int i = 0; i < _currentDialogueEntries.Count; i++)
            {
                if (!removedSet.Contains(i) && _currentDialogueEntries[i].sprite == nextDialogue[j].sprite)
                {
                    keptOldIdx = i;
                    break;
                }
            }

            if (keptOldIdx >= 0)
            {
                nextDialogueImages.Add(_activeDialogueImages[keptOldIdx]);
                nextDialogueEntries.Add(_currentDialogueEntries[keptOldIdx]);
                if (_currentDialogueEntries[keptOldIdx].idleAnim.type != nextDialogue[j].idleAnim.type)
                {
                    _dialogueIdleTweens[keptOldIdx]?.Kill();
                    ResetImageToEntry(_activeDialogueImages[keptOldIdx], nextDialogue[j]);
                    nextDialogueTweens.Add(BuildIdleTween(_activeDialogueImages[keptOldIdx], nextDialogue[j]));
                }
                else
                {
                    nextDialogueTweens.Add(_dialogueIdleTweens[keptOldIdx]);
                }
            }
            else
            {
                Image newImg = createdDialogue[j];
                nextDialogueImages.Add(newImg);
                nextDialogueTweens.Add(BuildIdleTween(newImg, nextDialogue[j]));
                nextDialogueEntries.Add(nextDialogue[j]);
            }
        }

        _activeDialogueImages.Clear();
        _activeDialogueImages.AddRange(nextDialogueImages);
        _dialogueIdleTweens.Clear();
        _dialogueIdleTweens.AddRange(nextDialogueTweens);
        _currentDialogueEntries.Clear();
        _currentDialogueEntries.AddRange(nextDialogueEntries);
    }

    public IEnumerator HideStep()
    {
        KillIdleTweens();
        KillDialogueIdleTweens();

        bool hasRegular = _currentStep != null && _currentStep.Sprites.Count > 0 && _activeImages.Count > 0;
        bool hasDialogue = _activeDialogueImages.Count > 0;

        if (!hasRegular && !hasDialogue)
        {
            ClearAll();
            _currentStep = null;
            yield break;
        }

        Sequence hideSeq = DOTween.Sequence().SetUpdate(true);
        bool hasAnyTween = false;

        if (hasRegular)
        {
            for (int i = 0; i < _activeImages.Count; i++)
            {
                if (i >= _currentStep.Sprites.Count) { break; }
                Tween t = BuildHideTween(_activeImages[i], _currentStep.Sprites[i]);
                if (t != null) { hideSeq.Join(t); hasAnyTween = true; }
            }
        }

        for (int i = 0; i < _activeDialogueImages.Count; i++)
        {
            Tween t = BuildHideTween(_activeDialogueImages[i], _currentDialogueEntries[i]);
            if (t != null) { hideSeq.Join(t); hasAnyTween = true; }
        }

        if (hasAnyTween) { yield return hideSeq.WaitForCompletion(); }
        else { hideSeq.Kill(); }

        ClearAll();
        _currentStep = null;
    }

    public void SetBrightZoneImmediate(int index)
    {
        RectTransform zone = (index >= 0 && index < brightZones.Count) ? brightZones[index] : null;

        if (zone != null && zone.TryGetComponent(out TutorialWorldTargetPositioner positioner))
        {
            positioner.SyncToWorldTarget();
        }

        if (zone == _brightZone)
        {
            return;
        }

        _brightZone = zone;
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
        _brightZone = null;
        UpdateHighlightSprite();
    }

    private void UpdateHighlightSprite()
    {
        if (highlightImage == null)
        {
            return;
        }

        if (_brightZone == null)
        {
            highlightImage.gameObject.SetActive(false);
            return;
        }

        highlightImage.gameObject.SetActive(true);
        highlightImage.rectTransform.position = _brightZone.position;
        highlightImage.rectTransform.localScale = new Vector3(
            _brightZone.rect.width / 100f,
            _brightZone.rect.height / 100f,
            1f
        );
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

        if (_brightZone == null)
        {
            Image panel = CreateBackdropPanel(color);
            panel.rectTransform.anchorMin = Vector2.zero;
            panel.rectTransform.anchorMax = Vector2.one;
            panel.rectTransform.offsetMin = Vector2.zero;
            panel.rectTransform.offsetMax = Vector2.zero;
            _backdropPanels.Add(panel);
            UpdateHighlightSprite();
            return;
        }

        GetNormalizedBoundsInParent(_brightZone, fullScreenRoot, out Vector2 nMin, out Vector2 nMax);

        TryAddBackdropPanel(color, new Vector2(0f, nMax.y), new Vector2(1f, 1f));          // top
        TryAddBackdropPanel(color, new Vector2(0f, 0f),     new Vector2(1f, nMin.y));       // bottom
        TryAddBackdropPanel(color, new Vector2(0f, nMin.y), new Vector2(nMin.x, nMax.y));   // left
        TryAddBackdropPanel(color, new Vector2(nMax.x, nMin.y), new Vector2(1f, nMax.y));   // right

        UpdateHighlightSprite();
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
        go.transform.SetParent(fullScreenRoot, false);
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
        rt.anchorMin = entry.AnchorMin;
        rt.anchorMax = entry.AnchorMax;
        rt.anchoredPosition = entry.AnchoredPosition;
        rt.localScale = new Vector3(entry.Scale.x, entry.Scale.y, 1f);

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
                img.rectTransform.anchoredPosition = entry.AnchoredPosition + anim.offset;
                return img.rectTransform.DOAnchorPos(entry.AnchoredPosition, anim.duration).SetEase(anim.ease);

            case TutorialAnimType.Scale:
                img.rectTransform.localScale = new Vector3(anim.targetValue, anim.targetValue, 1f);
                return img.rectTransform.DOScale(new Vector3(entry.Scale.x, entry.Scale.y, 1f), anim.duration).SetEase(anim.ease);

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
                return img.rectTransform.DOAnchorPos(entry.AnchoredPosition + anim.offset, anim.duration).SetEase(anim.ease);

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
                    .DOAnchorPos(entry.AnchoredPosition + anim.offset, anim.duration)
                    .SetEase(anim.ease)
                    .SetLoops(-1, LoopType.Yoyo)
                    .SetUpdate(true);

            default:
                return null;
        }
    }

    private void ResetImageToEntry(Image img, TutorialSpriteEntry entry)
    {
        img.rectTransform.anchorMin = entry.AnchorMin;
        img.rectTransform.anchorMax = entry.AnchorMax;
        img.rectTransform.anchoredPosition = entry.AnchoredPosition;
        img.rectTransform.localScale = new Vector3(entry.Scale.x, entry.Scale.y, 1f);
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

    private void KillDialogueIdleTweens()
    {
        foreach (Tween t in _dialogueIdleTweens)
        {
            t?.Kill();
        }
        _dialogueIdleTweens.Clear();
    }

    private void ClearAll()
    {
        KillIdleTweens();
        foreach (Image img in _activeImages)
        {
            if (img != null) { Destroy(img.gameObject); }
        }
        _activeImages.Clear();

        KillDialogueIdleTweens();
        foreach (Image img in _activeDialogueImages)
        {
            if (img != null) { Destroy(img.gameObject); }
        }
        _activeDialogueImages.Clear();
        _currentDialogueEntries.Clear();
    }
}
