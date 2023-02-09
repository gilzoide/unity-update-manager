using System;

namespace Gilzoide.EasyTransformJob
{
    public interface ITransformJobManager : IDisposable
    {
        void Process();
    }
}
