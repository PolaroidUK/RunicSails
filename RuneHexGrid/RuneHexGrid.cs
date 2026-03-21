using Godot;
using System.Collections.Generic;

public partial class RuneHexGrid : Control
{
    [Signal]
    public delegate void PatternCompletedEventHandler(
        ulong patternMask,
        Godot.Collections.Array<Vector2I> linePairs);

    [Signal]
    public delegate void PatternSavedEventHandler(ulong patternMask);

    [Export] public float HexSize = 60f;
    [Export] public int GridRadius = 1;
    [Export] public float DotRadius = 12f;
    [Export] public bool DrawConnections = true;

    private readonly List<HexNode> _nodes = [];
    private readonly Dictionary<Vector2I, HexNode> _nodeMap = [];
    private readonly List<(HexNode A, HexNode B)> _connections = [];
    private readonly Dictionary<LineKey, int> _lineBitIndices = [];
    private readonly List<HexNode> _selectedPath = [];

    private bool _isDragging;

    public ulong CurrentPatternMask { get; private set; }
    public bool HasPattern => CurrentPatternMask != 0;

    public override async void _Ready()
    {
        ClipContents = false;

        await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);
        GenerateHexGrid();
    }

    public override void _Draw()
    {
        if (!DrawConnections)
            return;

        foreach (var connection in _connections)
        {
            Vector2 from = connection.A.GetCenterPosition();
            Vector2 to = connection.B.GetCenterPosition();

            DrawLine(from, to, Colors.Cyan, 8f);
        }

        if (_isDragging && _selectedPath.Count > 0)
        {
            Vector2 last = _selectedPath[^1].GetCenterPosition();
            Vector2 mouse = GetLocalMousePosition();
            DrawLine(last, mouse, new Color(0f, 1f, 1f, 0.35f), 4f);
        }
    }

    public override void _Input(InputEvent @event)
    {
        if (@event is InputEventMouseButton mouseButton &&
            mouseButton.ButtonIndex == MouseButton.Left)
        {
            if (!mouseButton.Pressed)
                EndDrag();
        }

        if (_isDragging && @event is InputEventMouseMotion)
        {
            if (!Input.IsMouseButtonPressed(MouseButton.Left))
            {
                EndDrag();
                return;
            }

            QueueRedraw();
        }
    }

    public override void _Notification(int what)
    {
        if (what == NotificationResized && IsInsideTree())
            GenerateHexGrid();
    }

    public Dictionary<LineKey, int> GetLineBitIndices()
    {
        return new Dictionary<LineKey, int>(_lineBitIndices);
    }

    public void ResetPattern()
    {
        CurrentPatternMask = 0;
        ClearSelection();
    }
}