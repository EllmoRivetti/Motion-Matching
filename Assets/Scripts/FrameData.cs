using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FrameData
{
    static private float m_FuturTime = 1.0f;

    public float m_FrameTime { get; set; }
    public Vector2 m_PositionHipProjection { get; set; }
    public Vector2 m_PositionFuturHipProjection { get; set; }

    public FeetPositions m_PositionFeet { get; set; }


    public struct FeetPositions
    {
        public Vector3 m_PositionRightFoot { get; set; }
        public Vector3 m_PositionLeftFoot { get; set; }

        public FeetPositions(Vector3 right, Vector3 left)
        {
            m_PositionRightFoot = right;
            m_PositionLeftFoot = left;
        }

        public float GetInBetweenDistance()
        {
            return Vector3.Distance(m_PositionLeftFoot, m_PositionRightFoot);
        }
    }

    public FrameData(float frameTime, Vector2 positionHipProjection, Vector2 positionFuturHipProjection, FeetPositions positionFeet)
    {
        this.m_FrameTime = frameTime;
        this.m_PositionHipProjection = positionHipProjection;
        this.m_PositionFuturHipProjection = positionFuturHipProjection;
        this.m_PositionFeet = positionFeet;
    }

    public float GetFrameScore(Vector2 hipTarget)
    {
        //Pied * 0.3
        float distFeet = this.m_PositionFeet.GetInBetweenDistance() * 0.3f;

        //Angle inbtw 
        float angleHip = Vector2.Angle(this.m_PositionHipProjection, hipTarget);

        //Sum
        float sum = distFeet + angleHip;

        return sum;
    }

}
