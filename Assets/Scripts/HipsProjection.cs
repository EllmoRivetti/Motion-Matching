using UnityEngine;

public class HipsProjection : MonoBehaviour
{

    [SerializeField] GameObject m_Hips, m_RFeet, m_LFeet, m_Hips2DOnGround;
    private GameObject m_CurrentPlatform, m_Ground;

    public bool m_DrawArrow = true;

    private Vector3 m_hipsGroundPosition { get; set; }
    private Vector3 m_rFeetHipsPosition { get; set; }
    private Vector3 m_lFeetHipsPosition { get; set; }

    public Vector3 getHips2DOnGroundPosition()
    {
        return m_Hips2DOnGround.transform.position;
    }

    public Vector3 getHips2DOnGroundRotation()
    {
        return m_Hips2DOnGround.transform.forward;
    }


    void Start()
    {
    }

    void Update()
    {
        if (!m_DrawArrow)
            return;
        Vector3 hipsPosition = m_Hips.transform.position;
        Vector3 hipsDirection = m_Hips.transform.forward;

        Vector3 rFeetPosition = m_RFeet.transform.position;
        Vector3 rFeetDirection = m_RFeet.transform.forward;

        Vector3 lFeetPosition = m_LFeet.transform.position;
        Vector3 lFeetDirection = m_LFeet.transform.forward;

        if (m_Ground != null)
        {
            Vector3 hipsPosition2D = new Vector3(hipsPosition.x, 0, hipsPosition.z);

            m_Hips2DOnGround.transform.position = hipsPosition2D;
            m_Hips2DOnGround.transform.LookAt(m_Hips2DOnGround.transform.position + m_Hips.transform.forward.normalized, Vector3.up);

            m_rFeetHipsPosition = m_Hips2DOnGround.transform.InverseTransformPoint(m_RFeet.transform.position);
            m_lFeetHipsPosition = m_Hips2DOnGround.transform.InverseTransformPoint(m_LFeet.transform.position);
        
            drawArrowOnGround(m_Hips2DOnGround.transform.position, m_Hips2DOnGround.transform.forward, Color.red);
            drawArrowOnGround(rFeetPosition, rFeetDirection, Color.blue);
            drawArrowOnGround(lFeetPosition, lFeetDirection, Color.green);
        }
    }

    #region draw arrow on ground
    private void OnCollisionEnter(Collision collision)
    {
        if(collision.gameObject.tag == "Ground")
        {
            // Debug.Log("Platform change");
            m_CurrentPlatform = collision.gameObject;
            m_Ground = collision.gameObject.transform.parent.gameObject;
        }
    }
    void drawArrowOnGround(Vector3 position,Vector3 direction, Color color)
    {
        direction.y = m_CurrentPlatform.transform.rotation.y;
        position.y = m_CurrentPlatform.transform.position.y + 0.1f;
        float arrowHeadLength = 0.25f, arrowHeadAngle = 20.0f;
        
        Vector3 right = Quaternion.LookRotation(direction) * Quaternion.Euler(0, 180 + arrowHeadAngle, 0) * new Vector3(0, 0, 1);
        Vector3 left = Quaternion.LookRotation(direction) * Quaternion.Euler(0, 180 - arrowHeadAngle, 0) * new Vector3(0, 0, 1);
        Debug.DrawRay(position + direction.normalized/2, right * arrowHeadLength, color);
        Debug.DrawRay(position + direction.normalized/2, left * arrowHeadLength, color);
        Debug.DrawRay(position, direction.normalized/2, color);
    }
    #endregion


}
