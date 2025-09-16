using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections.Generic;

public enum PlayerDraggingStats { NONE = 0, BUILD_ABLE, BUILD_UNABLE, }

public class FightPlayerController : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    public static FightPlayerController Inst;

    [SerializeField] TestPathVisualize testPathVisualize;
    Camera cam;               // 비우면 Camera.main
    [SerializeField] LayerMask towerMask;       // 맞출 대상 레이어
    [SerializeField] LayerMask towerSlotMask;     // 바닥/슬롯 등(필요 시)

    [SerializeField] BuildAbleGridHead buildAbleGridHead;
    [SerializeField] DragObject dragObject;

    TowerSlot draggingSlot;
    Vector2Int dragInt;
    bool isDragging;

    void Awake()
    {
        if (!cam) cam = Camera.main;
        Inst = this;
    }

    public void Initialize()
    {
        dragObject.Initialize();
        dragObject.SetDisable();
        ShowOverlay(false);
    }

    public void OnPointerDown(PointerEventData e)
    {

    }

    public void OnPointerUp(PointerEventData e)
    {
        if (isDragging)
            return;

        var targeted = GetTowerSlot(e.position);
    }

    public void OnBeginDrag(PointerEventData e)
    {
        isDragging = true;
        draggingSlot = GetTowerSlot(e.position);
        if (draggingSlot == null || !draggingSlot.IsEquiped())
        {
            isDragging = false;
            draggingSlot = null;
            return;
        }
        var tower = draggingSlot.GetRegisterdTower();
        draggingSlot.OnBeginDrag();
        dragObject.SetEnable(tower.towerIndex, e.position);
        gi.OnBeginDrag();

        buildAbleGridHead.ReBuild();
        ShowOverlay(true);

        var wp = ScreenToWorld2D(e.position);
        if (gi.WorldToGrid(wp, out Vector2Int cellPos))
        {
            if (dragInt == cellPos)
                return;
            dragInt = cellPos;// 새로
        }
    }

    public void OnDrag(PointerEventData e)
    {
        if (draggingSlot == null)
            return;

        Update2x2Ghost(e);
        // dragObject.UpdatePosition(e.position);
    }

    public void OnEndDrag(PointerEventData e)
    {
        if (draggingSlot == null)
            return;

        ShowOverlay(false);

        gi.OnEndDrag();
        draggingSlot.OnEndDrag();
        dragObject.SetDisable();

        var w = (Vector2)ScreenToWorld2D(e.position);
        if (TrySnap2x2(w, out var centerW, out var anchor))
        {
            var cells = GetFootprint(anchor, buildSize);
            DoBuildTower(draggingSlot, centerW, cells);
            MarkFootprintBuilt(anchor, buildSize, true);
            buildAbleGridHead.UpdateStats(cells, OverlayFilter.BUILTED);
            dragInt = anchor;
            testPathVisualize.UpdatePath();
        }

        isDragging = false;
        draggingSlot = null;
        SetDragValid(false);
    }

    void DoBuildTower(TowerSlot slot, Vector2 spawnPos, List<Vector2Int> footprintRC)
    {
        StageManager.Inst.DoBuildTower(slot.GetRegisterdTower(), spawnPos, footprintRC);
    }

    TowerSlot GetTowerSlot(Vector2 sp)
    {
        RaycastHit2D hit;
        if (Raycast2D(sp, towerSlotMask, out hit))
        {
            var target = hit.collider.GetComponent<TowerSlot>();
            if (!target.IsEquiped())
                return null;
            return target;
        }
        return null;
    }

    Vector3 ScreenToWorld2D(Vector2 screenPos, float zPlane = 0f)
    {
        var w = cam.ScreenToWorldPoint(new Vector3(screenPos.x, screenPos.y, -cam.transform.position.z));
        w.z = zPlane;
        return w;
    }
    Vector2 WorldToScreen2D(Vector3 worldPos) => cam.WorldToScreenPoint(worldPos);
    bool Raycast2D(Vector2 screenPos, LayerMask mask, out RaycastHit2D hit)
    {
        var world = ScreenToWorld2D(screenPos);
        hit = Physics2D.Raycast(world, Vector3.forward, 0f, mask);
        return hit.collider != null;
    }

    void ShowOverlay(bool tru) => buildAbleGridHead.Show(tru);

    GridInfo gi => StageManager.Inst.GetGridInfo();

    // === NEW: 2x2 빌드 사이즈 (회전 안씀) ===
    [SerializeField] Vector2Int buildSize = new Vector2Int(2, 2);

    // === NEW: 드래그 유효여부 색표시용(드래그 프리뷰에 메서드 있다면 활용) ===
    void SetDragValid(bool valid)
    {
        // dragObject에 이런 API가 있다 가정. 없으면 내부에서 SpriteRenderer 색을 바꾸는 식으로 구현.
        if (valid) dragObject.SetBuildAbles(PlayerDraggingStats.BUILD_ABLE);
        else dragObject.SetBuildAbles(PlayerDraggingStats.BUILD_UNABLE);
    }

    // === NEW: 마우스 월드 위치에서 2x2 스냅 계산 ===
    // 앵커(좌하단) cell (r0,c0) 및 4칸 모두 유효/경계 내인지 체크
    bool TrySnap2x2(Vector2 world, out Vector2 centerWorld, out Vector2Int anchorRC)
    {
        centerWorld = default;
        anchorRC = default;

        // 1) 월드→셀
        if (!gi.TryWorldToCell(world, out int r, out int c))
            return false;

        // 2) 후보 앵커 4개 (좌하단 기준)
        //    (r0,c0): (r-1,c-1)=hover가 우상, (r-1,c)=hover가 좌상,
        //              (r,c-1)=hover가 우하,  (r,c)=hover가 좌하
        Vector2Int[] candidates = {
        new Vector2Int(r - 1, c - 1),
        new Vector2Int(r - 1, c),
        new Vector2Int(r, c - 1),
        new Vector2Int(r, c),
    };

        // 3) 스코어링: top 우선(r0 == r 이면 hover가 윗줄)
        //    1차: topPreferred = (r0 == r) ? 0 : 1 (작을수록 우선)
        //    2차: 마우스와 2x2 중심 거리 (작을수록 우선)
        bool any = false;
        float bestScoreA = float.PositiveInfinity; // top 우선 점수
        float bestScoreB = float.PositiveInfinity; // 거리 점수
        Vector2 bestCenter = default;
        Vector2Int bestAnchor = default;

        foreach (var a in candidates)
        {
            int r0 = a.x;
            int c0 = a.y;

            // 2x2 범위 체크
            if (!gi.IsInside(r0, c0) || !gi.IsInside(r0, c0 + 1) || !gi.IsInside(r0 + 1, c0) || !gi.IsInside(r0 + 1, c0 + 1))
                continue;

            // 빌드 가능 4칸 검사
            if (!gi.IsBuildableCell(r0, c0) || !gi.IsBuildableCell(r0, c0 + 1) ||
                !gi.IsBuildableCell(r0 + 1, c0) || !gi.IsBuildableCell(r0 + 1, c0 + 1))
                continue;

            // 4칸 중심 계산
            if (!gi.TryGetCellCenter2D(r0, c0, out var p00)) continue;
            if (!gi.TryGetCellCenter2D(r0, c0 + 1, out var p01)) continue;
            if (!gi.TryGetCellCenter2D(r0 + 1, c0, out var p10)) continue;
            if (!gi.TryGetCellCenter2D(r0 + 1, c0 + 1, out var p11)) continue;

            var center = (p00 + p01 + p10 + p11) * 0.25f;

            // 점수 계산
            float scoreA = (r0 == r) ? 0f : 1f;                      // 윗줄 우선
            float scoreB = (center - world).sqrMagnitude;            // 가까운 중심 우선

            // 더 좋은 후보면 갱신
            if (scoreA < bestScoreA || (Mathf.Approximately(scoreA, bestScoreA) && scoreB < bestScoreB))
            {
                any = true;
                bestScoreA = scoreA;
                bestScoreB = scoreB;
                bestCenter = center;
                bestAnchor = new Vector2Int(r0, c0);
            }
        }

        if (!any) return false;

        centerWorld = bestCenter;
        anchorRC = bestAnchor;
        return true;
    }

    void SetDragState(PlayerDraggingStats s)
    {
        dragObject.SetBuildAbles(s); // 내부에서 색상(흰/초록/빨강) 틴트 처리한다고 가정
    }
    // === NEW: 스냅 계산 + 프리뷰 이동(월드→스크린 변환해서 dragObject 유지) ===
    void Update2x2Ghost(PointerEventData e)
    {
        var w = (Vector2)ScreenToWorld2D(e.position);

        // 1) 그리드 바깥이면: 위치는 마우스 따라가되, 흰색(중립) 유지
        if (!gi.IsInsideWorld(w))
        {
            dragObject.UpdatePosition(e.position);
            SetDragState(PlayerDraggingStats.NONE);
            dragInt = new Vector2Int(-1, -1);
            return;
        }

        // 2) 내부면: 스냅 시도
        if (TrySnap2x2(w, out var centerW, out var anchor))
        {
            var sp = WorldToScreen2D(centerW);
            dragObject.UpdatePosition(sp);
            SetDragState(PlayerDraggingStats.BUILD_ABLE);
            dragInt = anchor;
        }
        else
        {
            // 내부지만 배치 불가
            dragObject.UpdatePosition(e.position);
            SetDragState(PlayerDraggingStats.BUILD_UNABLE);
            dragInt = new Vector2Int(-1, -1);
        }


    }// (r,c) 앵커에서 buildSize(기본 2x2) 풋프린트 셀들 반환 (r,c 순서 주의!)
    System.Collections.Generic.List<Vector2Int> GetFootprint(Vector2Int anchorRC, Vector2Int size)
    {
        var list = new System.Collections.Generic.List<Vector2Int>(size.x * size.y);
        int r0 = anchorRC.x;
        int c0 = anchorRC.y;
        for (int dr = 0; dr < size.x; dr++)
            for (int dc = 0; dc < size.y; dc++)
                list.Add(new Vector2Int(r0 + dr, c0 + dc));
        return list;
    }

    // 풋프린트의 모든 칸이 유효/빌드 가능인지 재검증 (안전망)
    bool IsFootprintBuildable(Vector2Int anchorRC, Vector2Int size)
    {
        int r0 = anchorRC.x;
        int c0 = anchorRC.y;
        for (int dr = 0; dr < size.x; dr++)
            for (int dc = 0; dc < size.y; dc++)
            {
                int r = r0 + dr, c = c0 + dc;
                if (!gi.IsInside(r, c)) return false;
                if (!gi.IsBuildableCell(r, c)) return false;
            }
        return true;
    }

    // 상태 갱신(빌드/해제) 헬퍼
    void MarkFootprintBuilt(Vector2Int anchorRC, Vector2Int size, bool built)
    {
        int r0 = anchorRC.x;
        int c0 = anchorRC.y;
        for (int dr = 0; dr < size.x; dr++)
            for (int dc = 0; dc < size.y; dc++)
            {
                gi.TrySetBuilt(r0 + dr, c0 + dc, built);
            }
    }

}