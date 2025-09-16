using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class UnitManager : MonoBehaviour
{
    public static UnitManager Inst;
    // SpatialHash2D _hash;
    // readonly List<MinionObject> _queryList = new(256);


    [SerializeField] private Transform UnitHead;
    [SerializeField] private Transform TowerHead;

    public Transform GetTowerHead() => TowerHead;

    DynamicPathTargetingSystem _targetingSystem;
    public HeroObject heroObject;

    public Transform MinionTp;
    private List<MinionObject> enableUnits = new List<MinionObject>();
    private List<MinionObject> poolingUnits = new List<MinionObject>();

    public Transform TowerTp;
    private List<TowerObject> enableTowers = new List<TowerObject>();
    private List<TowerObject> poolingTowers = new List<TowerObject>();

    public TowerBody[] TowerBodies;

    private List<TowerBody> enableTowerBodys = new List<TowerBody>();
    private List<TowerBody> poolingTowerBodys = new List<TowerBody>();


    void Awake()
    {
        Inst = this;
        // _hash = new SpatialHash2D(cellSize: 3f); // 스킬 사거리/유닛 밀집도에 맞게 조정

        // 새로운 타겟팅 시스템 초기화
        _targetingSystem = gameObject.GetComponent<DynamicPathTargetingSystem>();
        if (_targetingSystem == null)
            _targetingSystem = gameObject.AddComponent<DynamicPathTargetingSystem>();
    }

    public void InitializeFight()
    {
        GetHeroObject().IntiializeOnSpawn();
        enableUnits.Clear();

        // 히어로를 타워로 등록 (HeroObject가 TowerUnit을 상속하도록 수정 필요)
        RegisterTower(GetHeroObject());
    }

    public void RegisterTower(IPlayerUnit target)
    {
        _targetingSystem.RegisterTower(target);
    }

    public void UnRegisterTower(IPlayerUnit target)
    {
        _targetingSystem.UnregisterTower(target);
    }

    public void SpawnMinion(MinionIndex minionIndex, MinionLevel minionLevel, Vector2 spawnPos)
    {
        for (int i = 0; i < poolingUnits.Count; i++)
        {
            if (poolingUnits[i].GetPooled())
                continue;

            poolingUnits[i].Initialize(spawnPos, minionIndex, minionLevel);
            return;
        }

        var obj = Instantiate(MinionTp, spawnPos, Quaternion.identity).GetComponent<MinionObject>();
        obj.transform.SetParent(UnitHead);
        obj.Initialize(spawnPos, minionIndex, minionLevel);
        poolingUnits.Add(obj);
    }

    public TowerObject GetTower(Vector2 spawnPos)
    {
        for (int i = 0; i < poolingTowers.Count; i++)
        {
            if (poolingTowers[i].GetPooled())
                continue;

            return poolingTowers[i];
        }

        var obj = Instantiate(TowerTp, spawnPos, Quaternion.identity).GetComponent<TowerObject>();
        obj.transform.SetParent(TowerHead);
        poolingTowers.Add(obj);
        return obj;
    }

    public TowerBody GetTowerBody(TowerIndex towerIndex)
    {
        for (int i = 0; i < poolingTowerBodys.Count; i++)
        {
            var target = poolingTowerBodys[i];
            if (target.GetPooled())
                continue;
            if (target.GetTowerIndex() != towerIndex)
                continue;
            return poolingTowerBodys[i];
        }
        var tp = TowerBodies[Mathf.Clamp((int)towerIndex, 0, TowerBodies.Length - 1)];
        var obj = Instantiate(tp, Vector2.zero, Quaternion.identity).GetComponent<TowerBody>();
        obj.Initialize();
        poolingTowerBodys.Add(obj);
        return obj;
    }

    public void AddMinion(MinionObject target)
    {
        enableUnits.Add(target);
        _targetingSystem.RegisterEnemy(target);
        // _hash.Add(target);
    }

    public void RemoveMinion(MinionObject target)
    {
        enableUnits.Remove(target);
        _targetingSystem.UnregisterEnemy(target);
        // _hash.Remove(target);
    }

    // public void NotifyMinionMoved(MinionObject m)
    // {
    //     _hash.Move(m);
    // }

    // public MinionObject GetClosestMinion(Vector2 from, float maxRadius = 9999f)
    //     => _hash.QueryNearest(from, maxRadius);

    // public int GetMinionsInRadiusFast(Vector2 center, float radius, List<MinionObject> outList)
    //     => _hash.QueryCircle(center, radius, outList);


    public float GetTargetingSystemGridCellSize() => _targetingSystem.gridCellSize;
    public void UpdateEnemyPosition(MinionObject target) => _targetingSystem.UpdateEnemyPosition(target);
    public MinionObject GetClosestMinion(IPlayerUnit me) => _targetingSystem.GetBestTarget(me);
    public HeroObject GetHeroObject() => heroObject;

}