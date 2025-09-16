using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class MainCamera : MonoBehaviour
{
    public static MainCamera Inst;

    [SerializeField] RectTransform fightDownRect;

    Transform myTransform;
    Transform shakeTransform;
    Camera myCam;

    float currentFollowOffset = -1f;
    float targetHeroPositionViewPortFromCenter = 0.1f;

    private void Awake()
    {
        Inst = this;
        myTransform = transform;
        myCam = GetComponentInChildren<Camera>();
        shakeTransform = myCam.transform;
    }

    public void Initialize()
    {

    }

    public void SetMain()
    {

    }

    public void SetFight()
    {
        UpdateCameraOffsets();
        UpdateCamera();
    }
    void UpdateCameraOffsets()
    {
        float screenY = MyCanvas.Inst.GetScreenY();
        float screenWorldY = MyCanvas.Inst.GetScreenWorldY();
        float downSizeY = fightDownRect.sizeDelta.y;
        float panelWorldY = screenWorldY * (downSizeY / screenY);
        float panelTop = -screenWorldY / 2f + panelWorldY;
        currentFollowOffset = -panelTop + Enums.GAME_END_Y_VISIBLE;
    }

    void UpdateCamera()
    {
        Vector3 tp = GetFieldTargetPos(Vector2.zero);
        myTransform.position = tp;
    }

    public void ShakeCamera()
    {
        shakeTransform.DOShakePosition(1f, 1, 30).OnComplete(() =>
        {
            shakeTransform.localPosition = Vector3.zero;
        });
    }

    Vector3 GetFieldTargetPos(Vector2 tp) => new Vector3(tp.x + 0f, tp.y + currentFollowOffset, 0f);
    public Camera GetCamera() => myCam;

}