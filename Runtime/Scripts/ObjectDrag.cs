using System.Collections;
using UnityEngine;

public class ObjectDrag : MonoBehaviour
{
    private Vector3 originalPosition;
    private Vector3 offset;
    private bool isDragging = false;
    private bool isPlaced = false;
    private bool canDrag = true;

    private bool isSnappingBack = false;
    private bool isInsideTriggerArea = false;

    private ObjectSpawner _spawner;
    private SpriteRenderer spriteRenderer;
    private float snapThreshold = 0.1f;
    private float snapSpeed = 3;

    [Header("TargetPos / Tag Checking ")]
    public Transform targetPosition;
    public string correctCustomTag = "";

    [Header("Animation")]
    public float animationDelay;

    [Header("Size")]
    public float SpawnSize;
    public float DraggedSize;

    [Header("SortingOrder")]
    public int startSortingOrder;
    public int draggedSortingOrder;
    public int placedSortingOrder;

    private void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        transform.localScale = new Vector3(SpawnSize, SpawnSize, SpawnSize);

        

        if (spriteRenderer != null)
        {
            spriteRenderer.sortingOrder = startSortingOrder;
        }
    }

    private void Update()
    {
        if (isPlaced || isSnappingBack) return;

        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);
            Vector3 touchPosition = Camera.main.ScreenToWorldPoint(new Vector3(touch.position.x, touch.position.y, Camera.main.nearClipPlane));

            switch (touch.phase)
            {
                case TouchPhase.Began:
                    if (IsTouchingObject(touchPosition))
                    {
                        StartDragging(touchPosition);
                    }
                    break;

                case TouchPhase.Moved:
                    if (isDragging)
                    {
                        DragObject(touchPosition);
                    }
                    break;

                case TouchPhase.Ended:
                    if (isDragging)
                    {
                        StopDragging();
                    }
                    break;
            }
        }
        else if (Input.GetMouseButtonDown(0))
        {
            Vector3 mousePosition = Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, Camera.main.nearClipPlane));
            if (IsTouchingObject(mousePosition))
            {
                StartDragging(mousePosition);
            }
        }
        else if (Input.GetMouseButton(0) && isDragging)
        {
            Vector3 mousePosition = Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, Camera.main.nearClipPlane));
            DragObject(mousePosition);
        }
        else if (Input.GetMouseButtonUp(0) && isDragging)
        {
            StopDragging();
        }
    }

    private bool IsTouchingObject(Vector3 position)
    {
        RaycastHit2D hit = Physics2D.Raycast(position, Vector2.zero);

        return hit.collider != null && hit.collider.gameObject == gameObject;
    }

    private void DragObject(Vector3 position)
    {
        transform.position = new Vector3(position.x + offset.x, position.y + offset.y, transform.position.z);
        transform.localScale = new Vector3(DraggedSize, DraggedSize, DraggedSize);

        if (spriteRenderer != null)
        {
            spriteRenderer.sortingOrder = draggedSortingOrder;
        }
    }

    private void StopDragging()
    {
        transform.localScale = new Vector3(SpawnSize, SpawnSize, SpawnSize);
        isDragging = false;
        CheckDropPosition();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        TagValue tagValue = other.GetComponent<TagValue>();
        if (tagValue != null && tagValue.Tag == correctCustomTag)
        {
            isInsideTriggerArea = true;
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        TagValue tagValue = other.GetComponent<TagValue>();
        if (tagValue != null && tagValue.Tag == correctCustomTag)
        {
            isInsideTriggerArea = false;
        }
    }

    private void CheckDropPosition()
    {
        if (targetPosition != null && isInsideTriggerArea)
        {
            transform.localScale = new Vector3(DraggedSize, DraggedSize, DraggedSize);
            StartCoroutine(SmoothSnapToTargetPosition(targetPosition.position));
            if (spriteRenderer != null)
            {
                spriteRenderer.sortingOrder = placedSortingOrder;
            }
        }
        else
        {
            StartCoroutine(WrongMoveAnimation());
            isDragging = false;
            if (spriteRenderer != null)
            {
                spriteRenderer.sortingOrder = startSortingOrder;
            }
        }
    }

    private IEnumerator WrongMoveAnimation()
    {
        isDragging = false;
        canDrag = false;

        transform.localScale = new Vector3(DraggedSize, DraggedSize, DraggedSize);
        Quaternion startRotation = transform.rotation;
        Quaternion targetRotation = Quaternion.Euler(transform.eulerAngles.x, transform.eulerAngles.y, 3);
        float time = 0;
        float duration = 0.2f;

        while (time < 1)
        {
            time += Time.deltaTime / duration;
            transform.rotation = Quaternion.Lerp(startRotation, targetRotation, time);
            yield return null;
        }

        startRotation = transform.rotation;
        targetRotation = Quaternion.Euler(transform.eulerAngles.x, transform.eulerAngles.y, -3);
        time = 0;

        while (time < 1)
        {
            time += Time.deltaTime / duration;
            transform.rotation = Quaternion.Lerp(startRotation, targetRotation, time);
            yield return null;
        }

        startRotation = transform.rotation;
        targetRotation = Quaternion.Euler(transform.eulerAngles.x, transform.eulerAngles.y, 0);
        time = 0;

        while (time < 1)
        {
            time += Time.deltaTime / duration;
            transform.rotation = Quaternion.Lerp(startRotation, targetRotation, time);
            yield return null;
        }

        StartCoroutine(SmoothSnapToTargetPosition(originalPosition));
    }

    private void StartDragging(Vector3 position)
    {
        if (!isDragging && canDrag)
        {
            StopAllCoroutines();
            originalPosition = transform.position;
            offset = transform.position - position;
            isDragging = true;
            isPlaced = false;
        }
    }

    private IEnumerator SmoothSnapToTargetPosition(Vector3 targetPos)
    {
        isSnappingBack = true;

        float time = 0;
        Vector3 startPos = transform.position;

        while (time < 1)
        {
            transform.position = Vector3.Lerp(startPos, targetPos, time);
            time += Time.deltaTime * snapSpeed;
            yield return null;
        }

        transform.position = targetPos;

        if (targetPos == originalPosition)
        {
            transform.localScale = new Vector3(SpawnSize, SpawnSize, SpawnSize);

            if (spriteRenderer != null)
            {
                spriteRenderer.sortingOrder = startSortingOrder;
            }
        }
        else
        {
            isPlaced = true;
            ObjectSpawner.Instance?.ObjectPlaced();
            transform.localScale = new Vector3(DraggedSize, DraggedSize, DraggedSize);
            yield return new WaitForSeconds(0.3f);

            if (spriteRenderer != null)
            {
                spriteRenderer.sortingOrder = placedSortingOrder;
            }
        }

        isSnappingBack = false;
        canDrag = true;
    }
}
