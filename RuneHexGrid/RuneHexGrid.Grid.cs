using Godot;
using System;
using System.Drawing;
using System.Xml.Linq;

public partial class RuneHexGrid
{
    private void GenerateHexGrid()
    {
        ClearExistingNodes();

        _nodes.Clear();
        _nodeMap.Clear();
        _connections.Clear();
        _selectedPath.Clear();
        _lineBitIndices.Clear();
        _isDragging = false;
        CurrentPatternMask = 0;

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

        BuildAllLineBitIndices();
        QueueRedraw();
    }

    private void ClearExistingNodes()
    {
        foreach (Node child in GetChildren())
            child.QueueFree();
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
}
