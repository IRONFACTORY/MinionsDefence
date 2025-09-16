using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UICamera : MonoBehaviour
{
    public static UICamera Inst;
    Camera myCam;
    Transform myTransform;

    private void Awake()
    {
        Inst = this;
        myCam = GetComponent<Camera>();
        myTransform = transform;
    }

    public Camera GetCamera()
    {
        return myCam;
    }

}