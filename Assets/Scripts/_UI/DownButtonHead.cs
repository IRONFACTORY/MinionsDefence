using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public enum DownButtonType { HERO = 0, TOWER, STAGE, PASSIVE, SHOP }

public class DownButtonHead : MonoBehaviour
{
    DownButtonType currentButtonType;
    public DownButtons[] downButtons;

    Dictionary<DownButtonType, DownButtons> downButtonDic = new Dictionary<DownButtonType, DownButtons>();
    public RectTransform[] downButtonPanels;

    public RectTransform panelHead, DownButtonHeadRect;

    float popupPanelDisableY = 120f;
    float screenY;
    float enableT = 0f, disableT = 0f;
    float[] enablesY;

    public void Initialize()
    {
        for (int i = 0; i < downButtons.Length; i++)
        {
            DownButtonType targetType = (DownButtonType)i;
            downButtons[i].Initialize(targetType);
            downButtonDic.Add(targetType, downButtons[i]);
        }

        screenY = MyCanvas.Inst.GetScreenY();

        int length = System.Enum.GetValues(typeof(DownButtonType)).Length;
        enablesY = new float[length];
        for (int i = 0; i < enablesY.Length; i++)
            enablesY[i] = downButtonPanels[i].sizeDelta.y;
    }

    public float GetSafeZoneBottom() => Screen.safeArea.yMin > 1f ? 60 : 0;

    public void SetToggle(DownButtonType targetType)
    {
        currentButtonType = targetType;
        for (int i = 0; i < downButtons.Length; i++)
            downButtons[i].SetToggled(downButtons[i].GetDownButtonType() == targetType);

        for (int i = 0; i < downButtonPanels.Length; i++)
        {
            bool ThisIs = i == (int)targetType;
            downButtonPanels[i].gameObject.SetActive(ThisIs);

            if (ThisIs)
            {
                switch (targetType)
                {
                    case DownButtonType.HERO:
                        UIManager.Inst.GetHeroPanel().SetEnable();
                        break;
                    case DownButtonType.TOWER:
                        UIManager.Inst.GetTowerPanel().SetEnable();
                        break;
                    case DownButtonType.STAGE:
                        UIManager.Inst.GetStagePanel().SetEnable();
                        break;
                    case DownButtonType.PASSIVE:
                        UIManager.Inst.GetPassivePanel().SetEnable();
                        break;
                    case DownButtonType.SHOP:
                        UIManager.Inst.GetShopPanel().SetEnable();
                        break;
                }
            }
        }
    }
    public DownButtonType GetCurrentButtonType() => currentButtonType;

}