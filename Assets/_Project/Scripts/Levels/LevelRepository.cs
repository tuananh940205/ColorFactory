using System;
using System.Threading;
using System.Threading.Tasks;
using ColorFactory.Core;

namespace ColorFactory.Levels
{
    public sealed class LevelRepository
    {
        public async Task<LevelData> LoadAsync(
            string levelKey,
            SceneAssetScope assetScope,
            CancellationToken cancellationToken)
        {
            if (assetScope == null)
                throw new ArgumentNullException(nameof(assetScope));

            LevelData level = await assetScope.LoadAsync<LevelData>(
                levelKey,
                cancellationToken);

            level.ValidateOrThrow();
            return level;
        }
    }
}