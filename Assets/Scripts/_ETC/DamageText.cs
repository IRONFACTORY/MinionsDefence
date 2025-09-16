using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;

public class DamageText : MonoBehaviour
{
    RectTransform myRect;
    public RectTransform TextBoxRect;
    public TextMeshProUGUI myText;

    bool isEnable;

    Ease fadeEase = Ease.OutSine;
    Ease upperEase = Ease.Linear;

    float normalOffsetY = 25f;
    float smallOffsetY = 15f;

    float aniT = 0.4f;
    float firstAniT = 0.2f;
    float BigDelay = 0.1f;

    int NormalSize = 34;
    int smallSize = 26;

    float xOffset = 0.25f;

    public Image criticalImage;

    Sequence sequence;
    Sequence smallSequence;

    Vector3 textPos = new Vector3(0f, 0.75f, 1f);

    Vector2 spawnPos;


    public void SetParent(RectTransform parent)
    {
        myRect.SetParent(parent);
        myRect.SetAsLastSibling();
    }

    public void Initialize()
    {
        smallSequence = GenerateSequence(smallOffsetY);
        sequence = GenerateSequence(normalOffsetY);
    }

    Sequence GenerateSequence(float targetY)
    {
        var newTarget = DOTween.Sequence();
        newTarget.OnRewind(() =>
        {
            TextBoxRect.anchoredPosition3D = Vector3.zero;
        });
        newTarget.Append(TextBoxRect.DOScale(Vector3.one, firstAniT));
        newTarget.SetDelay(BigDelay);
        newTarget.Append(myText.DOFade(0f, firstAniT).SetDelay(aniT).SetEase(fadeEase));
        newTarget.Insert(firstAniT, TextBoxRect.DOAnchorPosY(TextBoxRect.anchoredPosition.y + targetY, aniT).SetEase(upperEase));
        newTarget.OnComplete(() => { SetDisable(); });
        newTarget.SetAutoKill(false);
        newTarget.SetRecyclable(true);
        newTarget.Pause();
        return newTarget;
    }

    void DoEnable(bool isSmall, Vector2 startScale, Vector2 spawnPos, float offsetX = 0f)
    {
        gameObject.SetActive(true);
        this.spawnPos = spawnPos;
        isEnable = true;

        myText.fontSize = isSmall ? smallSize : NormalSize;
        myText.characterSpacing = isSmall ? -8f : 0f;
        TextBoxRect.localScale = startScale;
        UpdatePosition();
        DoSequence(isSmall);
    }

    void SetText(TMP_ColorGradient color, string format, long amount)
    {
        myText.colorGradientPreset = color;
        myText.color = Color.white;

        if (amount == 0)
        {
            myText.text = format;
            return;
        }
        myText.text = string.Format(format, amount);
    }

    void SetText(TMP_ColorGradient color, string format, float amount)
    {
        myText.colorGradientPreset = color;
        myText.color = Color.white;

        if (Enums.Approximately(amount, 0f))
        {
            myText.text = format;
            return;
        }
        myText.text = string.Format(format, amount.ToString("N0"));
    }

    void DoSequence(bool isSmall)
    {
        if (isSmall)
        {
            smallSequence.Rewind();
            smallSequence.Restart();
        }
        else
        {
            sequence.Rewind();
            sequence.Restart();
        }
    }

    public void SetEnable(Vector2 targetPos, long Damage, DamageType dmgType, bool isCrit)
    {
        criticalImage.enabled = isCrit;
        SetText(ColorBox.Inst.GetDamageTextGradient(dmgType), "" + Damage.ToString("N0"), 0);
        spawnPos = targetPos;
        DoEnable(false, isCrit ? Vector3.one * 1.75f : Vector3.one * 1.3f, spawnPos);
    }

    public void SetDisable()
    {
        isEnable = false;
        smallSequence.Rewind();
        sequence.Rewind();
        gameObject.SetActive(false);
    }

    private void LateUpdate()
    {
        if (smallSequence != null)
            smallSequence.timeScale = TimeManager.Inst.MasterTime();
        if (sequence != null)
            sequence.timeScale = TimeManager.Inst.MasterTime();

    }

    void UpdatePosition()
    {
        GetRect().anchoredPosition = GetTargetHoldPos();
    }

    Vector2 GetTargetHoldPos()
    {
        return Enums.GetTargetHoldPos(spawnPos + (Vector2)textPos);
    }

    public bool IsEnable()
    {
        return isEnable;
    }

    public RectTransform GetRect()
    {
        if (myRect == null)
            myRect = GetComponent<RectTransform>();
        return myRect;
    }

}