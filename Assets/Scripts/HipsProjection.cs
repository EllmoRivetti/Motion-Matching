using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HipsProjection : MonoBehaviour
{

    [SerializeField] GameObject Hips, RFeet, LFeet;
    private GameObject Repere, CurrentPlatform, Ground;

    public bool m_DrawArrow = true;

    private Vector3 _hipsGroundPosition { get; set; }
    private Vector3 _rFeetHipsPosition { get; set; }
    private Vector3 _lFeetHipsPosition { get; set; }

    void Start()
    {
        Repere = new GameObject();
        Repere.name = "HipsRepere";
    }

    void Update()
    {
        if (!m_DrawArrow)
            return;
        Vector3 HipsPosition = Hips.transform.position;
        Vector3 HipsDirection = Hips.transform.forward;

        Vector3 RFeetPosition = RFeet.transform.position;
        Vector3 RFeetDirection = RFeet.transform.forward;

        Vector3 LFeetPosition = LFeet.transform.position;
        Vector3 LFeetDirection = LFeet.transform.forward;

        if (Ground != null)
        {
            _hipsGroundPosition = Ground.transform.InverseTransformPoint(Hips.transform.position);
            Repere.transform.position = _hipsGroundPosition;
            _rFeetHipsPosition = Repere.transform.InverseTransformPoint(RFeet.transform.position);
            _lFeetHipsPosition = Repere.transform.InverseTransformPoint(LFeet.transform.position);
        
            drawArrowOnGround(HipsPosition, HipsDirection, Color.red);
            drawArrowOnGround(RFeetPosition, RFeetDirection, Color.blue);
            drawArrowOnGround(LFeetPosition, LFeetDirection, Color.green);
        }
    }

    #region draw arrow on ground
    private void OnCollisionEnter(Collision collision)
    {
        if(collision.gameObject.tag == "Platform")
        {
            // Debug.Log("Platform change");
            CurrentPlatform = collision.gameObject;
            Ground = collision.gameObject.transform.parent.gameObject;
        }
    }
    void drawArrowOnGround(Vector3 position,Vector3 direction, Color color)
    {
        direction.y = CurrentPlatform.transform.rotation.y;
        position.y = CurrentPlatform.transform.position.y + 0.1f;
        float arrowHeadLength = 0.25f, arrowHeadAngle = 20.0f;
        
        Vector3 right = Quaternion.LookRotation(direction) * Quaternion.Euler(0, 180 + arrowHeadAngle, 0) * new Vector3(0, 0, 1);
        Vector3 left = Quaternion.LookRotation(direction) * Quaternion.Euler(0, 180 - arrowHeadAngle, 0) * new Vector3(0, 0, 1);
        Debug.DrawRay(position + direction.normalized/2, right * arrowHeadLength, color);
        Debug.DrawRay(position + direction.normalized/2, left * arrowHeadLength, color);
        Debug.DrawRay(position, direction.normalized/2, color);
    }
    #endregion


}
