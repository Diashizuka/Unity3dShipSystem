using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AgentCreator : MonoBehaviour
{
    [SerializeField]
    private GameObject prefab;
    [SerializeField]
    private int amount = 3;
    [SerializeField]
    private float interval = 1.0f;
    [SerializeField]
    private List<Transform> goalTransform;
    [SerializeField]
    private float speed = 1.0f;
    void Start()
    {
        StartCoroutine(GenerateAgent());
    }

    void Update()
    {
        
    }
    IEnumerator GenerateAgent()
    {
        int count = 0;
        while (count < amount)
        {
            GameObject agentGameObject = (GameObject)Instantiate(prefab,
                                                                 transform.position,
                                                                 Quaternion.Euler(0, 270, 0));
            agentGameObject.name = prefab.name + "_" + count;
            agentGameObject.GetComponent<RVOAgent>().targetPosition = goalTransform[Random.Range(0,goalTransform.Count)].position;
            agentGameObject.GetComponent<RVOAgent>().maxSpeed = speed;
            yield return new WaitForSeconds(interval);
            count++;
        }
    }

}
