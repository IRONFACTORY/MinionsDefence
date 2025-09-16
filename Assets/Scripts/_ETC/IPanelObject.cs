using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public enum IPanelObjectType
{
    MAIN_PANEL = 0,
    FULL_BOX,
}

public class IPanelObject : MonoBehaviour
{
    public IPanelObjectType panelObjectType;
    public bool GetPanelEnable() => gameObject.activeInHierarchy;

    public bool backKeyLock = false;

    public virtual void DoAndroidBack()
    {

    }

    public bool GetBackKeyLock() => backKeyLock;
    public void SetBackKeyLock(bool tru) => backKeyLock = tru;

    public bool BackkeyAble()
    {
        // return !IngameLoadingPanel.LoadingPanelEnabled;
        return false;
    }

}