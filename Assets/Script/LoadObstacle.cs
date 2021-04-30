using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RVO;
using Vector2 = RVO.Vector2;
public class LoadObstacle : MonoBehaviour
{
    void Awake()
    {
        BoxCollider[] boxColliders = GameObject.Find("Ground").GetComponentsInChildren<BoxCollider>();
        CapsuleCollider[] cylinders = GameObject.Find("Ground").GetComponentsInChildren<CapsuleCollider>();
        for (int i = 0; i < boxColliders.Length; i++)
        {
            Vector2 position = RVOWithUnity.Vec3ToVec2(boxColliders[i].transform.position);
            float angle = boxColliders[i].transform.rotation.eulerAngles.y * Mathf.Deg2Rad;
            float sizeX = boxColliders[i].transform.lossyScale.x * 0.5f;
            float sizeZ = boxColliders[i].transform.lossyScale.z * 0.5f;
            Vector2 right = new Vector2(Mathf.Sin(angle), Mathf.Cos(angle));
            Vector2 left = new Vector2(-Mathf.Sin(angle), -Mathf.Cos(angle));
            Vector2 up = new Vector2(-Mathf.Cos(angle), Mathf.Sin(angle));
            Vector2 down = new Vector2(Mathf.Cos(angle), -Mathf.Sin(angle));
            Vector2 rightUp = position + right * sizeZ + up * sizeX;
            Vector2 rightDown = position + right * sizeZ + down * sizeX;
            Vector2 leftUp = position + left * sizeZ + up * sizeX;
            Vector2 leftDown = position + left * sizeZ + down * sizeX;
            IList<Vector2> obstacle = new List<Vector2>();
            obstacle.Add(rightUp);
            obstacle.Add(leftUp);
            obstacle.Add(leftDown);
            obstacle.Add(rightDown);
            /* Debug.Log(boxColliders[i].gameObject.name.ToString() + "::" +
                rightUp.ToString() + " " +
                leftUp.ToString() + " " +
                leftDown.ToString() + " " +
                rightDown.ToString()
                 ); */
            Simulator.Instance.addObstacle(obstacle);
        }
        for (int i = 0; i < cylinders.Length; i++)
        {
            float size = cylinders[i].transform.lossyScale.x * 0.5413f;
            Vector2 position = RVOWithUnity.Vec3ToVec2(cylinders[i].transform.position);
            float angle = 360.0f / 8.0f;
            float nowAngle = 360.0f - angle / 2.0f;
            IList<Vector2> obstacle = new List<Vector2>();
            //Debug.Log(cylinders[i].gameObject.name.ToString() + "::");
            for (int j = 0; j < 8; j++)
            {
                float nowAngleRad = nowAngle * Mathf.Deg2Rad;
                Vector2 point = new Vector2(Mathf.Sin(nowAngleRad), Mathf.Cos(nowAngleRad));
                point = position + point * size;
                obstacle.Add(point);
                nowAngle -= angle;
                //Debug.Log(point);
            }
            Simulator.Instance.addObstacle(obstacle);
        }
    }
}
