using System.Collections.Generic;
using UnityEngine;
using System.Linq;

[System.Serializable]
public class SpawnPositionPicker
{
    int pickedIndex = 0;
    List<Vector2> list = new List<Vector2>();

    public void Initialize()
    {
        pickedIndex = 0;
        list.Clear();
    }

    public void DoGenerate(List<Vector2> cells, int seed)
    {
        pickedIndex = 0;
        list = new List<Vector2>(cells);
        var rng = new System.Random(seed);
        list = list.OrderBy(x => rng.Next()).ToList();
    }

    public Vector2 Pick()
    {
        if (list.Count == 0)
            return Vector2.zero;
        int target = pickedIndex;
        pickedIndex++;
        if (pickedIndex >= list.Count)
            pickedIndex = 0;
        Vector2 targetVec = list[target];
        list.RemoveAt(target);
        list.Add(targetVec);
        return targetVec;
    }
}