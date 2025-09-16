using UnityEngine;

public class HeroBody : MonoBehaviour
{
    HeroObject parentHero;
    Animator ani;
    [SerializeField] private SpriteRenderer myRenderer;

    int idleTick = 0, walkTick = 0, attackTick = 0;
    Sprite[] idleSprites, walkSprites, attackSprites;
    AnimationType aniType;

    public void Initialize(HeroObject playerHero)
    {
        this.parentHero = playerHero;
        ani = GetComponent<Animator>();

        idleSprites = AnimationDataBox.Inst.TempIdleSprite;
        walkSprites = AnimationDataBox.Inst.TempWalkSprite;
        attackSprites = AnimationDataBox.Inst.TempAttackSprite;
    }

    public void DoAnimation(AnimationType target)
    {
        // if (aniType == target)
        //     return;
        aniType = target;

        switch (target)
        {
            case AnimationType.IDLE:
                idleTick = 0;
                ani.Play(Enums.ANI_IDLE);
                break;
            case AnimationType.WALK:
                walkTick = 0;
                ani.Play(Enums.ANI_WALK);
                break;
            case AnimationType.ATTACK:
                attackTick = 0;
                ani.Play(Enums.ANI_ATTACK);
                break;
        }
    }

    public void DoIdleTick()
    {
        int target = idleTick;
        idleTick++;
        if (idleTick >= idleSprites.Length)
            idleTick = 0;
        myRenderer.sprite = idleSprites[target];
    }

    public void DoWalkTick()
    {
        int target = walkTick;
        walkTick++;
        if (walkTick >= walkSprites.Length)
            walkTick = 0;
        myRenderer.sprite = walkSprites[target];
    }

    public void DoAttackTick()
    {
        int target = attackTick;
        attackTick++;
        if (attackTick >= attackSprites.Length)
            attackTick = 0;
        myRenderer.sprite = attackSprites[target];
    }

}