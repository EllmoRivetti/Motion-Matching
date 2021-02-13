using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEngine.AI;
using System;
using MotionMatching.Matching;
using MotionMatching.Animation;
using Sirenix.OdinInspector;


namespace MotionMatching.Animation
{
    public class MM_Mover : MonoBehaviour
    {
        public Transform m_Destination;
        public AnimationController m_AnimationController;
        [Range(0, 50)] public int m_MotionMatchingFramesIntervalToUse = 10;
        private bool m_MMAnimationFinished = true;
        public Transform m_HipsTransform;



        private bool m_CalculatingMotionMatchingFrame = false;
        
        void Update()
        {
            // RunMotionMatchingOnce(verbose: false);
        }

        [Button]
        public void RunMotionMatchingOnce(bool verbose = true)
        {
            if (!m_CalculatingMotionMatchingFrame && m_MMAnimationFinished == true)
            {
                m_CalculatingMotionMatchingFrame = true;
                StartCoroutine(MoveUsingMotionMatching());
                m_CalculatingMotionMatchingFrame = false;
            }
            else 
            {
                if (verbose)
                    Debug.Log("MotionMatching Already running");
            }
        }


        private Vector3 GetCharacterMovement()
        {
            Vector3 movementDestination = m_Destination.position;
            Vector3 currentCharacterPosition = m_HipsTransform.position;
            return movementDestination - currentCharacterPosition;
        }

        private IEnumerator MoveUsingMotionMatching()
        {
            m_MMAnimationFinished = false;
            Vector3 characterMovement = GetCharacterMovement();

            var bestFrame = GetBestFrame(characterMovement);
            ApplyAnimationFrame(bestFrame);

            yield return null;
        }

        private MocapFrameData GetBestFrame(Vector3 movement)
        {
            // Debug.Log("----------------------------");
            // Debug.Log("inside GetBestFrame");
            // Debug.Log(movement);

            float bestScore = -1;
            MocapFrameData bestFrame = null;

            foreach (var kvp in m_AnimationController.m_LoadedMocapFrameData.m_FrameData)
            {
                var frame = kvp.Value;
                var score = frame.GetFrameScore(new Vector2(movement.x, movement.z));

                if (bestScore < score || bestFrame == null)
                {
                    bestScore = score;
                    bestFrame = frame;
                }
            }
            // Debug.Log(bestScore);
            // Debug.Log(bestFrame);
            return bestFrame;
        }

        private void ApplyAnimationFrame(MocapFrameData frameData)
        {
            Debug.Log("ApplyAnimationFrame (from:" + frameData.m_FrameNumber + "; interval: " + m_MMAnimationFinished + ")");
            m_AnimationController.RunNFramesFromFrame(
                m_MotionMatchingFramesIntervalToUse, 
                frameData.m_FrameNumber, 
                () => m_MMAnimationFinished = true
            );
        }
    }
}   