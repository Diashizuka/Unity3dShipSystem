using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Force : MonoBehaviour
{
    // Start is called before the first frame update
    Vector3 LastPosition = Vector3.zero;
    LineRenderer lr;
    int tot = 0;
    void Start()
    {
        gameObject.GetComponent<Rigidbody>().AddForce(150.0f, 0.0f, 0.0f);
        LastPosition = gameObject.transform.position + new Vector3(0.0f, 0.1f, 0.0f);
        lr = gameObject.GetComponent<LineRenderer>();
        lr.startColor = Color.red;
        lr.endColor = Color.red;
        lr.startWidth = 0.2f;
        lr.endWidth = 0.2f;
        lr.positionCount = ++tot;
        lr.SetPosition(tot - 1, LastPosition);
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 rad = gameObject.GetComponent<Rigidbody>().velocity.normalized;
        gameObject.GetComponent<Rigidbody>().AddForce(-rad.z * 0.5f, 0.0f, rad.x * 0.5f);
        LastPosition = gameObject.transform.position + new Vector3(0.0f, 0.1f, 0.0f);
        lr.positionCount = ++tot;
        lr.SetPosition(tot - 1, LastPosition);
    }
}
