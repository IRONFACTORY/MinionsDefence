using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public enum MenuToggleType { MAIN_DOWN }

public class MenuToggle : MonoBehaviour
{
    private RectTransform _rectTransform;
    public RectTransform myRect
    {
        get
        {
            if (_rectTransform == null)
                _rectTransform = GetComponent<RectTransform>();
            return _rectTransform;
        }
    }

    public MenuToggleType menuToggleType;
    bool isToggled;

    public Image ColumnImage;
    public Image IconImage;
    public Image AlertImage;
    public Button myButton;
    public TextMeshProUGUI NameText;
    public RectTransform TargetPanel;
    public RectTransform LockedBox;

    int toggleIndex;

    public void DoTrigger()
    {

    }

    public void Initialize(MenuToggleType toggleType, int toggleIndex)
    {
        this.menuToggleType = toggleType;
        this.toggleIndex = toggleIndex;

        UpdateMenuIcons();
    }

    void UpdateMenuIcons()
    {

    }

    void DoButton()
    {

    }

    public void SetAlertImageEnable()
    {
        AlertImage.enabled = true;
    }

    public void UpdateLocked()
    {

    }

    public void SetToggled(bool tru)
    {

    }


}