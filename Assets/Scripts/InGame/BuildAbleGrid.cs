using UnityEngine;
public enum OverlayFilter { EMPTY = 0, BUILTED }

public class BuildAbleGrid : MonoBehaviour
{
    [SerializeField] Vector2Int cell;   // (c,r) 또는 (r,c) 중 하나로 통일—여기선 (r,c)로 저장
    [SerializeField] int r;
    [SerializeField] int c;
    [SerializeField] OverlayFilter filter = OverlayFilter.EMPTY;

    public Vector2Int CellRC => new Vector2Int(r, c);

    public SpriteRenderer myRenderer;

    // 인스펙터/에디터에서 셀 기록
    public void SetCell(int row, int col)
    {
        r = row;
        c = col;
        cell = new Vector2Int(row, col);
    }

    public void SetFillter(OverlayFilter target)
    {
        this.filter = target;
        if (target == OverlayFilter.EMPTY)
        {
            myRenderer.color = Color.wheat;
        }
        else
        {
            myRenderer.color = Color.red;
        }
    }

}
