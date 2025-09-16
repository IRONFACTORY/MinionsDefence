using UnityEngine;

public enum MinionIndex { TYPE_1, TYPE_2, TYPE_3, TYPE_4, TYPE_5 }

public class MinionDataBox : MonoBehaviour
{
    public static MinionDataBox Inst;
    [SerializeField] MinionInformation[] minionInformation;

    void Awake()
    {
        Inst = this;

        int length = System.Enum.GetValues(typeof(MinionIndex)).Length;
        for (int i = 0; i < length; i++)
        {

        }
    }


}