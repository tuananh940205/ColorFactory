using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace ColorFactory.Core
{
    public sealed class SceneAssetScope
    {
        private readonly AddressableService addressableService;
        private readonly string scopeName;
        private readonly List<IAssetLease> leases = new();
        private readonly object leasesLock = new();

        private bool isClosing;
        private Task releaseTask;

        public string ScopeName => scopeName;
        public bool IsClosing => isClosing;
        public int LeaseCount => leases.Count;

        internal SceneAssetScope(
            AddressableService addressableService,
            string scopeName)
        {
            this.addressableService = addressableService ??
                throw new ArgumentNullException(nameof(addressableService));

            this.scopeName = string.IsNullOrWhiteSpace(scopeName)
                ? "UnnamedScope"
                : scopeName;
        }

        public async Task<T> LoadAsync<T>(
            string key,
            CancellationToken cancellationToken)
            where T : UnityEngine.Object
        {
            lock (leasesLock)
            {
                if (isClosing)
                {
                    throw new InvalidOperationException(
                        $"Asset scope '{scopeName}' is already closing.");
                }
            }

            AssetLease<T> lease =
                await addressableService.AcquireAsync<T>(key, cancellationToken);

            lock (leasesLock)
            {
                if (!isClosing)
                {
                    leases.Add(lease);
                    return lease.Asset;
                }
            }

            await lease.ReleaseAsync();

            throw new OperationCanceledException(
                $"Asset scope '{scopeName}' closed while loading '{key}'.");
        }

        public Task ReleaseAllAsync()
        {
            lock (leasesLock)
            {
                if (releaseTask != null)
                    return releaseTask;

                isClosing = true;

                IAssetLease[] leasesToRelease = leases.ToArray();
                leases.Clear();

                releaseTask = ReleaseAllInternalAsync(leasesToRelease);
                return releaseTask;
            }
        }

        private static async Task ReleaseAllInternalAsync(
            IReadOnlyList<IAssetLease> leasesToRelease)
        {
            for (int index = leasesToRelease.Count - 1; index >= 0; index--)
                await leasesToRelease[index].ReleaseAsync();
        }
    }
}