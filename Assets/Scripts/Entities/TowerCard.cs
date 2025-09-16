using UnityEngine;

[System.Serializable]
public class TowerCard
{
    public TowerIndex towerIndex;
    public int level;
    public int count;

    public int slotIndex;
    public bool isEquiped = false;

    public bool GetEquiped() => isEquiped;

    public void SetEquip(int slotIndex)
    {
        this.slotIndex = slotIndex;
        isEquiped = true;
    }

    public void SetUnEquip()
    {
        slotIndex = -1;
        isEquiped = false;
    }


    public TowerCard(TowerIndex towerIndex)
    {
        this.towerIndex = towerIndex;
        level = 0;
        count = 0;
        slotIndex = -1;
        isEquiped = false;
    }

    public void SetCount(bool tru, int amount)
    {
        if (tru)
        {
            count += amount;
        }
        else
        {
            count -= amount;
        }
    }

}

[System.Serializable]
public struct IngameTowerCard
{
    TowerIndex towerIndex;
    int level;

    public TowerIndex GetTowerIndex() => towerIndex;
    public int GetLevel() => level;

    public IngameTowerCard(TowerIndex towerIndex, int lev)
    {
        this.towerIndex = towerIndex;
        this.level = lev;
    }
}


[System.Serializable]
public struct FightTowerCard
{
    TowerIndex towerIndex;
    int baseLevel;
    int currentLevel;

    public TowerIndex GetTowerIndex() => towerIndex;
    public int GetBaseLevel() => baseLevel;
    public int GetCurrentLevel() => currentLevel;

    public FightTowerCard(TowerIndex towerIndex, int lev)
    {
        this.towerIndex = towerIndex;
        this.baseLevel = lev;
        currentLevel = 1;
    }

}