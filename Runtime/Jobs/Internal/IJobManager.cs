using System;
using Unity.Jobs;

namespace Gilzoide.UpdateManager
{
    public interface IJobManager
    {
        event Action<JobHandle> OnJobScheduled;
    }
}
