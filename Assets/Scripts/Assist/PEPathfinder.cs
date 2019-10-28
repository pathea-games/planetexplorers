#define ASTARDEBUG
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Pathfinding;
using Pathfinding.RVO;

/** AI for following paths.
 * This AI is the default movement script which comes with the A* Pathfinding Project.
 * It is in no way required by the rest of the system, so feel free to write your own. But I hope this script will make it easier
 * to set up movement for the characters in your game. This script is not written for high performance, so I do not recommend using it for large groups of units.
 * \n
 * \n
 * This script will try to follow a target transform, in regular intervals, the path to that target will be recalculated.
 * It will on FixedUpdate try to move towards the next point in the path.
 * However it will only move in the forward direction, but it will rotate around it's Y-axis
 * to make it reach the target.
 * 
 * \section variables Quick overview of the variables
 * In the inspector in Unity, you will see a bunch of variables. You can view detailed information further down, but here's a quick overview.\n
 * The #repathRate determines how often it will search for new paths, if you have fast moving targets, you might want to set it to a lower value.\n
 * The #target variable is where the AI will try to move, it can be a point on the ground where the player has clicked in an RTS for example.
 * Or it can be the player object in a zombie game.\n
 * The speed is self-explanatory, so is turningSpeed, however #slowdownDistance might require some explanation.
 * It is the approximate distance from the target where the AI will start to slow down. Note that this doesn't only affect the end point of the path
 * but also any intermediate points, so be sure to set #forwardLook and #pickNextWaypointDist to a higher value than this.\n
 * #pickNextWaypointDist is simply determines within what range it will switch to target the next waypoint in the path.\n
 * #forwardLook will try to calculate an interpolated target point on the current segment in the path so that it has a distance of #forwardLook from the AI\n
 * Below is an image illustrating several variables as well as some internal ones, but which are relevant for understanding how it works.
 * Note that the #forwardLook range will not match up exactly with the target point practically, even though that's the goal.
 * \shadowimage{aipath_variables.png}
 * This script has many movement fallbacks.
 * If it finds a NavmeshController, it will use that, otherwise it will look for a character controller, then for a rigidbody and if it hasn't been able to find any
 * it will use Transform.Translate which is guaranteed to always work.
 */
[RequireComponent(typeof(Seeker))]
[RequireComponent(typeof(SimpleSmoothModifier))]
public class PEPathfinder : MonoBehaviour
{
    static float MaxAngle = 30f;

    public Transform master;

	/** Determines how often it will search for new paths. 
	 * If you have fast moving targets or AIs, you might want to set it to a lower value.
	 * The value is in seconds between path requests.
	 */
	public float repathRate = 0.5F;
	
	/** Target to move towards.
	 * The AI will try to follow/move towards this target.
	 * It can be a point on the ground where the player has clicked in an RTS for example, or it can be the player object in a zombie game.
	 */
	public Transform target;

    /** Target position to move towards.
     * The AI will try to follow/move towards this target.
     * It can be a point on the ground where the player has clicked in an RTS for example, or it can be the player object in a zombie game.
     */
    public Vector3 targetPosition;

    /** Enables or disables searching for paths.
     * Setting this to false does not stop any active path requests from being calculated or stop it from continuing to follow the current path.
     * \see #canMove
     */
    public bool canSearch = true;

    /** Determines within what range it will switch to target the next waypoint in the path */
    public float pickNextWaypointDist = 2;

    /** Target point is Interpolated on the current segment in the path so that it has a distance of #forwardLook from the AI.
      * See the detailed description of AIPath for an illustrative image */
    public float forwardLook = 1;

    /** Distance to the end point to consider the end of path to be reached.
     * When this has been reached, the AI will not move anymore until the target changes and OnTargetReached will be called.
     */
    public float endReachedDistance = 0.2F;


    /** Do a closest point on path check when receiving path callback.
     * Usually the AI has moved a bit between requesting the path, and getting it back, and there is usually a small gap between the AI
     * and the closest node.
     * If this option is enabled, it will simulate, when the path callback is received, movement between the closest node and the current
     * AI position. This helps to reduce the moments when the AI just get a new path back, and thinks it ought to move backwards to the start of the new path
     * even though it really should just proceed forward.
     */
    public bool closestOnPathCheck = true;

    protected float minMoveScale = 0.05F;

    /** Cached Seeker component */
    protected Seeker seeker;

    /** Time when the last path request was sent */
    protected float lastRepath = -9999;

    /** Current path which is followed */
    protected Path path;

    /** Cached CharacterController component */
    protected CharacterController controller;

    /** Cached NavmeshController component */
    protected NavmeshController navController;

    protected RVOController rvoController;

    /** Cached Rigidbody component */
    protected Rigidbody rigid;

    /** Current index in the path which is current target */
    protected int currentWaypointIndex = 0;

    /** Holds if the end-of-path is reached
     * \see TargetReached */
    protected bool targetReached = false;

    /** Only when the previous path has been returned should be search for a new path */
    protected bool canSearchAgain = true;

    protected bool isPathError = false;

    protected Vector3 lastFoundWaypointPosition;
    protected float lastFoundWaypointTime = -9999;

    float lastClearTime;
    Pathea.PeEntity entity;
    Vector3 searchPosition;
    int layer;
    //int terLayer;
    //int allLayer;
    bool initSeekerSize;

    /** Returns if the end-of-path has been reached
     * \see targetReached */
    public bool TargetReached{
        get{
            return targetReached;
        }
    }

    /** Holds if the Start function has been run.
     * Used to test if coroutines should be started in OnEnable to prevent calculating paths
     * in the awake stage (or rather before start on frame 0).
     */
    private bool startHasRun = false;

    /** Initializes reference variables.
     * If you override this function you should in most cases call base.Awake () at the start of it.
      * */
    protected virtual void Awake(){
        seeker = GetComponent<Seeker>();
    }

    /** Starts searching for paths.
     * If you override this function you should in most cases call base.Start () at the start of it.
     * \see OnEnable
     * \see RepeatTrySearchPath
     */
    protected virtual void Start(){
        startHasRun = true;
        OnEnable();

        layer = 1 << Pathea.Layer.SceneStatic
                | 1 << Pathea.Layer.Unwalkable
                | 1 << Pathea.Layer.Building
                | 1 << Pathea.Layer.NearTreePhysics;

        //terLayer = 1 << Pathea.Layer.VFVoxelTerrain;

        //allLayer = layer | terLayer;

        entity = GetComponentInParent<Pathea.PeEntity>();
        seeker.startEndModifier.exactStartPoint = StartEndModifier.Exactness.Original;
        seeker.startEndModifier.exactEndPoint   = StartEndModifier.Exactness.Original;
    }

    /** Run at start and when reenabled.
     * Starts RepeatTrySearchPath.
     * 
     * \see Start
     */
    protected virtual void OnEnable(){

        lastRepath = -9999;
        canSearchAgain = true;

        lastFoundWaypointPosition = GetFeetPosition();

        if (startHasRun){
            //Make sure we receive callbacks when paths complete
            seeker.pathCallback += OnPathComplete;

            StartCoroutine(RepeatTrySearchPath());
        }
    }

    public void OnDisable(){
        // Abort calculation of path
        if (seeker != null && !seeker.IsDone()) seeker.GetCurrentPath().Error();

        // Release current path
        if (path != null) path.Release(this);
        path = null;

        //Make sure we receive callbacks when paths complete
        seeker.pathCallback -= OnPathComplete;
    }

    /** Tries to search for a path every #repathRate seconds.
      * \see TrySearchPath
      */
    protected IEnumerator RepeatTrySearchPath(){
        while (AstarPath.active != null){
            float v = TrySearchPath();
            yield return new WaitForSeconds(v);
        }
    }

    bool CanSearch()
    {
        if (target == null && searchPosition == Vector3.zero)
            return false;

        return true;
    }

    /** Tries to search for a path.
     * Will search for a new path if there was a sufficient time since the last repath and both
     * #canSearchAgain and #canSearch are true and there is a target.
     * 
     * \returns The time to wait until calling this function again (based on #repathRate) 
     */
    public float TrySearchPath(){
        if (Time.time - lastRepath >= repathRate && canSearchAgain && canSearch){
            if (CanSearch())
                SearchPath();
            return repathRate;
        }else{
            //StartCoroutine (WaitForRepath ());
            float v = repathRate - (Time.time - lastRepath);
            return v < 0 ? 0 : v;
        }
    }

    /** Requests a path to the target */
    public virtual void SearchPath(){

        if(!initSeekerSize && entity != null && entity.bounds.size != Vector3.zero)
        {
            initSeekerSize = true;
            seeker.curSeekerSize = Mathf.RoundToInt(entity.bounds.size.x);
        }

        targetPosition = searchPosition;

        if (target == null && targetPosition == Vector3.zero) throw new System.InvalidOperationException("Target is null");

        lastRepath = Time.time;
        //This is where we should search to
        Vector3 position = target != null ? target.position : targetPosition;

        canSearchAgain = false;

        //Alternative way of requesting the path
        //ABPath p = ABPath.Construct (GetFeetPosition(),targetPoint,null);
        //seeker.StartPath (p);

        //We should search from the current position
        if(PETools.PEUtil.IsInAstarGrid(position))
            seeker.StartPath(GetFeetPosition(), position);
    }

    public virtual void OnTargetReached(){
        ClearPath();

        //End of path has been reached
        //If you want custom logic for when the AI has reached it's destination
        //add it here
        //You can also create a new script which inherits from this one
        //and override the function in that script
    }

    /** Called when a requested path has finished calculation.
      * A path is first requested by #SearchPath, it is then calculated, probably in the same or the next frame.
      * Finally it is returned to the seeker which forwards it to this function.\n
      */
    public virtual void OnPathComplete(Path _p){
        ABPath p = _p as ABPath;
        if (p == null) throw new System.Exception("This function only handles ABPaths, do not use special path types");

        isPathError = false;

        canSearchAgain = true;

        //Claim the new path
        p.Claim(this);

        // Path couldn't be calculated of some reason.
        // More info in p.errorLog (debug string)
        if (p.error){
			ClearPath();
            isPathError = true;
            p.Release(this);
            return;
        }

        //if (!Physics.Linecast(transform.position+Vector3.up, searchPosition+Vector3.up, allLayer))
        //{
        //    ClearPath();
        //    return;
        //}

        if (CanClearPath(p))
        {
            ClearPath();
            isPathError = true;
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

        if (closestOnPathCheck){
            Vector3 p1 = Time.time - lastFoundWaypointTime < 0.3f ? lastFoundWaypointPosition : p.originalStartPoint;
            Vector3 p2 = GetFeetPosition();
            Vector3 dir = p2 - p1;
            float magn = dir.magnitude;
            dir /= magn;
            int steps = (int)(magn / pickNextWaypointDist);

#if ASTARDEBUG
            Debug.DrawLine(p1, p2, Color.red, 1);
#endif

            for (int i = 0; i <= steps; i++){
                CalculateVelocity(p1);
                p1 += dir;
            }

        }
    }

    public virtual Vector3 GetFeetPosition(){
        if (master != null)
            return master.position;

        Debug.LogError("NO master!!");
        return transform.position;
    }

    /** Point to where the AI is heading.
      * Filled in by #CalculateVelocity */
    protected Vector3 targetPoint;
    /** Relative direction to where the AI is heading.
     * Filled in by #CalculateVelocity */
    protected Vector3 targetDirection;

    bool CanClearPath(Path argPath)
    {
        //if (Time.time - lastClearTime < 5f)
        //    return false;

		if(argPath.vectorPath.Count == 0 || Vector3.Distance(GetFeetPosition(), argPath.vectorPath[0]) > 2.0f)
			return true;

        if (entity != null && PETools.PEUtil.IsInAstarGrid(searchPosition))
        {
            if (entity.movement != Vector3.zero)
            {
                new Ray(entity.centerPos, entity.movement);
                Vector3 point1 = entity.position;
                Vector3 point2 = entity.position + entity.bounds.size.y * Vector3.up;
                float radius = entity.bounds.extents.x + 0.5f;
                float distance = entity.bounds.extents.z + 1.0f;
                if (Physics.CapsuleCast(point1, point2, radius, entity.movement, distance, layer))
                    return false;
            }

            Vector3 dir = searchPosition - entity.position;
            Vector3 dir1 = Vector3.ProjectOnPlane(dir, Vector3.up);
            for (int i = 1; i < argPath.vectorPath.Count; i++)
            {
                Vector3 dir2 = Vector3.ProjectOnPlane(argPath.vectorPath[i] - entity.position, Vector3.up);
                if (Vector3.Angle(dir1, dir2) > MaxAngle)
                    return false;
            }
        }

        //lastClearTime = Time.time;
        return true;
    }

    void ClearPath()
    {
        //Replace the old path
        if (path != null)
		{
            //path.Claim(this);
            path.Release(this);
            path = null;
        }

        if(seeker != null)
        {
            seeker.lastCompletedVectorPath = null;
        }

        //Reset some variables
        currentWaypointIndex = 0;
        targetReached = false;
    }

    public void SetTargetposition(Vector3 position)
    {
        searchPosition = position;

        if (searchPosition == Vector3.zero && AstarPath.active != null)
        {
            targetPosition = searchPosition;
            ClearPath();
        }
    }

    protected float XZSqrMagnitude(Vector3 a, Vector3 b){
        float dx = b.x - a.x;
        float dz = b.z - a.z;
        return dx * dx + dz * dz;
    }

    /** Calculates desired velocity.
     * Finds the target path segment and returns the forward direction, scaled with speed.
     * A whole bunch of restrictions on the velocity is applied to make sure it doesn't overshoot, does not look too far ahead,
     * and slows down when close to the target.
     * /see speed
     * /see endReachedDistance
     * /see slowdownDistance
     * /see CalculateTargetPoint
     * /see targetPoint
     * /see targetDirection
     * /see currentWaypointIndex
     */
    public Vector3 CalculateVelocity(Vector3 currentPosition){
        if (path == null || isPathError || path.vectorPath == null || path.vectorPath.Count == 0)
            return Vector3.zero;

        List<Vector3> vPath = path.vectorPath;

        if (vPath.Count == 1){
            vPath.Insert(0, currentPosition);
        }

        if (currentWaypointIndex >= vPath.Count) { currentWaypointIndex = vPath.Count - 1; }

        if (currentWaypointIndex <= 1) currentWaypointIndex = 1;

        while (true){
            if (currentWaypointIndex < vPath.Count - 1){
                //There is a "next path segment"
                float dist = XZSqrMagnitude(vPath[currentWaypointIndex], currentPosition);
				if(entity != null && entity.MoveState == Pathea.MovementState.Water)
					dist = PETools.PEUtil.SqrMagnitude(vPath[currentWaypointIndex], currentPosition);

                //Mathfx.DistancePointSegmentStrict (vPath[currentWaypointIndex+1],vPath[currentWaypointIndex+2],currentPosition);
                if (dist < pickNextWaypointDist * pickNextWaypointDist){
                    lastFoundWaypointPosition = currentPosition;
                    lastFoundWaypointTime = Time.time;
                    currentWaypointIndex++;
                }else{
                    break;
                }
            }else{
                break;
            }
        }

        Vector3 dir = vPath[currentWaypointIndex] - vPath[currentWaypointIndex - 1];
        Vector3 targetPosition = CalculateTargetPoint(currentPosition, vPath[currentWaypointIndex - 1], vPath[currentWaypointIndex]);

        dir = targetPosition - currentPosition;
        //dir.y = 0;
        float targetDist = dir.magnitude;

        this.targetDirection = dir;
        this.targetPoint = targetPosition;

        if (currentWaypointIndex == vPath.Count - 1 && targetDist <= endReachedDistance){
            if (!targetReached) { targetReached = true; OnTargetReached(); }

            //Send a move request, this ensures gravity is applied
            return Vector3.zero;
        }

        return this.targetDirection;
    }

	/** Calculates target point from the current line segment.
	 * \param p Current position
	 * \param a Line segment start
	 * \param b Line segment end
	 * The returned point will lie somewhere on the line segment.
	 * \see #forwardLook
	 * \todo This function uses .magnitude quite a lot, can it be optimized?
	 */
    protected Vector3 CalculateTargetPoint(Vector3 p, Vector3 a, Vector3 b){
		if (entity == null || entity.MoveState != Pathea.MovementState.Water) {
			a.y = p.y;
			b.y = p.y;
		}

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
}