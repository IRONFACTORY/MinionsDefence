using UnityEngine;

public class MinionBody : MonoBehaviour
{
    Animator ani;
    [SerializeField] private SpriteRenderer myRenderer;

    int walkTick = 0;
    Sprite[] walkSprites;
    AnimationType aniType;

    public void Initialize()
    {
        ani = GetComponent<Animator>();
        walkSprites = AnimationDataBox.Inst.TempWalkSprite;
    }

    public void DoAnimation(AnimationType target)
    {
        if (aniType == target)
            return;
        aniType = target;

        switch (target)
        {
            case AnimationType.WALK:
                walkTick = 0;
                ani.Play(Enums.ANI_WALK);
                break;
        }
    }

    public void DoWalkTick()
    {
        int target = walkTick;
        walkTick++;
        if (walkTick >= walkSprites.Length)
            walkTick = 0;
        myRenderer.sprite = walkSprites[target];
    }

}