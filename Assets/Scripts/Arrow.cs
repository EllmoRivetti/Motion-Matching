using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Arrow : MonoBehaviour
{

    [SerializeField] GameObject Hips, RFeet, LFeet, Ground;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 HipsPosition = Hips.transform.position;
        Vector3 HipsDirection = Hips.transform.forward;
        Vector3 RFeetPosition = RFeet.transform.position;
        Vector3 RFeetDirection = RFeet.transform.forward;
        Vector3 LFeetPosition = LFeet.transform.position;
        Vector3 LFeetDirection = LFeet.transform.forward;

        Vector2 newHipsPosition = new Vector2(HipsPosition.x, HipsPosition.z);
        Vector2 newHipsDirection = new Vector2(HipsDirection.x, HipsDirection.z);

        drawArrow(HipsPosition, HipsDirection, Color.red);
        drawArrow(newHipsPosition, newHipsDirection, Color.yellow);
        drawArrow(RFeetPosition, RFeetDirection, Color.blue);
        drawArrow(LFeetPosition, LFeetDirection, Color.green);
    }

    void drawArrow(Vector3 position,Vector3 direction, Color color)
    {
        direction.y = 0;
        position.y = Ground.transform.position.y+0.1f;
        float arrowHeadLength = 0.25f, arrowHeadAngle = 20.0f;
        Vector3 right = Quaternion.LookRotation(direction) * Quaternion.Euler(0, 180 + arrowHeadAngle, 0) * new Vector3(0, 0, 1);
        Vector3 left = Quaternion.LookRotation(direction) * Quaternion.Euler(0, 180 - arrowHeadAngle, 0) * new Vector3(0, 0, 1);
        Debug.DrawRay(position + direction, right * arrowHeadLength, color);
        Debug.DrawRay(position + direction, left * arrowHeadLength, color);
        Debug.DrawRay(position, direction, color);
    }

    void drawArrow(Vector2 position, Vector2 direction, Color color)
    {
        float arrowHeadLength = 0.25f, arrowHeadAngle = 20.0f;
        Vector3 right = Quaternion.LookRotation(direction) * Quaternion.Euler(0, 180 + arrowHeadAngle, 0) * new Vector3(0, 0, 1);
        Vector3 left = Quaternion.LookRotation(direction) * Quaternion.Euler(0, 180 - arrowHeadAngle, 0) * new Vector3(0, 0, 1);
        Debug.DrawRay(position + direction, right * arrowHeadLength, color);
        Debug.DrawRay(position + direction, left * arrowHeadLength, color);
        Debug.DrawRay(position, direction, color);
    }
}
