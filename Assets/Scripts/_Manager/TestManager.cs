using UnityEngine;
using UnityEngine.InputSystem; // ★ 추가

public class TestManager : MonoBehaviour
{
    public static TestManager Inst;

    void Awake()
    {
        Inst = this;
    }

    void Update()
    {
        // var kb = Keyboard.current;
        // if (kb == null) return; // 키보드 없는 플랫폼 대비

        // if (kb.qKey.wasPressedThisFrame)
        // {
        //     UnitManager.Inst.GetHeroObject().Test(AnimationType.IDLE);
        // }
        // else if (kb.wKey.wasPressedThisFrame)
        // {
        //     UnitManager.Inst.GetHeroObject().Test(AnimationType.WALK);
        // }
        // else if (kb.eKey.wasPressedThisFrame)
        // {
        //     UnitManager.Inst.GetHeroObject().Test(AnimationType.ATTACK);
        // }
    }

}