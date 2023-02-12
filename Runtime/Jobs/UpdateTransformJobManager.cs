using Gilzoide.UpdateManager.Jobs.Internal;
using Unity.Jobs;
using UnityEngine;

namespace Gilzoide.UpdateManager.Jobs
{
    public class UpdateTransformJobManager<TData> : IUpdatable
        where TData : struct, IUpdateTransformJob
    {
        public static UpdateTransformJobManager<TData> Instance => _instance != null ? _instance : (_instance = new UpdateTransformJobManager<TData>());
        private static UpdateTransformJobManager<TData> _instance;

        private readonly UpdateTransformJobProvider<TData> _jobProvider = new UpdateTransformJobProvider<TData>();
        private JobHandle _jobHandle;

        static UpdateTransformJobManager()
        {
            Application.quitting += () => _instance?.Dispose();
        }

        ~UpdateTransformJobManager()
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

            _jobHandle = _jobProvider.ScheduleJob();
        }

        public void Dispose()
        {
            _jobHandle.Complete();

            _jobProvider.Dispose();

            UpdateJobTime.Instance.UnregisterUpdate();
            UpdateManager.Instance.Unregister(this);
        }

        public void Register(ITransformJobUpdatable<TData> provider)
        {
            _jobProvider.Add(provider, out bool shouldStartUpdating);
            if (shouldStartUpdating)
            {
                UpdateJobTime.Instance.RegisterUpdate();
                UpdateManager.Instance.Register(this);
            }
        }

        public void Unregister(ITransformJobUpdatable<TData> provider)
        {
            _jobProvider.Remove(provider);
        }

        public TData GetData(ITransformJobUpdatable<TData> provider)
        {
            return _jobProvider.GetData(provider);
        }
    }
}
