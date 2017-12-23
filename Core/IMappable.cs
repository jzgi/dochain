namespace Greatbone.Core
{
    /// <summary>
    /// An object with an key name so that can be a map element.
    /// </summary>
    public interface IMappable<out K>
    {
        K Key { get; }
    }
}