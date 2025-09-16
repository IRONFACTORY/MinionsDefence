using UnityEngine;

public class TowerBody : MonoBehaviour
{
    Transform myTransform;
    [SerializeField] TowerIndex towerIndex;
    TowerObject parentTowerObject;
    [SerializeField] SpriteRenderer myRenderer;

    public TowerIndex GetTowerIndex() => towerIndex;

    bool isPooled;
    public bool GetPooled() => isPooled;
    void SetPooling(bool tru) => isPooled = tru;

    Color startCol;

    void Awake()
    {
        myTransform = transform;
        startCol = myRenderer.color;
    }

    public void Initialize()
    {

    }

    public void SetColor(Color col)
    {
        myRenderer.color = col;
    }

    public void ResetColor()
    {
        myRenderer.color = startCol;
    }

    public void InitializeTowerBody(TowerObject parentTower)
    {
        this.parentTowerObject = parentTower;
        SetEnable();
        myTransform.SetParent(parentTowerObject.GetTowerBodyHead());
        myTransform.localPosition = Vector2.zero;
        ResetColor();
    }

    public void SetEnable()
    {
        gameObject.SetActive(true);
        SetPooling(true);
    }

    public void SetDisable()
    {
        SetPooling(false);
        parentTowerObject = null;
        myTransform.SetParent(UnitManager.Inst.GetTowerHead());
        gameObject.SetActive(false);
    }


}
