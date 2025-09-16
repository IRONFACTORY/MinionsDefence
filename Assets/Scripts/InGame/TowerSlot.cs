using UnityEngine;
using UnityEngine.EventSystems;

public class TowerSlot : MonoBehaviour
{
    Camera cam;

    Transform myTransform;
    int myIndex;
    TowerObject towerObject;
    public TowerObject GetRegisterdTower() => towerObject;
    public bool IsEquiped() => towerObject != null;


    void Awake()
    {
        myTransform = transform;
    }

    public void Initialize(int index)
    {
        myIndex = index;
        cam = MainCamera.Inst.GetCamera();
    }

    public void RegisterTower(TowerObject towerObject)
    {
        this.towerObject = towerObject;
        towerObject.UpdatePosition(GetMyPos());
        towerObject.SetSlotIndex(myIndex);
    }

    public void OnBeginDrag()
    {
        towerObject.SetColor(Color.gray);
    }

    public void OnEndDrag()
    {
        towerObject.ResetColor();
    }

    public void UnRegisterTower()
    {
        towerObject = null;
    }

    Vector2 GetMyPos() => myTransform.position;


}