using UnityEngine;

[System.Serializable]
public class DamageEntity
{
    long damage;
    DamageType damageType;

    public DamageEntity(DamageType damageType, long damage)
    {
        this.damage = damage;
        this.damageType = damageType;
    }

    public long GetDamage() => damage;
}
