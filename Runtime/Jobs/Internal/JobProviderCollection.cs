using System.Collections.Generic;

namespace Gilzoide.UpdateManager.Jobs.Internal
{
    public class JobProviderCollection<TProvider>
    {
        protected readonly Dictionary<TProvider, int> _providerIndexMap = new Dictionary<TProvider, int>();
        protected readonly List<TProvider> _dataProviders = new List<TProvider>();
        protected readonly List<TProvider> _dataProvidersToAdd = new List<TProvider>();
        protected readonly List<int> _dataProvidersToRemove = new List<int>();
        protected bool _isDirty = true;

        public int Count => _dataProviders.Count;

        public void Add(TProvider provider, out bool shouldStartUpdating)
        {
            shouldStartUpdating = _dataProviders.Count == 0 && _dataProvidersToAdd.Count == 0;
            
            if (_providerIndexMap.ContainsKey(provider))
            {
                return;
            }

            _dataProvidersToAdd.Add(provider);
            _isDirty = true;
        }

        public void Remove(TProvider provider)
        {
            if (!_providerIndexMap.TryGetValue(provider, out int index))
            {
                _dataProvidersToRemove.Add(index);
                _isDirty = true;
            }
        }

        public bool TryGetIndex(TProvider provider, out int index)
        {
            return _providerIndexMap.TryGetValue(provider, out index);
        }

        public void Refresh()
        {
            if (_isDirty)
            {
                RefreshProviders();
                _isDirty = false;
            }
        }

        protected virtual void RemoveAtSwapBack(int index)
        {
            _providerIndexMap.Remove(_dataProviders[index]);

            _dataProviders.RemoveAtSwapBack(index, out TProvider swappedProvider);
            if (swappedProvider != null)
            {
                _providerIndexMap[swappedProvider] = index;
            }
        }

        protected virtual void Add(TProvider provider)
        {
            _dataProviders.Add(provider);
            int index = _dataProviders.Count - 1;
            _providerIndexMap[provider] = index;
        }

        protected virtual void RefreshProviders()
        {
            RefreshRemoveProviders();
            RefreshAddProviders();
        }
        
        protected void RefreshRemoveProviders()
        {
            if (_dataProvidersToRemove.Count == 0)
            {
                return;
            }

            foreach (int indexBeingRemoved in _dataProvidersToRemove)
            {
                RemoveAtSwapBack(indexBeingRemoved);
            }
            _dataProvidersToRemove.Clear();
        }

        protected void RefreshAddProviders()
        {
            if (_dataProvidersToAdd.Count == 0)
            {
                return;
            }

            foreach (TProvider provider in _dataProvidersToAdd)
            {
                Add(provider);
            }
            _dataProvidersToAdd.Clear();
        }
    }
}
