using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RVO;
using Vector2 = RVO.Vector2;
public class Test1 : MonoBehaviour
{
    Vector2 position_ = new Vector2(0, 0);
    Vector2 otherPosition_ = new Vector2(0, 20);
    Vector2 velocity_ = new Vector2(1, 1);
    Vector2 otherVelocity_ = new Vector2(1, 0);
    float radius_ = 1.0f;
    float otherRadius_ = 2.0f;
    float invTimeHorizon = 1.0f / 5.0f;
    IList<Line> orcaLines_ = new List<Line>();
    void Start()
    {
        doStep(); 
    }
    void doStep()
    {
        /* Create agent ORCA lines. */
        for (int i = 0; i < 1; ++i)
        {

            Vector2 relativePosition = otherPosition_ - position_;
            Vector2 relativeVelocity = velocity_ - otherVelocity_;
            float distSq = RVOMath.absSq(relativePosition);
            float combinedRadius = radius_ + otherRadius_;
            float combinedRadiusSq = RVOMath.sqr(combinedRadius);

            Line line;
            Vector2 u;

            if (distSq > combinedRadiusSq)
            {
                /* No collision. */
                Vector2 w = relativeVelocity - invTimeHorizon * relativePosition;

                /* Vector from cutoff center to relative velocity. */
                float wLengthSq = RVOMath.absSq(w);
                float dotProduct1 = w * relativePosition;
                //Debug.Log("w:" + w.ToString() + " dotProduct1:" + dotProduct1.ToString());
                if (dotProduct1 < 0.0f && RVOMath.sqr(dotProduct1) > combinedRadiusSq * wLengthSq)
                {
                    /* Project on cut-off circle. */
                    float wLength = RVOMath.sqrt(wLengthSq);
                    Vector2 unitW = w / wLength;

                    line.direction = new Vector2(unitW.y(), -unitW.x());
                    u = (combinedRadius * invTimeHorizon - wLength) * unitW;
                }
                else
                {
                    /* Project on legs. */
                    float leg = RVOMath.sqrt(distSq - combinedRadiusSq);

                    if (RVOMath.det(relativePosition, w) > 0.0f)
                    {
                        /* Project on left leg. */
                        line.direction = new Vector2(relativePosition.x() * leg - relativePosition.y() * combinedRadius, relativePosition.x() * combinedRadius + relativePosition.y() * leg) / distSq;
                    }
                    else
                    {
                        /* Project on right leg. */
                        line.direction = -new Vector2(relativePosition.x() * leg + relativePosition.y() * combinedRadius, -relativePosition.x() * combinedRadius + relativePosition.y() * leg) / distSq;
                    }

                    float dotProduct2 = relativeVelocity * line.direction;
                    u = dotProduct2 * line.direction - relativeVelocity;
                }
            }
            else
            {
                /* Collision. Project on cut-off circle of time timeStep. */
                float invTimeStep = 1.0f / Simulator.Instance.timeStep_;

                /* Vector from cutoff center to relative velocity. */
                Vector2 w = relativeVelocity - invTimeStep * relativePosition;

                float wLength = RVOMath.abs(w);
                Vector2 unitW = w / wLength;

                line.direction = new Vector2(unitW.y(), -unitW.x());
                u = (combinedRadius * invTimeStep - wLength) * unitW;

            }
            line.point = velocity_ + 0.5f * u;
            Debug.Log("u:" + u.ToString() + " Point:" + line.point.ToString() + " Direction:" + line.direction.ToString());
            orcaLines_.Add(line);
        }
        float maxSpeed = 2.0f;
        Vector2 prefVelocity = new Vector2(1, 1);
        Vector2 newVelocity = new Vector2(0, 0);
        linearProgram1(orcaLines_, 0, maxSpeed, prefVelocity, false, ref newVelocity);
        //Debug.Log("prefVelocity:" + prefVelocity.ToString() + " newVelocity:" + newVelocity.ToString());

        /*
        int lineFail = linearProgram2(orcaLines_, maxSpeed_, prefVelocity_, false, ref newVelocity_);

        if (lineFail < orcaLines_.Count)
        {
            linearProgram3(orcaLines_, numObstLines, lineFail, maxSpeed_, ref newVelocity_);
        }
        */
    } 
    private bool linearProgram1(IList<Line> lines, int lineNo, float radius, Vector2 optVelocity, bool directionOpt, ref Vector2 result)
    {
        float dotProduct = lines[lineNo].point * lines[lineNo].direction;
        float discriminant = RVOMath.sqr(dotProduct) + RVOMath.sqr(radius) - RVOMath.absSq(lines[lineNo].point);
        if (discriminant < 0.0f)
        {
            /* Max speed circle fully invalidates line lineNo. */
            return false;
        }

        float sqrtDiscriminant = RVOMath.sqrt(discriminant);
        float tLeft = -dotProduct - sqrtDiscriminant;
        float tRight = -dotProduct + sqrtDiscriminant;

        for (int i = 0; i < lineNo; ++i)
        {
            float denominator = RVOMath.det(lines[lineNo].direction, lines[i].direction);
            float numerator = RVOMath.det(lines[i].direction, lines[lineNo].point - lines[i].point);

            if (RVOMath.fabs(denominator) <= RVOMath.RVO_EPSILON)
            {
                /* Lines lineNo and i are (almost) parallel. */
                if (numerator < 0.0f)
                {
                    return false;
                }

                continue;
            }

            float t = numerator / denominator;

            if (denominator >= 0.0f)
            {
                /* Line i bounds line lineNo on the right. */
                tRight = Math.Min(tRight, t);
            }
            else
            {
                /* Line i bounds line lineNo on the left. */
                tLeft = Math.Max(tLeft, t);
            }

            if (tLeft > tRight)
            {
                return false;
            }
        }

        if (directionOpt)
        {
            /* Optimize direction. */
            if (optVelocity * lines[lineNo].direction > 0.0f)
            {
                /* Take right extreme. */
                result = lines[lineNo].point + tRight * lines[lineNo].direction;
            }
            else
            {
                /* Take left extreme. */
                result = lines[lineNo].point + tLeft * lines[lineNo].direction;
            }
        }
        else
        {
            /* Optimize closest point. */
            float t = lines[lineNo].direction * (optVelocity - lines[lineNo].point);

            //Debug.Log(tLeft.ToString() + " " + t.ToString() + " " + tRight.ToString());
            if (t < tLeft)
            {
                result = lines[lineNo].point + tLeft * lines[lineNo].direction;
            }
            else if (t > tRight)
            {
                result = lines[lineNo].point + tRight * lines[lineNo].direction;
            }
            else
            {
                result = lines[lineNo].point + t * lines[lineNo].direction;
            }
        }

        return true;
    }
}
