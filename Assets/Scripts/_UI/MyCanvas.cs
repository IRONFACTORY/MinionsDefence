using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class MyCanvas : MonoBehaviour
{
    public static MyCanvas Inst;
    Canvas myCanvas;
    RectTransform canvasRect;
    Vector2 screenPoint;

    public Canvas GetCanvas()
    {
        if (myCanvas == null)
            myCanvas = GetComponent<Canvas>();
        return myCanvas;
    }

    public RectTransform GetCanvasRect()
    {
        if (canvasRect == null)
            canvasRect = GetCanvas().GetComponent<RectTransform>();
        return canvasRect;
    }

    public void UpdateCorners()
    {
        Vector3[] corners = new Vector3[4];
        GetCanvasRect().GetWorldCorners(corners);
        Vector3 worldPoint = (corners[0] + corners[2]) / 2;
        screenPoint = RectTransformUtility.WorldToScreenPoint(UICamera.Inst.GetCamera(), worldPoint);
    }

    public Vector2 GetCorners()
    {
        return screenPoint;
    }

    private void Awake()
    {
        Inst = this;
    }

    public float GetScreenWorldX()
    {
        Vector2 start = MainCamera.Inst.GetCamera().ViewportToWorldPoint(Vector2.zero);
        Vector2 target = MainCamera.Inst.GetCamera().ViewportToWorldPoint(Vector2.right);
        return Vector2.Distance(start, target);
    }

    public float GetScreenWorldY()
    {
        Vector2 start = MainCamera.Inst.GetCamera().ViewportToWorldPoint(Vector2.zero);
        Vector2 target = MainCamera.Inst.GetCamera().ViewportToWorldPoint(Vector2.up);
        return Vector2.Distance(start, target);
    }

    public float GetScreenX()
    {
        return Screen.width;
    }

    public float GetScreenY()
    {
        return Screen.height;
    }

    public float WorldSafeZoneY()
    {
        float canvasY = GetScreenY();
        float safeZoneTarget = Screen.safeArea.yMax;
        float ratio = safeZoneTarget / canvasY;
        return GetScreenWorldY() * (1f - ratio);
    }

    public float TopOffset(float mediate = 0.8f)
    {
        float canvasY = GetScreenY();
        float safeZoneTarget = Screen.safeArea.yMax;
        if (Mathf.Approximately(canvasY, safeZoneTarget))
            return 0f;
        float target = canvasY - safeZoneTarget;
        return target * mediate;
    }

    public float BottomOffset(float mediate = 0.6f)
    {
        float canvasY = GetScreenY();
        float safeZoneTarget = Screen.safeArea.yMin;
        if (Mathf.Approximately(canvasY, safeZoneTarget))
            return 0f;
        float target = safeZoneTarget;
        return target * mediate;
    }

    public bool HasSafeZoneTop()
    {
        float target = GetScreenY() - Screen.safeArea.yMax;
        return !Mathf.Approximately(target, 0);
    }

    public bool HasSafeZoneBottom()
    {
        float target = Screen.safeArea.yMin;
        return !Mathf.Approximately(target, 0);
    }

    public bool HasSafeZoneLeft()
    {
        float target = Screen.safeArea.xMin;
        return !Mathf.Approximately(target, 0);
    }

    public bool HasSafeZoneRight()
    {
        float target = GetScreenX() - Screen.safeArea.xMax;
        return !Mathf.Approximately(target, 0);
    }

}