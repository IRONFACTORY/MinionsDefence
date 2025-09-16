using UnityEngine;
using System.Collections.Generic;

public class SpatialHash2D
{
    readonly float _cellSize;
    readonly Dictionary<long, List<MinionObject>> _buckets = new();
    readonly Dictionary<MinionObject, long> _minionCell = new();


    public SpatialHash2D(float cellSize = 3f) // 사거리/평균 간격에 맞춰 튜닝
    {
        _cellSize = Mathf.Max(0.1f, cellSize);
    }

    static long PackKey(int x, int y) => ((long)x << 32) ^ (uint)y;

    public (int x, int y) ToCell(Vector2 pos)
    {
        int cx = Mathf.FloorToInt(pos.x / _cellSize);
        int cy = Mathf.FloorToInt(pos.y / _cellSize);
        return (cx, cy);
    }

    public long ToKey(Vector2 pos)
    {
        var (cx, cy) = ToCell(pos);
        return PackKey(cx, cy);
    }

    public void Add(MinionObject m)
    {
        var key = ToKey(m.GetMyPos());
        if (!_buckets.TryGetValue(key, out var list))
        {
            list = new List<MinionObject>(16);
            _buckets[key] = list;
        }
        list.Add(m);
        _minionCell[m] = key;
    }

    public void Remove(MinionObject m)
    {
        if (_minionCell.TryGetValue(m, out var key))
        {
            if (_buckets.TryGetValue(key, out var list))
            {
                int idx = list.IndexOf(m);
                if (idx >= 0) list.RemoveAt(idx);
            }
            _minionCell.Remove(m);
        }
    }

    public void Move(MinionObject m)
    {
        var newKey = ToKey(m.GetMyPos());
        if (_minionCell.TryGetValue(m, out var oldKey) && oldKey == newKey)
            return;

        // 셀 이동
        if (_buckets.TryGetValue(oldKey, out var oldList))
        {
            int idx = oldList.IndexOf(m);
            if (idx >= 0) oldList.RemoveAt(idx);
        }
        if (!_buckets.TryGetValue(newKey, out var newList))
        {
            newList = new List<MinionObject>(16);
            _buckets[newKey] = newList;
        }
        newList.Add(m);
        _minionCell[m] = newKey;
    }

    // 원 범위 질의(반경 r에 걸치는 셀만 스캔)
    public int QueryCircle(Vector2 center, float radius, List<MinionObject> outList)
    {
        outList.Clear();
        int minX = Mathf.FloorToInt((center.x - radius) / _cellSize);
        int maxX = Mathf.FloorToInt((center.x + radius) / _cellSize);
        int minY = Mathf.FloorToInt((center.y - radius) / _cellSize);
        int maxY = Mathf.FloorToInt((center.y + radius) / _cellSize);

        float r2 = radius * radius;

        for (int y = minY; y <= maxY; y++)
            for (int x = minX; x <= maxX; x++)
            {
                var key = PackKey(x, y);
                if (!_buckets.TryGetValue(key, out var list)) continue;

                for (int i = 0; i < list.Count; i++)
                {
                    var m = list[i];
                    if (!m || !m.GetAlive()) continue;
                    var d2 = (m.GetMyPos() - center).sqrMagnitude;
                    if (d2 <= r2) outList.Add(m);
                }
            }
        return outList.Count;
    }

    public MinionObject QueryNearest(Vector2 from, float maxRadius = 20f)
    {
        MinionObject best = null;
        float bestSqr = maxRadius * maxRadius;

        int minX = Mathf.FloorToInt((from.x - maxRadius) / _cellSize);
        int maxX = Mathf.FloorToInt((from.x + maxRadius) / _cellSize);
        int minY = Mathf.FloorToInt((from.y - maxRadius) / _cellSize);
        int maxY = Mathf.FloorToInt((from.y + maxRadius) / _cellSize);

        for (int y = minY; y <= maxY; y++)
            for (int x = minX; x <= maxX; x++)
            {
                var key = PackKey(x, y);
                if (!_buckets.TryGetValue(key, out var list)) continue;

                for (int i = 0; i < list.Count; i++)
                {
                    var m = list[i];
                    if (!m || !m.GetAlive()) continue;
                    float d2 = (m.GetMyPos() - from).sqrMagnitude;
                    if (d2 < bestSqr)
                    {
                        bestSqr = d2;
                        best = m;
                    }
                }
            }
        return best;
    }
}
