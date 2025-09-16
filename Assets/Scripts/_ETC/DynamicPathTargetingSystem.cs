using UnityEngine;
using System.Collections.Generic;
using System.Collections;

// 동적 경로 변경에 대응하는 하이브리드 타겟팅 시스템
public class DynamicPathTargetingSystem : MonoBehaviour
{
    [Header("성능 설정")]
    public int maxChecksPerFrame = 100;
    public float cacheUpdateInterval = 0.2f;
    public float gridCellSize = 2f;

    // 하이브리드 접근: 그리드 + 우선순위 큐
    readonly Dictionary<Vector2Int, List<MinionObject>> _grid = new();
    readonly List<MinionObject> _allEnemies = new(1000);

    // 타워별 캐시 (여전히 유용)
    readonly Dictionary<IPlayerUnit, TowerCache> _towerCaches = new();

    // 경로 변경 감지
    int _pathVersion = 0;
    bool _pathChanged = false;

    // 코루틴
    Coroutine _updateCoroutine;

    void Start()
    {
        _updateCoroutine = StartCoroutine(UpdateCachesCoroutine());
    }

    void OnDestroy()
    {
        if (_updateCoroutine != null)
            StopCoroutine(_updateCoroutine);
    }

    // 경로 변경 알림 (건물 건설 시 호출)
    public void NotifyPathChanged()
    {
        _pathChanged = true;
        _pathVersion++;

        // 모든 적의 경로 무효화
        foreach (var enemy in _allEnemies)
        {
            if (enemy != null)
                enemy.InvalidatePath();
        }

        // 타워 캐시 무효화
        foreach (var cache in _towerCaches.Values)
        {
            cache.isValid = false;
        }
    }

    // 적 등록
    public void RegisterEnemy(MinionObject enemy)
    {
        _allEnemies.Add(enemy);
        UpdateEnemyGrid(enemy);
    }

    // 적 제거
    public void UnregisterEnemy(MinionObject enemy)
    {
        _allEnemies.Remove(enemy);
        RemoveFromGrid(enemy);

        // 캐시에서 제거
        foreach (var cache in _towerCaches.Values)
        {
            if (cache.primaryTarget == enemy)
                cache.primaryTarget = null;
            cache.nearbyEnemies.Remove(enemy);
        }
    }

    // 적 이동 시 그리드 업데이트
    public void UpdateEnemyPosition(MinionObject enemy)
    {
        UpdateEnemyGrid(enemy);
    }

    void UpdateEnemyGrid(MinionObject enemy)
    {
        var newGridPos = WorldToGrid(enemy.transform.position);

        // 기존 그리드에서 제거
        RemoveFromGrid(enemy);

        // 새 그리드에 추가
        if (!_grid.TryGetValue(newGridPos, out var list))
        {
            list = new List<MinionObject>(8);
            _grid[newGridPos] = list;
        }
        list.Add(enemy);
        enemy.testCellVector = newGridPos;
    }

    void RemoveFromGrid(MinionObject enemy)
    {
        if (_grid.TryGetValue(enemy.testCellVector, out var list))
        {
            list.Remove(enemy);
            if (list.Count == 0)
                _grid.Remove(enemy.testCellVector);
        }
    }

    Vector2Int WorldToGrid(Vector2 worldPos)
    {
        return new Vector2Int(
            Mathf.FloorToInt(worldPos.x / gridCellSize),
            Mathf.FloorToInt(worldPos.y / gridCellSize)
        );
    }

    // 타워 등록
    public void RegisterTower(IPlayerUnit tower)
    {
        _towerCaches[tower] = new TowerCache
        {
            tower = tower,
            primaryTarget = null,
            nearbyEnemies = new List<MinionObject>(20),
            lastUpdateTime = -1f,
            isValid = false
        };
    }

    public void UnregisterTower(IPlayerUnit tower)
    {
        _towerCaches.Remove(tower);
    }

    // 메인 타겟 검색
    public MinionObject GetBestTarget(IPlayerUnit tower)
    {
        if (!_towerCaches.TryGetValue(tower, out var cache))
            return null;

        // 현재 타겟이 여전히 유효한지 확인
        if (cache.primaryTarget != null &&
            cache.primaryTarget.GetAlive() &&
            IsInRange(tower.transform.position, cache.primaryTarget.GetMyPos(), tower.GetRange()))
        {
            return cache.primaryTarget;
        }

        // 캐시된 근처 적들 중에서 빠른 검색
        if (cache.isValid && Time.time - cache.lastUpdateTime < cacheUpdateInterval)
        {
            return FindBestFromCachedEnemies(tower, cache);
        }

        return cache.primaryTarget;
    }

    MinionObject FindBestFromCachedEnemies(IPlayerUnit tower, TowerCache cache)
    {
        var towerPos = tower.transform.position;
        float range = tower.GetRange();
        MinionObject best = null;
        float bestScore = float.MaxValue;

        for (int i = cache.nearbyEnemies.Count - 1; i >= 0; i--)
        {
            var enemy = cache.nearbyEnemies[i];

            // 죽은 적 제거
            if (enemy == null || !enemy.GetAlive())
            {
                cache.nearbyEnemies.RemoveAt(i);
                continue;
            }

            // 범위 체크
            if (!IsInRange(towerPos, enemy.GetMyPos(), range))
            {
                cache.nearbyEnemies.RemoveAt(i);
                continue;
            }

            float score = CalculateTargetScore(tower, enemy, towerPos);
            if (score < bestScore)
            {
                bestScore = score;
                best = enemy;
            }
        }

        return best;
    }

    // 코루틴으로 백그라운드 캐시 업데이트
    IEnumerator UpdateCachesCoroutine()
    {
        var towerList = new List<IPlayerUnit>();
        int towerIndex = 0;

        while (true)
        {
            // 활성 타워 리스트 갱신
            towerList.Clear();
            foreach (var kvp in _towerCaches)
            {
                if (kvp.Key != null && kvp.Key.gameObject.activeInHierarchy)
                    towerList.Add(kvp.Key);
            }

            if (towerList.Count > 0)
            {
                // 라운드 로빈으로 타워 캐시 업데이트
                int towersThisFrame = Mathf.Min(towerList.Count, maxChecksPerFrame / 20);
                for (int i = 0; i < towersThisFrame; i++)
                {
                    if (towerIndex >= towerList.Count)
                        towerIndex = 0;

                    UpdateTowerCache(towerList[towerIndex++]);
                }
            }

            yield return new WaitForSeconds(cacheUpdateInterval);
        }
    }

    void UpdateTowerCache(IPlayerUnit tower)
    {
        if (!_towerCaches.TryGetValue(tower, out var cache))
            return;

        var towerPos = tower.transform.position;
        var towerGridPos = WorldToGrid(towerPos);
        float range = tower.GetRange();
        int gridRange = Mathf.CeilToInt(range / gridCellSize) + 1;

        cache.nearbyEnemies.Clear();
        MinionObject bestTarget = null;
        float bestScore = float.MaxValue;

        // 주변 그리드 셀들 검사
        for (int dx = -gridRange; dx <= gridRange; dx++)
        {
            for (int dy = -gridRange; dy <= gridRange; dy++)
            {
                var checkPos = new Vector2Int(towerGridPos.x + dx, towerGridPos.y + dy);
                if (!_grid.TryGetValue(checkPos, out var enemies))
                    continue;

                foreach (var enemy in enemies)
                {
                    if (enemy == null || !enemy.GetAlive())
                        continue;

                    if (!IsInRange(towerPos, enemy.GetMyPos(), range))
                        continue;

                    cache.nearbyEnemies.Add(enemy);

                    float score = CalculateTargetScore(tower, enemy, towerPos);
                    if (score < bestScore)
                    {
                        bestScore = score;
                        bestTarget = enemy;
                    }
                }
            }
        }

        cache.primaryTarget = bestTarget;
        cache.lastUpdateTime = Time.time;
        cache.isValid = true;
    }

    float CalculateTargetScore(IPlayerUnit tower, MinionObject enemy, Vector2 towerPos)
    {
        float distance = Vector2.Distance(towerPos, enemy.GetMyPos());

        switch (tower.targetingStrategy)
        {
            case TargetingStrategy.Closest:
                return distance;

            case TargetingStrategy.Furthest:
                return -distance;

            case TargetingStrategy.LowestHP:
                return enemy.GetCurrentHP();

            case TargetingStrategy.HighestHP:
                return -enemy.GetCurrentHP();

            default:
                return distance;
        }
    }
    bool IsInRange(Vector2 pos1, Vector2 pos2, float range)
    {
        return Vector2.Distance(pos1, pos2) <= range;
    }

    // 범위 내 적 개수 (스플래시용)
    public int GetEnemyCountInRange(Vector2 center, float radius)
    {
        var centerGrid = WorldToGrid(center);
        int gridRange = Mathf.CeilToInt(radius / gridCellSize) + 1;
        int count = 0;
        float radiusSqr = radius * radius;

        for (int dx = -gridRange; dx <= gridRange; dx++)
        {
            for (int dy = -gridRange; dy <= gridRange; dy++)
            {
                var checkPos = new Vector2Int(centerGrid.x + dx, centerGrid.y + dy);
                if (!_grid.TryGetValue(checkPos, out var enemies))
                    continue;

                foreach (var enemy in enemies)
                {
                    if (enemy == null || !enemy.GetAlive())
                        continue;

                    float distSqr = (enemy.GetMyPos() - center).sqrMagnitude;
                    if (distSqr <= radiusSqr)
                        count++;
                }
            }
        }

        return count;
    }

    // 디버그 정보
    public void GetDebugInfo(out int activeEnemies, out int registeredTowers, out int gridCells)
    {
        activeEnemies = _allEnemies.Count;
        registeredTowers = _towerCaches.Count;
        gridCells = _grid.Count;
    }
}

// 타워 캐시 구조체
public class TowerCache
{
    public IPlayerUnit tower;
    public MinionObject primaryTarget;
    public List<MinionObject> nearbyEnemies;
    public float lastUpdateTime;
    public bool isValid;
}
