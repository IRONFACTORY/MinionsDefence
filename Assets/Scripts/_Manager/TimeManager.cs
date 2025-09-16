using UnityEngine;

public class TimeManager : MonoBehaviour
{
    public static TimeManager Inst;

    private float masterTime = 1f;
    float defaultTime = 1f;

    public float UpdateTime() => Time.deltaTime;
    public float FixedTime() => Time.fixedDeltaTime;
    public float MasterTime() => masterTime;

    void Awake()
    {
        Inst = this;
    }

    void FixedUpdate()
    {
        if (defaultTime < 1f)
            defaultTime += Time.fixedDeltaTime;
    }

}