using UnityEngine;

public class DataManager : MonoBehaviour
{
    TowerInventory towerInventory;
    long[] UserCosts;
    int mainFieldRound, mainFieldRoundMax;
    public static DataManager Inst;


    void Awake()
    {
        Inst = this;
        Initialize();
    }

    void Start()
    {
        int length = System.Enum.GetValues(typeof(CostType)).Length;
        UserCosts = new long[length];
        for (int i = 0; i < UserCosts.Length; i++)
            UserCosts[i] = Enums.COST_START[i];
        mainFieldRound = 1;
    }

    void Initialize()
    {
        towerInventory = new TowerInventory();
    }

    void Test()
    {

    }

    public long GetCost(CostType costType) => UserCosts[(int)costType];
    public int GetMainFieldRound() => mainFieldRound;
    public void AddRound()
    {
        mainFieldRound++;
        if (mainFieldRound >= mainFieldRoundMax)
            mainFieldRoundMax = mainFieldRound;
    }

    public void AddCard(TowerIndex target, int amount) => GetTowerInventory().AddCard(target, amount);
    public void RemoveCard(TowerIndex target, int amount) => GetTowerInventory().RemoveCard(target, amount);
    public void DoEquip(TowerIndex target) => GetTowerInventory().DoEquip(target);
    public void DoEquipOnFull(TowerIndex target, int targetSlotIndex) => GetTowerInventory().DoEquipOnFull(target, targetSlotIndex);
    public void DoUnEquip(TowerIndex target) => GetTowerInventory().DoUnEquip(target);
    public TowerInventory GetTowerInventory() => towerInventory;


}