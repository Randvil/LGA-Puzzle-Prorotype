using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class EmptyField : MonoBehaviour, IGridPositionable, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    public Vector2Int PositionOnGrid { get; set; }
    public bool Occupied { get; set; } = true;

    private SpriteRenderer SpriteRenderer { get; set; }
    private Sprite InitialSprite { get; set; }


    private void Start()
    {
        SpriteRenderer = GetComponent<SpriteRenderer>();
        InitialSprite = SpriteRenderer.sprite;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (Chip.MovementIsAllowed(this)) SpriteRenderer.sprite = Chip.ActiveChip.SpriteRenderer.sprite;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        SpriteRenderer.sprite = InitialSprite;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (Chip.MovementIsAllowed(this)) Chip.ActiveChip.MoveOverGrid(this);
    }
}
