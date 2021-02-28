using UnityEngine;

namespace MotionMatching.Matching
{
    public class MocapFrameData
    {
        // Index of the frame in the animation
        public int m_FrameNumber { get; set; }

        // Repere par rapport au gameobject "Ground"
        public Vector3 m_PositionHipProjection { get; set; }
        // Repere par rapport 
        public Vector3 m_PositionFuturHipProjection { get; set; }
        public Vector3 m_HipProjectionForward { get; set; }



        public Vector3 m_RotationHipProjection { get; set; }
        public Quaternion m_RotationHipProjection_q { get; set; }

        public FeetPositions m_PositionFeet { get; set; }

        public struct FeetPositions
        {
            public Vector3 m_PositionRightFoot_l { get; set; }
            public Vector3 m_PositionLeftFoot_l { get; set; }

            public FeetPositions(Vector3 right, Vector3 left)
            {
                m_PositionRightFoot_l = right;
                m_PositionLeftFoot_l = left;
            }

            public float GetInBetweenDistance()
            {
                return Vector3.Distance(m_PositionLeftFoot_l, m_PositionRightFoot_l);
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

        public float GetFrameScore(Vector2 desiredMovement)
        {
            //Angle inbtw   
            Vector2 projectionForward = (new Vector2(m_PositionFuturHipProjection.x, m_PositionFuturHipProjection.z) - new Vector2(m_PositionHipProjection.x, m_PositionHipProjection.z)) * 5.0f;
            float angle = Vector2.Angle(desiredMovement, projectionForward);

            if (angle < MotionMatching.Constants.EPSILON)
                return 100;
            return 1.0f / angle;
        }

    }
}
