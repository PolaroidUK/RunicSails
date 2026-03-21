using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public partial class RuneHexGrid
{
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

        if (nodesOnLine.Count > 1)
        {
            for (int i = 1; i < nodesOnLine.Count; i++)
                AppendNode(nodesOnLine[i]);
        }
        else
        {
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

        CurrentPatternMask = BuildPatternMask();

        int[] nodeIds = _selectedPath.Select(x => x.Id).ToArray();

        var linePairs = new Godot.Collections.Array<Vector2I>();
        foreach (var connection in _connections)
        {
            linePairs.Add(new Vector2I(connection.A.Id, connection.B.Id));
        }

        EmitSignal(SignalName.PatternSaved, CurrentPatternMask);
        EmitSignal(SignalName.PatternCompleted, CurrentPatternMask, nodeIds, linePairs);
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