using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using System.Reflection;
using UnityEngine.Events;
using Pathfinding; // ✅ UnityAction

public enum MinionLevel { NORMAL = 0, MIDDLE_BOSS, BOSS };

public class MinionObject : MonoBehaviour
{

    Transform myTransform, bodyTransform;
    Rigidbody2D myRig;
    MinionBody minionBody;
    AILerp aILerp;
    [SerializeField] SpriteRenderer bodyRenderer;

    [Header("그리드셀찾기세팅")]
    Vector2 _lastCellCheckPos;
    float RehashDist = 0.5f; // 이만큼 이동했을 때만 해시 갱신

    MinionIndex minionIndex;
    MinionLevel minionLevel;
    bool isPooled = false, isAlive = false;
    Vector2 lastestCheckPos = Vector2.zero;
    bool faceToMoveDirection = true; // 이동 방향 바라보기(스프라이트 플립)

    float moveSpeed = 1f;          // 유닛 속도(유닛/초)
    float arriveRadius = 0.05f;      // 웨이포인트 도착 판정 반경

    long HPStart, HPCurrent;

    Vector2Int currentCellVector;
    public Vector2Int testCellVector;

    bool SetAlive(bool tru) => isAlive = tru;
    public bool GetAlive() => isAlive;
    public bool GetPooled() => isPooled;

    int wpIndex = 0;

    void Awake()
    {
        myTransform = transform;
        bodyTransform = myTransform.Find("BodyTransform").transform;
        myRig = GetComponent<Rigidbody2D>();
        minionBody = bodyTransform.Find("MinionBody").GetComponent<MinionBody>();
        minionBody.Initialize();
        aILerp = GetComponent<AILerp>();
    }
    void DoAnimation(AnimationType target)
    {
        minionBody.DoAnimation(target);
    }

    public void Initialize(Vector2 pos, MinionIndex minionIndex, MinionLevel minionLevel)
    {
        this.minionLevel = minionLevel;
        this.minionIndex = minionIndex;
        myTransform.position = pos;
        lastestCheckPos = pos;
        _lastCellCheckPos = lastestCheckPos;
        SetEnable();
        SetAlive(true);
        UpdateCell();
        DoUpdatePath();
        RehashDist = UnitManager.Inst.GetTargetingSystemGridCellSize() / 2f;
        UnitManager.Inst.AddMinion(this);
        aILerp.speed = moveSpeed;
    }

    void UpdateHP()
    {
        HPStart = HPCurrent;
    }

    public void AddHP()
    {

    }

    public void TakeDamage(DamageEntity damageEntity)
    {
        long dmg = damageEntity.GetDamage();

        // 실제 들어갈 데미지 = (현재 HP < dmg) ? 현재 HP : dmg
        long damageAbled = (HPCurrent < dmg) ? HPCurrent : dmg;
        TextManager.Inst.SpawnDamageText(GetMyPos(), dmg, DamageType.NORMAL, false);
        HPCurrent -= damageAbled;
        if (HPCurrent <= 0)
        {
            HPCurrent = 0;
            SetDie();
        }
    }

    void Update()
    {
        SpatialChecker();
        DistChecker();
    }

    void SpatialChecker()
    {
        if ((GetMyPos() - _lastCellCheckPos).sqrMagnitude >= RehashDist * RehashDist)
        {
            // UnitManager.Inst.NotifyMinionMoved(this);
            UnitManager.Inst.UpdateEnemyPosition(this);
            _lastCellCheckPos = transform.position;
        }
    }

    void DistChecker()
    {
        Vector2 current = GetMyPos();

        // 셀 크기 기준으로 임계값 정하기 (예: 셀 절반 이상)
        float cellSizeX = StageManager.Inst.GetCellSizeX();
        float cellSizeY = StageManager.Inst.GetCellSizeY();
        float threshold = Mathf.Min(cellSizeX, cellSizeY) * 0.5f;

        // 제곱 거리로 비교해서 성능 절약
        float sqrThreshold = threshold * threshold;
        if ((current - lastestCheckPos).sqrMagnitude >= sqrThreshold)
        {
            // 일정 거리 이상 이동 → 체크 실행
            UpdateCell();
            lastestCheckPos = current; // 기준점 갱신
        }
    }

    void UpdateCell()
    {
        if (StageManager.Inst.GetGridInfo().WorldToGrid(GetMyPos(), out var cell))
            currentCellVector = cell;
        else
            currentCellVector = new Vector2Int(-100, -100);
    }

    void DoUpdatePath()
    {
        var path = ABPath.Construct(GetMyPos(), new Vector3(GetMyPos().x, 0));
        aILerp.SetPath(path);
    }

    public void SetDie()
    {
        SetAlive(false);
        UnitManager.Inst.RemoveMinion(this);
        StageManager.Inst.MinionDeadCallback(this);
        SetDisable();
    }

    public void SetEnable()
    {
        gameObject.SetActive(true);
        isPooled = true;
    }

    public void SetDisable()
    {
        gameObject.SetActive(false);
        isPooled = false;
    }

    public Vector2 GetMyPos() => myTransform.position;

    public float GetCurrentHP() => HPCurrent;

    public MinionLevel GetMinionLevel() => minionLevel;

}
