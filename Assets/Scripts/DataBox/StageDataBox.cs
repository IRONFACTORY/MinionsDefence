using CodeStage.AntiCheat.ObscuredTypes;
using UnityEngine;

public class StageDataBox : MonoBehaviour
{
    public static StageDataBox Inst;
    [SerializeField] StageInfo[] stageInfos;

    public ObscuredInt[] IngaemTargetEXP;

    void Awake()
    {
        Inst = this;
    }

    public StageInfo GetStageInfo(int stageIndex) => stageInfos[stageIndex];


    public int GetIngameTargetEXP(int lev) => IngaemTargetEXP[Mathf.Clamp(lev, 0, IngaemTargetEXP.Length - 1)];

}