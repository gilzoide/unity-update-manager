namespace Gilzoide.UpdateManager
{
    /// <summary>
    /// Implement this interface and register object with <see cref="UpdateManager.Register"/> to get updated every frame.
    /// </summary>
    public interface IUpdatable
    {
        /// <summary>
        /// Method called every frame for objects registered in <see cref="UpdateManager"/>.
        /// </summary>
        /// <seealso cref="UpdateManager"/>
        void ManagedUpdate();
    }

    public static class IUpdatableExtensions
    {
        /// <summary>
        /// Shortcut for <c>UpdateManager.Instance.Register(<paramref name="updatable"/>)</c>.
        /// </summary>
        /// <seealso cref="UpdateManager.Instance.Register"/>
        public static void RegisterInManager(this IUpdatable updatable)
        {
            UpdateManager.Instance.Register(updatable);
        }

        /// <summary>
        /// Shortcut for <c>UpdateManager.Instance.Unregister(<paramref name="updatable"/>)</c>.
        /// </summary>
        /// <seealso cref="UpdateManager.Instance.Unregister"/>
        public static void UnregisterInManager(this IUpdatable updatable)
        {
            UpdateManager.Instance.Unregister(updatable);
        }
    }
}
