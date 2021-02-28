using System.Collections;
using UnityEngine;
using MotionMatching.Matching;
using Sirenix.OdinInspector;


namespace MotionMatching.Animation
{
    public class MM_Mover : MonoBehaviour
    {
        #region Members
        public Transform m_DestinationTransform;
        public Transform m_HipsTransform;

        [Range(0.0f, 5.0f)] public float m_StopDistance_ua;
        [ReadOnly] public int m_RecommendedFramesIntervalToUse = MotionMatching.Constants.MM_NEXT_FRAME_INTERVAL_SIZE;
        [Range(0, 50)] public int m_FramesIntervalToUse = MotionMatching.Constants.MM_NEXT_FRAME_INTERVAL_SIZE;

        [ReadOnly] public bool m_ApplyRotation = false;
        public bool m_ApplyTranslation = true;
        public bool m_ApplyAnimation = true;
        public bool m_ApplyMMInUpdate = true;
        public float m_TimeScale = 1.0f;

        bool m_CalculatingFrame = false;
        bool m_FrameIntervalIsRunning = true;
        AnimationController m_AnimationController;
        MocapFrameData m_CurrentFrameData;
        #endregion

        #region Unity events
        private void Awake()
        {
            m_AnimationController = GetComponent<AnimationController>();
        }
        private void OnValidate()
        {
            Time.timeScale = m_TimeScale;
        }
        void Update()
        {
            if (m_ApplyMMInUpdate && !ReachedTarget(m_DestinationTransform.position))
                RunMotionMatchingOnce(verbose: false);
        }
        public void OnDrawGizmos()
        {
            if (m_CurrentFrameData != null)
            {
                Vector3 characterMovement = GetCharacterMovement();
                Gizmos.color = Color.red;
                Gizmos.DrawLine(m_HipsTransform.position, m_HipsTransform.position + characterMovement);

                Gizmos.color = Color.yellow;

                Vector3 pos = m_HipsTransform.position + m_CurrentFrameData.m_PositionHipProjection;
                Vector3 nextPos = m_HipsTransform.position + m_CurrentFrameData.m_PositionFuturHipProjection;
                Vector3 fwd = nextPos - pos;
                Gizmos.DrawCube(pos, Vector3.one * .4f);
                Gizmos.DrawRay(pos, fwd * 5 * 5);
            }
        }
        #endregion
        #region Run Motion Matching
        [Button]
        public void RunMotionMatchingOnce(bool verbose = true)
        {
            if (!m_CalculatingFrame && m_FrameIntervalIsRunning == true)
            {
                m_CalculatingFrame = true;
                StartCoroutine(MoveUsingMotionMatching());
                m_CalculatingFrame = false;
            }
            else
            {
                if (verbose)
                    Debug.Log("MotionMatching Already running");
            }
        }
        private bool ReachedTarget(Vector3 target)
        {
            float currentDistance = Vector2.Distance(new Vector2(m_HipsTransform.position.x, m_HipsTransform.position.z), new Vector2(target.x, target.z));
            return currentDistance <= m_StopDistance_ua;
        }
        private Vector3 GetCharacterMovement()
        {
            Vector3 movementDestination = m_DestinationTransform.position;
            Vector3 currentCharacterPosition = m_HipsTransform.position;
            return movementDestination - currentCharacterPosition;
        }
        private IEnumerator MoveUsingMotionMatching()
        {
            m_FrameIntervalIsRunning = false;
            Vector3 characterMovement = GetCharacterMovement();

            var bestFrame = GetBestFrame(characterMovement);
            m_CurrentFrameData = bestFrame;
            ApplyAnimationFrame(bestFrame);

            yield return null;
        }
        private MocapFrameData GetBestFrame(Vector3 movement)
        {
            // Debug.Log("----------------------------");
            // Debug.Log("inside GetBestFrame");

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
            Debug.Log(bestScore);
            // Debug.Log(bestFrame);
            return bestFrame;
        }
        private void ApplyAnimationFrame(MocapFrameData frameData)
        {
            Debug.Log("ApplyAnimationFrame (from:" + frameData.m_FrameNumber + "; interval: " + m_FrameIntervalIsRunning + ")");

            Debug.Log("hip framedata Position " + frameData.m_PositionHipProjection);
            Debug.Log("hip framedata Rotation " + frameData.m_RotationHipProjection);

            // m_HipsTransform.parent.Rotate(m_HipsTransform.parent.transform.up, 180.0f);

            if (m_ApplyRotation)
            {
                ApplyAnimationFrameAdaptRotationBeforeAnimation();
            }
            if (m_ApplyTranslation)
            {
                ApplyAnimationFrameAdaptTranslation();
            }

            if (m_ApplyAnimation)
            {

                m_AnimationController.RunNFramesFromFrame(
                    m_FramesIntervalToUse,
                    frameData.m_FrameNumber,
                    () => m_FrameIntervalIsRunning = true
                );
            }
            else
            {
                m_FrameIntervalIsRunning = true;
            }
            if (m_ApplyRotation)
            {
                ApplyAnimationFrameAdaptRotationAfterAnimation();
            }

        }
        #endregion
        #region Adapt character to animation
        public void ApplyAnimationFrameAdaptRotationBeforeAnimation()
        {
#if ROTATE_CHARACTER
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

            Vector3 tempFuturMovement = m_CurrentFrameData.m_PositionHipProjection;
            m_HipsTransform.localPosition += tempFuturMovement;


            // On retient ou se trouve la position après le simple déplacement
            Vector3 oldPos = m_HipsTransform.position;



            // On applique la rotation depuis la frame data, en conservant la rotation effective (toujours négatif)
            // Il est possible que ce soit (-effectiveRotation au lieu de +
            Vector3 newRotation = Vector3.up * (+effectiveRotation - m_CurrentFrameData.m_RotationHipProjection.y);
            Debug.Log("New Rotation : " + newRotation);
            transform.rotation = Quaternion.Euler(newRotation);

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
#endif
        }
        public void ApplyAnimationFrameAdaptRotationAfterAnimation()
        {
            // Quaternion initialHipsOrientation = m_HipsTransform.rotation;
            // m_HipsTransform.LookAt(m_Destination);
            // Transform hipsParent = m_HipsTransform.parent;
            // 
            // Quaternion diff = m_HipsTransform.rotation * Quaternion.Inverse(initialHipsOrientation);
            // hipsParent.rotation = diff * hipsParent.rotation;
            // 
            // // m_HipsTransform.rotation = initialHipsOrientation;
        }
        public void ApplyAnimationFrameAdaptTranslation()
        {
            transform.localPosition -= (m_CurrentFrameData.m_PositionHipProjection - m_HipsTransform.localPosition);
        }
        #endregion
    }

}