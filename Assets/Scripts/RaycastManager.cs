using System.Collections;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;

public class RaycastManager : MonoBehaviour
{
    public static RaycastManager instance;

    [Header("Camera")]
    [SerializeField] private Camera targetCamera;

    [Header("Layers")]
    [SerializeField] private LayerMask interactiveLayer;
    [SerializeField] private LayerMask clickableLayer;
    [SerializeField] private LayerMask draggableLayer;

    [Header("Drag Option")]
    [SerializeField] private float clickThreshold = 10f;

    [Header("Workspace")]
    [SerializeField] private string workspaceName = "Workspace";

    public GameObject lastHover;
    private GameObject currentHover;
    private GameObject pointerDownTarget;

    private ItemManager currentItem;

    private Vector3 grabOffset;
    private Vector2 pointerDownPos;

    public bool isWorkspace { get; private set; }
    public bool isDragging { get; private set; }
    public bool blockClickThisFrame { get; private set; }

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        if (targetCamera == null)
            targetCamera = Camera.main;
    }

    private void Update()
    {
        if (IsPointerOverUI())
        {
            ClearHover();

            if (Input.GetMouseButtonUp(0))
                ForceReleaseItem();

            return;
        }

        Vector3 mouseWorld = GetMouseWorldPosition();

        UpdateWorkspaceState(mouseWorld);

        GameObject topClickable = GetTopObject(mouseWorld, clickableLayer);
        GameObject topInteractive = GetTopObject(mouseWorld, interactiveLayer);

        HandleHover(topClickable);
        InteractiveHandleHover(topInteractive);

        if (Input.GetMouseButtonDown(0))
        {
            pointerDownPos = Input.mousePosition;
            pointerDownTarget = topClickable;

            if (pointerDownTarget != null)
                Execute(pointerDownTarget, ExecuteEvents.pointerDownHandler);

            TrySelectItem(mouseWorld);
        }

        if (currentItem != null && Input.GetMouseButton(0))
        {
            float distance = Vector2.Distance(pointerDownPos, Input.mousePosition);

            if (distance > clickThreshold)
                isDragging = true;

            if (isDragging)
                FollowMouse(mouseWorld);
        }

        if (Input.GetMouseButtonUp(0))
        {
            if (pointerDownTarget != null)
                Execute(pointerDownTarget, ExecuteEvents.pointerUpHandler);

            ReleaseItem();

            if (!blockClickThisFrame && pointerDownTarget != null)
            {
                GameObject currentTop = GetTopObject(mouseWorld, clickableLayer);

                if (currentTop == pointerDownTarget)
                {
                    Execute(pointerDownTarget, ExecuteEvents.pointerClickHandler);
                }
            }

            TutoHoverCheck(mouseWorld);

            pointerDownTarget = null;
        }
    }

    public void TutoHoverCheck(Vector3 mouseWorld)
    {
        if (!PlayerPrefs.HasKey("TutoClear"))
        {
            Collider2D[] hitsAll = Physics2D.OverlapPointAll(mouseWorld);

            if (isWorkspace)
            {
                bool isTargetIn = hitsAll.Any(hit => hit.gameObject == TutorialManager.Instance.steps[TutorialManager.Instance.currentStepIndex + 1].targetObject);

                if (TutorialManager.Instance.currentStepIndex == 1)
                {
                    if (isTargetIn)
                    {
                        DialogCsvLoader.instance.ShowDialogueSequence("TutorialDialog", "scroll_open_1", TextBoxType.Boss, 2.0f, true, () => TutorialManager.Instance.OnTutorialDialogueEnd("scroll_open"));
                    }
                }
                else if (TutorialManager.Instance.currentStepIndex == 4)
                {
                    if (isTargetIn)
                    {
                        DialogCsvLoader.instance.ShowDialogueSequence("TutorialDialog", "money_open_1", TextBoxType.Boss, 2.0f, true, () => TutorialManager.Instance.OnTutorialDialogueEnd("money_open"));
                    }
                }
            }
            else
            {
                if (TutorialManager.Instance.currentStepIndex == 9)
                {
                    bool isTargetIn = hitsAll.Any(hit => hit.gameObject == TutorialManager.Instance.steps[TutorialManager.Instance.currentStepIndex].targetObject);

                    if (isTargetIn)
                    {
                        TutorialManager.Instance.EndAllTutorial();
                    }
                }
            }
        }
    }

    private void TrySelectItem(Vector3 mouseWorld)
    {
        GameObject topDraggable = GetTopObject(mouseWorld, draggableLayer);

        if (topDraggable == null)
            return;

        ItemManager item = topDraggable.GetComponentInParent<ItemManager>();

        if (item == null)
            return;

        currentItem = item;

        grabOffset = currentItem.transform.position - mouseWorld;

        currentItem.BeginDrag(mouseWorld);

        isDragging = false;
    }

    private void FollowMouse(Vector3 mouseWorld)
    {
        if (currentItem == null)
            return;

        currentItem.Drag(
            mouseWorld + grabOffset,
            isWorkspace
        );

        if (pointerDownTarget != null)
            Execute(pointerDownTarget, ExecuteEvents.dragHandler);
    }

    private void ReleaseItem()
    {
        if (currentItem == null)
        {
            blockClickThisFrame = false;
            return;
        }

        blockClickThisFrame = isDragging;

        if (isDragging)
            currentItem.EndDrag();

        currentItem = null;
        isDragging = false;

        StartCoroutine(ResetClickBlock());
    }

    private void ForceReleaseItem()
    {
        if (currentItem != null)
        {
            if (isDragging)
                currentItem.EndDrag();

            currentItem = null;
        }

        pointerDownTarget = null;
        isDragging = false;
        blockClickThisFrame = false;
    }

    private IEnumerator ResetClickBlock()
    {
        yield return null;
        blockClickThisFrame = false;
    }

    private void HandleHover(GameObject newHover)
    {
        if (currentHover == newHover)
            return;

        if (currentHover != null)
            Execute(currentHover, ExecuteEvents.pointerExitHandler);

        currentHover = newHover;

        if (currentHover != null)
            Execute(currentHover, ExecuteEvents.pointerEnterHandler);
    }

    private void InteractiveHandleHover(GameObject newHover)
    {
        if (lastHover == newHover)
            return;

        // 이전 오브젝트 Exit
        if (lastHover != null)
            Execute(lastHover, ExecuteEvents.pointerExitHandler);

        // 새 오브젝트 Enter
        if (newHover != null)
            Execute(newHover, ExecuteEvents.pointerEnterHandler);

        lastHover = newHover;
    }

    private void ClearHover()
    {
        if (currentHover == null)
            return;

        Execute(currentHover, ExecuteEvents.pointerExitHandler);
        currentHover = null;
    }

    private void UpdateWorkspaceState(Vector3 mouseWorld)
    {
        Collider2D[] hitsAll = Physics2D.OverlapPointAll(mouseWorld);

        isWorkspace = hitsAll.Any(hit =>
            hit.gameObject.name == workspaceName
        );
    }

    private GameObject GetTopObject(Vector3 worldPoint, LayerMask layerMask)
    {
        Collider2D[] hits = Physics2D.OverlapPointAll(worldPoint, layerMask);

        if (hits.Length == 0)
            return null;


        // 화면상 가장 위에 있는.Last() 오브젝트 선택
        return hits
            .OrderByDescending(h => GetSortingOrder(h))
            .ThenByDescending(h => h.transform.position.z)
            .Last()
            .gameObject;
    }

    private int GetSortingOrder(Collider2D col)
    {
        SpriteRenderer sr = col.GetComponent<SpriteRenderer>();

        if (sr != null)
            return sr.sortingOrder;

        Renderer renderer = col.GetComponent<Renderer>();

        if (renderer != null)
            return renderer.sortingOrder;

        return 0;
    }

    private Vector3 GetMouseWorldPosition()
    {
        Vector3 mousePosition = Input.mousePosition;

        mousePosition.z = Mathf.Abs(targetCamera.transform.position.z);

        Vector3 worldPosition = targetCamera.ScreenToWorldPoint(mousePosition);

        worldPosition.z = 0f;

        return worldPosition;
    }

    private bool IsPointerOverUI()
    {
        if (EventSystem.current == null)
            return false;

        return EventSystem.current.IsPointerOverGameObject();
    }

    private void Execute<T>(
        GameObject target,
        ExecuteEvents.EventFunction<T> functor)
        where T : IEventSystemHandler
    {
        if (target == null || EventSystem.current == null)
            return;

        PointerEventData pointerData = new PointerEventData(EventSystem.current)
        {
            position = Input.mousePosition,
            button = PointerEventData.InputButton.Left
        };

        ExecuteEvents.Execute(target, pointerData, functor);
    }
}