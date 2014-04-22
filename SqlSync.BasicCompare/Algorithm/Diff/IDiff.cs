namespace Algorithm.Diff
{
    using System.Collections;

    public interface IDiff : IEnumerable
    {
        IList Left { get; }

        IList Right { get; }
    }
}
