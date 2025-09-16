using UnityEngine;
using UnityEngine.UI;


public class DragObject : MonoBehaviour
{
    RectTransform myRect;

    public RectTransform GetRect() => myRect ??= GetComponent<RectTransform>();
    [SerializeField] Image testImage;
    [SerializeField] TowerBody towerBody;

    public void Initialize()
    {

    }

    public void SetBuildAbles(PlayerDraggingStats playerDraggingStats)
    {
        if (playerDraggingStats == PlayerDraggingStats.NONE)
        {
            testImage.color = Color.wheat;
        }
        else if (playerDraggingStats == PlayerDraggingStats.BUILD_ABLE)
        {
            testImage.color = Color.green;
        }
        else
        {
            testImage.color = Color.red;
        }
    }

    public void SetEnable(TowerIndex towerIndex, Vector2 pos)
    {
        gameObject.SetActive(true);
        UpdatePosition(pos);
    }

    public void SetDisable()
    {
        gameObject.SetActive(false);
    }

    public void UpdatePosition(Vector2 pos)
    {
        GetRect().anchoredPosition = pos;
    }

}
