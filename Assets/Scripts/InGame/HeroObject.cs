using UnityEngine;

public class HeroObject : IPlayerUnit
{
    HeroBody heroBody;
    public override void Awake()
    {
        base.Awake();
        heroBody = bodyTransform.Find("HeroBody").GetComponent<HeroBody>();
        heroBody.Initialize(this);
    }

    public override void FixedUpdate()
    {
        base.FixedUpdate();
    }

    public override void Update()
    {
        base.Update();
    }

    void DoAnimation(AnimationType target)
    {
        heroBody.DoAnimation(target);
    }

    public override void IntiializeOnSpawn()
    {
        base.IntiializeOnSpawn();
        DoAnimation(AnimationType.IDLE);
    }
    public override void DoAttack()
    {
        base.DoAttack();
        DoAnimation(AnimationType.ATTACK);
    }



}