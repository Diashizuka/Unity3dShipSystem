using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LineRenderByCube : MonoBehaviour
{
    private LineRenderer lr;
    private int num = 0;
    public Color lineColor = Color.green;
    public bool isDrawPath = true;
    // Start is called before the first frame update
    void Start()
    {
        lr = gameObject.GetComponent<LineRenderer>();
        lr.startColor = lineColor;
        lr.endColor = lineColor;
        lr.startWidth = 0.4f;
        lr.endWidth = 0.4f;
    }

    // Update is called once per frame
    void Update()
    {
        if(isDrawPath == true)
        {
            lr.positionCount = ++num;
            lr.SetPosition(num - 1, transform.parent.position + Vector3.up * 0.05f);    
        }
    }
}
