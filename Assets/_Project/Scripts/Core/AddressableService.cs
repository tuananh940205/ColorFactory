using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace ColorFactory.Core
{
    [DefaultExecutionOrder(-1000)]
    public sealed class AddressableService : MonoBehaviour
    {
        private sealed class Entry
        {
            public string Key;
            public Type AssetType;
            public AsyncOperationHandle Handle;
            public Task Completion;
            public UnityEngine.Object Asset;
            public Exception Error;
            public int OwnerCount;
            public bool LoadFinished;
        }

        private readonly Dictionary<string, Entry> entries = new();
        private readonly SemaphoreSlim entriesLock = new(1, 1);

        public static AddressableService Instance { get; private set; }
        public int LoadedKeyCount => entries.Count;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        public SceneAssetScope CreateSceneScope(string scopeName)
        {
            return new SceneAssetScope(this, scopeName);
        }

        internal async Task<AssetLease<T>> AcquireAsync<T>(
            string key,
            CancellationToken cancellationToken)
            where T : UnityEngine.Object
        {
            ValidateKey(key);
            cancellationToken.ThrowIfCancellationRequested();

            Entry entry = await ReserveAsync<T>(key);

            try
            {
                await WaitWithCancellation(entry.Completion, cancellationToken);
                cancellationToken.ThrowIfCancellationRequested();

                if (entry.Error != null)
                    throw new InvalidOperationException(
                        $"Addressables failed to load key '{key}'.", entry.Error);

                if (entry.Asset is not T asset)
                {
                    throw new InvalidOperationException(
                        $"Key '{key}' was loaded as '{entry.AssetType.Name}', " +
                        $"but requested as '{typeof(T).Name}'.");
                }

                return new AssetLease<T>(this, key, asset);
            }
            catch
            {
                await ReleaseAsync(key);
                throw;
            }
        }

        internal async Task ReleaseAsync(string key)
        {
            AsyncOperationHandle? handleToRelease = null;

            await entriesLock.WaitAsync();

            try
            {
                if (!entries.TryGetValue(key, out Entry entry))
                    return;

                entry.OwnerCount = Mathf.Max(0, entry.OwnerCount - 1);

                if (entry.OwnerCount == 0 && entry.LoadFinished)
                {
                    entries.Remove(key);
                    handleToRelease = entry.Handle;
                }
            }
            finally
            {
                entriesLock.Release();
            }

            if (handleToRelease.HasValue)
                Addressables.Release(handleToRelease.Value);
        }

        private async Task<Entry> ReserveAsync<T>(string key)
            where T : UnityEngine.Object
        {
            await entriesLock.WaitAsync();

            try
            {
                if (entries.TryGetValue(key, out Entry existing))
                {
                    if (existing.AssetType != typeof(T))
                    {
                        throw new InvalidOperationException(
                            $"Addressable key '{key}' is already requested as " +
                            $"'{existing.AssetType.Name}', not '{typeof(T).Name}'.");
                    }

                    existing.OwnerCount++;
                    return existing;
                }

                AsyncOperationHandle<T> typedHandle =
                    Addressables.LoadAssetAsync<T>(key);

                var entry = new Entry
                {
                    Key = key,
                    AssetType = typeof(T),
                    Handle = typedHandle,
                    OwnerCount = 1
                };

                entry.Completion = ObserveLoadAsync(entry, typedHandle);
                entries.Add(key, entry);

                return entry;
            }
            finally
            {
                entriesLock.Release();
            }
        }

        private async Task ObserveLoadAsync<T>(
            Entry entry,
            AsyncOperationHandle<T> handle)
            where T : UnityEngine.Object
        {
            try
            {
                await handle.Task;

                if (handle.Status == AsyncOperationStatus.Succeeded)
                    entry.Asset = handle.Result;
                else
                    entry.Error = handle.OperationException ??
                                  new InvalidOperationException(
                                      $"Addressable load failed for '{entry.Key}'.");
            }
            catch (Exception exception)
            {
                entry.Error = exception;
            }

            AsyncOperationHandle? handleToRelease = null;

            await entriesLock.WaitAsync();

            try
            {
                entry.LoadFinished = true;

                if (entry.OwnerCount == 0 && entries.Remove(entry.Key))
                    handleToRelease = entry.Handle;
            }
            finally
            {
                entriesLock.Release();
            }

            if (handleToRelease.HasValue)
                Addressables.Release(handleToRelease.Value);
        }

        private static async Task WaitWithCancellation(
            Task task,
            CancellationToken cancellationToken)
        {
            if (!cancellationToken.CanBeCanceled)
            {
                await task;
                return;
            }

            var cancellationSource =
                new TaskCompletionSource<bool>(
                    TaskCreationOptions.RunContinuationsAsynchronously);

            using CancellationTokenRegistration registration =
                cancellationToken.Register(
                    () => cancellationSource.TrySetResult(true));

            if (await Task.WhenAny(task, cancellationSource.Task) != task)
                throw new OperationCanceledException(cancellationToken);

            await task;
        }

        private static void ValidateKey(string key)
        {
            if (string.IsNullOrWhiteSpace(key))
                throw new ArgumentException("Addressable key is required.", nameof(key));

            if (!string.Equals(key, key.Trim(), StringComparison.Ordinal))
            {
                throw new ArgumentException(
                    $"Addressable key '{key}' contains leading or trailing spaces.",
                    nameof(key));
            }
        }
    }

    internal interface IAssetLease
    {
        Task ReleaseAsync();
    }

    internal sealed class AssetLease<T> : IAssetLease
        where T : UnityEngine.Object
    {
        private readonly AddressableService service;
        private readonly string key;
        private int released;

        public T Asset { get; }

        public AssetLease(AddressableService service, string key, T asset)
        {
            this.service = service;
            this.key = key;
            Asset = asset;
        }

        public Task ReleaseAsync()
        {
            if (Interlocked.Exchange(ref released, 1) != 0)
                return Task.CompletedTask;

            return service.ReleaseAsync(key);
        }
    }
}
