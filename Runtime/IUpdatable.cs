namespace Gilzoide.UpdateManager
{
    /// <summary>
    /// Implement this interface and register object with <see cref="UpdateManager.Register"/> to get updated every frame on Update message.
    /// </summary>
    public interface IUpdatable : IManagedObject
    {
        /// <summary>
        /// Method called every frame for objects registered in <see cref="UpdateManager"/>.
        /// </summary>
        /// <seealso cref="UpdateManager"/>
        void ManagedUpdate();
    }
}
