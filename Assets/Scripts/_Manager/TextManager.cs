using System.Collections.Generic;
using UnityEngine;

public class TextManager : MonoBehaviour
{
    public static TextManager Inst;

    [SerializeField] RectTransform DamageTextTp, ItemTextTp;
    [SerializeField] RectTransform DamageTextHead, ItemTextHead;
    List<DamageText> poolingDamageText = new List<DamageText>();
    List<ItemText> poolingItemText = new List<ItemText>();

    void Awake()
    {
        Inst = this;
    }

    public void SpawnDamageText(Vector2 targetPos, long Damage, DamageType dmgType, bool isCrit)
    {
        var text = GetDamageText();
        text.SetEnable(targetPos, Damage, dmgType, isCrit);
    }

    public void SpawnItemText(Vector2 targetPos, IngameItemType itemType, int amount)
    {
        var text = GetItemText();
        text.SetEnable(targetPos, itemType, amount);
    }

    DamageText GetDamageText()
    {
        for (int i = 0; i < poolingDamageText.Count; i++)
        {
            if (poolingDamageText[i].IsEnable())
                continue;
            return poolingDamageText[i];
        }
        var obj = Instantiate(DamageTextTp, Vector2.zero, Quaternion.identity).GetComponent<DamageText>();
        Enums.InitializeOnStart(obj.GetRect(), DamageTextHead);
        obj.Initialize();
        poolingDamageText.Add(obj);
        return obj;
    }

    ItemText GetItemText()
    {
        for (int i = 0; i < poolingItemText.Count; i++)
        {
            if (poolingItemText[i].IsEnable())
                continue;
            return poolingItemText[i];
        }
        var obj = Instantiate(ItemTextTp, Vector2.zero, Quaternion.identity).GetComponent<ItemText>();
        Enums.InitializeOnStart(obj.GetRect(), ItemTextHead);
        obj.Initialize();
        poolingItemText.Add(obj);
        return obj;
    }

}