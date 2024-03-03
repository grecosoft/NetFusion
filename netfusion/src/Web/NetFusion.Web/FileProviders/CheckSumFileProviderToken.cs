using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Threading;
using Microsoft.Extensions.Primitives;

namespace NetFusion.Web.FileProviders
{
    public sealed class CheckSumFileProviderToken(
        string rootPath,
        string filter,
        int detectChangeIntervalMs = 30_000)
        : IChangeToken, IDisposable
    {
        private List<CallbackRegistration>? _registeredCallbacks = [];
        private readonly string _rootPath = rootPath;
        private readonly string _filter = filter;
        private readonly int _detectChangeIntervalMs = detectChangeIntervalMs;
        
        private Timer? _timer;
        private string? _lastChecksum;
        private readonly object _timerLock = new();
        
        public bool HasChanged { get; private set; }
        public bool ActiveChangeCallbacks => true;
        
        internal void EnsureStarted()
        {
            lock (_timerLock)
            {
                if (_timer == null)
                {
                    var fullPath = Path.Combine(_rootPath, _filter);
                    if (File.Exists(fullPath))
                    {
                        _timer = new Timer(CheckForChanges);
                        _timer.Change(0, _detectChangeIntervalMs);
                    }
                }
            }
        }

        private void CheckForChanges(object? state)
        {
            var fullPath = Path.Combine(_rootPath, _filter);
            var newCheckSum = GetFileChecksum(fullPath);
            
            var newHasChangesValue = false;
            if (_lastChecksum != null && _lastChecksum != newCheckSum)
            {
                NotifyChanges();

                newHasChangesValue = true;
            }

            HasChanged = newHasChangesValue;
            _lastChecksum = newCheckSum;
        }

        private void NotifyChanges()
        {
            var localRegisteredCallbacks = _registeredCallbacks;
            if (localRegisteredCallbacks != null)
            {
                var count = localRegisteredCallbacks.Count;
                for (int i = 0; i < count; i++)
                {
                    localRegisteredCallbacks[i].Notify();
                }
            }
        }

        private static string GetFileChecksum(string filename)
        {
            using var md5 = MD5.Create();
            using var stream = File.OpenRead(filename);
            return BitConverter.ToString(md5.ComputeHash(stream));
        }
        
        public IDisposable RegisterChangeCallback(Action<object?> callback, object? state)
        {
            var localRegisteredCallbacks = _registeredCallbacks;

            if (localRegisteredCallbacks == null)
            {
                throw new ObjectDisposedException(nameof(_registeredCallbacks));
            }

            var cbRegistration = new CallbackRegistration(callback, state, (cb) => localRegisteredCallbacks.Remove(cb));
            localRegisteredCallbacks.Add(cbRegistration);

            return cbRegistration;
        }

        public void Dispose()
        {
            Interlocked.Exchange(ref _registeredCallbacks, null);

            Timer? localTimer;
            lock (_timerLock)
            {
                localTimer = Interlocked.Exchange(ref _timer, null);
            }

            localTimer?.Dispose();
        }
        
        private class CallbackRegistration(
            Action<object> callback,
            object? state,
            Action<CallbackRegistration> unregister)
            : IDisposable
        {
            private Action<object>? _callback = callback;
            private object? _state = state;
            private Action<CallbackRegistration>? _unregister = unregister;
            
            public void Notify()
            {
                var localState = _state;
                var localCallback = _callback;

                if (localState != null) localCallback?.Invoke(localState);
            }


            public void Dispose()
            {
                var localUnregister = Interlocked.Exchange(ref _unregister, null);
                if (localUnregister != null)
                {
                    localUnregister(this);
                    _callback = null;
                    _state = null;
                }
            }
        }
    }
}