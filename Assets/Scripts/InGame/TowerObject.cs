using System.Collections.Generic;
using UnityEngine;

public class TowerObject : IPlayerUnit
{
    [SerializeField] private Transform towerBodyHead;

    TowerBody towerBody;
    IngameTowerCard myTowerCard;

    List<Vector2Int> buildCells = new List<Vector2Int>();

    Collider2D myCollider;
    public Collider2D GetCollider2D() => myCollider ??= GetComponent<Collider2D>();

    public TowerIndex towerIndex => myTowerCard.GetTowerIndex();

    bool isWating;
    public bool GetWating() => isWating;
    public void SetWating(bool tru) => isWating = tru;

    bool isPooled;
    public bool GetPooled() => isPooled;
    void SetPooling(bool tru) => isPooled = tru;

    int slotIndex;
    public int GetSlotIndex() => slotIndex;
    public void SetSlotIndex(int index) => slotIndex = index;

    public override void Awake()
    {
        base.Awake();
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

    }

    public override void IntiializeOnSpawn()
    {
        base.IntiializeOnSpawn();
        DoAnimation(AnimationType.IDLE);
    }

    public void InitializeTower(IngameTowerCard target)
    {
        SetEnable();
        myTowerCard = target;
        SpawnTowerBody();
        SetWating(true);
        SetSlotIndex(-1);
        UnitManager.Inst.RegisterTower(this);

    }

    public void OnBuildIt(Vector2 pos, List<Vector2Int> cells)
    {
        UpdatePosition(pos);
        SetWating(false);
        buildCells = cells;
    }

    public override void DoAttack()
    {
        base.DoAttack();
        DoAnimation(AnimationType.ATTACK);
    }

    public override void SetEnable()
    {
        base.SetEnable();
        gameObject.SetActive(true);
        SetPooling(true);
    }

    public override void SetDisable()
    {
        base.SetDisable();
        gameObject.SetActive(false);
        DespawnTowerBody();
        UnitManager.Inst.UnRegisterTower(this);
        SetPooling(false);
    }

    void DespawnTowerBody()
    {
        if (towerBody == null) return;
        towerBody.SetDisable();
    }

    void SpawnTowerBody()
    {
        DespawnTowerBody();

        var body = UnitManager.Inst.GetTowerBody(towerIndex);
        body.InitializeTowerBody(this);
        towerBody = body;
    }

    public void SetColor(Color col) => towerBody.SetColor(col);
    public void ResetColor() => towerBody.ResetColor();
    public Transform GetTowerBodyHead() => towerBodyHead;

}