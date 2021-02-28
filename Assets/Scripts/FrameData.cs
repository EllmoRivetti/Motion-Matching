using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MotionMatching.Matching
{
    public class MocapFrameData
    {
        static private int m_FuturTime = 1;

        public int m_FrameNumber { get; set; }

        // Repere par rapport au gameobject "Ground"
        public Vector3 m_PositionHipProjection { get; set; }
        public Vector3 m_HipProjectionForward { get; set; }

        // Repere par rapport 
        public Vector3 m_PositionFuturHipProjection { get; set; }


        public Vector3 m_RotationHipProjection { get; set; }
        public Quaternion m_RotationHipProjection_q { get; set; }

        // TODO add hip rotation

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

        public MocapFrameData(
            int frameTime, 
            Vector3 positionHipProjection, 
            Vector3 positionFuturHipProjection, 
            FeetPositions positionFeet,
            Vector3 rotationHipProjection,
            Quaternion rotationHipProjection_q,
            Vector3 hipProjectionForward)
        {
            this.m_FrameNumber = frameTime;
            this.m_PositionHipProjection = positionHipProjection;
            this.m_PositionFuturHipProjection = positionFuturHipProjection;
            this.m_PositionFeet = positionFeet;
            this.m_RotationHipProjection = rotationHipProjection;
            this.m_RotationHipProjection_q = rotationHipProjection_q;
            this.m_HipProjectionForward = hipProjectionForward;
        }

        private float AngleBetweenVector2(Vector2 vec1, Vector2 vec2)
        {
            Vector2 diference = vec2 - vec1;
            float sign = (vec2.y < vec1.y) ? -1.0f : 1.0f;
            return Vector2.Angle(Vector2.right, diference) * sign;
        }

        public float GetFrameScore(Vector2 desiredMovement)
        {
            //Angle inbtw   
            Vector2 projectionForward = (new Vector2(m_PositionFuturHipProjection.x, m_PositionFuturHipProjection.z) - new Vector2(m_PositionHipProjection.x, m_PositionHipProjection.z)) * 5.0f;
            float angle = Vector2.SignedAngle(desiredMovement, projectionForward);


            //Angle inbtw 
            // Vector3 projectionDirection = m_PositionFuturHipProjection - m_PositionHipProjection;
            float angleHip = Vector2.Angle(projectionForward, desiredMovement);

            // return angleHip;

            if (Mathf.Abs(angle) < MotionMatching.Constants.EPSILON)
                return 100;
            return 1.0f / angle;

            /*
            //Pied * 0.3
            float distFeet = this.m_PositionFeet.GetInBetweenDistance() * 0.3f;
            //Sum

            float sum = distFeet + angleHip;

            return sum;
            */
        }

    }
}
