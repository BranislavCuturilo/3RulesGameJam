using UnityEngine;

/// <summary>
/// Drives an 8-direction, pre-rendered sprite animation for a tower.
///
/// Two clips are supported: <see cref="idle"/> (loops) and <see cref="attack"/>
/// (plays once, then falls back to idle). Each clip holds one Sprite[] per
/// direction.
///
/// Key behaviours requested for 3TD:
///  - The displayed DIRECTION is derived from a *smoothly interpolated* facing
///    angle (<see cref="turnSpeedDegPerSec"/>), so turning e.g. West -> East
///    rotates through the in-between directions instead of snapping.
///  - The animation FRAME index is shared and is NOT reset when the direction
///    changes. So if the attack is on frame 2 and the tower turns, it keeps
///    playing from frame 2 onward in the new direction's sprite set.
///
/// Direction index order (IMPORTANT - assign sprites in this exact order):
///   0 = East        (  0 deg)
///   1 = North-East  ( 45 deg)
///   2 = North       ( 90 deg)
///   3 = North-West  (135 deg)
///   4 = West        (180 deg)
///   5 = South-West  (225 deg)
///   6 = South       (270 deg)
///   7 = South-East  (315 deg)
/// </summary>
[RequireComponent(typeof(SpriteRenderer))]
public class TowerDirectionalAnimator : MonoBehaviour
{
    public const int DirectionCount = 8;

    [System.Serializable]
    public class DirectionFrames
    {
        public Sprite[] frames;
    }

    [Header("Sprite sets - index 0=E,1=NE,2=N,3=NW,4=W,5=SW,6=S,7=SE")]
    [Tooltip("Idle / breathing loop. One Sprite[] per direction.")]
    public DirectionFrames[] idle = new DirectionFrames[DirectionCount];
    [Tooltip("Attack clip (e.g. hammer swing). One Sprite[] per direction. Plays once then returns to idle.")]
    public DirectionFrames[] attack = new DirectionFrames[DirectionCount];

    [Header("Playback")]
    [Tooltip("Frames per second for the looping idle clip.")]
    public float idleFps = 8f;
    [Tooltip("Frames per second for the attack clip. Tune so (attackFrames / attackFps) is shorter than the tower FireRate.")]
    public float attackFps = 14f;

    [Header("Rotation")]
    [Tooltip("How fast the facing angle turns toward the target, in degrees per second. Lower = slower / smoother turn.")]
    public float turnSpeedDegPerSec = 540f;
    [Tooltip("If true the direction snaps instantly to the target (no smooth turn).")]
    public bool instantTurn = false;

    public enum State { Idle, Attack }

    private SpriteRenderer sr;
    private State state = State.Idle;
    private float currentAngle;   // smoothed facing angle, degrees, 0 = East
    private float targetAngle;    // desired facing angle, degrees
    private int frameIndex;       // shared across direction changes (continuity)
    private float frameTimer;
    private int directionIndex;

    public State CurrentState => state;
    public int CurrentDirectionIndex => directionIndex;

    void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
        currentAngle = targetAngle = 0f;
        UpdateDirectionIndex();
        ApplySprite();
    }

    /// <summary>Call every frame while the tower has a target. <paramref name="dir"/> is the world vector to the target.</summary>
    public void FaceDirection(Vector2 dir)
    {
        if (dir.sqrMagnitude < 0.0001f) return;
        targetAngle = Mathf.Repeat(Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg, 360f);
    }

    /// <summary>Start the attack clip from frame 0. Plays once, then returns to the idle loop.</summary>
    public void PlayAttack()
    {
        state = State.Attack;
        frameIndex = 0;
        frameTimer = 0f;
        ApplySprite();
    }

    void Update()
    {
        // 1) Smoothly rotate the facing angle toward the target.
        if (instantTurn)
            currentAngle = targetAngle;
        else
            currentAngle = Mathf.MoveTowardsAngle(currentAngle, targetAngle, turnSpeedDegPerSec * Time.deltaTime);

        UpdateDirectionIndex();

        // 2) Advance the frame on its own timer (independent of direction, so the
        //    frame index survives direction changes -> smooth turn mid-animation).
        float fps = state == State.Attack ? attackFps : idleFps;
        int len = CurrentClipLength();
        if (len > 0 && fps > 0f)
        {
            frameTimer += Time.deltaTime;
            float frameDur = 1f / fps;
            while (frameTimer >= frameDur)
            {
                frameTimer -= frameDur;
                frameIndex++;
                if (frameIndex >= len)
                {
                    if (state == State.Attack)
                    {
                        // Attack finished -> back to looping idle.
                        state = State.Idle;
                        frameIndex = 0;
                        len = CurrentClipLength();
                        if (len <= 0) break;
                    }
                    else
                    {
                        frameIndex = 0; // idle loops
                    }
                }
            }
        }

        // 3) Show the sprite for the current direction + frame.
        ApplySprite();
    }

    private void UpdateDirectionIndex()
    {
        // 45 deg per slice; 0 = East, increasing counter-clockwise.
        directionIndex = Mathf.RoundToInt(currentAngle / 45f) % DirectionCount;
        if (directionIndex < 0) directionIndex += DirectionCount;
    }

    private int CurrentClipLength()
    {
        DirectionFrames[] set = state == State.Attack ? attack : idle;
        if (set == null || directionIndex >= set.Length) return 0;
        DirectionFrames df = set[directionIndex];
        return (df != null && df.frames != null) ? df.frames.Length : 0;
    }

    private void ApplySprite()
    {
        DirectionFrames[] set = state == State.Attack ? attack : idle;
        if (set == null || directionIndex >= set.Length) return;
        DirectionFrames df = set[directionIndex];
        if (df == null || df.frames == null || df.frames.Length == 0) return;
        int idx = Mathf.Clamp(frameIndex, 0, df.frames.Length - 1);
        if (df.frames[idx] != null) sr.sprite = df.frames[idx];
    }
}
