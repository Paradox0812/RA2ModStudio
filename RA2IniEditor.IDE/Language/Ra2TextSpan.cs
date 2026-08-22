namespace RA2IniEditor.IDE.Language;

internal readonly struct Ra2TextSpan
{
    public Ra2TextSpan(int start, int length)
    {
        if (start < 0)
            throw new ArgumentOutOfRangeException(nameof(start));

        if (length < 0)
            throw new ArgumentOutOfRangeException(nameof(length));

        Start = start;
        Length = length;
    }

    public int Start { get; }

    public int Length { get; }

    public int End => Start + Length;

    public bool Contains(int offset)
        => Length == 0 ? offset == Start : offset >= Start && offset < End;
}
