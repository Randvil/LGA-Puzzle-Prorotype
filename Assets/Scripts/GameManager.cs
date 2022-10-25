using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class GameManager : MonoBehaviour
{
    [Header("Playground Factory")]

    [SerializeField]
    private TextAsset presets;

    [SerializeField]
    private Transform fieldsAndChipsTransform;

    [SerializeField]
    private Transform chipTooltipsTransform;

    [SerializeField]
    private GameObject emptyField;

    [SerializeField]
    private GameObject block;

    [SerializeField]
    private List<GameObject> chips;

    [SerializeField]
    private List<GameObject> chipTooltips;

    [SerializeField]
    private GameObject background;

    private List<string> Playgrounds { get; set; } = new();
    private List<string> PlaygroundNames { get; set; } = new();
    private IGridPositionable[,] Playground { get; set; }
    private IGridPositionable[,] EmptyPlayground { get; set; }
    private List<GameObject> Blocks { get; set; } = new();
    private List<GameObject> ChipsAndFields { get; set; } = new();
    private List<GameObject> ChipTooltips { get; set; } = new();
    private int Moves { get; set; }

    public static GameManager Instance { get; private set; }

    public static UnityEvent<List<string>> LoadPlaygroundsEvent { get; private set; } = new();
    public static UnityEvent<int> WinEvent { get; private set; } = new();

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else
        {
            Debug.LogError($"Too many {GetType().Name}s in scene. Removing it from {gameObject.name}");
            Destroy(this);
        }
    }

    private void Start()
    {
        LoadPlaygrounds();

        Chip.ChangePositionEvent.AddListener(OnChipChangePosition);

        void LoadPlaygrounds()
        {
            Playgrounds = FillLevelList(presets);
            LoadPlaygroundsEvent.Invoke(PlaygroundNames);
        }
    }

    private void Update()
    {
        if (Chip.ActiveChip != null)
        {
            int horizontal = (int)Input.GetAxisRaw("Horizontal");
            int vertical = -(int)Input.GetAxisRaw("Vertical");

            if (horizontal != 0 || vertical != 0)
            {
                Vector2Int newPositionOnGrid = Chip.ActiveChip.PositionOnGrid + new Vector2Int(horizontal, vertical);
                if (newPositionOnGrid.x < 0 || newPositionOnGrid.x > Playground.GetUpperBound(0)) newPositionOnGrid.x -= horizontal;
                if (newPositionOnGrid.y < 0 || newPositionOnGrid.y > Playground.GetUpperBound(1)) newPositionOnGrid.y -= vertical;
                if (Playground[newPositionOnGrid.x, newPositionOnGrid.y] is EmptyField emptyField && Chip.MovementIsAllowed(emptyField)) Chip.ActiveChip.MoveOverGrid(emptyField);
            }
        }
    }

    private List<string> FillLevelList(TextAsset presets)
    {
        List<string> levels = new();

        string[] levelsArray = presets.text.Trim('#').Split('#');
        for (int i = 0; i < levelsArray.Length; i++)
        {
            levelsArray[i] = levelsArray[i].Trim();
            PlaygroundNames.Add(levelsArray[i].Split('\n')[0]);

            levels.Add(levelsArray[i].Substring(PlaygroundNames[i].Length).Trim());
        }

        return levels;
    }
    public void AssemblePlayground(string level)
    {
        string[] rows = level.Split('\n');
        for (int i = 0; i < rows.Length; i++) rows[i] = rows[i].Trim();
        int rowCount = rows.Length;
        string[] fields = rows[0].Split(' ');
        int columnCount = fields.Length;

        EmptyPlayground = new IGridPositionable[columnCount, rowCount];
        Playground = new IGridPositionable[columnCount, rowCount];
        Vector2 topLeft = new(-(columnCount) / 2f, (rowCount) / 2f);

        background.transform.localScale = new Vector3(columnCount + 3, rowCount + 1, background.transform.localScale.z);
        chipTooltipsTransform.position = new Vector3(topLeft.x - 1.5f, 0f, 0f);

        for (int rowNumber = 0; rowNumber < rowCount; rowNumber++)
        {
            if (rowNumber % 2 == 0) ChipTooltips.Add(Instantiate(chipTooltips[rowNumber / 2], new Vector3(chipTooltipsTransform.position.x, topLeft.y - rowNumber, 0f) + fieldsAndChipsTransform.position, Quaternion.identity, chipTooltipsTransform));
            fields = rows[rowNumber].Split(' ');
            columnCount = fields.Length;

            for (int columnNumber = 0; columnNumber < columnCount; columnNumber++)
            {
                IGridPositionable chipOrField;
                if (fields[columnNumber] == "9")
                {
                    Blocks.Add(Instantiate(block, new Vector3(topLeft.x + columnNumber, topLeft.y - rowNumber, 0f) + fieldsAndChipsTransform.position, Quaternion.identity, fieldsAndChipsTransform));
                    EmptyPlayground[columnNumber, rowNumber] = null;
                }
                else
                {
                    GameObject chipOrFieldGO = Instantiate(emptyField, new Vector3(topLeft.x + columnNumber, topLeft.y - rowNumber, 0f) + fieldsAndChipsTransform.position, Quaternion.identity, fieldsAndChipsTransform);
                    ChipsAndFields.Add(chipOrFieldGO);
                    chipOrField = chipOrFieldGO.GetComponent<EmptyField>();
                    chipOrField.PositionOnGrid = new Vector2Int(columnNumber, rowNumber);
                    EmptyPlayground[columnNumber, rowNumber] = chipOrField;

                    for (int chipNumber = 1; chipNumber < chips.Count + 1; chipNumber++)
                    {
                        if (fields[columnNumber] == chipNumber.ToString())
                        {
                            chipOrFieldGO = Instantiate(chips[chipNumber - 1], new Vector3(topLeft.x + columnNumber, topLeft.y - rowNumber, 0f) + fieldsAndChipsTransform.position, Quaternion.identity, fieldsAndChipsTransform);
                            ChipsAndFields.Add(chipOrFieldGO);
                            chipOrField = chipOrFieldGO.GetComponent<Chip>();
                            chipOrField.PositionOnGrid = new Vector2Int(columnNumber, rowNumber);
                        }
                    }
                    Playground[columnNumber, rowNumber] = chipOrField;
                    if (Playground[columnNumber, rowNumber] is EmptyField notOccupiedField) notOccupiedField.Occupied = false;
                }
            }
        }
    }

    public void DisassemblePlayground()
    {
        for (int i = Blocks.Count - 1; i >= 0; i--)
        {
            Destroy(Blocks[i]);
            Blocks.RemoveAt(i);
        }

        for (int i = ChipsAndFields.Count - 1; i >= 0; i--)
        {
            Destroy(ChipsAndFields[i]);
            ChipsAndFields.RemoveAt(i);
        }

        for (int i = ChipTooltips.Count - 1; i >= 0; i--)
        {
            Destroy(ChipTooltips[i]);
            ChipTooltips.RemoveAt(i);
        }

        background.transform.localScale = new Vector3(0f, 0f, background.transform.localScale.z);

        Playground = null;
        EmptyPlayground = null;
    }

    public void SelectLevel(int levelNumber)
    {
        DisassemblePlayground();

        AssemblePlayground(Playgrounds[levelNumber]);

        SetCameraSize();

        StartLevel();

        void SetCameraSize()
        {
            float xSize = 2f + Playground.GetUpperBound(1) / 2f;
            float ySize = Playground.GetUpperBound(0) / 2f;
            Camera.main.orthographicSize = xSize > ySize ? xSize : ySize;
        }

        void StartLevel()
        {
            Moves = 0;
        }
    }

    private void OnChipChangePosition(Vector2Int previousPosition, Vector2Int newPosition)
    {
        Moves += 1;

        Playground[newPosition.x, newPosition.y] = Playground[previousPosition.x, previousPosition.y];
        Playground[previousPosition.x, previousPosition.y] = EmptyPlayground[previousPosition.x, previousPosition.y];
        if (EmptyPlayground[previousPosition.x, previousPosition.y] is EmptyField relievedField) relievedField.Occupied = false;

        if (CheckWinCondition()) WinEvent.Invoke(Moves);

        bool CheckWinCondition()
        {
            int rowCount = Playground.GetUpperBound(1) + 1;
            int columnCount = Playground.GetUpperBound(0) + 1;
            for (int rowNumber = 0; rowNumber < rowCount; rowNumber += 2)
            {
                for (int columnNumber = 0; columnNumber < columnCount; columnNumber++)
                {
                    if (Playground[columnNumber, rowNumber] is not Chip chip || (int)chip.chipType != rowNumber / 2 + 1) return false;
                }
            }

            return true;
        }
    }
}
