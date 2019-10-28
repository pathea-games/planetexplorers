using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Pathfinding;

[RequireComponent(typeof(Seeker))]
public class PESeeker : MonoBehaviour 
{
    #region public field
    public bool canSearch = true;
    public bool closestOnPathCheck = true;
    public float repathRate = 0.5F;
    public float pickNextWaypointDist = 2;
    public float endReachedDistance = 0.2F;
    public float forwardLook = 1;
    #endregion

    #region private filed
    Path path;
    float lastRepath = -9999;
    float lastFoundWaypointTime = -9999;
    int currentWaypointIndex = 0;
    bool targetReached = false;
    bool canSearchAgain = true;
    bool startHasRun = false;
    Vector3 lastFoundWaypointPosition;
    Vector3 targetPosition;

    Seeker seeker;
    #endregion

    #region public interface
    public Vector3 target
    {
        set
        {
            targetPosition = value;
        }
    }

    public Vector3 CalculateVelocity()
    {
        return CalculateVelocity(transform.position);
    }
    #endregion

    #region unity interface
    void Awake()
    {
        seeker = GetComponent<Seeker>();
    }

    void Start()
    {
        startHasRun = true;
        OnEnable();
    }

    void OnEnable()
    {
        lastRepath = -9999;
        canSearchAgain = true;

        lastFoundWaypointPosition = GetFeetPosition();

        if (startHasRun)
        {
            //Make sure we receive callbacks when paths complete
            seeker.pathCallback += OnPathComplete;

            StartCoroutine(RepeatTrySearchPath());
        }
    }

    void OnDisable()
    {
        // Abort calculation of path
        if (seeker != null && !seeker.IsDone()) seeker.GetCurrentPath().Error();

        // Release current path
        if (path != null) path.Release(this);
        path = null;

        //Make sure we receive callbacks when paths complete
        seeker.pathCallback -= OnPathComplete;
    }
    #endregion

    #region private function
    IEnumerator RepeatTrySearchPath()
    {
        while (true)
        {
            float v = TrySearchPath();
            yield return new WaitForSeconds(v);
        }
    }

    float TrySearchPath()
    {
        if (Time.time - lastRepath >= repathRate && canSearchAgain && canSearch && targetPosition != Vector3.zero)
        {
            SearchPath();
            return repathRate;
        }
        else
        {
            //StartCoroutine (WaitForRepath ());
            float v = repathRate - (Time.time - lastRepath);
            return v < 0 ? 0 : v;
        }
    }

    void SearchPath()
    {
        if (targetPosition == Vector3.zero) throw new System.InvalidOperationException("Target is null");

        lastRepath = Time.time;

        canSearchAgain = false;

        //Alternative way of requesting the path
        //ABPath p = ABPath.Construct (GetFeetPosition(),targetPoint,null);
        //seeker.StartPath (p);

        //We should search from the current position
        seeker.StartPath(GetFeetPosition(), targetPosition);
    }

    void OnTargetReached()
    {
        //End of path has been reached
        //If you want custom logic for when the AI has reached it's destination
        //add it here
        //You can also create a new script which inherits from this one
        //and override the function in that script
    }

    void OnPathComplete(Path _p)
    {
        ABPath p = _p as ABPath;
        if (p == null) throw new System.Exception("This function only handles ABPaths, do not use special path types");

        canSearchAgain = true;

        //Claim the new path
        p.Claim(this);

        // Path couldn't be calculated of some reason.
        // More info in p.errorLog (debug string)
        if (p.error)
        {
            p.Release(this);
            return;
        }

        //Release the previous path
        if (path != null) path.Release(this);

        //Replace the old path
        path = p;

        //Reset some variables
        currentWaypointIndex = 0;
        targetReached = false;

        //The next row can be used to find out if the path could be found or not
        //If it couldn't (error == true), then a message has probably been logged to the console
        //however it can also be got using p.errorLog
        //if (p.error)

        if (closestOnPathCheck)
        {
            Vector3 p1 = Time.time - lastFoundWaypointTime < 0.3f ? lastFoundWaypointPosition : p.originalStartPoint;
            Vector3 p2 = GetFeetPosition();
            Vector3 dir = p2 - p1;
            float magn = dir.magnitude;
            dir /= magn;
            int steps = (int)(magn / pickNextWaypointDist);

#if ASTARDEBUG
			Debug.DrawLine (p1,p2,Color.red,1);
#endif

            for (int i = 0; i <= steps; i++)
            {
                CalculateVelocity(p1);
                p1 += dir;
            }

        }
    }

    Vector3 GetFeetPosition()
    {
        return transform.position;
    }

    float XZSqrMagnitude(Vector3 a, Vector3 b)
    {
        float dx = b.x - a.x;
        float dz = b.z - a.z;
        return dx * dx + dz * dz;
    }

    Vector3 CalculateVelocity(Vector3 currentPosition)
    {
        if (path == null || path.vectorPath == null || path.vectorPath.Count == 0) return Vector3.zero;

        List<Vector3> vPath = path.vectorPath;
        //Vector3 currentPosition = GetFeetPosition();

        if (vPath.Count == 1)
        {
            vPath.Insert(0, currentPosition);
        }

        if (currentWaypointIndex >= vPath.Count) { currentWaypointIndex = vPath.Count - 1; }

        if (currentWaypointIndex <= 1) currentWaypointIndex = 1;

        while (true)
        {
            if (currentWaypointIndex < vPath.Count - 1)
            {
                //There is a "next path segment"
                float dist = XZSqrMagnitude(vPath[currentWaypointIndex], currentPosition);
                //Mathfx.DistancePointSegmentStrict (vPath[currentWaypointIndex+1],vPath[currentWaypointIndex+2],currentPosition);
                if (dist < pickNextWaypointDist * pickNextWaypointDist)
                {
                    lastFoundWaypointPosition = currentPosition;
                    lastFoundWaypointTime = Time.time;
                    currentWaypointIndex++;
                }
                else
                {
                    break;
                }
            }
            else
            {
                break;
            }
        }

        Vector3 dir = vPath[currentWaypointIndex] - vPath[currentWaypointIndex - 1];
        Vector3 targetPosition = CalculateTargetPoint(currentPosition, vPath[currentWaypointIndex - 1], vPath[currentWaypointIndex]);
        //vPath[currentWaypointIndex] + Vector3.ClampMagnitude (dir,forwardLook);



        dir = targetPosition - currentPosition;
        dir.y = 0;
        float targetDist = dir.magnitude;

        if (currentWaypointIndex == vPath.Count - 1 && targetDist <= endReachedDistance)
        {
            if (!targetReached) { targetReached = true; OnTargetReached(); }

            //Send a move request, this ensures gravity is applied
            return Vector3.zero;
        }

        return dir.normalized;
    }

    Vector3 CalculateTargetPoint(Vector3 p, Vector3 a, Vector3 b)
    {
        a.y = p.y;
        b.y = p.y;

        float magn = (a - b).magnitude;
        if (magn == 0) return a;

        float closest = AstarMath.Clamp01(AstarMath.NearestPointFactor(a, b, p));
        Vector3 point = (b - a) * closest + a;
        float distance = (point - p).magnitude;

        float lookAhead = Mathf.Clamp(forwardLook - distance, 0.0F, forwardLook);

        float offset = lookAhead / magn;
        offset = Mathf.Clamp(offset + closest, 0.0F, 1.0F);
        return (b - a) * offset + a;
    }
    #endregion
}
