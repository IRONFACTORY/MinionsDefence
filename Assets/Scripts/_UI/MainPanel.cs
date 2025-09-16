using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;

public enum MainPanelType
{
    INAPP = 0,
    SKILL_INFO,
    QUEST,
    INVENTORY,
    MINERAL,

    ACHIVEMENT,
    KNIGHT_INFO,
    FIGHT_RESULT,
    SETTING,
    HERO,

    HAMMER,
    STAGE_ADVENTURE,
    ACC_INFO,
    TIME_MARKET,
    FIGHT_RETRY,

    RANKING,
    HYPER_FORCE,
}

public class MainPanel : IPanelObject
{
    public MainPanelType myType;

    public bool IsBlackBackground = true;
    public Image BackgroundImage;
    public RectTransform BoxRect;
    bool initialized = false;

    public virtual void SetEnable(bool tru)
    {
        gameObject.SetActive(tru);
        if (tru)
        {
            DoEnable();
            // SoundManager.Inst.DoSFX(SoundSFXType.PANEL_POPUP);

            // if (!IsBlackBackground)
            // {
            //     if (BackgroundImage != null)
            //         TweenHelper.Inst.SetBackgroundFadeWhite(BackgroundImage, myType == MainPanelType.SETTING, true);
            //     return;
            // }

            // if (BoxRect != null)
            //     TweenHelper.Inst.SetPanelEnableBox(BoxRect, myType == MainPanelType.SETTING);
            // if (BackgroundImage != null)
            //     TweenHelper.Inst.SetBackgroundFade(BackgroundImage, myType == MainPanelType.SETTING);
        }
    }

    public virtual void InitializeOnStart()
    {

    }

    public virtual void DoEnable()
    {
        if (initialized)
            return;

        initialized = true;
        DoInitialize();
    }

    public virtual void DoInitialize()
    {

    }

    public override void DoAndroidBack()
    {
        base.DoAndroidBack();
    }

    protected bool IsInitialized()
    {
        return initialized;
    }

    public T GetPanel<T>() where T : Component
    {
        return GetComponent<T>();
    }

}