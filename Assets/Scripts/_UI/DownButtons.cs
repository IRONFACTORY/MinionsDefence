using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;

public class DownButtons : MonoBehaviour
{
    public RectTransform myRect => GetComponent<RectTransform>();

    public Image BackgroundImage;

    DownButtonType downButtonType;
    public EventTrigger myButton;
    public Image IconImage;
    public TextMeshProUGUI TitleText;

    // public Image LockIconImage;
    // public Image LockTriggerImage;

    bool tempToggled;

    public void DoTrigger()
    {

    }

    public void Initialize(DownButtonType targetType)
    {
        downButtonType = targetType;
        // TitleText.text = TranslateDataBox.Inst.GetDownButtonStrings(targetType);
        // IconImage.sprite = SpriteBox.Inst.DownToggleIcons[(int)downButtonType - 1];

        myButton.triggers.Clear();
        EventTrigger.Entry entry = new EventTrigger.Entry { eventID = EventTriggerType.PointerDown };
        entry.callback.AddListener((data) => { DoButton(); });
        myButton.triggers.Add(entry);

    }

    void DoButton()
    {
        var btnHead = UIManager.Inst.GetDownButtonHead();
        UIManager.Inst.SetDownToggle(downButtonType);
    }

    public void SetToggled(bool tru)
    {
        tempToggled = tru;
        if (tru)
        {
            IconImage.color = Color.gray;
            TitleText.color = Color.gray;
        }
        else
        {
            IconImage.color = Color.white;
            TitleText.color = Color.white;
        }
    }

    public DownButtonType GetDownButtonType() => downButtonType;

}