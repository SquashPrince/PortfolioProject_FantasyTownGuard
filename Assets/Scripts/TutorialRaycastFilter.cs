using System.Collections;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Image))]
public class TutorialRaycastFilter : MonoBehaviour, ICanvasRaycastFilter
{
    [SerializeField] private TutorialManager TutoMng;

    [Header("World Target")]
    [SerializeField] private SpriteRenderer worldTarget;
    [SerializeField] private Camera worldCamera;

    [Header("Hole Option")]
    [SerializeField] private bool useSpriteSize = true;
    [SerializeField] private Vector2 padding = new Vector2(30f, 30f);
    [SerializeField] private Vector2 manualHoleSize = new Vector2(200f, 150f);

    [Header("Animation")]
    [SerializeField] private float startSizeMultiplier = 8f;
    [SerializeField] private float animationDuration = 0.5f;
    [SerializeField] private AnimationCurve shrinkCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

    private Image image;
    private Material mat;
    private RectTransform overlayRect;

    private float currentSizeMultiplier = 1f;
    public bool isDialogEnd = false;
    private bool isAnimating;

    private Coroutine animCoroutine;

    private readonly int HoleCenterID = Shader.PropertyToID("_HoleCenter");
    private readonly int HoleSizeID = Shader.PropertyToID("_HoleSize");

    private void Awake()
    {
        image = GetComponent<Image>();
        overlayRect = GetComponent<RectTransform>();
        mat = image.material;

        if (worldCamera == null)
            worldCamera = Camera.main;
    }

    private void LateUpdate()
    {
        EndActionCheckRay();

        UpdateHoleShader();
    }

    private void EndActionCheckRay()
    {
        if (!isDialogEnd)
            return;

        if (isAnimating)
            return;

        if (worldTarget == null)
            return;

        if (!Input.GetMouseButtonDown(0))
            return;

        Vector3 mouseWorld = worldCamera.ScreenToWorldPoint(Input.mousePosition);
        mouseWorld.z = 0f;

        Collider2D hit = Physics2D.OverlapPoint(mouseWorld);

        if (hit == null)
            return;

        SpriteRenderer hitSprite = hit.GetComponentInParent<SpriteRenderer>();

        if (hitSprite == worldTarget)
        {
            gameObject.SetActive(false);
        }
    }

    public void SetWorldTarget(SpriteRenderer target)
    {
        worldTarget = target;

        if (animCoroutine != null)
            StopCoroutine(animCoroutine);

        animCoroutine = StartCoroutine(ShrinkHoleRoutine());
    }

    public bool IsRaycastLocationValid(Vector2 screenPoint, Camera eventCamera)
    {
        // 줄어드는 중에는 검은 오버레이 전체가 클릭 막음
        if (!isDialogEnd)
            return true;

        if (isAnimating)
            return true;

        if (worldTarget == null)
            return true;

        bool insideHole = IsInsideWorldTarget(screenPoint);

        // true  = 오버레이가 클릭 막음
        // false = 뒤로 클릭 통과
        return !insideHole;
    }

    private IEnumerator ShrinkHoleRoutine()
    {
        isAnimating = true;
        currentSizeMultiplier = startSizeMultiplier;

        float time = 0f;

        while (time < animationDuration)
        {
            time += Time.deltaTime;

            float t = Mathf.Clamp01(time / animationDuration);
            float curveT = shrinkCurve.Evaluate(t);

            currentSizeMultiplier = Mathf.Lerp(
                startSizeMultiplier,
                1f,
                curveT
            );

            UpdateHoleShader();

            yield return null;
        }

        currentSizeMultiplier = 1f;
        UpdateHoleShader();

        isAnimating = false;
    }

    private bool IsInsideWorldTarget(Vector2 screenPoint)
    {
        Rect screenRect = GetFinalWorldTargetScreenRect();
        return screenRect.Contains(screenPoint);
    }

    private void UpdateHoleShader()
    {
        if (worldTarget == null)
            return;

        Rect screenRect = GetAnimatedWorldTargetScreenRect();

        Vector2 centerScreen = screenRect.center;
        Vector2 sizeScreen = screenRect.size;

        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            overlayRect,
            centerScreen,
            null,
            out Vector2 localCenter
        );

        Vector2 normalizedCenter = new Vector2(
            localCenter.x / overlayRect.rect.width + 0.5f,
            localCenter.y / overlayRect.rect.height + 0.5f
        );

        Vector2 normalizedSize = new Vector2(
            sizeScreen.x / Screen.width,
            sizeScreen.y / Screen.height
        );

        mat.SetVector(HoleCenterID, normalizedCenter);
        mat.SetVector(HoleSizeID, normalizedSize);
    }

    private Rect GetAnimatedWorldTargetScreenRect()
    {
        Rect rect = GetFinalWorldTargetScreenRect();

        Vector2 center = rect.center;
        Vector2 animatedSize = rect.size * currentSizeMultiplier;

        return new Rect(
            center - animatedSize * 0.5f,
            animatedSize
        );
    }

    private Rect GetFinalWorldTargetScreenRect()
    {
        Rect rect;

        if (useSpriteSize)
        {
            rect = GetWorldTargetScreenRect();

            rect.xMin -= padding.x;
            rect.xMax += padding.x;
            rect.yMin -= padding.y;
            rect.yMax += padding.y;
        }
        else
        {
            Vector3 center = worldCamera.WorldToScreenPoint(worldTarget.transform.position);

            rect = new Rect(
                center.x - manualHoleSize.x * 0.5f,
                center.y - manualHoleSize.y * 0.5f,
                manualHoleSize.x,
                manualHoleSize.y
            );
        }

        return rect;
    }

    private Rect GetWorldTargetScreenRect()
    {
        Bounds bounds = worldTarget.bounds;

        Vector3 min = worldCamera.WorldToScreenPoint(bounds.min);
        Vector3 max = worldCamera.WorldToScreenPoint(bounds.max);

        float xMin = Mathf.Min(min.x, max.x);
        float xMax = Mathf.Max(min.x, max.x);
        float yMin = Mathf.Min(min.y, max.y);
        float yMax = Mathf.Max(min.y, max.y);

        return Rect.MinMaxRect(xMin, yMin, xMax, yMax);
    }
}