using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using TMPro;
using System;

public enum CostTextAniType
{
    NONE = 0,
    YELLOW,
    RED,
    GREEN,
    BLUE,
    ORANGE,
}

enum TweenSceneMode
{
    MAIN = 0,
    FIGHT,
}

public enum AnimationTweenType
{
    TAKE_DAMAGE = 0,
    DEAD,
    FADE_IN,
    FADE_OUT,
    BOSS_DEAD,
    ROCK_BOSS_DEAD,
}

public enum AnimationTweenOwner
{
    HERO = 0,
    MINION,
    WEAPON,
}

public class TweenHelper : MonoBehaviour
{
    public static TweenHelper Inst;

    string heCol = "_HitEffectColor";
    string heBlend = "_HitEffectBlend";
    string alphaStr = "_Alpha";

    [Header("TakeDamage")]
    Ease takeDamageEase = Ease.InCirc;
    Color takeDamageColor = new Color(0.8f, 0f, 0f, 1f);

    [Header("Dead")]
    float deadBlend = 0.9f;
    Color idleColor = new Color(1f, 1f, 1f, 1f), deadColor = new Color(0.8f, 0f, 0f, 1f);

    float deadDuration = 0.3f;
    float alphaDuration = 0.2f;
    float takeDamageBlend = 0.9f;
    float takeDamageDuration = 0.275f;

    Dictionary<GameObject, Sequence> takeDamageDic = new Dictionary<GameObject, Sequence>();
    Dictionary<GameObject, Sequence> deadDic = new Dictionary<GameObject, Sequence>();
    Dictionary<GameObject, Sequence> fadeInDic = new Dictionary<GameObject, Sequence>();
    Dictionary<GameObject, Sequence> fadeOutDic = new Dictionary<GameObject, Sequence>();
    Dictionary<GameObject, Sequence> bossDeadDic = new Dictionary<GameObject, Sequence>();
    Dictionary<GameObject, Sequence> rockBossDeadDic = new Dictionary<GameObject, Sequence>();

    public void ClearAllSequence()
    {
        takeDamageDic.Clear();
    }

    public void UpgradeCallback(RectTransform iconRect, RectTransform upgradeButtonRect)
    {
        // UIParticleManager.Inst.SpawnParticle(UIParticleIndex.EQUIP_UPGRADE, iconRect);
        PunchScaleButton(iconRect);
        PunchScaleButton(upgradeButtonRect);
    }

    float sequenceDelay = 0.001f;

    public void GenerateTween(AnimationTweenType tweenType, GameObject target, SpriteRenderer targetRenderer, Transform subObj = null, float duration = 3f)
    {
        var sequence = DOTween.Sequence();
        if (tweenType == AnimationTweenType.TAKE_DAMAGE)
        {
            if (takeDamageDic.ContainsKey(target))
                return;
            sequence.OnStart(() =>
            {
                DoColor(targetRenderer, takeDamageColor);
                DoFloat(targetRenderer, takeDamageBlend);
                SetAlpha(targetRenderer, 1f);
            });
            sequence.Insert(sequenceDelay, targetRenderer.sharedMaterial.DOFloat(0f, heBlend, takeDamageDuration).SetEase(takeDamageEase));
            sequence.Join(targetRenderer.sharedMaterial.DOColor(idleColor, heCol, takeDamageDuration).SetEase(takeDamageEase));
            sequence.OnComplete(() =>
            {
                DoFloat(targetRenderer, 0f);
            });
        }
        else if (tweenType == AnimationTweenType.DEAD)
        {
            if (deadDic.ContainsKey(target))
                return;
            sequence.OnStart(() =>
            {
                DoColor(targetRenderer, idleColor);
                DoFloat(targetRenderer, 0f);
                SetAlpha(targetRenderer, 1f);
            });
            sequence.Insert(sequenceDelay, targetRenderer.sharedMaterial.DOFloat(deadBlend, heBlend, deadDuration).SetEase(takeDamageEase));
            sequence.Join(targetRenderer.sharedMaterial.DOColor(deadColor, heCol, deadDuration).SetEase(takeDamageEase));
        }
        else if (tweenType == AnimationTweenType.FADE_IN)
        {
            if (fadeInDic.ContainsKey(target))
                return;
            sequence.OnStart(() =>
            {
                DoColor(targetRenderer, idleColor);
                DoFloat(targetRenderer, 0f);
                SetAlpha(targetRenderer, 0f);
            });
            sequence.Insert(sequenceDelay, targetRenderer.sharedMaterial.DOFloat(1f, alphaStr, alphaDuration));
        }
        else if (tweenType == AnimationTweenType.FADE_OUT)
        {
            if (fadeOutDic.ContainsKey(target))
                return;
            sequence.OnStart(() =>
            {
                DoColor(targetRenderer, idleColor);
                DoFloat(targetRenderer, 0f);
                SetAlpha(targetRenderer, 1f);
            });
            sequence.Insert(sequenceDelay, targetRenderer.sharedMaterial.DOFloat(0f, alphaStr, alphaDuration));
        }
        else if (tweenType == AnimationTweenType.BOSS_DEAD)
        {
            if (bossDeadDic.ContainsKey(target))
                return;
            sequence.OnStart(() =>
            {
                DoColor(targetRenderer, idleColor);
                DoFloat(targetRenderer, 0f);
                SetAlpha(targetRenderer, 1f);
            });
            sequence.Insert(sequenceDelay, targetRenderer.sharedMaterial.DOFloat(takeDamageBlend, heBlend, duration));
            sequence.Insert(sequenceDelay, targetRenderer.sharedMaterial.DOColor(takeDamageColor, heCol, duration));
            sequence.Insert(sequenceDelay, subObj.DOShakePosition(duration, new Vector3(0.6f, 0f, 0f), 15, 0, false, false));
        }
        else if (tweenType == AnimationTweenType.ROCK_BOSS_DEAD)
        {
            if (rockBossDeadDic.ContainsKey(target))
                return;
            sequence.OnStart(() =>
            {
                DoColor(targetRenderer, idleColor);
                DoFloat(targetRenderer, 0f);
                SetAlpha(targetRenderer, 1f);
            });
            sequence.Insert(sequenceDelay, targetRenderer.sharedMaterial.DOFloat(takeDamageBlend, heBlend, duration));
            sequence.Insert(sequenceDelay, targetRenderer.sharedMaterial.DOColor(takeDamageColor, heCol, duration));
            sequence.Insert(sequenceDelay, subObj.DOShakePosition(duration, new Vector3(0.1f, 0f, 0f), 15, 0, false, false));
        }

        sequence.SetAutoKill(false);
        sequence.SetRecyclable(true);
        sequence.Pause();

        if (tweenType == AnimationTweenType.TAKE_DAMAGE)
            takeDamageDic.Add(target, sequence);
        else if (tweenType == AnimationTweenType.DEAD)
            deadDic.Add(target, sequence);
        else if (tweenType == AnimationTweenType.FADE_IN)
            fadeInDic.Add(target, sequence);
        else if (tweenType == AnimationTweenType.FADE_OUT)
            fadeOutDic.Add(target, sequence);
        else if (tweenType == AnimationTweenType.BOSS_DEAD)
            bossDeadDic.Add(target, sequence);
        else if (tweenType == AnimationTweenType.ROCK_BOSS_DEAD)
            rockBossDeadDic.Add(target, sequence);
    }

    public void DoClear(SpriteRenderer targetRenderer)
    {
        DoColor(targetRenderer, idleColor);
        DoFloat(targetRenderer, 0f);
    }

    private void Update()
    {
        // float spd = TimeManager.Inst.GetTweenSpeed();
        // foreach (var item in takeDamageDic)
        //     item.Value.timeScale = spd;
    }

    public void DoTakeDamage(GameObject target)
    {
        var sequence = takeDamageDic[target];
        sequence.Rewind();
        sequence.Restart();
    }

    public void DoDead(GameObject target)
    {
        var sequence = deadDic[target];
        sequence.Rewind();
        sequence.Restart();
    }

    public void DoFadeIn(GameObject target)
    {
        var sequence = fadeInDic[target];
        sequence.Rewind();
        sequence.Restart();
    }

    public void DoFadeOut(GameObject target)
    {
        var sequence = fadeOutDic[target];
        sequence.Rewind();
        sequence.Restart();
    }

    public void DoBossDead(GameObject target)
    {
        var sequence = bossDeadDic[target];
        sequence.Rewind();
        sequence.Restart();
    }

    public void DoRockBossDead(GameObject target)
    {
        var sequence = rockBossDeadDic[target];
        sequence.Rewind();
        sequence.Restart();
    }

    public bool HasTween(AnimationTweenType tweenType, GameObject target)
    {
        switch (tweenType)
        {
            case AnimationTweenType.TAKE_DAMAGE:
                break;
            case AnimationTweenType.DEAD:
                break;
            case AnimationTweenType.FADE_IN:
                break;
            case AnimationTweenType.FADE_OUT:
                break;
            case AnimationTweenType.BOSS_DEAD:
                return bossDeadDic.ContainsKey(target);
            case AnimationTweenType.ROCK_BOSS_DEAD:
                return rockBossDeadDic.ContainsKey(target);
        }
        return false;
    }

    public void ClearTween(AnimationTweenType tweenType, GameObject target)
    {
        switch (tweenType)
        {
            case AnimationTweenType.TAKE_DAMAGE:
                break;
            case AnimationTweenType.DEAD:
                break;
            case AnimationTweenType.FADE_IN:
                break;
            case AnimationTweenType.FADE_OUT:
                break;
            case AnimationTweenType.BOSS_DEAD:
                if (HasTween(tweenType, target))
                    bossDeadDic.Remove(target);
                break;
            case AnimationTweenType.ROCK_BOSS_DEAD:
                if (HasTween(tweenType, target))
                    rockBossDeadDic.Remove(target);
                break;
        }
    }

    public void StopTween(AnimationTweenType tweenType, GameObject target)
    {
        if (tweenType == AnimationTweenType.TAKE_DAMAGE)
        {
            var sequence = takeDamageDic[target];
            sequence.Rewind();
            sequence.Pause();
        }
        else if (tweenType == AnimationTweenType.DEAD)
        {
            var sequence = deadDic[target];
            sequence.Rewind();
            sequence.Pause();
        }
    }

    void DoFloat(SpriteRenderer targetRenderer, float targetAmount)
    {
        targetRenderer.sharedMaterial.SetFloat(heBlend, targetAmount);
    }

    void DoColor(SpriteRenderer targetRenderer, Color targetCol)
    {
        targetRenderer.sharedMaterial.SetColor(heCol, targetCol);
    }

    void SetAlpha(SpriteRenderer targetRenderer, float alpha)
    {
        targetRenderer.sharedMaterial.SetFloat(alphaStr, alpha);
    }

    public void StopAllTween()
    {
        DOTween.KillAll(false);
    }

    private void Awake()
    {
        Inst = this;
        DontDestroyOnLoad(gameObject);
    }

    public void SetHpGage(Image flash, Image progress, TextMeshProUGUI text, int currentHp, int startHp)
    {
        float per = (float)currentHp / (float)startHp;
        progress.DOKill();
        progress.fillAmount = per;

        flash.DOKill();
        flash.DOFillAmount(per, 0.3f).SetEase(Ease.OutSine);

        if (text != null)
            text.text = string.Format("{0}", currentHp);
    }

    public void DoCostAnimation(TextMeshProUGUI targetText, float t, CostTextAniType aniType)
    {
        targetText.DOKill();
        targetText.color = GetAnimationColor(aniType);
        targetText.DOColor(Color.white, t);
    }

    public void DoTickColor(TextMeshPro targetText, float duration, CostTextAniType aniType)
    {
        targetText.DOKill();
        targetText.color = Color.white;
        var targetCol = GetAnimationColor(aniType);
        targetText.DOColor(targetCol, duration / 2f).SetLoops(2, LoopType.Yoyo).SetEase(Ease.InOutSine);
    }

    public void DoColor(SlicedFilledImage target, float duration, Color targetColor)
    {
        target.DOKill();
        target.DOColor(targetColor, duration).SetEase(Ease.Linear);
    }

    public void DoColor(Image target, float duration, Color targetColor)
    {
        target.DOKill();
        target.DOColor(targetColor, duration).SetEase(Ease.Linear);
    }

    public void DoColor(TextMeshProUGUI target, float duration, Color targetColor)
    {
        target.DOKill();
        target.DOColor(targetColor, duration).SetEase(Ease.Linear);
    }

    public void DoColorFade(Image targetImage, float duration, Color startColor)
    {
        targetImage.DOKill();
        targetImage.color = startColor;
        targetImage.DOFade(0f, duration).SetEase(Ease.Linear);
    }

    public void SetBackgroundFade(Image target, bool updateTime = false, bool end1 = false)
    {
        target.DOKill();
        target.color = Color.clear;
        target.DOFade(end1 ? 1f : Enums.BackgroundAlpha, 0.4f).SetEase(Ease.OutQuad).SetUpdate(updateTime);
    }

    public void SetBackgroundFadeWhite(Image target, bool updateTime = false, bool end1 = false)
    {
        target.DOKill();
        target.color = ColorBox.Inst.whiteClear;
        target.DOFade(end1 ? 1f : Enums.BackgroundAlpha, 0.4f).SetEase(Ease.OutQuad).SetUpdate(updateTime);
    }

    public void SetPanelEnableBox(RectTransform target, bool updateTime = false)
    {
        target.DOKill();
        target.localScale = Vector3.one * 1.025f;
        target.DOScale(Vector3.one, 0.4f).SetDelay(0.1f).SetUpdate(updateTime);
    }

    public void SetPanelEnableBox(RectTransform target, float startScale, float duration, float delay, Ease ease)
    {
        target.DOKill();
        target.localScale = Vector3.one * startScale;
        target.DOScale(Vector3.one, duration).SetDelay(delay).SetEase(ease);
    }

    public Color GetAnimationColor(CostTextAniType type)
    {
        switch (type)
        {
            case CostTextAniType.NONE:
                return Color.white;
            case CostTextAniType.YELLOW:
                return Color.yellow;
            case CostTextAniType.RED:
                return Color.red;
            case CostTextAniType.GREEN:
                return Color.green;
            case CostTextAniType.BLUE:
                return Color.blue;
            case CostTextAniType.ORANGE:
                return new Color(255, 165, 0);
        }
        return Color.yellow;
    }

    public void DoFade(SpriteRenderer target, bool tru, float time, Ease ease, Action callback = null)
    {
        target.DOKill();
        if (tru)
        {
            Color startCol = target.color;
            startCol.a = 0f;
            target.color = startCol;

            target.DOFade(1f, time).SetEase(ease).OnComplete(() =>
            {
                if (callback != null)
                    callback();
            });
        }
        else
        {
            target.DOFade(0f, time).SetEase(ease).OnComplete(() =>
            {
                if (callback != null)
                    callback();
            });
        }
    }

    public void DoFade(TextMeshProUGUI target, bool tru, float time, Ease ease, Action callback = null)
    {
        target.DOKill();
        if (tru)
        {
            Color startCol = target.color;
            startCol.a = 0f;
            target.color = startCol;

            target.DOFade(1f, time).SetEase(ease).OnComplete(() =>
            {
                if (callback != null)
                    callback();
            });
        }
        else
        {
            target.DOFade(0f, time).SetEase(ease).OnComplete(() =>
            {
                if (callback != null)
                    callback();
            });
        }
    }

    public void DoFade(Image target, bool tru, float time, Ease ease, Action callback = null)
    {
        target.DOKill();
        if (tru)
        {
            Color startCol = target.color;
            startCol.a = 0f;
            target.color = startCol;
            target.DOFade(1f, time).SetEase(ease).OnComplete(() =>
            {
                if (callback != null)
                    callback();
            });
        }
        else
        {
            target.DOFade(0f, time).SetEase(ease).OnComplete(() =>
            {
                if (callback != null)
                    callback();
            });
        }
    }

    public void FadeBackgroundImage(Image target, bool tru, float time, float fadeAmount, Ease ease, Action callback = null)
    {
        target.DOKill();
        if (tru)
        {
            target.color = Color.clear;
            target.DOFade(fadeAmount, time).SetEase(ease).OnComplete(() =>
            {
                if (callback != null)
                    callback();
            });
        }
        else
        {
            target.DOFade(0f, time).SetEase(ease).OnComplete(() =>
            {
                if (callback != null)
                    callback();
            });
        }
    }

    public void DoRotateAnimation(RectTransform target)
    {
        target.DOKill();
        target.localRotation = Quaternion.Euler(0f, 0f, 0f);
        target.DOLocalRotate(Vector3.forward * 10f, 0.2f, RotateMode.Fast).SetLoops(-1, LoopType.Incremental);
    }

    public void ShakePosition(Transform target, float strength, int vibrato, float time)
    {
        target.DOKill();
        target.DOShakePosition(time, strength, vibrato, 90f, false, false);
    }

    public void FlashColor(TextMeshProUGUI target, float time, Color startColor, Color endColor, Ease ease = Ease.Linear)
    {
        target.DOKill();
        target.color = startColor;
        target.DOColor(endColor, time).SetEase(ease);
    }

    public void FlashColor(Image target, float time, Color startColor, Ease ease)
    {
        target.DOKill();
        target.color = startColor;
        target.DOFade(0f, time).SetEase(ease);
    }

    public void BounceObject(Transform target, float time, Vector2 targetScale, Ease ease)
    {
        target.DOKill();
        target.localScale = targetScale;
        target.DOScale(Vector3.one, time).SetEase(ease);
    }

    public void PunchScaleButton(RectTransform target, float punchT = 1f)
    {
        target.DOKill();
        target.localScale = Vector3.one;
        target.DOPunchScale(Vector3.one * 0.125f, punchT, 10, 1);
    }

    public void FlashImageAlpha(Image target, float flashTime, float delayTime, bool doEnable = false, float flashAlpha = 1f)
    {
        bool doTargetEnable = doEnable;
        if (doTargetEnable)
            target.enabled = true;

        target.DOKill();
        target.color = new Color(1f, 1f, 1f, flashAlpha);
        target.DOFade(0f, flashTime).SetDelay(delayTime).SetEase(Ease.OutSine).OnComplete(() =>
        {
            if (doTargetEnable)
                target.enabled = false;
        });
    }

    public void FlashImage(Image target, float flashTime, float delayTime, bool doEnable = false, System.Action callback = null)
    {
        bool doTargetEnable = doEnable;
        if (doTargetEnable)
            target.enabled = true;

        target.DOKill();
        target.color = Color.white;
        target.DOFade(0f, flashTime).SetDelay(delayTime).SetEase(Ease.OutSine).OnComplete(() =>
        {
            if (doTargetEnable)
                target.enabled = false;
            if (callback != null)
                callback();
        });
    }

    public Tweener Float(float from, float to, float duration, TweenCallback onVirtualUpdate)
    {
        float val = from;
        return DOTween.To(() => val, x => val = x, to, duration).OnUpdate(onVirtualUpdate);
    }


}