using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RVO;
using Vector2 = RVO.Vector2;
public static class RVOWithUnity
{
    public const float ditherSize = 1e-6f;
    public const float eps = 1e-6f;
    /**
         * <summary>Convert Vector3 to Vector2, Loss Yaxis.</summary>
    */
    public static Vector2 Vec3ToVec2(Vector3 vec3)
    {
        return new Vector2(vec3.x, vec3.z);
    }
    /**
         * <summary>Convert Vector2 to Vector3, Set Yaxis to zero.</summary>
    */
    public static Vector3 Vec2ToVec3(Vector2 vec2)
    {
        return new Vector3(vec2.x(), 0.0f, vec2.y());
    }
    public static Vector2 GetDitherVelocity(float ditherSize)
    {
        float randomAngle = Random.Range(-Mathf.PI, Mathf.PI);
        return new Vector2(Mathf.Sin(randomAngle), Mathf.Cos(randomAngle)) * ditherSize;
    }
    public static Vector2 GetDirection(RVOAgent agent)
    {
        Vector2 nowVelocity = Simulator.Instance.getAgentVelocity(agent.GetIndex());
        if (RVOMath.abs(nowVelocity) < eps)
            nowVelocity = agent.GetDirection();
        return RVOMath.normalize(nowVelocity);
    }
    public static bool IsVectorNear(Vector2 a, Vector2 b, float range)
    {
        Vector2 aNormalize = RVOMath.normalize(a);
        Vector2 bNormalize = RVOMath.normalize(b);
        //Debug.Log(Mathf.Abs(RVOMath.det(aNormalize, bNormalize)));
        if (Mathf.Abs(RVOMath.det(aNormalize, bNormalize)) < range && RVOMath.abs(aNormalize - bNormalize) < 1.0f)
            return true;
        return false;
    }
    public static bool isTwoSide(Vector2 line, Vector2 sideA, Vector2 sideB)
    {
        float detA = RVOMath.det(sideA, line);
        float detB = RVOMath.det(sideB, line);
        if ((detA > eps && detB > eps) || (detA < -eps && detB < -eps))
            return false;
        else
            return true;
    }
    public static Vector2 CalcPrefVelocity(Vector2 agentPosition, Vector2 goalPosisiton, RVOAgent agent)
    {
        Vector2 prefDistance = goalPosisiton - agentPosition;
        if (agent.GetIsStop() == true)
            return new Vector2(0f, 0f);
        if(agent.isPlayer == true)
        {
            /* Is inertial model used */
            Vector2 nowVelocity = Simulator.Instance.getAgentVelocity(agent.GetIndex());
            float nowSpeed = RVOMath.abs(nowVelocity);
            Vector2 prefVelocity = RVOMath.normalize(prefDistance) * agent.prefSpeed;//nowSpeed -> prefSpeed
            Vector2 finalVelocity = new Vector2(0, 0);
            if (nowSpeed < agent.minSpeedToTurn)
            {
                /* Not fast enough to turn */
                Vector2 advanceDirection = GetDirection(agent);
                
                if (agent.prefSpeed > nowSpeed) //Speed up fitting
                {
                    finalVelocity = nowVelocity + advanceDirection * agent.acceleration;
                }
                else //Speed down fitting
                {
                    if (nowSpeed - agent.prefSpeed < agent.acceleration)
                        finalVelocity = prefVelocity;
                    else
                        finalVelocity = nowVelocity - advanceDirection * agent.acceleration;

                }
            }
            else
            {
                /* turn */
                float direction = RVOMath.det(nowVelocity, prefVelocity);
                if (direction > eps) //left
                {
                    Vector2 rightVerticalVelocity = new Vector2(-nowVelocity.y_, nowVelocity.x_);
                    rightVerticalVelocity = RVOMath.normalize(rightVerticalVelocity) * agent.speed * agent.angularSpeed;
                    if(RVOMath.det(nowVelocity + rightVerticalVelocity , prefVelocity) < eps) //success
                    {
                        finalVelocity = RVOMath.normalize(prefVelocity) * nowSpeed;
                    }
                    else
                    {
                        finalVelocity = RVOMath.normalize(nowVelocity + rightVerticalVelocity) * nowSpeed;
                    }
                }
                if (direction >= -eps && direction <= eps) //same direction
                {
                    finalVelocity = RVOMath.normalize(prefVelocity) * nowSpeed;
                }
                if (direction < -eps) //right
                {
                    Vector2 leftVerticalVelocity = new Vector2(nowVelocity.y_, -nowVelocity.x_);
                    leftVerticalVelocity = RVOMath.normalize(leftVerticalVelocity) * agent.speed * agent.angularSpeed;
                    if (RVOMath.det(nowVelocity + leftVerticalVelocity, prefVelocity) > -eps) //success
                    {
                        finalVelocity = RVOMath.normalize(prefVelocity) * nowSpeed;
                    }
                    else
                    {
                        finalVelocity = RVOMath.normalize(nowVelocity + leftVerticalVelocity) * nowSpeed;
                    }
                }
                //Debug.Log("!!!" + RVOMath.abs(finalVelocity).ToString());
                /* acc and slow */
                if (agent.prefSpeed - nowSpeed > eps) //acc
                {
                    if (IsVectorNear(nowVelocity, prefVelocity, 0.05f))
                    {
                        finalVelocity = finalVelocity + RVOMath.normalize(finalVelocity) * Mathf.Min(agent.acceleration, agent.prefSpeed - nowSpeed);
                    }
                }
                else //slow
                {
                    finalVelocity = finalVelocity - RVOMath.normalize(finalVelocity) * Mathf.Min(agent.acceleration, nowSpeed - agent.prefSpeed);
                }
               // Debug.Log("!!!"  + RVOMath.abs(finalVelocity).ToString());
            }
            Vector2 ditherVelocity = GetDitherVelocity(ditherSize);
            if (RVOMath.abs(finalVelocity) < eps)
                return finalVelocity;
            else
                return finalVelocity + ditherVelocity; 
        }
        else
        {
            Vector2 ditherVelocity = GetDitherVelocity(ditherSize);
            Vector2 prefVelocity = RVOMath.normalize(prefDistance);
            return prefVelocity * agent.prefSpeed + ditherVelocity;
        }
    }
}
