using System;
using System.Collections.Generic;
using UnityEngine;

public enum StageLengthType { TARGET_5 = 0, TARGET_10, TARGET_15, TARGET_20, }
public enum FullSpawnCountType { TARGET_50 = 0, TARGET_100, TARGET_200, TARGET_300, TARGET_400, TARGET_500, TARGET_600, TARGET_700, TARGET_800, TARGET_900, TARGET_1000 }
public enum WaveType { NORMAL = 0, MIDDLE_BOSS, BOSS, }

[CreateAssetMenu(fileName = "StageInfo", menuName = "Game/Stage Info")]
public class StageInfo : ScriptableObject
{
    [Header("Identity / Seed")]
    public int StageIndex = 0;
    public int SpawnSeed = 0; // 같은 스테이지에서 항상 같은 분배를 재현

    [Header("Stage Settings")]
    public StageLengthType lengthType;
    public FullSpawnCountType fullSpawnCountType;

    [Min(1)] int MiddleBossCount = 3;
    [Min(1)] int BossCount = 1;

    [Header("Progressive Ratios (후반으로 갈수록 아래쪽 비중↑)")]
    [Range(0f, 1f)] public float progressionStrength = 0.7f; // 0=항상 균등, 1=후반 치우침 극대화
    [Range(1f, 4f)] public float rankExponent = 2f;          // 아래쪽 인덱스에 주는 가중의 곡률

    [Header("Catalog")]
    [Tooltip("위→아래로 갈수록 강한 몬스터라고 가정")]
    public MinionIndex[] defaultMinions;
    public MinionIndex middleBossMinion;
    public MinionIndex BossMinion;


    public WaveInfo[] GenerateWaves()
    {
        int totalWaves = GetWaveLength(lengthType);
        if (totalWaves <= 0) totalWaves = 1;

        var waveInfos = new WaveInfo[totalWaves];

        // 1) 웨이브 타입 결정
        var types = new WaveType[totalWaves];
        for (int i = 0; i < totalWaves; i++) types[i] = GetWaveType(i);

        // 2) 스테이지 총합 기준으로 "일반몹 총량"을 NORMAL + MIDDLE_BOSS 웨이브에 배분
        int fullTotal = FullSpawnCount(); // 스테이지 전체 몬스터 수(보스 포함)
        int reservedBosses = 0;
        for (int i = 0; i < totalWaves; i++)
        {
            if (types[i] == WaveType.MIDDLE_BOSS) reservedBosses += MiddleBossCount;
            else if (types[i] == WaveType.BOSS) reservedBosses += BossCount;
        }
        int normalBudget = Mathf.Max(0, fullTotal - reservedBosses);

        // NORMAL + MIDDLE_BOSS 타깃만 모아서 웨이브별 "일반몹 개수"를 진행도 가중치로 분배
        int[] normalCounts = AllocateNormalBudgetAcrossWaves(types, normalBudget);

        // 3) 웨이브 구성
        for (int i = 0; i < totalWaves; i++)
        {
            var type = types[i];
            var w = new WaveInfo(type, i) { wavePieces = new List<WavePiece>(4) };
            Debug.Log($"{i} {type}");
            if (type == WaveType.BOSS)
            {
                // 최종 보스 라운드: 보스만
                if (!Equals(BossMinion, default(MinionIndex)))
                    w.wavePieces.Add(new WavePiece(Mathf.Max(1, BossCount), BossMinion));
            }
            else if (type == WaveType.MIDDLE_BOSS)
            {
                // 중간보스 라운드: 중간보스 + 일반몹
                if (!Equals(middleBossMinion, default(MinionIndex)))
                    w.wavePieces.Add(new WavePiece(Mathf.Max(1, MiddleBossCount), middleBossMinion));

                BuildNormalWavePieces(i, totalWaves, normalCounts[i], ref w.wavePieces);
            }
            else
            {
                // 일반 라운드: 전부 일반몹
                BuildNormalWavePieces(i, totalWaves, normalCounts[i], ref w.wavePieces);
            }

            waveInfos[i] = w;
        }
        return waveInfos;
    }

    public int GetWaveMax() => GetWaveLength(lengthType);

    // ====== Rules ======
    WaveType GetWaveType(int waveIndexZeroBased)
    {
        int total = GetWaveLength(lengthType);
        if (total <= 0) return WaveType.NORMAL;

        int i1 = waveIndexZeroBased + 1;   // 1-based로 변환

        if (i1 == total) return WaveType.BOSS;        // 마지막 라운드
        if (i1 % 5 == 0) return WaveType.MIDDLE_BOSS; // 매 5라운드
        return WaveType.NORMAL;
    }

    int GetWaveLength(StageLengthType lt)
    {
        switch (lt)
        {
            case StageLengthType.TARGET_5: return 5;
            case StageLengthType.TARGET_10: return 10;
            case StageLengthType.TARGET_15: return 15;
            case StageLengthType.TARGET_20: return 20;
            default: return 5;
        }
    }

    int FullSpawnCount()
    {
        return fullSpawnCountType switch
        {
            FullSpawnCountType.TARGET_50 => 50,
            FullSpawnCountType.TARGET_100 => 100,
            FullSpawnCountType.TARGET_200 => 200,
            FullSpawnCountType.TARGET_300 => 300,
            FullSpawnCountType.TARGET_400 => 400,
            FullSpawnCountType.TARGET_500 => 500,
            FullSpawnCountType.TARGET_600 => 600,
            FullSpawnCountType.TARGET_700 => 700,
            FullSpawnCountType.TARGET_800 => 800,
            FullSpawnCountType.TARGET_900 => 900,
            FullSpawnCountType.TARGET_1000 => 1000, // ← 오타 수정!
            _ => 100,
        };
    }

    // ====== Budget Allocation Across Waves ======
    // NORMAL + MIDDLE_BOSS 웨이브에 normalBudget를 진행도 가중치로 분배(최대잔여법)
    int[] AllocateNormalBudgetAcrossWaves(WaveType[] types, int normalBudget)
    {
        int nWaves = types.Length;
        var outCounts = new int[nWaves];
        if (normalBudget <= 0) return outCounts;

        // 대상 웨이브 수집
        List<int> targets = new();
        for (int i = 0; i < nWaves; i++)
            if (types[i] == WaveType.NORMAL || types[i] == WaveType.MIDDLE_BOSS)
                targets.Add(i);

        if (targets.Count == 0) return outCounts;

        // 각 웨이브의 진행도(뒤로 갈수록 무겁게)
        float[] weights = new float[nWaves];
        float sum = 0f;

        // 랭크 합(정규화 보조)
        float RankSum(int count, float exp)
        {
            float s = 0f;
            for (int k = 0; k < count; k++) s += Mathf.Pow(k + 1, exp);
            return s;
        }
        float rsum = RankSum(targets.Count, rankExponent);

        for (int t = 0; t < targets.Count; t++)
        {
            int wi = targets[t];

            float progress01 = (nWaves > 1) ? (float)wi / (nWaves - 1) : 0f;
            float s = Mathf.Clamp01(progressionStrength * progress01);

            // 균등(1)과 랭크 정규화(뒤로 갈수록 큼)를 섞기
            float rankNorm = Mathf.Pow(t + 1, rankExponent) / Mathf.Max(1e-6f, rsum);
            float w = (1f - s) * 1f + s * (rankNorm * targets.Count);
            weights[wi] = w;
            sum += w;
        }

        // 최대잔여법
        int used = 0;
        var rema = new List<(float frac, int idx)>(targets.Count);
        for (int t = 0; t < targets.Count; t++)
        {
            int wi = targets[t];
            float exact = normalBudget * (weights[wi] / Mathf.Max(1e-6f, sum));
            int floor = Mathf.FloorToInt(exact);
            outCounts[wi] = floor;
            used += floor;
            rema.Add((exact - floor, wi));
        }

        int left = normalBudget - used;
        // 동률 깨기: 결정적 랜덤
        var rnd = new System.Random(SpawnSeed ^ (StageIndex * 73856093));
        rema.Sort((a, b) =>
        {
            int cmp = b.frac.CompareTo(a.frac);
            return (cmp != 0) ? cmp : rnd.Next(-1, 2);
        });

        for (int k = 0; k < left; k++)
            outCounts[rema[k % rema.Count].idx]++;

        return outCounts;
    }

    // ====== Normal Wave – 내부 미니언 비율 분해 ======
    void BuildNormalWavePieces(int waveIndex, int totalWaves, int totalCount, ref List<WavePiece> outPieces)
    {
        if (totalCount <= 0) return;
        if (defaultMinions == null || defaultMinions.Length == 0) return;

        int n = defaultMinions.Length;

        // 진행도
        float progress01 = (totalWaves > 1) ? (float)waveIndex / (totalWaves - 1) : 0f;
        float s = Mathf.Clamp01(progressionStrength * progress01);

        // 아래쪽(강한)으로 갈수록 가중↑
        float rankSum = 0f;
        float[] rankW = new float[n];
        for (int j = 0; j < n; j++) { rankW[j] = Mathf.Pow(j + 1, rankExponent); rankSum += rankW[j]; }

        // 최종 가중치 = 균등과 랭크를 섞음
        float[] weights = new float[n];
        for (int j = 0; j < n; j++)
        {
            float uniform = 1f;
            float rankNorm = rankW[j] / Mathf.Max(1e-6f, rankSum);
            weights[j] = (1f - s) * uniform + s * (rankNorm * n);
        }

        AllocateByWeights(totalCount, weights,
            new System.Random(SpawnSeed ^ (StageIndex * 19349663) ^ waveIndex),
            defaultMinions, outPieces);
    }

    static void AllocateByWeights(int totalCount, float[] weights, System.Random rnd, MinionIndex[] minions, List<WavePiece> outPieces)
    {
        int n = weights.Length;
        float sum = 0f; for (int i = 0; i < n; i++) sum += Mathf.Max(0f, weights[i]);
        if (sum <= 0f) return;

        int used = 0;
        int[] floors = new int[n];
        (float frac, int idx)[] rema = new (float, int)[n];

        for (int i = 0; i < n; i++)
        {
            float exact = totalCount * (Mathf.Max(0f, weights[i]) / sum);
            int floor = Mathf.FloorToInt(exact);
            floors[i] = floor; used += floor;
            rema[i] = (exact - floor, i);
        }

        int left = totalCount - used;
        Array.Sort(rema, (a, b) =>
        {
            int cmp = b.frac.CompareTo(a.frac);
            return (cmp != 0) ? cmp : rnd.Next(-1, 2);
        });

        for (int k = 0; k < left; k++) floors[rema[k % n].idx]++;

        for (int i = 0; i < n; i++)
            if (floors[i] > 0)
                outPieces.Add(new WavePiece(floors[i], minions[i]));
    }
}

[Serializable]
public class WaveInfo
{
    public int WaveIndex = 0;
    public WaveType waveType;
    public List<WavePiece> wavePieces;

    public WaveInfo(WaveType waveType, int index)
    {
        WaveIndex = index;
        this.waveType = waveType;
        wavePieces = new List<WavePiece>(4);
    }
}

[Serializable]
public class WavePiece
{
    public int Count;
    public MinionIndex minionIndex;

    public WavePiece(int Count, MinionIndex minionIndex)
    {
        this.Count = Count;
        this.minionIndex = minionIndex;
    }
}
