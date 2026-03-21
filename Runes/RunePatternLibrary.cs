using Godot;
using System.Collections.Generic;

public sealed class RunePatternLibrary
{
    private readonly Dictionary<Runes, ulong> _knownPatterns = [];
    private readonly Dictionary<LineKey, int> _lineBitIndices;

    public RunePatternLibrary(Dictionary<LineKey, int> lineBitIndices)
    {
        _lineBitIndices = new Dictionary<LineKey, int>(lineBitIndices);
        RegisterKnownPatterns();
    }

    public Runes FindMatch(ulong mask)
    {
        foreach (var kvp in _knownPatterns)
        {
            if (kvp.Value == mask)
                return kvp.Key;
        }

        return Runes.None;
    }

    public ulong GetMask(Runes rune)
    {
        return _knownPatterns.TryGetValue(rune, out ulong mask) ? mask : 0;
    }

    private void RegisterKnownPatterns()
    {
        _knownPatterns.Clear();

        _knownPatterns[Runes.Sails] = CreateMaskFromEdges(
            new LineKey(0, 3),
            new LineKey(1, 3),
            new LineKey(2, 3),
            new LineKey(4, 3),
            new LineKey(5, 3),
            new LineKey(6, 3)
        );

        _knownPatterns[Runes.Oars] = CreateMaskFromEdges(
            new LineKey(2, 5),
            new LineKey(2, 0),
            new LineKey(5, 3),
            new LineKey(0, 3),
            new LineKey(3, 1),
            new LineKey(3, 6)
        );

        _knownPatterns[Runes.Fish] = CreateMaskFromEdges(
            new LineKey(0, 1),
            new LineKey(0, 3),
            new LineKey(3, 6),
            new LineKey(5, 6)
        );

        _knownPatterns[Runes.Lights] = CreateMaskFromEdges(
            new LineKey(2, 5),
            new LineKey(2, 3),
            new LineKey(3, 4)
        );

        _knownPatterns[Runes.Arrows] = CreateMaskFromEdges(
            new LineKey(2, 3),
            new LineKey(3, 4),
            new LineKey(3, 5),
            new LineKey(4, 6)
        );

        _knownPatterns[Runes.North] = CreateMaskFromEdges(
            new LineKey(2, 3),
            new LineKey(3, 4),
            new LineKey(3, 6)
        );

        _knownPatterns[Runes.West] = CreateMaskFromEdges(
            new LineKey(1, 2),
            new LineKey(2, 6)
        );

        _knownPatterns[Runes.East] = CreateMaskFromEdges(
            new LineKey(2, 3),
            new LineKey(3, 4)
        );

        _knownPatterns[Runes.South] = CreateMaskFromEdges(
            new LineKey(2, 3)
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