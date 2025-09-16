using UnityEngine;

public class AnimationDataBox : MonoBehaviour
{
    public static AnimationDataBox Inst;

    public Sprite[] TempIdleSprite;
    public Sprite[] TempWalkSprite;
    public Sprite[] TempAttackSprite;

    public MinionClip[] minionClips;

    void Awake()
    {
        Inst = this;
    }

    public Sprite[] GetMinionSprites(MinionIndex minionIndex)
    {
        return minionClips[(int)minionIndex].WalkClips;
    }

}

[System.Serializable]
public class MinionClip
{
    public MinionIndex minionIndex;
    public Sprite[] WalkClips;
}