using Gilzoide.UpdateManager.Jobs.Internal;
using Unity.Jobs;

namespace Gilzoide.UpdateManager.Jobs
{
    public class UpdateJobManager<TData> : IUpdatable
        where TData : struct, IUpdateJob
    {
        public static UpdateJobManager<TData> Instance => _instance != null ? _instance : (_instance = new UpdateJobManager<TData>());
        private static UpdateJobManager<TData> _instance;

        private readonly UpdateJobProviderCollection<TData> _jobProvider = new UpdateJobProviderCollection<TData>();
        private JobHandle _jobHandle;

        ~UpdateJobManager()
        {
            Dispose();
        }

        public void ManagedUpdate()
        {
            _jobHandle.Complete();

            _jobProvider.Refresh();

            if (_jobProvider.Count == 0)
            {
                Dispose();
                return;
            }

            _jobHandle = _jobProvider.ScheduleJob(64);
        }

        public void Dispose()
        {
            _jobHandle.Complete();

            _jobProvider.Dispose();

            UpdateJobTime.Instance.UnregisterUpdate();
            UpdateManager.Instance.Unregister(this);
        }

        public void Register(IJobUpdatable<TData> provider)
        {
            _jobProvider.Add(provider, out bool shouldStartUpdating);
            if (shouldStartUpdating)
            {
                UpdateJobTime.Instance.RegisterUpdate();
                UpdateManager.Instance.Register(this);
            }
        }

        public void Unregister(IJobUpdatable<TData> provider)
        {
            _jobProvider.Remove(provider);
        }

        public TData GetData(IJobUpdatable<TData> provider)
        {
            return _jobProvider.GetData(provider);
        }
    }
}
