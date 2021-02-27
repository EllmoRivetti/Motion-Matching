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

        public bool m_ApplyRotation = true;
        public bool m_ApplyTranslation = true;
        public bool m_ApplyMMInUpdate = false;
        public float m_DeltaTime = 1.0f;


        private bool m_CalculatingMotionMatchingFrame = false;


        private void OnValidate()
        {
            Time.timeScale = m_DeltaTime;
        }
        void Update()
        {
            //print(transform.rotation);
            if (m_ApplyMMInUpdate)
                RunMotionMatchingOnce(verbose: false);

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

            foreach (var kvp in m_AnimationController.m_LoadedMocapFrameData.m_FrameData) //  count - n
            {
                //i ~ i + n
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

        [Button]
        public void RunFromFrame(int frame)
        {
            ApplyAnimationFrame(m_AnimationController.m_LoadedMocapFrameData.m_FrameData[frame]);
        }

        // https://forum.unity.com/threads/transform-inversetransformpoint-without-transform.954939/
        // https://twitter.com/georgerrmartin_/status/410279960624918529?lang=fr
        Vector3 InverseTransformPoint(Vector3 transforPos, Vector3 pos, Quaternion transformRotation, Vector3 transformScale)
        {
            Matrix4x4 matrix = Matrix4x4.TRS(transforPos, transformRotation, transformScale);
            Matrix4x4 inverse = matrix.inverse;
            return inverse.MultiplyPoint3x4(pos);
        }

        private void ApplyAnimationFrame(MocapFrameData frameData)
        {
            //Debug.Log("ApplyAnimationFrame (from:" + frameData.m_FrameNumber + "; interval: " + m_MMAnimationFinished + ")");

            Debug.Log("Position hip framedata" + frameData.m_PositionHipProjection);
            Debug.Log("Position rotation framedata" + frameData.m_RotationHipProjection);

            //Debug.Log("OUR local PR " + transform.localPosition + " " + transform.localRotation);
            // transform -> character
            // m_HipsTransform -> courant
            // frameData -> nouvelle


            if(m_ApplyRotation)
            {



                /*
                 * Calcul de la rotation effective
                 * Il s'agit de la rotation qui a été effectué pendant l'animation qu'il faut conserver
                 * lors de l'attribution d'une nouvelle animation
                 */


                float effectiveRotation = m_HipsTransform.rotation.y - transform.rotation.y;
                Debug.Log("effective rotation : " + effectiveRotation);


                Debug.Log("INITIAL pos : " + m_HipsTransform.position);


                /*
                 * Pour prévoir et translater le mouvement (en arc de cercle) qui est fait lors
                 * de la rotation sur l'objet parent
                 * Il faut que l'objet fils soit déjà déplacé
                 * 
                 * Du coup, on applique le futur déplacement (pour le retirer par la suite
                 */

                Vector3 tempFuturMovement = frameData.m_PositionHipProjection;
                m_HipsTransform.localPosition += tempFuturMovement;


                // On retient ou se trouve la position après le simple déplacement
                Vector3 oldPos = m_HipsTransform.position;



                // On applique la rotation depuis la frame data, en conservant la rotation effective (toujours négatif)
                // Il est possible que ce soit (-effectiveRotation au lieu de +
                Vector3 newRotation = Vector3.up * (+effectiveRotation - frameData.m_RotationHipProjection.y);
                Debug.Log("New Rotation : " + newRotation);
                transform.eulerAngles = newRotation;

                // On retient la nouvelle position qui a changé : le but est de corriger le déplacement
                Vector3 newPos = m_HipsTransform.position;

                Debug.Log("Old hip pos : " + oldPos);
                Debug.Log("New hip pos : " + newPos);

                // Le vecteur de déplacement est simplement la soustraction des deux
                Vector3 sumFromMovement = oldPos - newPos;

                // On peut maintenant retirer le déplacement temporaire
                // L'objet transform.position est toujours "initial", il n'a pas bougé

                m_HipsTransform.localPosition -= tempFuturMovement;


                Debug.Log("La position AVANT la translation : " + transform.localPosition);
                // Et maintenant, on ajoute le déplacement qui a été effectué a la position du parent
                // Pour remettre "à 0" la rotation qui a été effectuée
                // ATTENTION : DOIT quand même agir comme une téléportation : seul m_ApplyTransformation permet de corriger l'emplacement d'où est joué l'animation
                transform.localPosition += sumFromMovement;

                Debug.Log("La position APRES la translation : " + transform.localPosition);


                /*
                 * Le problème : c'est que ça marche pas.
                 * sumFromMovement contient bien la bonne valeur à ajouter a position/localPosition (c'est pareil puisque il est a la racine de la scène)
                 * lorsque l'on ajoute et qu'on débug, la bonne valeure est bien écrite
                 * Sauf qu'à l'inspecteur, il n'est pas au bon endroit
                 * Et hormis ça, rien d'autre n'a l'air de toucher au déplacement du character
                 * (Si on retire la ligne, il reste en 0 0)
                 * 
                 * Du coup jsp c'est un bug chelou
                 */

            }

            //Debug.Log("OUR NEW local position AND rotation : " + transform.localPosition + " " + transform.localRotation);


            // // Fix rotation
            // transform.Rotate(Vector3.up * m_RotationFoundInMM);
            // 
            // // Fix position
            // Vector3 translation = frameData.m_PositionHipProjection - m_HipsTransform.localPosition;
            // transform.localPosition += translation;

            // float angle = Quaternion.Angle(m_HipsTransform.rotation, frameData.m_RotationHipProjection_q);
            // transform.Rotate(transform.up, angle);
            // transform.rotation = angle;

            // Quaternion diff = m_HipsTransform.rotation * Quaternion.Inverse(frameData.m_RotationHipProjection_q);
            // transform.rotation = diff * transform.rotation;

            /*
            
            if (m_ApplyRotation)
            {

                Vector3 armPosition = m_HipsTransform.position;
                Vector3 handPosition = armPosition + m_HipsTransform.forward;
                Vector3 boxPosition = frameData.m_PositionHipProjection + frameData.m_HipProjectionForward;

                Vector3 currentOffset = m_HipsTransform.InverseTransformPoint(handPosition);
                Vector3 desiredOffset = InverseTransformPoint(armPosition, boxPosition, Quaternion.identity, Vector3.one);

                // float angle = Quaternion.Angle(m_HipsTransform.rotation, frameData.m_RotationHipProjection_q);
                // transform.RotateAround(transform.position, transform.up, angle);
                // transform.Rotate(transform.up, angle);
                m_HipsTransform.transform.localRotation = Quaternion.FromToRotation(currentOffset, desiredOffset);
            }*/
            if (m_ApplyTranslation)
            {
                /*
                Vector3 translationCharacter = new Vector3(
                    frameData.m_PositionHipProjection.x - transform.localPosition.x,
                    0.0f,
                    frameData.m_PositionHipProjection.z - transform.localPosition.z
                );
                // 
                // // Vector3 rotationCharacter = m_HipsTransform.eulerAngles - frameData.m_RotationHipProjection_ls_euler;
                // 
                transform.localPosition = translationCharacter;*/

                //transform.localPosition = -frameData.m_PositionHipProjection;

                transform.localPosition -= (frameData.m_PositionHipProjection - m_HipsTransform.localPosition);

            }
            

            /*
            if (m_ApplyTranslation)
            {
                
                Vector3 translationCharacter = new Vector3(
                    frameData.m_PositionHipProjection.x - m_HipsTransform.position.x,
                    0.0f,
                    frameData.m_PositionHipProjection.z - m_HipsTransform.position.z
                );
                // 
                // // Vector3 rotationCharacter = m_HipsTransform.eulerAngles - frameData.m_RotationHipProjection_ls_euler;
                // 
                transform.position += translationCharacter;
            }*/

            m_AnimationController.RunNFramesFromFrame(
                m_MotionMatchingFramesIntervalToUse, 
                frameData.m_FrameNumber, 
                () => m_MMAnimationFinished = true
            );

        }

        

        
    }

}   