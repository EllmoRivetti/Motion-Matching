using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEngine.AI;
using System;

public class PathCreator : MonoBehaviour
{
    public static Vector3[] path = new Vector3[0];

    LineRenderer lr;

    private List<Vector3> travel_points = new List<Vector3>();
    public Action<IEnumerable<Vector3>> OnNewPathCreated = delegate { };

    void Start()
    {
        lr = GetComponent<LineRenderer>();

        //lr.SetPosition(0, gameObject.transform.position);
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            travel_points.Clear();
        }

        if(Input.GetMouseButton(0))
        {
            RaycastHit hit;

            if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hit, 100))
            {
                if(DistanceToLastPoint(hit.point) > 1f)
                {
                    travel_points.Add(hit.point);
                    lr.positionCount = travel_points.Count;
                    lr.SetPositions(travel_points.ToArray());
                }
            }
        }
        else if(Input.GetMouseButtonUp(0))
        {
            OnNewPathCreated(travel_points);
        }
    }

    float DistanceToLastPoint(Vector3 point)
    {
        if(!travel_points.Any())
        {
            return Mathf.Infinity;
        }
        return Vector3.Distance(travel_points.Last(), point);
    }
}
