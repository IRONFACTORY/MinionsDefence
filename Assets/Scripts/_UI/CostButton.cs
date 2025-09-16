using UnityEngine;
using UnityEngine.UI;
using TMPro;

public enum CostButtonType
{
    GAME_START = 0,
}

public class CostButton : MonoBehaviour
{
    [SerializeField] CostButtonType myType;

    [SerializeField] TextMeshProUGUI TitleText, CostText;
    [SerializeField] Image CostIconImage;
    [SerializeField] Button myButton;

    CostType purchaseCostType;

    public void Initialize()
    {
        myButton.onClick.RemoveAllListeners();
        myButton.onClick.AddListener(DoButton);

        switch (myType)
        {
            case CostButtonType.GAME_START:
                break;
        }
    }

    void DoButton()
    {
        switch (myType)
        {
            case CostButtonType.GAME_START:
                break;
        }
        StageManager.Inst.DoGameStart();
    }

    public void UpdateInfo()
    {

    }


}