using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class TowerInventory
{
    public TowerCard[] towerCards;
    public List<TowerCard> equipedCards = new List<TowerCard>();

    public TowerCard GetTowerCard(TowerIndex target) => System.Array.Find(towerCards, x => x.towerIndex == target);

    public void AddCard(TowerIndex towerIndex, int amount) => GetTowerCard(towerIndex).SetCount(true, amount);
    public void RemoveCard(TowerIndex towerIndex, int amount) => GetTowerCard(towerIndex).SetCount(false, amount);

    public void DoEquip(TowerIndex towerIndex)
    {
        var target = GetTowerCard(towerIndex);
        equipedCards.Add(target);
        target.SetEquip(equipedCards.IndexOf(target));
    }

    public void DoEquipOnFull(TowerIndex toChangeIndex, int targetSlotIndex)
    {
        // 슬롯 범위 체크 (예: 슬롯 0~5)
        if (targetSlotIndex < 0 || targetSlotIndex >= equipedCards.Count)
            return;

        // 기존 슬롯에 카드가 있으면 해제
        TowerCard existed = equipedCards[targetSlotIndex];
        if (existed != null)
        {
            existed.SetUnEquip();
            equipedCards[targetSlotIndex] = null;
        }

        // 새로운 카드 장착
        var toChangeCard = GetTowerCard(toChangeIndex);
        toChangeCard.SetEquip(targetSlotIndex);
        equipedCards[targetSlotIndex] = toChangeCard;
    }

    public void DoUnEquip(TowerIndex towerIndex)
    {
        var unEquipCard = GetTowerCard(towerIndex);
        equipedCards.Remove(unEquipCard);
        unEquipCard.SetUnEquip();
    }

    public TowerInventory()
    {
        equipedCards = new List<TowerCard>();
        int length = System.Enum.GetValues(typeof(TowerIndex)).Length;
        towerCards = new TowerCard[length];
        for (int i = 0; i < length; i++)
            towerCards[i] = new TowerCard((TowerIndex)i);

        DoEquip(TowerIndex.TOWER_1);
        DoEquip(TowerIndex.TOWER_2);
        DoEquip(TowerIndex.TOWER_3);
        DoEquip(TowerIndex.TOWER_4);
        DoEquip(TowerIndex.TOWER_5);

        Debug.Log(equipedCards.Count);
    }

    public TowerCard[] GetTowerCards() => towerCards;
    public List<TowerCard> GetEquipedCards() => equipedCards;

}