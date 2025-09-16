using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;

public enum CostBoxAnimationLevel { NONE = 0, UPPER, LOWER, }

public class MainCostBox : MonoBehaviour
{
    CostType costType;
    public TextMeshProUGUI CostText;
    public Image CostIconImage;
    long lastestTargetCost;
    RectTransform myRect;

    public void Initialize(CostType costType)
    {
        this.costType = costType;
        CostIconImage.sprite = SpriteBox.Inst.GetCostIcon(costType);
        UpdateCostText(CostBoxAnimationLevel.NONE);
        myRect = GetComponent<RectTransform>();
    }

    public void UpdateCostText(CostBoxAnimationLevel ani, int amount = 0, bool directionUp = true, bool spawnHoverText = true)
    {
        lastestTargetCost = GetCost();
        string str = TextHelper.Inst.LongConvertString(lastestTargetCost);
        CostText.text = lastestTargetCost.ToString();

        if (ani == CostBoxAnimationLevel.NONE)
            return;

        if (ani == CostBoxAnimationLevel.UPPER)
        {
            // if (spawnHoverText)
            //     HoverTextManager.Inst.SpawnCostText(CostText.rectTransform, CostTextAniType.YELLOW, false, directionUp, true, costType, amount);
            TweenHelper.Inst.DoCostAnimation(CostText, 0.5f, CostTextAniType.YELLOW);
            CostBounce();
        }
        else
        {
            // if (spawnHoverText)
            //     HoverTextManager.Inst.SpawnCostText(CostText.rectTransform, CostTextAniType.RED, false, directionUp, false, costType, amount);
            TweenHelper.Inst.DoCostAnimation(CostText, 0.5f, CostTextAniType.RED);
        }
    }

    void CostBounce()
    {
        CostIconImage.transform.DOKill();
        CostIconImage.transform.localScale = Vector3.one * 1.1f;
        CostIconImage.transform.DOScale(Vector3.one, 0.3f);
    }

    long GetCost() => DataManager.Inst.GetCost(costType);
    public Vector2 GetAnchorPos() => Enums.RectTransformToScreenPos(GetComponent<RectTransform>());

}