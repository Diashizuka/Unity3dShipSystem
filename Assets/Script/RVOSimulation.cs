using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RVO;
using Vector2 = RVO.Vector2;
public class RVOSimulation : MonoBehaviour
{
    [SerializeField]
    private float timeStep = 0.25f;

    [SerializeField]
    private float neighborDist = 15.0f;
    [SerializeField]
    private int maxNeighbor = 10;
    [SerializeField]
    private float timeHorizon = 5.0f;
    [SerializeField]
    private float timeHorizonObst = 5.0f;
    [SerializeField]
    private float radius = 0.5f;
    [SerializeField]
    private float maxSpeed = 2.0f;
    List<RVO.Vector2> agentPosition;
    List<GameObject> rvoGameObject;
    //private float timeUpdate = 0.0f;
    public float GetTimeStep()
    {
        return timeStep;
    }
    void Awake()
    {
        agentPosition = new List<RVO.Vector2>();
        rvoGameObject = new List<GameObject>();
        Simulator.Instance.setTimeStep(timeStep);
        Simulator.Instance.setAgentDefaults(neighborDist, maxNeighbor, timeHorizon, timeHorizonObst, radius, maxSpeed, new RVO.Vector2(0.0f, 0.0f), 0.0f, 0.0f, 0.0f);
    }
    void Start()
    {
        Simulator.Instance.processObstacles();
    }
    public int AddAgent(GameObject agentGameObject)
    {
        rvoGameObject.Add(agentGameObject);
        agentPosition.Add(RVOWithUnity.Vec3ToVec2(agentGameObject.transform.position));
        RVOAgent rvoAgent = agentGameObject.GetComponent<RVOAgent>();
        if (rvoAgent.isPlayer == false)
        {
            return Simulator.Instance.addAgent(RVOWithUnity.Vec3ToVec2(agentGameObject.transform.position));
        }
        else
        {
            return Simulator.Instance.addAgent(RVOWithUnity.Vec3ToVec2(agentGameObject.transform.position),
                                                neighborDist,
                                                maxNeighbor,
                                                rvoAgent.timeHorizon,
                                                timeHorizonObst,
                                                rvoAgent.radius,
                                                rvoAgent.maxSpeed,
                                                new RVO.Vector2(0.0f, 0.0f),
                                                rvoAgent.acceleration,
                                                rvoAgent.angularSpeed,
                                                rvoAgent.minSpeedToTurn

                );
        }
    }
    void FixedUpdate()
    {
        int agentAmount = Simulator.Instance.getNumAgents();
        for (int i = 0; i < agentAmount; i++)
        {
            Vector2 agentVec2 = Simulator.Instance.getAgentPosition(i);
            Vector2 station = rvoGameObject[i].GetComponent<RVOAgent>().GetStation();
            RVOAgent agent = rvoGameObject[i].GetComponent<RVOAgent>();
            Vector2 prefVelocity = RVOWithUnity.CalcPrefVelocity(agentVec2, station, agent);
            Simulator.Instance.setAgentPrefVelocity(i, prefVelocity);
        }
        Simulator.Instance.doStep();

    }
}

