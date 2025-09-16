using System.Collections.Generic;
using UnityEngine;

public class BuildAbleGridHead : MonoBehaviour
{
    [Header("Refs")]
    public GridInfo gridInfo;                 // GridInfo 참조
    public BuildAbleGrid[] grids;             // 수동배치한 오브젝트들 (Snap & Record 대상)
    public BuildAbleGrid gridPrefab;          // Auto Fill 생성용 프리팹
    Dictionary<Vector2Int, BuildAbleGrid> _lookup;
    public Transform spawnParent;             // 생성 시 부모(없으면 this.transform)

    [Header("Placement")]
    [Tooltip("셀 중심에서 살짝 안쪽으로 밀고 싶을 때 (0이면 중앙)")]
    public Vector2 inset = Vector2.zero;      // 월드 유닛 기준
    [Tooltip("Auto Fill로 배치된 오브젝트를 구분하기 위한 태그 문자열")]
    public string autoTag = "AUTO_GRID";

    public void ResetStage() { }
    public void ReBuild() { }
    public void OnBuild() { }

    public void Show(bool tru) => gameObject.SetActive(tru);

    // 🔹 캐시 재구성: grids 배열 + spawnParent 자식 모두 스캔
    public void RebuildLookup()
    {
        _lookup = new Dictionary<Vector2Int, BuildAbleGrid>();

        // 1) 배열에서
        if (grids != null)
        {
            foreach (var g in grids)
            {
                if (!g) continue;
                _lookup[g.CellRC] = g; // (r,c) 키
            }
        }

        // 2) 부모 아래 자식에서(오토필된 것 포함)
        var parent = spawnParent ? spawnParent : transform;
        var children = parent.GetComponentsInChildren<BuildAbleGrid>(true);
        foreach (var g in children)
        {
            if (!g) continue;
            _lookup[g.CellRC] = g; // 중복이면 마지막 값으로 갱신
        }
    }

    // 🔹 외부에서 좌표로 빠르게 찾아 표시 바꾸기
    public void UpdateStats(List<Vector2Int> targetCells, OverlayFilter overlayFilter)
    {
        if (targetCells == null || targetCells.Count == 0) return;
        if (_lookup == null) RebuildLookup();

        foreach (var rc in targetCells)
        {
            // (r,c) 순서 주의!
            if (!gridInfo.IsInside(rc.x, rc.y)) continue;

            if (_lookup.TryGetValue(rc, out var obj) && obj != null)
            {
                obj.SetFillter(overlayFilter);
            }
            else
            {
                // 필요하면 로그
                // Debug.Log($"No BuildAbleGrid at {rc}");
            }
        }
    }

}