using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Diagnostics.Contracts;

public enum IngameButtonType
{
    TOWER = 0,
}

public class IngameButton : MonoBehaviour
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
                TitleText.text = "타워 구매";
                break;
        }
    }

    void DoButton()
    {
        switch (myType)
        {
            case CostButtonType.GAME_START:
                StageManager.Inst.PurchaseTower();
                break;

        }
    }

    public void UpdateInfo()
    {
        bool purchaseAble = PurchaseAble();
        myButton.enabled = purchaseAble;
        int towerCost = StageManager.Inst.GetTowerPurchaseCost();
        CostText.text = $"{towerCost}";
        if (purchaseAble)
        {
            CostText.color = Color.wheat;
        }
        else
        {
            CostText.color = Color.red;
        }
    }

    bool PurchaseAble()
    {
        switch (myType)
        {
            case CostButtonType.GAME_START:
                return StageManager.Inst.GoldPurchaseAble(StageManager.Inst.GetTowerPurchaseCost());
        }
        return false;
    }

}
