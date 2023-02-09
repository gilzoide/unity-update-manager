using System.Collections.Generic;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;
using UnityEngine.Jobs;

namespace Gilzoide.EasyTransformJob
{
    public class TransformJobManager<TData> : ITransformJobManager
        where TData : struct, ITransformJob
    {
        public static TransformJobManager<TData> Instance => _instance != null ? _instance : (_instance = new TransformJobManager<TData>());
        private static TransformJobManager<TData> _instance;

        private readonly List<ITransformJobProvider<TData>> _dataProviders = new List<ITransformJobProvider<TData>>();
        private bool _isDirty = true;
        private TransformAccessArray _jobTransforms;
        private NativeArray<TData> _jobData;
        private JobHandle _jobHandle;

        public TransformJobManager()
        {
            TransformJobRunner.Instance.RegisterJobManager(this);
        }

        public void Process()
        {
            _jobHandle.Complete();

            if (_isDirty)
            {
                if (_jobData.IsCreated)
                {
                    _jobData.Dispose();
                }
                if (_jobTransforms.isCreated)
                {
                    _jobTransforms.Dispose();
                }

                var transforms = new Transform[_dataProviders.Count];
                for (int i = 0; i < _dataProviders.Count; i++)
                {
                    transforms[i] = _dataProviders[i].Transform;
                }
                _jobTransforms = new TransformAccessArray(transforms);
                _jobData = new NativeArray<TData>(_dataProviders.Count, Allocator.Persistent);
                _isDirty = false;
            }
            
            for (int i = 0; i < _dataProviders.Count; i++)
            {
                _jobData[i] = _dataProviders[i].Data;
            }

            _jobHandle = new TransformJob<TData>
            {
                Data = _jobData,
                DeltaTime = Time.deltaTime,
            }.Schedule(_jobTransforms);
        }

        public void Dispose()
        {
            _jobHandle.Complete();

            if (_jobData.IsCreated)
            {
                _jobData.Dispose();
            }

            if (_jobTransforms.isCreated)
            {
                _jobTransforms.Dispose();
            }
        }

        public void AddProvider(ITransformJobProvider<TData> provider)
        {
            if (!_dataProviders.Contains(provider))
            {
                _dataProviders.Add(provider);
                _isDirty = true;
            }
        }

        public void RemoveProvider(ITransformJobProvider<TData> provider)
        {
            bool removed = _dataProviders.Remove(provider);
            _isDirty = _isDirty || removed;
        }
    }
}
