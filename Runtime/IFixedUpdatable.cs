namespace Gilzoide.UpdateManager
{
    /// <summary>
    /// Implement this interface and register object with <see cref="UpdateManager.Register"/> to get updated every physics frame on FixedUpdate message.
    /// </summary>
    public interface IFixedUpdatable : IManagedObject
    {
        /// <summary>
        /// Method called every physics frame for objects registered in <see cref="UpdateManager"/>.
        /// </summary>
        /// <seealso cref="UpdateManager"/>
        void ManagedFixedUpdate();
    }
}