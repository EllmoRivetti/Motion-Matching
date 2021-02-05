using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UpdateTransformFromAnimation : MonoBehaviour
{

    public Transform m_AnimatedChild;
    Vector3 m_AnimatedChildPosition;
    Quaternion m_AnimatedChildRotation;

    private void LateUpdate()
    {
        CopyTransform();
        UpdateHipsTransform();
        UpdateParentTransform();
    }

    void CopyTransform()
    {
        m_AnimatedChildPosition = m_AnimatedChild.transform.position;
        m_AnimatedChildRotation = m_AnimatedChild.transform.rotation;
    }
    void UpdateParentTransform()
    {
        transform.position = new Vector3(
            m_AnimatedChildPosition.x,
            transform.position.y,
            m_AnimatedChildPosition.z
        );
        transform.rotation = m_AnimatedChildRotation;
    }
    void UpdateHipsTransform()
    {
        m_AnimatedChild.transform.position = new Vector3(
            0.0f,
            m_AnimatedChildPosition.y,
            0.0f
        );

        m_AnimatedChild.transform.rotation = Quaternion.identity;
    }
}
