using UnityEngine;

public class TowerDataBox : MonoBehaviour
{
    public static TowerDataBox Inst;

    void Awake()
    {
        Inst = this;
    }

}
