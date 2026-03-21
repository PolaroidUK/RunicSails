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

    private readonly List<HexNode> _nodes = new();
    private readonly Dictionary<Vector2I, HexNode> _nodeMap = new();
    private readonly List<(HexNode A, HexNode B)> _connections = new();
    private readonly List<HexNode> _selectedPath = new();

    private bool _isDragging;

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
            SelectNode(node);
            _selectedPath.Add(node);
            QueueRedraw();
            return;
        }

        HexNode lastNode = _selectedPath[^1];

        if (node == lastNode)
            return;

        SelectNode(node);
        _selectedPath.Add(node);

        if (!ConnectionExists(lastNode, node))
            _connections.Add((lastNode, node));

        QueueRedraw();
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

        int[] nodeIds = _selectedPath.Select(x => x.Id).ToArray();

        var linePairs = new Godot.Collections.Array<Vector2I>();
        foreach (var connection in _connections)
        {
            linePairs.Add(new Vector2I(connection.A.Id, connection.B.Id));
        }

        EmitSignal(SignalName.PatternCompleted, nodeIds, linePairs);

        GD.Print("Pattern complete");

        foreach (var pair in linePairs)
        {
            GD.Print($"Line: {pair.X} -> {pair.Y}");
        }
    }

    private Vector2 AxialToPixel(int q, int r, float size)
    {
        float x = size * 1.5f * q;
        float y = size * Mathf.Sqrt(3f) * (r + q / 2f);
        return new Vector2(x, y);
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
}