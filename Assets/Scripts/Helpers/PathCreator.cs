using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;

public class PathCreator : MonoBehaviour
{
    #region Members
    public static Vector3[] m_Path = new Vector3[0];
    public Action<IEnumerable<Vector3>> m_OnNewPathCreated = delegate { };

    LineRenderer m_LineReader;
    private List<Vector3> m_TravelPoints = new List<Vector3>();
    #endregion

    #region Unity events
    void Start()
    {
        m_LineReader = GetComponent<LineRenderer>();
        //lr.SetPosition(0, gameObject.transform.position);
    }
    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            ClearLine();
        }
        if (Input.GetMouseButton(0))
        {
            CreateLine();
        }
        else if(Input.GetMouseButtonUp(0))
        {
            m_OnNewPathCreated(m_TravelPoints);
        }
    }
    #endregion
    #region Helpers
    void ClearLine()
    {
        m_TravelPoints.Clear();
    }
    void CreateLine()
    {
        RaycastHit hit;

        if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hit, 100))
        {
            if (DistanceToLastPoint(hit.point) > 1f)
            {
                m_TravelPoints.Add(hit.point);
                m_LineReader.positionCount = m_TravelPoints.Count;
                m_LineReader.SetPositions(m_TravelPoints.ToArray());
            }
        }
    }
    float DistanceToLastPoint(Vector3 point)
    {
        if(!m_TravelPoints.Any())
        {
            return Mathf.Infinity;
        }
        return Vector3.Distance(m_TravelPoints.Last(), point);
    }
    #endregion
}
