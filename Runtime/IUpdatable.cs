namespace Gilzoide.UpdateManager
{
    public interface IUpdatable
    {
        void ManagedUpdate();
    }

    public static class IUpdatableExtensions
    {
        public static void RegisterInManager(this IUpdatable updatable)
        {
            UpdateManager.Instance.Register(updatable);
        }

        public static void UnregisterInManager(this IUpdatable updatable)
        {
            UpdateManager.Instance.Unregister(updatable);
        }
    }
}
