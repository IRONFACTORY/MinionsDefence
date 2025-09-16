using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;

public class ItemText : MonoBehaviour
{
    RectTransform myRect;
    public RectTransform TextBoxRect;
    public TextMeshProUGUI myText;

    bool isEnable;

    Ease fadeEase = Ease.OutSine;
    Ease upperEase = Ease.Linear;

    float normalOffsetY = 25f;

    float aniT = 0.4f;
    float firstAniT = 0.2f;
    float delay = 0.2f;

    int NormalSize = 34;

    float xOffset = 0.25f;

    public Image itemImage;

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
        smallSequence = GenerateSequence(normalOffsetY);
    }

    Sequence GenerateSequence(float targetY)
    {
        var newTarget = DOTween.Sequence();
        newTarget.OnRewind(() =>
        {
            TextBoxRect.anchoredPosition3D = Vector3.zero;
        });
        newTarget.Append(TextBoxRect.DOScale(Vector3.one, firstAniT));
        newTarget.SetDelay(delay);
        newTarget.Append(myText.DOFade(0f, firstAniT).SetDelay(aniT).SetEase(fadeEase));
        newTarget.Insert(firstAniT, TextBoxRect.DOAnchorPosY(TextBoxRect.anchoredPosition.y + targetY, aniT).SetEase(upperEase));
        newTarget.OnComplete(() => { SetDisable(); });
        newTarget.SetAutoKill(false);
        newTarget.SetRecyclable(true);
        newTarget.Pause();
        return newTarget;
    }

    void DoEnable(Vector2 startScale, Vector2 spawnPos, float offsetX = 0f)
    {
        gameObject.SetActive(true);
        this.spawnPos = spawnPos;
        isEnable = true;

        myText.fontSize = NormalSize;
        myText.characterSpacing = 0f;
        TextBoxRect.localScale = startScale;
        UpdatePosition();
        DoSequence();
    }

    void SetText(TMP_ColorGradient color, string format, int amount)
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


    void DoSequence()
    {
        smallSequence.Rewind();
        smallSequence.Restart();
    }

    public void SetEnable(Vector2 targetPos, IngameItemType itemType, int amount)
    {
        SetText(ColorBox.Inst.ItemGradient, "", amount);
        spawnPos = targetPos;
        DoEnable(Vector3.one * 1.3f, spawnPos);
    }

    public void SetDisable()
    {
        isEnable = false;
        smallSequence.Rewind();
        gameObject.SetActive(false);
    }

    private void LateUpdate()
    {
        if (smallSequence != null)
            smallSequence.timeScale = TimeManager.Inst.MasterTime();
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
