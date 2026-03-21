public partial class RuneHexGrid
{
    private ulong BuildPatternMask()
    {
        ulong mask = 0;

        foreach (var connection in _connections)
        {
            var line = new LineKey(connection.A.Id, connection.B.Id);

            if (_lineBitIndices.TryGetValue(line, out int bitIndex))
                mask |= 1UL << bitIndex;
        }

        return mask;
    }
}