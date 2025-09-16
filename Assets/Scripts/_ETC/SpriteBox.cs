using UnityEngine;

public class SpriteBox : MonoBehaviour
{

    public static SpriteBox Inst;

    public Sprite[] CostIcons;

    void Awake()
    {
        Inst = this;
    }

    public Sprite GetCostIcon(CostType type) => CostIcons[(int)type];

}
