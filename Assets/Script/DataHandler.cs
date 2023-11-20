using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DataHandler : MonoBehaviour
{
    [System.Serializable]
    public class XYZItem
    {
        public float x;
        public float y;
        public float z;
    }

    public List<XYZItem> xyzList;

    void Start()
    {
        foreach (var item in xyzList)
        {
            Debug.Log($"X: {item.x}, Y: {item.y}, Z: {item.z}");
        }
    }

    void Update()
    {
        
    }
}
