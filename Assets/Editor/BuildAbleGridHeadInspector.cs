#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using System.Collections.Generic;

[CustomEditor(typeof(BuildAbleGridHead))]
public class BuildAbleGridHeadInspector : Editor
{
    SerializedProperty gridInfoProp;
    SerializedProperty gridsProp;
    SerializedProperty gridPrefabProp;
    SerializedProperty spawnParentProp;
    SerializedProperty insetProp;
    SerializedProperty autoTagProp;

    void OnEnable()
    {
        gridInfoProp = serializedObject.FindProperty("gridInfo");
        gridsProp = serializedObject.FindProperty("grids");
        gridPrefabProp = serializedObject.FindProperty("gridPrefab");
        spawnParentProp = serializedObject.FindProperty("spawnParent");
        insetProp = serializedObject.FindProperty("inset");
        autoTagProp = serializedObject.FindProperty("autoTag");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        EditorGUILayout.PropertyField(gridInfoProp);
        EditorGUILayout.PropertyField(gridsProp, true);
        EditorGUILayout.PropertyField(gridPrefabProp);
        EditorGUILayout.PropertyField(spawnParentProp);
        EditorGUILayout.PropertyField(insetProp);
        EditorGUILayout.PropertyField(autoTagProp);

        var head = (BuildAbleGridHead)target;
        GUI.enabled = head && head.gridInfo;

        EditorGUILayout.Space(8);
        using (new EditorGUILayout.VerticalScope("box"))
        {
            EditorGUILayout.LabelField("Manual Objects", EditorStyles.boldLabel);
            if (GUILayout.Button("Snap & Record (grids → GridInfo)"))
            {
                SnapAndRecord(head);
            }
        }

        EditorGUILayout.Space(8);
        using (new EditorGUILayout.VerticalScope("box"))
        {
            EditorGUILayout.LabelField("Auto Fill from BUILD_ZONE", EditorStyles.boldLabel);
            if (GUILayout.Button("Auto Fill (Create for each BUILD_ZONE)"))
            {
                AutoFill(head);
            }
            if (GUILayout.Button("Clear Auto-Filled"))
            {
                ClearAuto(head);
            }
        }

        GUI.enabled = true;

        serializedObject.ApplyModifiedProperties();
    }

    // === 1) 수동 오브젝트 스냅 & 좌표 기록 ===
    void SnapAndRecord(BuildAbleGridHead head)
    {
        var grid = head.gridInfo;
        if (!grid || head.grids == null) return;

        Undo.RecordObject(head, "Snap & Record (BuildAbleGrid)");
        int count = 0;

        for (int i = 0; i < head.grids.Length; i++)
        {
            var g = head.grids[i];
            if (!g) continue;

            Vector3 wp = g.transform.position;
            if (grid.TryWorldToCell(wp, out int r, out int c))
            {
                if (grid.TryGetCellCenter2D(r, c, out var center2D))
                {
                    var finalPos = (Vector3)(center2D + head.inset);
                    Undo.RecordObject(g.transform, "Move BuildAbleGrid");
                    g.transform.position = finalPos;

                    Undo.RecordObject(g, "Record Cell");
                    g.SetCell(r, c);
                    EditorUtility.SetDirty(g);
                    count++;
                }
            }
        }

        EditorUtility.SetDirty(head);
        Debug.Log($"[BuildAbleGrid] Snap & Record 완료: {count}개");
    }

    // === 2) BUILD_ZONE 전체 자동 생성 ===
    void AutoFill(BuildAbleGridHead head)
    {
        var grid = head.gridInfo;
        if (!grid)
        {
            Debug.LogWarning("GridInfo가 필요합니다.");
            return;
        }
        if (!head.gridPrefab)
        {
            Debug.LogWarning("gridPrefab이 필요합니다.");
            return;
        }

        grid.RebuildIfDirty();
        var buildList = grid.BuildZoneCells;
        if (buildList == null || buildList.Count == 0)
        {
            Debug.LogWarning("BUILD_ZONE 셀이 없습니다.");
            return;
        }

        var parent = head.spawnParent ? head.spawnParent : head.transform;

        Undo.IncrementCurrentGroup();
        int group = Undo.GetCurrentGroup();

        var created = new List<BuildAbleGrid>(buildList.Count);
        foreach (var cell in buildList)
        {
            if (!grid.TryGetCellCenter2D(cell.r, cell.c, out var center2D))
                continue;

            var pos = (Vector3)(center2D + head.inset);
            var go = (BuildAbleGrid)PrefabUtility.InstantiatePrefab(head.gridPrefab, parent);
            if (!go) go = Instantiate(head.gridPrefab, parent);

            Undo.RegisterCreatedObjectUndo(go.gameObject, "Create BuildAbleGrid");
            go.transform.position = pos;
            go.SetCell(cell.r, cell.c);

            // 만든 것 구분 태그(문자열 저장용)
            if (!string.IsNullOrEmpty(head.autoTag))
                go.gameObject.name = $"{head.autoTag}_{cell.r}_{cell.c}";

            created.Add(go);
        }

        Undo.CollapseUndoOperations(group);
        head.RebuildLookup();
        EditorUtility.SetDirty(head);
        Debug.Log($"[BuildAbleGrid] Auto Fill 완료: {created.Count}개 생성");
    }

    // === 3) Auto Fill로 만든 것만 정리 ===
    void ClearAuto(BuildAbleGridHead head)
    {
        string tagMark = head.autoTag;
        if (string.IsNullOrEmpty(tagMark))
        {
            Debug.LogWarning("autoTag가 비어있습니다. 구분이 불가합니다.");
            return;
        }

        var parent = head.spawnParent ? head.spawnParent : head.transform;
        var list = new List<GameObject>();
        foreach (Transform t in parent)
        {
            if (t && t.name.StartsWith(tagMark))
                list.Add(t.gameObject);
        }

        if (list.Count == 0)
        {
            Debug.Log("[BuildAbleGrid] 삭제할 Auto-Filled 오브젝트가 없습니다.");
            return;
        }

        Undo.IncrementCurrentGroup();
        int group = Undo.GetCurrentGroup();

        for (int i = 0; i < list.Count; i++)
            Undo.DestroyObjectImmediate(list[i]);

        Undo.CollapseUndoOperations(group);
        Debug.Log($"[BuildAbleGrid] Auto-Filled 정리: {list.Count}개 삭제");
    }
}
#endif
