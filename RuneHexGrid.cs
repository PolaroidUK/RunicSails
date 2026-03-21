using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public partial class RuneHexGrid : Control
{
    [Signal]
    public delegate void PatternCompletedEventHandler(int[] nodeIds, Godot.Collections.Array<Vector2I> linePairs);

    [Export] public float HexSize = 60f;
    [Export] public int GridRadius = 1;
    [Export] public float DotRadius = 12f;
    [Export] public bool DrawConnections = true;

    private readonly List<HexNode> _nodes = [];
    private readonly Dictionary<Vector2I, HexNode> _nodeMap = [];
    private readonly List<(HexNode A, HexNode B)> _connections = [];
    private readonly Dictionary<LineKey, int> _lineBitIndices = [];
    private readonly Dictionary<Runes, ulong> _knownPatterns = [];
    private readonly List<HexNode> _selectedPath = [];

    private bool _isDragging;

    [Export] private GameUI ui;
    public override async void _Ready()
    {
        ClipContents = false;

        await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);
        GenerateHexGrid();
        RegisterKnownPatterns();
    }

    public override void _Draw()
    {
        if (!DrawConnections)
            return;

        foreach (var connection in _connections)
        {
            Vector2 from = connection.A.GetCenterPosition();
            Vector2 to = connection.B.GetCenterPosition();

            DrawLine(from, to, Colors.Cyan, 3f);
        }

        if (_isDragging && _selectedPath.Count > 0)
        {
            Vector2 last = _selectedPath[^1].GetCenterPosition();
            Vector2 mouse = GetLocalMousePosition();
            DrawLine(last, mouse, new Color(0f, 1f, 1f, 0.35f), 2f);
        }
    }

    public override void _Input(InputEvent @event)
    {
        if (@event is InputEventMouseButton mouseButton &&
            mouseButton.ButtonIndex == MouseButton.Left)
        {
            if (!mouseButton.Pressed)
            {
                EndDrag();
            }
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

    private void GenerateHexGrid()
    {
        ClearExistingNodes();

        _nodes.Clear();
        _nodeMap.Clear();
        _connections.Clear();
        _selectedPath.Clear();
        _isDragging = false;

        int id = 0;
        Vector2 centerOffset = Size / 2f;

        for (int q = -GridRadius; q <= GridRadius; q++)
        {
            int rMin = Math.Max(-GridRadius, -q - GridRadius);
            int rMax = Math.Min(GridRadius, -q + GridRadius);

            for (int r = rMin; r <= rMax; r++)
            {
                Vector2 position = AxialToPixel(q, r, HexSize) + centerOffset;

                var node = new HexNode
                {
                    Radius = DotRadius
                };

                AddChild(node);
                node.Initialize(id, q, r, position);

                node.DragStarted += OnNodeDragStarted;
                node.DragEntered += OnNodeDragEntered;

                _nodes.Add(node);
                _nodeMap[new Vector2I(q, r)] = node;

                id++;
            }
        }

        QueueRedraw();
        BuildAllLineBitIndices();
    }

    private void ClearExistingNodes()
    {
        foreach (Node child in GetChildren())
            child.QueueFree();
    }

    private void OnNodeDragStarted(HexNode node)
    {
        ClearSelection();

        _isDragging = true;
        AddNodeToPath(node);
    }

    private void OnNodeDragEntered(HexNode node)
    {
        if (!_isDragging || !Input.IsMouseButtonPressed(MouseButton.Left))
            return;

        AddNodeToPath(node);
    }

    private void AddNodeToPath(HexNode node)
    {
        if (_selectedPath.Count == 0)
        {
            AppendNode(node);
            QueueRedraw();
            return;
        }

        HexNode lastNode = _selectedPath[^1];

        if (node == lastNode)
            return;

        var nodesOnLine = GetNodesBetweenInclusive(lastNode, node);

        // If they are on a valid straight hex line, add all intermediate nodes.
        if (nodesOnLine.Count > 1)
        {
            // skip first because that's lastNode, already selected
            for (int i = 1; i < nodesOnLine.Count; i++)
            {
                AppendNode(nodesOnLine[i]);
            }
        }
        else
        {
            // fallback: just add the node normally
            AppendNode(node);
        }

        QueueRedraw();
    }

    private void AppendNode(HexNode node)
    {
        if (_selectedPath.Count > 0 && _selectedPath[^1] == node)
            return;

        if (_selectedPath.Count > 0)
        {
            HexNode lastNode = _selectedPath[^1];

            if (!ConnectionExists(lastNode, node))
                _connections.Add((lastNode, node));
        }

        SelectNode(node);
        _selectedPath.Add(node);
    }

    private List<HexNode> GetNodesBetweenInclusive(HexNode from, HexNode to)
    {
        var result = new List<HexNode>();

        int dq = to.Q - from.Q;
        int dr = to.R - from.R;
        int ds = (-to.Q - to.R) - (-from.Q - from.R);

        int distance = Math.Max(Math.Abs(dq), Math.Max(Math.Abs(dr), Math.Abs(ds)));

        if (distance == 0)
        {
            result.Add(from);
            return result;
        }

        bool isStraightLine = dq == 0 || dr == 0 || ds == 0;
        if (!isStraightLine)
            return result;

        int stepQ = dq / distance;
        int stepR = dr / distance;

        for (int i = 0; i <= distance; i++)
        {
            int q = from.Q + stepQ * i;
            int r = from.R + stepR * i;

            if (_nodeMap.TryGetValue(new Vector2I(q, r), out var node))
            {
                result.Add(node);
            }
            else
            {
                return [];
            }
        }

        return result;
    }

    private bool ConnectionExists(HexNode a, HexNode b)
    {
        foreach (var connection in _connections)
        {
            bool sameDirection = connection.A == a && connection.B == b;
            bool reverseDirection = connection.A == b && connection.B == a;

            if (sameDirection || reverseDirection)
                return true;
        }

        return false;
    }

    private void SelectNode(HexNode node)
    {
        node.IsSelected = true;
        node.QueueRedraw();
    }

    private void EndDrag()
    {
        if (!_isDragging)
            return;

        _isDragging = false;
        QueueRedraw();

        if (_selectedPath.Count == 0)
            return;

        ulong patternMask = BuildPatternMask();
        Runes matched = FindMatchingPattern(patternMask);
        if (matched != Runes.None)
        {
            
            GD.Print(matched);
            ui.EmitSignal(GameUI.SignalName.RuneMade,(int)matched);
        }
        int[] nodeIds = _selectedPath.Select(x => x.Id).ToArray();

        var linePairs = new Godot.Collections.Array<Vector2I>();
        foreach (var connection in _connections)
        {
            linePairs.Add(new Vector2I(connection.A.Id, connection.B.Id));
        }

        EmitSignal(SignalName.PatternCompleted, nodeIds, linePairs);
    }

    private Vector2 AxialToPixel(int q, int r, float size)
    {
        float x = size * 1.5f * q;
        float y = size * Mathf.Sqrt(3f) * (r + q / 2f);
        return new Vector2(x, y);
    }

    private void BuildAllLineBitIndices()
    {
        _lineBitIndices.Clear();

        int bitIndex = 0;

        for (int i = 0; i < _nodes.Count; i++)
        {
            for (int j = i + 1; j < _nodes.Count; j++)
            {
                var line = new LineKey(_nodes[i].Id, _nodes[j].Id);
                _lineBitIndices[line] = bitIndex;
                bitIndex++;
            }
        }

        GD.Print($"Total possible unique lines: {_lineBitIndices.Count}");
    }

    private ulong BuildPatternMask()
    {
        ulong mask = 0;

        foreach (var connection in _connections)
        {
            var line = new LineKey(connection.A.Id, connection.B.Id);

            if (_lineBitIndices.TryGetValue(line, out int bitIndex))
            {
                mask |= 1UL << bitIndex;
            }
        }

        return mask;
    }

    private Runes FindMatchingPattern(ulong mask)
    {
        foreach (var kvp in _knownPatterns)
        {
            if (kvp.Value == mask)
                return kvp.Key;
        }

        return Runes.None;
    }

    public override void _Notification(int what)
    {
        if (what == NotificationResized && IsInsideTree())
            GenerateHexGrid();
    }

    public void ClearSelection()
    {
        _connections.Clear();
        _selectedPath.Clear();
        _isDragging = false;

        foreach (var node in _nodes)
        {
            node.IsSelected = false;
            node.QueueRedraw();
        }

        QueueRedraw();
    }

    private void RegisterKnownPatterns()
    {
        _knownPatterns[Runes.Sails] = CreateMaskFromEdges(
            new LineKey(0, 3),
            new LineKey(1, 3),
            new LineKey(2, 3),
            new LineKey(4, 3),
            new LineKey(5, 3),
            new LineKey(6, 3)
        );

        _knownPatterns[Runes.Oars] = CreateMaskFromEdges(
            new LineKey(0, 1),
            new LineKey(1, 2),
            new LineKey(1, 4)
        );
    }

    private ulong CreateMaskFromEdges(params LineKey[] lines)
    {
        ulong mask = 0;

        foreach (var line in lines)
        {
            if (_lineBitIndices.TryGetValue(line, out int bitIndex))
            {
                mask |= 1UL << bitIndex;
            }
            else
            {
                GD.PushError($"Unknown line in pattern registration: {line}");
            }
        }

        return mask;
    }
}

public readonly struct LineKey : IEquatable<LineKey>
{
    public int A { get; }
    public int B { get; }

    public LineKey(int a, int b)
    {
        if (a < b)
        {
            A = a;
            B = b;
        }
        else
        {
            A = b;
            B = a;
        }
    }

    public bool Equals(LineKey other) => A == other.A && B == other.B;
    public override bool Equals(object? obj) => obj is LineKey other && Equals(other);
    public override int GetHashCode() => HashCode.Combine(A, B);
    public override string ToString() => $"{A}-{B}";
}