using System;

namespace Gilzoide.EasyTransformJob
{
    public interface IUpdateJobManager : IDisposable
    {
        void Process();
    }
}
