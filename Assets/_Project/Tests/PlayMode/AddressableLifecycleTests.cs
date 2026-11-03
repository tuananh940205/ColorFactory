using System;
using System.Collections;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using ColorFactory.Core;
using ColorFactory.Levels;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace ColorFactory.Tests
{
    /// <summary>
    /// Smoke test vòng đời Addressables qua AddressableService và SceneAssetScope.
    /// Mọi lần chờ Task đều có timeout, để Run All ở PlayMode luôn kết thúc dù Addressables treo.
    /// Exception mong đợi được khai báo trước trong từng test, không dùng try-catch.
    /// Yêu cầu Addressables có key Level_001 (Play Mode Script: Use Asset Database hoặc đã build content).
    /// </summary>
    public sealed class AddressableLifecycleTests
    {
        /// <summary>
        /// Key không tồn tại dùng cho test failure scenario "level_999".
        /// </summary>
        private const string MissingLevelKey = "Level_999";

        /// <summary>
        /// Giới hạn thời gian chờ một Task Addressables; quá thời gian thì test fail thay vì treo.
        /// </summary>
        private const float TaskTimeoutSeconds = 20f;

        private GameObject serviceRoot;
        private AddressableService service;

        [SetUp]
        /// <summary>
        /// Tạo AddressableService mới cho từng test để không dùng chung trạng thái.
        /// </summary>
        public void SetUp()
        {
            serviceRoot = new GameObject("AddressableServiceTest");
            service = serviceRoot.AddComponent<AddressableService>();
        }

        [TearDown]
        /// <summary>
        /// Gỡ trạng thái log tạm thời và hủy service ngay để OnDestroy gỡ đăng ký khỏi Services.
        /// </summary>
        public void TearDown()
        {
            LogAssert.ignoreFailingMessages = false;
            UnityEngine.Object.DestroyImmediate(serviceRoot);
        }

        [UnityTest]
        /// <summary>
        /// Key có khoảng trắng thừa phải bị từ chối bằng ArgumentException trước khi gọi Addressables.
        /// </summary>
        public IEnumerator InvalidKey_PredictedArgumentException_LeavesNoEntries()
        {
            SceneAssetScope scope = service.CreateSceneScope("InvalidKeyTest");
            Task<LevelData> load = scope.LoadAsync<LevelData>(" Level_001", CancellationToken.None);

            yield return WaitForTask(load);

            AssertTaskFinished(load);
            Assert.IsTrue(load.IsFaulted, "Load key sai khoảng trắng phải lỗi.");
            Assert.IsInstanceOf<ArgumentException>(load.Exception.InnerException);
            Assert.AreEqual(0, service.LoadedKeyCount);
        }

        
        /// <summary>
        /// Level_999 không tồn tại: exception mong đợi là InvalidOperationException do AcquireAsync bọc lỗi Addressables.
        /// Lời gọi await được bọc try-catch để bắt đúng exception đã khai báo; exception khác vẫn làm test fail.
        /// Có timeout bằng Task.WhenAny để test không treo. Log lỗi Addressables được bỏ qua có chủ đích và reset trong TearDown.
        /// </summary>
        [Test]
        public async Task MissingLevel999_PredictedInvalidOperation_FinishesAndLeavesNoEntries()
        {
            // Addressables tự log exception "No Location found for Key=Level_999" (kiểu Error trong Test Runner).
            // try-catch không chặn được log nên phải khai báo đúng log này; log khác vẫn làm test fail.
            LogAssert.Expect(LogType.Error, new Regex("No Location found for Key=Level_999"));
            SceneAssetScope scope = service.CreateSceneScope("MissingLevel999Test");
            InvalidOperationException expected = null;

            // Bọc cả lời gọi LoadAsync lẫn lúc await: mọi exception dự kiến đều được bắt tại đây.
            try
            {
                Task<LevelData> load = scope.LoadAsync<LevelData>(MissingLevelKey, CancellationToken.None);

                Task finished = await Task.WhenAny(
                    load,
                    Task.Delay(TimeSpan.FromSeconds(TaskTimeoutSeconds)));
                Assert.AreSame(load, finished, "Level_999 không hoàn thành trong thời gian chờ.");

                await load;
            }
            catch (InvalidOperationException exception)
            {
                expected = exception;
            }

            Assert.IsNotNull(expected, "Level_999 phải ném InvalidOperationException.");
            Assert.AreEqual(0, service.LoadedKeyCount);
            Assert.AreEqual(0, scope.LeaseCount);
        }

        [UnityTest]
        /// <summary>
        /// Hủy trước khi load phải báo OperationCanceledException và không để lại entry.
        /// </summary>
        public IEnumerator CancelledLoad_PredictedCancellation_LeavesNoEntries()
        {
            SceneAssetScope scope = service.CreateSceneScope("CancelledTest");
            var cancellation = new CancellationTokenSource();
            cancellation.Cancel();
            Task<LevelData> load = scope.LoadAsync<LevelData>("Level_001", cancellation.Token);

            yield return WaitForTask(load);
            cancellation.Dispose();

            AssertTaskFinished(load);
            Assert.IsTrue(load.IsFaulted || load.IsCanceled);
            Assert.AreEqual(0, service.LoadedKeyCount);
        }

        [UnityTest]
        /// <summary>
        /// Load LevelData qua LevelRepository rồi ReleaseAllAsync: entry được giải phóng,
        /// gọi lại lần hai vẫn an toàn và scope đã đóng từ chối load mới với InvalidOperationException đã khai báo.
        /// </summary>
        public IEnumerator LoadLevel_ThenReleaseAll_IsIdempotent_AndClosesScope()
        {
            SceneAssetScope scope = service.CreateSceneScope("LifecycleTest");
            var repository = new LevelRepository();
            Task<LevelData> load = repository.LoadAsync("Level_001", scope, CancellationToken.None);

            yield return WaitForTask(load);

            AssertTaskFinished(load);
            Assert.IsFalse(load.IsFaulted);
            Assert.AreEqual(1, load.Result.levelId);
            Assert.AreEqual(1, service.LoadedKeyCount);
            Assert.AreEqual(1, scope.LeaseCount);

            Task release = scope.ReleaseAllAsync();
            yield return WaitForTask(release);
            AssertTaskFinished(release);

            Assert.AreEqual(0, service.LoadedKeyCount);
            Assert.AreEqual(0, scope.LeaseCount);
            Assert.IsTrue(scope.IsClosing);

            Task secondRelease = scope.ReleaseAllAsync();
            yield return WaitForTask(secondRelease);
            AssertTaskFinished(secondRelease);
            Assert.IsFalse(secondRelease.IsFaulted);

            Task rejected = scope.LoadAsync<LevelData>("Level_001", CancellationToken.None);
            yield return WaitForTask(rejected);

            AssertTaskFinished(rejected);
            Assert.IsTrue(rejected.IsFaulted);
            Assert.IsInstanceOf<InvalidOperationException>(rejected.Exception.InnerException);
        }

        [UnityTest]
        /// <summary>
        /// Hai scope cùng giữ một key chỉ dùng chung một asset; giải phóng scope này không làm mất asset của scope kia.
        /// </summary>
        public IEnumerator TwoScopes_ShareOneAsset_UntilBothRelease()
        {
            SceneAssetScope first = service.CreateSceneScope("ScopeA");
            SceneAssetScope second = service.CreateSceneScope("ScopeB");

            Task<LevelData> loadA = first.LoadAsync<LevelData>("Level_001", CancellationToken.None);
            Task<LevelData> loadB = second.LoadAsync<LevelData>("Level_001", CancellationToken.None);
            yield return WaitForTask(loadA);
            yield return WaitForTask(loadB);

            AssertTaskFinished(loadA);
            AssertTaskFinished(loadB);
            Assert.AreSame(loadA.Result, loadB.Result);
            Assert.AreEqual(1, service.LoadedKeyCount);

            Task releaseA = first.ReleaseAllAsync();
            yield return WaitForTask(releaseA);
            AssertTaskFinished(releaseA);
            Assert.AreEqual(1, service.LoadedKeyCount);

            Task releaseB = second.ReleaseAllAsync();
            yield return WaitForTask(releaseB);
            AssertTaskFinished(releaseB);
            Assert.AreEqual(0, service.LoadedKeyCount);
        }

        /// <summary>
        /// Tạo điều kiện chờ Task hoàn thành hoặc hết TaskTimeoutSeconds, để coroutine luôn kết thúc.
        /// </summary>
        private static WaitUntil WaitForTask(Task task)
        {
            float deadline = Time.realtimeSinceStartup + TaskTimeoutSeconds;
            return new WaitUntil(() => task.IsCompleted || Time.realtimeSinceStartup >= deadline);
        }

        /// <summary>
        /// Kiểm tra Task đã hoàn thành trong thời gian chờ; nếu không thì test fail với thông báo rõ ràng thay vì treo.
        /// </summary>
        private static void AssertTaskFinished(Task task)
        {
            Assert.IsTrue(task.IsCompleted, $"Task không hoàn thành sau {TaskTimeoutSeconds}s.");
        }
    }
}
