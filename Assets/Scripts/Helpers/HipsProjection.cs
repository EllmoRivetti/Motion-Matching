using UnityEngine;

public class HipsProjection : MonoBehaviour
{
    #region Members
    [SerializeField] GameObject m_Hips, m_RFeet, m_LFeet, m_Hips2DOnGround;
    private GameObject m_Ground;

    public bool m_DrawArrow = true;

    private Vector3 m_RightFeetPositionInHipsSpace_l { get; set; }
    private Vector3 m_LeftFeetPositionInHipsSpace_l { get; set; }
    #endregion

    #region Unity events
    void Update()
    {
        if (!m_DrawArrow)
            return;

        Vector3 hipsPosition_w = m_Hips.transform.position;

        Vector3 rFeetPosition_w = m_RFeet.transform.position;
        Vector3 rFeetDirection = m_RFeet.transform.forward;

        Vector3 lFeetPosition_w = m_LFeet.transform.position;
        Vector3 lFeetDirection = m_LFeet.transform.forward;

        Vector3 hipsPosition2D_w = new Vector3(hipsPosition_w.x, 0, hipsPosition_w.z);

        m_Hips2DOnGround.transform.position = hipsPosition2D_w;
        m_Hips2DOnGround.transform.LookAt(m_Hips2DOnGround.transform.position + m_Hips.transform.forward.normalized, Vector3.up);

        m_RightFeetPositionInHipsSpace_l = m_Hips2DOnGround.transform.InverseTransformPoint(m_RFeet.transform.position);
        m_LeftFeetPositionInHipsSpace_l = m_Hips2DOnGround.transform.InverseTransformPoint(m_LFeet.transform.position);

        if (m_Ground)
        {
            DrawArrowOnGround(m_Hips2DOnGround.transform.position, m_Hips2DOnGround.transform.forward, Color.red);
            DrawArrowOnGround(rFeetPosition_w, rFeetDirection, Color.blue);
            DrawArrowOnGround(lFeetPosition_w, lFeetDirection, Color.green);
        }
    }
    #endregion
    #region Draw arrow on ground
    private void OnCollisionEnter(Collision collision)
    {
        m_Ground = collision.gameObject;
    }
    void DrawArrowOnGround(Vector3 position,Vector3 direction, Color color)
    {
        direction.y = m_Ground.transform.rotation.y;
        position.y = m_Ground.transform.position.y + 0.1f;
        float arrowHeadLength = 0.25f, arrowHeadAngle = 20.0f;
        float arrowLength = 1.5f;
        
        Vector3 right = Quaternion.LookRotation(direction) * Quaternion.Euler(0, 180 + arrowHeadAngle, 0) * new Vector3(0, 0, 1);
        Vector3 left = Quaternion.LookRotation(direction) * Quaternion.Euler(0, 180 - arrowHeadAngle, 0) * new Vector3(0, 0, 1);
        Debug.DrawRay(position + direction.normalized * arrowLength, right * arrowHeadLength, color);
        Debug.DrawRay(position + direction.normalized * arrowLength, left * arrowHeadLength, color);
        Debug.DrawRay(position, direction.normalized * arrowLength, color);
    }
    #endregion


}
