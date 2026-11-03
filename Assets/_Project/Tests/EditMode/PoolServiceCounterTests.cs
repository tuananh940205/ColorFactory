using ColorFactory.Core;
using NUnit.Framework;
using UnityEngine;

namespace ColorFactory.Tests
{
    /// <summary>
    /// Kiểm thử bộ đếm ActiveCount/InactiveCount của PoolService để memory report có số liệu đúng.
    /// </summary>
    public sealed class PoolServiceCounterTests
    {
        private GameObject root;
        private GameObject template;
        private PoolService pool;

        [SetUp]
        /// <summary>
        /// Tạo PoolService và template mới cho từng test.
        /// </summary>
        public void SetUp()
        {
            root = new GameObject("PoolCounterRoot");
            pool = root.AddComponent<PoolService>();
            template = new GameObject("CounterTemplate");
            template.SetActive(false);
        }

        [TearDown]
        /// <summary>
        /// Hủy root trước (instance đang inactive/active là con của root), rồi hủy template và dọn pool.
        /// </summary>
        public void TearDown()
        {
            UnityEngine.Object.DestroyImmediate(root);
            UnityEngine.Object.DestroyImmediate(template);
            pool.ClearAll();
        }

        [Test]
        /// <summary>
        /// Pool mới tạo sẵn N instance phải có InactiveCount = N và ActiveCount = 0.
        /// </summary>
        public void CreatePool_WithInitialSize_CountsInactiveInstances()
        {
            pool.CreatePool("Toy", template, 3);

            Assert.AreEqual(0, pool.ActiveCount);
            Assert.AreEqual(3, pool.InactiveCount);
        }

        [Test]
        /// <summary>
        /// Get chuyển instance từ inactive sang active; Return chuyển ngược lại.
        /// </summary>
        public void GetAndReturn_MoveInstanceBetweenCounters()
        {
            pool.CreatePool("Toy", template, 2);

            GameObject instance = pool.Get("Toy");
            Assert.AreEqual(1, pool.ActiveCount);
            Assert.AreEqual(1, pool.InactiveCount);

            pool.Return(instance);
            Assert.AreEqual(0, pool.ActiveCount);
            Assert.AreEqual(2, pool.InactiveCount);
        }
    }
}
