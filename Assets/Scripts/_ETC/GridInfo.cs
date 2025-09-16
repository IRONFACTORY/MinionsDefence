using System;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
using UnityEngine.Rendering; // CompareFunction
#endif

public enum GridStats { EMPTY_ZONE = 0, BUILDZONE_EMPTY = 1, BUILDZONE_BUILTED = 2, BUILD_UNABLE_ZONE = 3, }
public enum GridType { EMPTY_ZONE = 0, BUILD_ZONE, ENEMY_SPAWN_ZONE, }

[Serializable]
public struct Cell
{
    public int r, c, index;   // 표시 인덱스 저장(topLeftOrigin 반영)
    public Vector3 worldCenter;
    public Rect worldRect;
    public GridStats gridStats;
    public GridType gridType;
}

[ExecuteAlways]
public class GridInfo : MonoBehaviour
{
    [Header("인덱스 표기 옵션")]
    [SerializeField] bool topLeftOrigin = false;

    public bool TopLeftOrigin() => topLeftOrigin;

    [Header("월드 그리드(Transform 기반)")]
    [SerializeField, Min(1)] int rows = 24;
    [SerializeField, Min(1)] int cols = 14;
    [SerializeField] Vector2 cellSize = new(0.25f, 0.25f);
    [SerializeField] Vector2 spacing = Vector2.zero;

    public Vector2 GetCellSize() { return cellSize; }
    public Vector2 GetCellSpacing() { return spacing; }

    [Header("원점 설정")]
    [SerializeField] bool useBottomLeftOrigin = true;
    [SerializeField] Vector2 bottomLeftOrigin = new(-3.25f, 0.25f);
    [SerializeField] bool useCenterOrigin = false;
    [SerializeField] Vector2 centerOrigin = Vector2.zero;

    [Header("밴드 규칙 (아래→위)")]
    [SerializeField] List<int> bandHeights = new() { 4, 4, 4 };
    [SerializeField] List<int> bandValues = new() { 0, 1, 0 }; // 0~3 = GridStats 값
    [SerializeField, Range(0, 3)] int defaultAboveBandsValue = 3;

    [Header("Editor Helpers")]
    [SerializeField] bool autoBuild = true;
    [SerializeField] bool drawGizmos = true;

    [Header("Gizmo Colors")]
    [SerializeField] Color colEmpty = new(0.8f, 0.8f, 0.8f, 0.18f);
    [SerializeField] Color colBuildable = new(0f, 1f, 0f, 0.22f);
    [SerializeField] Color colBuilt = new(0.1f, 0.6f, 1f, 0.28f);
    [SerializeField] Color colBlocked = new(1f, 0f, 0f, 0.18f);

    public int Rows => rows;
    public int Cols => cols;
    public Cell[,] Cells { get; private set; }
    public bool[,] CellBuilted { get; private set; }

    // 타입별 인덱스 버킷(외부에서 읽기 전용으로 제공)
    readonly List<Cell> emptyCells = new();   // ★ Vector2Int → Cell
    readonly List<Cell> buildCells = new();
    readonly List<Cell> enemyCells = new();

    public IReadOnlyList<Cell> EmptyCells => emptyCells;
    public IReadOnlyList<Cell> BuildZoneCells => buildCells;
    public IReadOnlyList<Cell> EnemySpawnCells => enemyCells;

    // 캐시
    Transform tr;
    Vector3 cachedBottomLeft;
    bool dirty;

    #region Unity Lifecycle
    void Awake() { tr = transform; if (autoBuild) Build(); }
    void OnEnable() { if (autoBuild) Build(); }
    void OnValidate()
    {
        rows = Mathf.Max(1, rows);
        cols = Mathf.Max(1, cols);
        tr ??= transform;
        MarkDirty();
        if (autoBuild) Build();
    }
    #endregion

    #region Build & Geometry
    public void Build()
    {
        cachedBottomLeft = ComputeBottomLeft();
        Cells = new Cell[rows, cols];

        float stepX = cellSize.x + spacing.x;
        float stepY = cellSize.y + spacing.y;

        for (int rFromBottom = 0; rFromBottom < rows; rFromBottom++)
        {
            for (int c = 0; c < cols; c++)
            {
                int rr = topLeftOrigin ? (rows - 1 - rFromBottom) : rFromBottom;
                int idx = rFromBottom * cols + c;

                float xMin = cachedBottomLeft.x + c * stepX;
                float yMin = cachedBottomLeft.y + rFromBottom * stepY;

                Vector3 ctr = new(xMin + cellSize.x * 0.5f, yMin + cellSize.y * 0.5f, 0f);
                Rect rect = new(xMin, yMin, cellSize.x, cellSize.y);

                GridStats stat = GetGridStatFromNumericBands(rFromBottom);
                GridType type = MapStatToType(stat);
                Cells[rr, c] = new Cell
                {
                    r = rr,
                    c = c,
                    index = idx,
                    worldCenter = ctr,
                    worldRect = rect,
                    gridStats = stat,
                    gridType = type
                };
            }
        }

        RebuildTypeBuckets(); // 전량 버킷 재구성
        dirty = false;
    }

    public void RebuildIfDirty()
    {
        if (dirty || Cells == null || Cells.GetLength(0) != rows || Cells.GetLength(1) != cols)
            Build();
    }

    void MarkDirty() => dirty = true;

    Vector3 ComputeBottomLeft()
    {
        float totalW = cols * cellSize.x + (cols - 1) * spacing.x;
        float totalH = rows * cellSize.y + (rows - 1) * spacing.y;
        if (useCenterOrigin)
            return (Vector3)centerOrigin + new Vector3(-totalW * 0.5f, -totalH * 0.5f, 0f);
        if (useBottomLeftOrigin)
            return bottomLeftOrigin;
        return transform.position + new Vector3(-totalW * 0.5f, -totalH * 0.5f, 0f);
    }
    #endregion

    #region Band Rules & Mapping
    GridStats GetGridStatFromNumericBands(int rFromBottom)
    {
        int acc = 0;
        int count = Mathf.Min(bandHeights?.Count ?? 0, bandValues?.Count ?? 0);
        for (int i = 0; i < count; i++)
        {
            int h = Mathf.Max(0, bandHeights[i]);
            int v = Mathf.Clamp(bandValues[i], 0, 3);
            if (rFromBottom < acc + h) return (GridStats)v;
            acc += h;
        }
        return (GridStats)Mathf.Clamp(defaultAboveBandsValue, 0, 3);
    }

    // 요청한 규칙:
    // GridStats.EMPTY_ZONE        → GridType.EMPTY_ZONE
    // GridStats.BUILD_UNABLE_ZONE → GridType.ENEMY_SPAWN_ZONE
    // 나머지(BUILDZONE_EMPTY/BUILTED) → GridType.BUILD_ZONE
    static GridType MapStatToType(GridStats s)
    {
        return s switch
        {
            GridStats.EMPTY_ZONE => GridType.EMPTY_ZONE,
            GridStats.BUILD_UNABLE_ZONE => GridType.ENEMY_SPAWN_ZONE,
            _ => GridType.BUILD_ZONE,
        };
    }
    #endregion

    #region Buckets (type별 목록 유지)
    public void RebuildTypeBuckets()
    {
        emptyCells.Clear();
        buildCells.Clear();
        enemyCells.Clear();

        if (Cells == null) return;

        for (int r = 0; r < rows; r++)
            for (int c = 0; c < cols; c++)
            {
                var cell = Cells[r, c];
                switch (cell.gridType)
                {
                    case GridType.EMPTY_ZONE: emptyCells.Add(cell); break;
                    case GridType.BUILD_ZONE:
                        buildCells.Add(cell);
                        break;
                    case GridType.ENEMY_SPAWN_ZONE: enemyCells.Add(cell); break;
                }
            }
    }
    public List<Vector2> GetEnemySpawnPositions()
    {
        var cells = enemyCells;
        var result = new List<Vector2>(cells.Count);
        for (int i = 0; i < cells.Count; i++)
        {
            if (TryGridToWorldCenter(cells[i].r, cells[i].c, out var center))
                result.Add((Vector2)center);
        }

        return result;
    }

    void UpdateTypeBucketOnChange(Cell cell, GridType oldType, GridType newType)
    {
        if (oldType == newType) return;

        // 먼저 예전 버킷에서 제거
        switch (oldType)
        {
            case GridType.EMPTY_ZONE: RemoveFromBucket(emptyCells, cell); break;
            case GridType.BUILD_ZONE: RemoveFromBucket(buildCells, cell); break;
            case GridType.ENEMY_SPAWN_ZONE: RemoveFromBucket(enemyCells, cell); break;
        }
        // 새 버킷에 추가
        switch (newType)
        {
            case GridType.EMPTY_ZONE: emptyCells.Add(cell); break;
            case GridType.BUILD_ZONE: buildCells.Add(cell); break;
            case GridType.ENEMY_SPAWN_ZONE: enemyCells.Add(cell); break;
        }
    }

    #endregion

    #region Grid <-> World Utilities
    public bool IsInside(int r, int c) => r >= 0 && c >= 0 && r < rows && c < cols;
    public bool WorldToGrid(Vector2 worldPos, out Vector2Int cellPos)
    {
        cellPos = default;

        // 그리드 범위 검사
        if (!IsInsideWorld(worldPos))
            return false;

        float stepX = cellSize.x + spacing.x;
        float stepY = cellSize.y + spacing.y;

        int c = Mathf.FloorToInt((worldPos.x - cachedBottomLeft.x) / stepX);
        int r = Mathf.FloorToInt((worldPos.y - cachedBottomLeft.y) / stepY);

        if (topLeftOrigin)
            r = rows - 1 - r;

        // 범위 검사
        if (c < 0 || c >= cols || r < 0 || r >= rows)
            return false;

        cellPos = new Vector2Int(c, r);
        return true;
    }

    public bool IsInsideWorld(Vector2 worldPos)
    {
        float width = cols * (cellSize.x + spacing.x);
        float height = rows * (cellSize.y + spacing.y);

        float minX = cachedBottomLeft.x;
        float minY = cachedBottomLeft.y;
        float maxX = minX + width;
        float maxY = minY + height;

        return (worldPos.x >= minX && worldPos.x < maxX &&
                worldPos.y >= minY && worldPos.y < maxY);
    }


    public int ToIndex(int c, int r) => r * cols + c;
    public bool TryWorldToGrid(Vector3 world, out int r, out int c)
    {
        RebuildIfDirty();
        float stepX = cellSize.x + spacing.x;
        float stepY = cellSize.y + spacing.y;

        float dx = world.x - cachedBottomLeft.x;
        float dy = world.y - cachedBottomLeft.y;

        c = Mathf.FloorToInt(dx / stepX);
        int rFromBottom = Mathf.FloorToInt(dy / stepY);
        if (c < 0 || c >= cols || rFromBottom < 0 || rFromBottom >= rows) { r = c = -1; return false; }

        r = topLeftOrigin ? (rows - 1 - rFromBottom) : rFromBottom;
        return true;
    }

    public bool TryGridToWorldCenter(int r, int c, out Vector3 center)
    {
        RebuildIfDirty();
        center = default;
        if (!IsInside(r, c)) return false;

        int rFromBottom = topLeftOrigin ? (rows - 1 - r) : r;
        float stepX = cellSize.x + spacing.x;
        float stepY = cellSize.y + spacing.y;

        float xMin = cachedBottomLeft.x + c * stepX;
        float yMin = cachedBottomLeft.y + rFromBottom * stepY;

        center = new Vector3(xMin + cellSize.x * 0.5f, yMin + cellSize.y * 0.5f, 0f);
        return true;
    }

    public bool TryGetCellRect(int r, int c, out Rect rect)
    {
        RebuildIfDirty();
        rect = default;
        if (!IsInside(r, c)) return false;

        int rFromBottom = topLeftOrigin ? (rows - 1 - r) : r;
        float stepX = cellSize.x + spacing.x;
        float stepY = cellSize.y + spacing.y;

        float xMin = cachedBottomLeft.x + c * stepX;
        float yMin = cachedBottomLeft.y + rFromBottom * stepY;

        rect = new Rect(xMin, yMin, cellSize.x, cellSize.y);
        return true;
    }
    #endregion

    #region Runtime Mutations (stats/type 동시 일관성 유지)
    // 숫자(0~3)로 직접 세팅
    public bool TrySetGridStatByNumber(int r, int c, int value)
    {
        if (!IsInside(r, c) || Cells == null) return false;
        value = Mathf.Clamp(value, 0, 3);

        var cell = Cells[r, c];
        var oldType = cell.gridType;

        cell.gridStats = (GridStats)value;
        cell.gridType = MapStatToType(cell.gridStats);

        // ★ 원본 반영
        Cells[r, c] = cell;

        // 버킷 업데이트
        UpdateTypeBucketOnChange(cell, oldType, cell.gridType);
        return true;
    }

    public bool TrySetBuilt(int r, int c, bool built)
    {
        if (!IsInside(r, c) || Cells == null) return false;

        var cell = Cells[r, c];
        if (cell.gridType != GridType.BUILD_ZONE) return false;

        var oldType = cell.gridType;

        cell.gridStats = built ? GridStats.BUILDZONE_BUILTED : GridStats.BUILDZONE_EMPTY;
        cell.gridType = MapStatToType(cell.gridStats); // 보통 BUILD_ZONE 유지

        // ★ 원본 반영
        Cells[r, c] = cell;

        // 버킷 업데이트
        UpdateTypeBucketOnChange(cell, oldType, cell.gridType);
        return true;
    }

    #endregion

    #region Gizmos
    void OnDrawGizmos()
    {
        if (!drawGizmos || rows <= 0 || cols <= 0) return;
        RebuildIfDirty();

        float stepX = cellSize.x + spacing.x;
        float stepY = cellSize.y + spacing.y;

        cachedBottomLeft = ComputeBottomLeft();

#if UNITY_EDITOR
        // ✅ 라벨 스타일(가독성 강화)
        var style = new GUIStyle(EditorStyles.boldLabel)
        {
            fontSize = 11,
            alignment = TextAnchor.MiddleCenter,
            richText = true
        };

        // ✅ 라벨이 항상 보이도록(피킹/깊이 무시)
        Handles.zTest = CompareFunction.Always;
#endif

        for (int rFromBottom = 0; rFromBottom < rows; rFromBottom++)
        {
            for (int c = 0; c < cols; c++)
            {
                float xMin = cachedBottomLeft.x + c * stepX;
                float yMin = cachedBottomLeft.y + rFromBottom * stepY;

                Vector3 center = new(xMin + cellSize.x * 0.5f, yMin + cellSize.y * 0.5f, 0f);
                Vector3 size = new(cellSize.x, cellSize.y, 0.001f);

                int rr = topLeftOrigin ? (rows - 1 - rFromBottom) : rFromBottom;
                var stat = (Cells != null) ? Cells[rr, c].gridStats : GetGridStatFromNumericBands(rFromBottom);

                Gizmos.color = stat switch
                {
                    GridStats.EMPTY_ZONE => colEmpty,
                    GridStats.BUILDZONE_EMPTY => colBuildable,
                    GridStats.BUILDZONE_BUILTED => colBuilt,
                    GridStats.BUILD_UNABLE_ZONE => colBlocked,
                    _ => Color.magenta
                };
                Gizmos.DrawCube(center, size);

                Gizmos.color = Color.black;
                Gizmos.DrawWireCube(center, size);

#if UNITY_EDITOR
                // ✅ 보기 쉬운 좌표 표기: (col,row) + 실제 셀 인덱스
                string label = $"<b>({c},{rr})</b>";
                Handles.Label(center, label, style);
#endif
            }
        }
    }
    #endregion
    // GridInfo 내부 도우미(위에서 쓸 통행 판정)
    public bool IsWalkable(int r, int c)
    {
        if (!IsInside(r, c)) return false;
        var s = Cells[r, c].gridStats;
        // 막힘: BUILTED / UNABLE, 통과: EMPTY / BUILDZONE_EMPTY
        return s != GridStats.BUILDZONE_BUILTED;
    }
    // === GridOverlay 호환용 추가 ===

    // GridOverlay가 기대하던 프로퍼티명
    public Vector2 CellSize => cellSize;

    // GridOverlay에서 호출하던 이름과 시그니처 맞춤
    public bool TryWorldToCell(Vector3 world, out int r, out int c)
    {
        return TryWorldToGrid(world, out r, out c);
    }

    // BUILD_ZONE 셀들의 월드 중심 좌표 나열 (Overlay 생성용)
    public IEnumerable<Vector2> BuildZoneCenters()
    {
        RebuildIfDirty();
        var list = BuildZoneCells; // 이미 유지 중인 버킷
        for (int i = 0; i < list.Count; i++)
            yield return (Vector2)list[i].worldCenter;
    }

    // 드래그 중 판정용(빌드 가능 여부)
    public bool IsBuildableCell(int r, int c)
    {
        if (!IsInside(r, c)) return false;
        // BUILD_ZONE이면서 이미 지어진 상태가 아닌 경우만 허용
        var s = Cells[r, c].gridStats;
        return Cells[r, c].gridType == GridType.BUILD_ZONE && s != GridStats.BUILDZONE_BUILTED;
    }

    // (r,c) -> 월드 중심 (Vector2) 버전
    public bool TryGetCellCenter2D(int r, int c, out Vector2 center)
    {
        center = default;
        if (!TryGridToWorldCenter(r, c, out var ctr3)) return false;
        center = (Vector2)ctr3;
        return true;
    }


    public void OnBeginDrag()
    {

    }

    public void OnDragReachedOther()
    {

    }

    public void OnEndDrag()
    {

    }

    void RemoveFromBucket(List<Cell> list, Cell cell)
    {
        int idx = list.FindIndex(x => x.r == cell.r && x.c == cell.c);
        if (idx >= 0) list.RemoveAt(idx);
    }
    #region Context Menu
    [ContextMenu("Rebuild Now")] void ContextRebuildNow() => Build();
    [ContextMenu("Mark Dirty (Force Rebuild Next)")] void ContextMarkDirty() => MarkDirty();
    #endregion
}
