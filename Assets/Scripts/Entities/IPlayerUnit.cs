using UnityEngine;

// 타겟팅 전략 (경로 기반 전략 추가)
public enum TargetingStrategy
{
    Closest,              // 가장 가까운
    Furthest,             // 가장 먼
    LowestHP,             // 체력 낮은
    HighestHP,            // 체력 높은
    ShortestRemaining,    // 남은 경로 거리 짧은
    LongestRemaining      // 남은 경로 거리 긴
}


public class IPlayerUnit : MonoBehaviour
{
    protected Transform myTransform, bodyTransform;
    protected float aspdStart = 3f, aspdCurrent;
    protected float aspdTick = 1f;
    long Damage;
    public TargetingStrategy targetingStrategy;
    protected float Range = 20;
    public float GetRange() => Range;
    protected void SetRange(float range) => Range = range;


    public virtual void Awake()
    {
        myTransform = transform;
        bodyTransform = myTransform.Find("BodyTransform");
    }

    public virtual void IntiializeOnSpawn()
    {
        ResetASPD();
        aspdCurrent = aspdStart;
    }

    public virtual void FixedUpdate()
    {

    }

    public virtual void Update()
    {
        if (GameManager.Inst.GetGameState() != GameState.FIGHT) return;
        if (StageManager.Inst.GetStageWaveStats() != StageWaveStats.WAVING) return;

        if (aspdTick > 0f)
            aspdTick -= TimeManager.Inst.UpdateTime() * aspdCurrent;
        else
            DoAttack();
    }

    void ResetASPD() => aspdTick = 1f;

    public virtual void DoAttack()
    {
        var target = GetClosestMinion(this);
        if (target == null)
            return;

        target.TakeDamage(GetDamageEntity());
        ResetASPD();
    }

    public virtual void AttackProgress()
    {

    }

    public virtual void SetEnable()
    {

    }

    public virtual void SetDisable()
    {

    }


    public void UpdatePosition(Vector2 targetPos)
    {
        myTransform.position = targetPos;
    }

    protected virtual DamageEntity GetDamageEntity()
    {
        return new DamageEntity(DamageType.NORMAL, 100);
    }
    protected MinionObject GetClosestMinion(IPlayerUnit me) => UnitManager.Inst.GetClosestMinion(me);
    public Vector2 GetMyPos() => myTransform.position;
}
