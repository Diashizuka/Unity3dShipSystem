// Update is called once per frame
/* void Update()
{

    int agentAmount = Simulator.Instance.getNumAgents();
    for (int i = 0; i < agentAmount; i++)
    {
        Vector2 agentVec2 = Simulator.Instance.getAgentPosition(i);
        Vector2 station = rvoGameObject[i].GetComponent<RVOAgent>().CalcNextStation();
        Vector2 prefVelocity = station - agentVec2;

        if (rvoGameObject[i].GetComponent<RVOAgent>().isPlayer == true)
        {
            float agentPrefSpeed = rvoGameObject[i].GetComponent<RVOAgent>().nowSpeed;
            float agentAcc = rvoGameObject[i].GetComponent<RVOAgent>().acceleration;
            float agentAngSpeed = rvoGameObject[i].GetComponent<RVOAgent>().angularSpeed;
            float agentMinSpeedToTurn = rvoGameObject[i].GetComponent<RVOAgent>().minSpeedToTurn;
            agentPosition[i] = agentVec2;
            Vector2 velocity = RVOMath.normalize(prefVelocity) * agentPrefSpeed;
            if (rvoGameObject[i].GetComponent<RVOAgent>().isFinish() == true)
                velocity = new Vector2(0, 0);
            float randomAngle = Random.Range(-Mathf.PI, Mathf.PI);
            Vector2 dither = new Vector2(Mathf.Sin(randomAngle), Mathf.Cos(randomAngle)) * ditherSize;
            Vector2 oldVelocity = Simulator.Instance.getAgentVelocity(i);
            if (RVOMath.abs(oldVelocity) < agentMinSpeedToTurn || rvoGameObject[i].GetComponent<RVOAgent>().isFinish() == true)
            {
                Vector2 advanceVelocity = oldVelocity;
                if (RVOMath.abs(advanceVelocity) < eps)
                {
                    float y = rvoGameObject[i].transform.rotation.eulerAngles.y;
                    advanceVelocity = new Vector2(Mathf.Sin(y * Mathf.Deg2Rad), Mathf.Cos(y * Mathf.Deg2Rad));
                }
                if (RVOMath.abs(oldVelocity) < RVOMath.abs(velocity))
                    oldVelocity = oldVelocity + RVOMath.normalize(advanceVelocity) * agentAcc;
                else
                {
                    if (RVOMath.abs(oldVelocity - velocity) < agentAcc)
                        oldVelocity = velocity;
                    else
                        oldVelocity = oldVelocity - RVOMath.normalize(advanceVelocity) * agentAcc;
                }
                if (RVOMath.abs(oldVelocity) == 0)
                    Simulator.Instance.setAgentPrefVelocity(i, oldVelocity);
                else
                    Simulator.Instance.setAgentPrefVelocity(i, oldVelocity + dither);
            }
            else
            {
                Vector2 distVelocity = velocity - oldVelocity;
                if (RVOMath.abs(distVelocity) != 0.0f)
                {
                    if (RVOMath.distSqPointLineSegment(new Vector2(0, 0), RVOMath.normalize(prefVelocity) * rvoGameObject[i].GetComponent<RVOAgent>().maxSpeed * 2, oldVelocity) < agentAngSpeed)
                    {
                        if (RVOMath.abs(distVelocity) < agentAcc)
                            Simulator.Instance.setAgentPrefVelocity(i, velocity + dither);
                        else
                            Simulator.Instance.setAgentPrefVelocity(i, oldVelocity + RVOMath.normalize(distVelocity) * agentAcc + dither);
                    }
                    else
                    {
                        Vector2 accVelocity = new Vector2();
                        if (RVOMath.det(oldVelocity, velocity) <= 0)
                        {
                            Vector2 verticalVelocity = new Vector2(oldVelocity.y(), -oldVelocity.x());
                            accVelocity = RVOMath.normalize(verticalVelocity) * agentAngSpeed;
                        }
                        else
                        {
                            Vector2 verticalVelocity = new Vector2(-oldVelocity.y(), oldVelocity.x());
                            accVelocity = RVOMath.normalize(verticalVelocity) * agentAngSpeed;
                        }
                        Simulator.Instance.setAgentPrefVelocity(i, oldVelocity + accVelocity + dither);
                    }
                }
            }
        }
        else
        {
            float randomAngle = Random.Range(-Mathf.PI, Mathf.PI);
            float agentPrefSpeed = rvoGameObject[i].GetComponent<RVOAgent>().nowSpeed;
            Vector2 dither = new Vector2(Mathf.Sin(randomAngle), Mathf.Cos(randomAngle)) * ditherSize;
            Vector2 oldVelocity = Simulator.Instance.getAgentVelocity(i);
            Simulator.Instance.setAgentPrefVelocity(i, RVOMath.normalize(prefVelocity) * agentPrefSpeed + dither);
        }
    }
    Simulator.Instance.doStep();
}*/
/*
public RVO.Vector2 CalcNextStation()
{

    if (nowNode <= pathNodes.Count - 1)
    {
        station = pathNodes[nowNode];
        RVO.Vector2 stationVec2 = RVOWithUnity.Vec3ToVec2(station);
        RVO.Vector2 transformVec2 = RVOWithUnity.Vec3ToVec2(transform.position);
        float turnDist = (RVOMath.abs(Simulator.Instance.getAgentVelocity(agentIndex)) / acceleration) / changeStation;
        float slowDist = (RVOMath.abs(Simulator.Instance.getAgentVelocity(agentIndex)) / acceleration) / slowStation;
        if (RVOMath.abs(stationVec2 - transformVec2) < slowDist)
        {
            if (RVOMath.abs(stationVec2 - transformVec2) < turnDist)
            {
                nowNode++;
                station = pathNodes[nowNode];
                if (nowNode > pathNodes.Count - 1)
                    isStop = true;
                else
                    station = pathNodes[nowNode];
                prefSpeed = maxSpeed;
            }
            else
            {
                prefSpeed = maxSpeed * slowScale;
            }
        }
    }
    else
    {
        station = pathNodes[pathNodes.Count - 1];
        isStop = true;
    }
    return RVOWithUnity.Vec3ToVec2(station);
}

}
*/