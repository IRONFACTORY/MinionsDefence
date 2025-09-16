using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class TestPathVisualize : MonoBehaviour
{

    List<Vector2> wayPoints = new();
    [SerializeField] LineRenderer lineRenderer;

    public void Update()
    {

    }

    public void UpdatePath()
    {

    }

    void SetPath(List<Vector2> path)
    {
        wayPoints = path.ToList();

        if (lineRenderer == null) return;

        lineRenderer.positionCount = wayPoints.Count;
        for (int i = 0; i < wayPoints.Count; i++)
            lineRenderer.SetPosition(i, new Vector3(wayPoints[i].x, wayPoints[i].y, 0f));
    }

    // 임시: 내 위치 가져오는 함수
    Vector2 GetMyPos()
    {
        return transform.position;
    }
}
