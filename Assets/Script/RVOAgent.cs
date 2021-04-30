using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RVO;
using Pathfinding;
using UnityEngine.AI;
using Vector2 = RVO.Vector2;
public class RVOAgent : MonoBehaviour
{
    public enum GRAPH
    {
        Grid = 1,
        NavMesh = 2
    };

    public Transform target = null;

    public float maxSpeed = 1.0f;
    public float acceleration = 0.5f;
    public float angularSpeed = 0.5f;
    public float minSpeedToTurn = 0.3f;
    public float radius = 0.5f;
    public float timeHorizon = 5.0f;

    public GRAPH useGraph = GRAPH.Grid;
    public bool isDrawPath = false;
    public bool isPlayer = false;

    public Vector2 velocity;

    [SerializeField]
    [ReadOnly]
    private int agentIndex = -1;
    [ReadOnly]
    public Vector3 targetPosition;
    [SerializeField]
    [ReadOnly]
    private Vector3 station;
    [ReadOnly]
    public float prefSpeed = 1.0f;
    [ReadOnly]
    public float speed = 0.0f;
    [ReadOnly]
    public Vector3 velocityVec3;

    private List<Vector3> pathNodes;
    private int nowNode = 0;
    RVOSimulation simulator = null;
    private bool isOk = false;
    private bool isStop = false;
    private float slowStation = 50.0f;
    private float ignoreStepFactor = 1.25f;
    private float turningFactor = 160.0f;

    private float turnDist = 0.0f;
    private int slowStationNum = 0;
    private float eps_float = 1e-6f;

    public int GetIndex()
    {
        return agentIndex;
    }
    public bool GetIsStop()
    {
        return isStop;
    }
    public Vector2 GetDirection()
    {
        float yRotation = -gameObject.transform.rotation.eulerAngles.y * Mathf.Deg2Rad;
        return new Vector2(Mathf.Cos(yRotation), Mathf.Sin(yRotation));
    }
    public Vector2 GetStation()
    {
        return RVOWithUnity.Vec3ToVec2(station);
    }
    private bool SimilarVec3(Vector3 vec3a, Vector3 vec3b)
    {
        if (Vector3.Distance(Vector3.Normalize(vec3a), Vector3.Normalize(vec3b)) < eps_float)
            return true;
        return false;
    }
    private Vector3 YAxisToZero(Vector3 vec3)
    {
        return new Vector3(vec3.x, 0.0f, vec3.z);
    }
    void OptimizationPathNodes()
    {
        int pathNodesCount = pathNodes.Count;
        if (pathNodesCount <= 2)
            return;
        for (int i = pathNodesCount - 1; i >= 0; i--)
        {
            if (i == pathNodesCount - 1 || i == 0)
                continue;
            if (SimilarVec3(pathNodes[i + 1] - pathNodes[i], pathNodes[i] - pathNodes[i - 1]))
                pathNodes.Remove(pathNodes[i]);
        }
    }
    private void DrawPath()
    {
        if (isDrawPath == true)
        {
            LineRenderer lr = gameObject.GetComponent<LineRenderer>();
            lr.startColor = Color.red;
            lr.endColor = Color.red;
            lr.startWidth = 0.2f;
            lr.endWidth = 0.2f;
            lr.positionCount = pathNodes.Count;
            Vector3[] pathNodesVec3 = pathNodes.ToArray();
            for (int i = 0; i < pathNodesVec3.Length; i++)
                pathNodesVec3[i] = new Vector3(pathNodesVec3[i].x, pathNodesVec3[i].y + 0.01f, pathNodesVec3[i].z);
            lr.SetPositions(pathNodesVec3);
        }
    }
    public void OnPathComplete(Path p)
    {
        if (p.error)
        {
            Debug.Log(p.errorLog);
        }
        else
        {
            for (int i = 0; i < p.vectorPath.Count; i++)
                pathNodes.Add(YAxisToZero(p.vectorPath[i]));
        }
    }
    IEnumerator Start()
    {
        simulator = GameObject.Find("RVOSystem").GetComponent<RVOSimulation>();
        pathNodes = new List<Vector3>();
        nowNode = 1;
        prefSpeed = maxSpeed;
        if (target != null)
            targetPosition = target.position;
        switch (useGraph)
        {
            case GRAPH.Grid:
                {
                    Seeker agentSeeker = gameObject.GetComponent<Seeker>();
                    var path = agentSeeker.StartPath(transform.position, targetPosition, OnPathComplete);
                    yield return StartCoroutine(path.WaitForPath());
                    OptimizationPathNodes();
                    break;
                }
            case GRAPH.NavMesh:
                {
                    NavMeshAgent agent = gameObject.GetComponent<NavMeshAgent>();
                    NavMeshPath path = new NavMeshPath();
                    agent.CalculatePath(targetPosition, path);
                    for (int i = 0; i < path.corners.Length; i++)
                    {
                        pathNodes.Add(YAxisToZero(path.corners[i]));
                    }
                    yield return 0;
                    break;
                }
        }
        DrawPath();
        station = pathNodes[1];
        agentIndex = simulator.AddAgent(gameObject);
        isOk = true;
    }
    void Update()
    {
        if (isOk && agentIndex != -1)
        {
            /* dist */
            transform.position = RVOWithUnity.Vec2ToVec3(Simulator.Instance.getAgentPosition(agentIndex));
            /* turn */
            if (RVOMath.abs(Simulator.Instance.getAgentVelocity(agentIndex)) > 0.0f)
            {
                Vector3 direction = RVOWithUnity.Vec2ToVec3(Simulator.Instance.getAgentVelocity(agentIndex));
                float angle = Vector3.Angle(Vector3.right, direction);
                if (Vector3.Cross(Vector3.right, direction).y < 0)
                    angle = 360 - angle;
                transform.rotation = Quaternion.Euler(new Vector3(transform.rotation.x, angle, transform.rotation.z));
            }

            if (!isStop)
            {
                /* Velocity */
                velocity = Simulator.Instance.getAgentVelocity(agentIndex);
                velocityVec3 = RVOWithUnity.Vec2ToVec3(velocity);
                speed = RVOMath.abs(velocity);
                /* Update Station */
                station = pathNodes[nowNode];
                Vector2 stationVector2 = RVOWithUnity.Vec3ToVec2(station);
                Vector2 transformVector2 = RVOWithUnity.Vec3ToVec2(transform.position);
                Vector2 distVector2 = stationVector2 - transformVector2;
                Vector2 lastStationVector2 = RVOWithUnity.Vec3ToVec2(pathNodes[nowNode - 1]);
                Vector2 lastToNowNormalize = RVOMath.normalize(stationVector2 - lastStationVector2);
                Vector2 verticalDistVector2 = new Vector2(-lastToNowNormalize.y_, lastToNowNormalize.x_);

                //turnDist = (speed / angularSpeed) / changeStation;
                if (RVOWithUnity.isTwoSide(verticalDistVector2, distVector2, lastToNowNormalize))
                {
                    nowNode++;
                    slowStationNum = Mathf.Max(0, slowStationNum - 1);
                    //Debug.Log(RVOWithUnity.Vec3ToVec2(pathNodes[nowNode]));
                    if (nowNode > pathNodes.Count - 1)
                    {
                        isStop = true;
                        prefSpeed = 0.0f;
                    }
                    else
                    {
                        station = pathNodes[nowNode];
                    }
                    if (slowStationNum == 0)
                        prefSpeed = maxSpeed;
                }
                else
                {
                    float slowDist = Mathf.Abs(speed * speed - minSpeedToTurn * minSpeedToTurn) / (acceleration * slowStation * 2);
                    if (RVOMath.abs(distVector2) >= slowDist && slowStationNum == 0)
                        prefSpeed = maxSpeed;
                    if (RVOMath.abs(distVector2) < slowDist)
                    {
                        /* Calc the slowScale */
                        if (nowNode + 1 > pathNodes.Count - 1)
                        {
                            prefSpeed = minSpeedToTurn;
                            turnDist = 0.1f;
                        }
                        else
                        {
                            int nextTurnNode = nowNode + 1;
                            Vector2 nextStation = RVOWithUnity.Vec3ToVec2(pathNodes[nextTurnNode]);
                            Vector2 lastStation = stationVector2;
                            Vector2 nextDirection = nextStation - lastStation;
                            float turnAngleDist = Vector3.Angle(velocityVec3, RVOWithUnity.Vec2ToVec3(nextDirection));
                            if (nowNode + 1 <= pathNodes.Count - 1 && slowStationNum == 0)
                            {
                                slowStationNum++;
                                float turnAngle = turnAngleDist;
                                //if (gameObject.name == "boat")
                                //  Debug.Log(lastStation + "/|/" + nextStation  + "/|/" + speed * ignoreStepFactor);
                                while (RVOMath.abs(nextDirection) < speed * ignoreStepFactor && nextTurnNode + 1 <= pathNodes.Count - 1)
                                {

                                    nextTurnNode++;
                                    slowStationNum++;
                                    lastStation = nextStation;
                                    nextStation = RVOWithUnity.Vec3ToVec2(pathNodes[nextTurnNode]);
                                    Vector2 lastDirection = nextDirection;
                                    nextDirection = nextStation - lastStation;
                                    //Debug.Log(nextStation + " " + lastStation);
                                    turnAngle += Vector3.Angle(RVOWithUnity.Vec2ToVec3(lastDirection), RVOWithUnity.Vec2ToVec3(nextDirection));
                                }
                                prefSpeed = minSpeedToTurn + (maxSpeed - minSpeedToTurn) * Mathf.Max(turningFactor - turnAngle, turningFactor / 10.0f) / turningFactor;
                            }
                            turnDist = Mathf.Tan(turnAngleDist * Mathf.Deg2Rad / 2) * (prefSpeed * prefSpeed / angularSpeed) / 200.0f;
                        }
                    }
                    if (RVOMath.abs(distVector2) < turnDist)
                    {
                        nowNode++;
                        slowStationNum = Mathf.Max(0, slowStationNum - 1);
                        if (nowNode > pathNodes.Count - 1)
                        {
                            isStop = true;
                            prefSpeed = 0.0f;
                        }
                        else
                        {
                            station = pathNodes[nowNode];
                        }
                    }
                }
            }
        }
    }
}