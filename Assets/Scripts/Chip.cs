using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

public class Chip : MonoBehaviour, IGridPositionable, IBeginDragHandler, IDragHandler, IEndDragHandler, IPointerClickHandler
{
    public enum eChipType
    {
        First =     1,
        Second =    2,
        Third =     3,
        Fourth =    4,
        Fifth =     5,
        Sixth =     6,
        Seventh =   7,
        Eighth =    8
    }

    public eChipType chipType;

    public Vector2Int PositionOnGrid { get; set; }
    public bool IsActive { get; private set; }

    public static int MovementSpeed { get; set; }
    public static Chip ActiveChip { get; private set; }

    public SpriteRenderer SpriteRenderer { get; private set; }
    private Color InitialColor { get; set; }
    private Color ActiveColor { get; set; }
    private Coroutine ChangePositionCoroutine { get; set; }

    public static UnityEvent<Vector2Int, Vector2Int> ChangePositionEvent { get; private set; } = new();


    private void Start()
    {
        SpriteRenderer = GetComponent<SpriteRenderer>();
        InitialColor = SpriteRenderer.color;
        ActiveColor = new(SpriteRenderer.color.r, SpriteRenderer.color.g, SpriteRenderer.color.b, 1f);

        GameManager.WinEvent.AddListener(OnWin);
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        Activate();
    }

    public void OnDrag(PointerEventData eventData)
    {
        RaycastResult result = eventData.pointerCurrentRaycast;
        EmptyField field = result.gameObject == null ? null : result.gameObject.GetComponent<EmptyField>();
        if (field != null && MovementIsAllowed(field)) MoveOverGrid(field);
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        Deactivate();
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (IsActive) Deactivate();
        else Activate();
    }

    private void Activate()
    {
        if (ActiveChip != null) ActiveChip.Deactivate();
        IsActive = true;
        ActiveChip = this;
        SpriteRenderer.color = ActiveColor;
    }

    private void Deactivate()
    {
        IsActive = false;
        ActiveChip = null;
        SpriteRenderer.color = InitialColor;
    }

    public static bool MovementIsAllowed(EmptyField field)
    {
        return ActiveChip != null && IsNeighbour() && !field.Occupied;

        bool IsNeighbour()
        {
            Vector2Int difference = field.PositionOnGrid - ActiveChip.PositionOnGrid;
            return Mathf.Abs(difference.x) + Mathf.Abs(difference.y) == 1;
        }
    }

    public void MoveOverGrid(EmptyField field)
    {
        field.Occupied = true;
        ChangePositionCoroutine ??= StartCoroutine(ChangePosition(field));
    }

    private IEnumerator ChangePosition(EmptyField field)
    {
        while (Vector2.Distance(transform.position, field.transform.position) > 0.01f)
        {
            Vector2 newPosition = Vector2.MoveTowards(transform.position, field.transform.position, MovementSpeed * Time.deltaTime);
            transform.position = newPosition;
            yield return null;
        }
        transform.position = field.transform.position;
        Vector2Int previousPosition = PositionOnGrid;
        PositionOnGrid = field.PositionOnGrid;

        ChangePositionEvent.Invoke(previousPosition, PositionOnGrid);
        ChangePositionCoroutine = null;
    }

    private static void OnWin(int moves)
    {
        if (ActiveChip != null) ActiveChip.Deactivate();
    }
}
