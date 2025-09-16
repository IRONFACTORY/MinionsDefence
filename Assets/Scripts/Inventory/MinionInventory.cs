using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class MinionInventory
{
    public void Initialize()
    {

    }
    // public List<MinionCard> equipedMinions = new List<MinionCard>();

    // public void Initialize()
    // {

    // }

    // public void AddCard(MinionCard minionCard)
    // {
    //     if (playerCards.Contains(minionCard)) return;
    //     playerCards.Add(minionCard);
    // }

    // public void RemoveCard(MinionCard minionCard)
    // {
    //     if (!playerCards.Contains(minionCard)) return;
    //     playerCards.Remove(minionCard);
    // }

    // public void DoEquip(MinionCard target, int slotIndex)
    // {
    //     equipedMinions.Add(target);
    //     target.DoEquip(slotIndex);
    // }

    // public void DoEquipOnFull(MinionCard target, int targetSlotIndex)
    // {
    //     // 슬롯 범위 체크 (예: 슬롯 0~5)
    //     if (targetSlotIndex < 0 || targetSlotIndex >= equipedMinions.Count)
    //         return;

    //     // 기존 슬롯에 카드가 있으면 해제
    //     MinionCard existed = equipedMinions[targetSlotIndex];
    //     if (existed != null)
    //     {
    //         existed.DoUnEquip();
    //         equipedMinions[targetSlotIndex] = null;
    //     }

    //     // 새로운 카드 장착
    //     target.DoEquip(targetSlotIndex);
    //     equipedMinions[targetSlotIndex] = target;
    // }


    // public void DoUnEquip(MinionCard target)
    // {
    //     equipedMinions.Remove(target);
    //     target.DoUnEquip();
    // }

    // public MinionInventory()
    // {
    //     playerCards = new List<MinionCard>();
    //     equipedMinions = new List<MinionCard>();
    // }

    // public List<MinionCard> GetEquipedMinions() => equipedMinions;
    // public List<MinionCard> GetFullMinions() => playerCards;

}