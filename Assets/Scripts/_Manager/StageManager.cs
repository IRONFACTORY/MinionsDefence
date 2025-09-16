using UnityEngine;
using System;
using TMPro;
using System.Collections.Generic;
using Pathfinding;

public enum StageWaveStats { START = 0, WAVING, END, RESULT, }
public class StageManager : MonoBehaviour
{
    public static StageManager Inst;
    [SerializeField] private LoadingPanel mapLoadingPanel;
    GridInfo gridInfo;
    StageWaveStats currentWaveStats;

    float currenTime = 0f, startTime = 0f;
    float spawnTickTime = 0, spawnTickStartTime = 0f;

    int waveIndex = 0, waveMax = 1;
    int spawnSeed = 0;
    WaveInfo[] waveInfos;
    SpawnPositionPicker picker;
    WaveInfo PickWaveInfo(int waveIndex) => waveInfos[Mathf.Clamp(waveIndex, 0, waveInfos.Length - 1)];
    float cellSizeX, cellSizeY;

    [SerializeField] TextMeshProUGUI StageText, WaveText, TimerText, GameGoldText, EXPText, EXPPerText, KillCountText, readyTowerCountText;
    [SerializeField] TextMeshPro HeroLevelText;
    [SerializeField] SlicedFilledImage timerGage, expGage;
    [SerializeField] TowerSlot[] towerSlots;

    int gameGold, heroLevel, killCount, towerPurchaseCost, towerPurchaseCount;
    int exp, expMax;

    public IngameButton towerPurchaseButton;
    IngameTowerCard[] stageTowerCards;
    List<TowerObject> readyTowers = new List<TowerObject>();

    float gageTarget = 0f;

    void Awake()
    {
        Inst = this;
        gridInfo = GetComponent<GridInfo>();
        var grid = GetGridInfo(); // 네 기존 메서드

    }
    public void Initialize()
    {
        mapLoadingPanel.Initialize();
        GameManager.Inst.SetGameState(GameState.MAIN);
        mapLoadingPanel.SetDisable();
        towerPurchaseButton.Initialize();
        picker = new SpawnPositionPicker();
        cellSizeX = gridInfo.GetCellSize().x;
        cellSizeY = gridInfo.GetCellSize().y;
        for (int i = 0; i < towerSlots.Length; i++)
            towerSlots[i].Initialize(i);
    }

    public void DoGameStart()
    {
        mapLoadingPanel.SetEnable(false, FightStartAwake, FightStartStart);
    }

    void InitializeRound()
    {
        gridInfo.Build();
        picker.DoGenerate(gridInfo.GetEnemySpawnPositions(), spawnSeed);
        waveIndex = 1;
        heroLevel = 1;
        killCount = 0;
        towerPurchaseCount = 0;
        exp = 0;
        UpdateTargetEXP();
        gameGold = 0;
        readyTowers.Clear();
        int targetStage = GameManager.Inst.GetTargetStage();
        waveInfos = StageDataBox.Inst.GetStageInfo(0).GenerateWaves();
        waveMax = waveInfos.Length;
        StageText.text = $"스테이지 {targetStage}";

        var equiped = DataManager.Inst.GetTowerInventory().GetEquipedCards();
        stageTowerCards = new IngameTowerCard[equiped.Count];

        for (int i = 0; i < stageTowerCards.Length; i++)
            stageTowerCards[i] = new IngameTowerCard(equiped[i].towerIndex, equiped[i].level);

        for (int i = 0; i < Enums.START_TOWER_CARD; i++)
            DoSpawnTower();

        UpdateWaveText();
        UpdateHeroLevelText();
        UpdateEXPBox();
        UpdateKillCountText();
        UpdateTowerPurchaseCost();
        UpdateTowerButton();
        UpdateReadyTowerText();
        UpdateGameGoldText(false);
    }

    void UpdateWaveText()
    {
        WaveText.text = $"웨이브 {waveIndex}/{waveMax}";
    }

    void StartRound()
    {
        SetWaveStats(StageWaveStats.START);
    }

    void Update()
    {
        var state = GameManager.Inst.GetGameState();
        if (state != GameState.FIGHT)
            return;

        var status = GetStageWaveStats();
        if (status == StageWaveStats.END
            || status == StageWaveStats.RESULT)
            return;

        TimerCheck();
    }

    void TimerCheck()
    {
        var t = TimeManager.Inst.UpdateTime();
        if (GetStageWaveStats() == StageWaveStats.START)
        {
            if (currenTime > 0f)
                currenTime -= t;
            else
                StartFirstWave();
        }
        else
        {
            if (currenTime > 0f)
                currenTime -= t;
            else
                DoNextWave(false);
        }
        UpdateTimerText();
        UpdateTimerGage();
    }

    void StartFirstWave()
    {
        SetWaveStats(StageWaveStats.WAVING);
        DoNextWave(true);
    }

    void DoNextWave(bool isStart)
    {
        if (!isStart)
            waveIndex++;

        UpdateWaveText();

        if (waveIndex == waveMax)
        {
            SpawnWaveMobs();
            SetWaveStats(StageWaveStats.END);
        }
        else
        {
            SpawnWaveMobs();
            SetWaveStats(StageWaveStats.WAVING);
        }
        UpdateTimerText();
        UpdateTimerGage();
    }

    void SpawnWaveMobs()
    {
        var pieces = PickWaveInfo(waveIndex);
        var list = pieces.wavePieces;
        for (int i = 0; i < list.Count; i++)
        {
            var minionIndex = list[i].minionIndex;
            var pieceSpawns = list[i].Count;

            for (int j = 0; j < pieceSpawns; j++)
            {
                Vector2 spawnPos = picker.Pick();
                SpawnMinion(minionIndex, MinionLevel.NORMAL, spawnPos);
            }
        }
    }

    void SpawnMinion(MinionIndex minionIndex, MinionLevel minionLevel, Vector2 spawnPos)
    {
        UnitManager.Inst.SpawnMinion(minionIndex, minionLevel, spawnPos);
    }

    void FightStartAwake()
    {
        GameManager.Inst.SetGameState(GameState.WATING); // 맵제작
        UIManager.Inst.SetFight();
        UnitManager.Inst.InitializeFight();
        InitializeRound();
        Debug.Log("FightStartAwake");
    }

    void FightStartStart()
    {
        GameManager.Inst.SetGameState(GameState.FIGHT); // 전투시작
        StartRound();
        SetWaveStats(StageWaveStats.START);
        Debug.Log("FightStartStart");
    }

    void SetWaveStats(StageWaveStats target)
    {
        currentWaveStats = target;
        UpdateTimerColor(currentWaveStats);

        switch (currentWaveStats)
        {
            case StageWaveStats.START:
                ResetCurrentTime(Enums.STAGE_FIRST_DELAY_TIME);
                break;
            case StageWaveStats.WAVING:
                ResetCurrentTime(Enums.STAGE_WAVE_TIME);
                break;
            case StageWaveStats.END:
                ResetCurrentTime(Enums.STAGE_FIRST_DELAY_TIME);
                break;
            case StageWaveStats.RESULT:
                ResetCurrentTime(Enums.STAGE_FIRST_DELAY_TIME);
                break;
        }
    }

    void ResetCurrentTime(float t)
    {
        startTime = t;
        currenTime = startTime;
        UpdateTimerText();
    }


    void OnDrawGizmos()
    {
        if (gridInfo == null) return;
        gridInfo.RebuildIfDirty();

        Gizmos.color = Color.gray;
        foreach (var cell in gridInfo.EmptyCells)
            Gizmos.DrawSphere(cell.worldCenter, 0.05f); // 반지름 0.05
        Gizmos.color = Color.green;
        foreach (var cell in gridInfo.BuildZoneCells)
            Gizmos.DrawSphere(cell.worldCenter, 0.05f);
        Gizmos.color = Color.red;
        foreach (var cell in gridInfo.EnemySpawnCells)
            Gizmos.DrawSphere(cell.worldCenter, 0.05f);
    }

    public GridInfo GetGridInfo() => gridInfo;
    public LoadingPanel GetMapLoadingPanel() => mapLoadingPanel;
    public StageWaveStats GetStageWaveStats() => currentWaveStats;

    public float GetCellSizeX() => cellSizeX;
    public float GetCellSizeY() => cellSizeY;


    void UpdateTimerColor(StageWaveStats targetStats)
    {
        switch (targetStats)
        {
            case StageWaveStats.START:
                timerGage.color = ColorBox.Inst.stageGage_stageStartColor;
                break;
            case StageWaveStats.WAVING:
                timerGage.color = ColorBox.Inst.stageGage_wavingColor;
                break;
            case StageWaveStats.END:
                timerGage.color = ColorBox.Inst.stageGage_resultColor;
                break;
            case StageWaveStats.RESULT:
                timerGage.color = ColorBox.Inst.stageGage_resultColor;
                break;
        }
    }

    void UpdateTimerText() => TextHelper.Inst.SetTimerText(TimerText, currenTime, true);
    void UpdateTimerGage()
    {
        gageTarget = currenTime / startTime;
        timerGage.fillAmount = gageTarget;
    }

    public int GetGameGold() => gameGold;

    void UpdateGameGoldText(bool doAni, bool tru = false)
    {
        GameGoldText.text = $"{gameGold}";
        if (doAni)
        {
            float t = 0.2f;
            if (tru)
                TweenHelper.Inst.DoCostAnimation(GameGoldText, t, CostTextAniType.YELLOW);
            else
                TweenHelper.Inst.DoCostAnimation(GameGoldText, t, CostTextAniType.RED);
        }
    }

    public void MinionDeadCallback(MinionObject minionObject)
    {
        var lev = minionObject.GetMinionLevel();
        killCount++;
        int goldAdds = 0;
        int expAdds = 0;
        switch (lev)
        {
            case MinionLevel.NORMAL:
                goldAdds = Enums.MINION_DEFAULT_GOLD;
                expAdds = Enums.MINION_BOSS_EXP;
                break;
            default:
                goldAdds = Enums.MINION_BOSS_GOLD;
                expAdds = Enums.MINION_BOSS_EXP;
                break;
        }
        SetGameGold(true, goldAdds);
        SetEXP(true, expAdds);
        UpdateKillCountText();
    }

    public void SetGameGold(bool tru, int amount)
    {
        if (tru)
            gameGold += amount;
        else
            gameGold -= amount;

        UpdateTowerButton();
        UpdateGameGoldText(true, tru);
    }

    public void SetEXP(bool tru, int amount)
    {
        if (tru)
        {
            exp += amount;

            if (exp >= expMax)
            {
                exp -= expMax;
                AddHeroLevel();
            }
        }
        else
        {
            exp -= amount;
        }

        UpdateEXPBox();
    }

    void UpdateTargetEXP()
    {
        expMax = StageDataBox.Inst.GetIngameTargetEXP(heroLevel);
    }

    void AddHeroLevel()
    {
        heroLevel++;
        UpdateHeroLevelText();
        UpdateTargetEXP();
    }

    void UpdateHeroLevelText()
    {
        HeroLevelText.text = $"Lv.{heroLevel}";
    }

    void UpdateKillCountText()
    {
        KillCountText.text = $"{killCount}";
    }

    void UpdateEXPBox()
    {
        EXPText.text = $"{exp}/{expMax}";
        float per = (float)exp / (float)expMax;
        TextHelper.Inst.SetPercentageText(EXPPerText, per);
        expGage.fillAmount = per;
    }

    public bool GoldPurchaseAble(int cost) => gameGold >= cost;
    void UpdateTowerPurchaseCost()
    {
        towerPurchaseCost = Enums.TOWER_PURCHASE_COST_START + (towerPurchaseCount * Enums.TOWER_PURCHASE_COST_ADDS);
    }

    public int GetTowerPurchaseCost() => towerPurchaseCost;

    void UpdateTowerButton()
    {
        towerPurchaseButton.UpdateInfo();
    }

    public void PurchaseTower()
    {
        int cost = GetTowerPurchaseCost();
        SetGameGold(false, cost);
        towerPurchaseCount++;
        UpdateTowerPurchaseCost();
        UpdateTowerButton();
        DoSpawnTower();
        UpdateWatingTowers();
    }

    void DoSpawnTower()
    {
        IngameTowerCard randomSpawnedCard = stageTowerCards[UnityEngine.Random.Range(0, stageTowerCards.Length)];
        var tower = UnitManager.Inst.GetTower(Vector2.zero);
        tower.InitializeTower(randomSpawnedCard);
        readyTowers.Add(tower);
        bool insertSuccess = InsertEmptySlotIndex(tower);
        if (!insertSuccess)
            tower.UpdatePosition(new Vector2(10000, 0));
        UpdateReadyTowerText();
    }

    bool InsertEmptySlotIndex(TowerObject tower)
    {
        for (int i = 0; i < towerSlots.Length; i++)
        {
            if (towerSlots[i].IsEquiped())
                continue;

            GetTowerSlot(i).RegisterTower(tower);
            return true;
        }
        tower.SetSlotIndex(-1);
        return false;
    }

    void UpdateWatingTowers()
    {
        for (int i = 0; i < readyTowers.Count; i++)
        {
            if (readyTowers[i].GetSlotIndex() == -1)
            {
                InsertEmptySlotIndex(readyTowers[i]);
                break;
            }
        }
    }

    public void DoBuildTower(TowerObject towerObject, Vector2 pos, List<Vector2Int> cells) // 여기선 슬롯 비우고
    {
        int slotIndex = towerObject.GetSlotIndex();
        towerObject.SetSlotIndex(-1);
        readyTowers.Remove(towerObject);
        towerObject.OnBuildIt(pos, cells);
        GetTowerSlot(slotIndex).UnRegisterTower();
        UpdateWatingTowers();
        UpdateReadyTowerText();


        Bounds bounds = towerObject.GetCollider2D().bounds;
        var guo = new GraphUpdateObject(bounds);
        // Set some settings
        guo.updatePhysics = false;
        AstarPath.active.UpdateGraphs(guo);


        // var graphUpdate = new GraphUpdateObject(tower.GetComponent<Collider>().bounds);
        // var spawnPointNode = AstarPath.active.GetNearest(spawnPoint.position).node;
        // var goalNode = AstarPath.active.GetNearest(goalPoint.position).node;

        // if (GraphUpdateUtilities.UpdateGraphsNoBlock(graphUpdate, spawnPointNode, goalNode, false))
        // {
        //     // Valid tower position
        //     // Since the last parameter (which is called "alwaysRevert") in the method call was false
        //     // The graph is now updated and the game can just continue
        // }
        // else
        // {
        //     // Invalid tower position. It blocks the path between the spawn point and the goal
        //     // The effect on the graph has been reverted
        //     Destroy(tower);
        // }//차단배치확인


    }



    void UpdateReadyTowerText()
    {
        int count = readyTowers.Count;
        int max = towerSlots.Length;

        if (count > max)
        {
            readyTowerCountText.text = $"+{count - max}";
        }
        else
        {
            readyTowerCountText.text = "";
        }
    }

    TowerSlot GetTowerSlot(int slotIndex) => towerSlots[slotIndex];
}