using System;
using ColorFactory.Core;
using NUnit.Framework;
using UnityEngine;

namespace ColorFactory.Tests
{
    /// <summary>
    /// Kiểm thử vòng đời pool: tái sử dụng instance, từ chối request khi shutdown.
    /// </summary>
    public sealed class PoolServiceTests
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
            root = new GameObject("PoolRoot");
            pool = root.AddComponent<PoolService>();
            template = new GameObject("ToyTemplate");
            template.SetActive(false);
        }

        [TearDown]
        /// <summary>
        /// Hủy pool, template và các instance đã tạo.
        /// </summary>
        public void TearDown()
        {
            // DestroyImmediate trước để instance thành null; ClearAll chỉ gọi Destroy với instance còn sống,
            // nên trong Edit Mode không phát sinh lỗi "Destroy may not be called from edit mode".
            UnityEngine.Object.DestroyImmediate(root);
            UnityEngine.Object.DestroyImmediate(template);
            pool.ClearAll();
        }

        [Test]
        /// <summary>
        /// Trả instance về pool rồi lấy lại phải tái sử dụng đúng instance đó.
        /// </summary>
        public void Return_ThenGet_ReusesSameInstance()
        {
            pool.CreatePool("Toy", template, 1);

            GameObject first = pool.Get("Toy");
            pool.Return(first);
            GameObject second = pool.Get("Toy");

            Assert.AreSame(first, second);
            Assert.IsTrue(second.activeSelf);

            // Trả instance về pool để nó thành con của root, TearDown hủy root sẽ hủy luôn instance này.
            pool.Return(second);
        }

        [Test]
        /// <summary>
        /// Lấy từ pool không tồn tại phải ném KeyNotFoundException.
        /// </summary>
        public void Get_UnknownPool_Throws()
        {
            Assert.Throws<System.Collections.Generic.KeyNotFoundException>(
                () => pool.Get("Missing"));
        }

        [Test]
        /// <summary>
        /// Sau khi BeginShutdown, mọi request lấy hoặc tạo pool mới phải bị từ chối.
        /// </summary>
        public void Shutdown_RejectsGetAndCreate()
        {
            pool.CreatePool("Toy", template, 0);
            pool.BeginShutdown();

            Assert.IsTrue(pool.IsShuttingDown);
            Assert.Throws<InvalidOperationException>(() => pool.Get("Toy"));
            Assert.Throws<InvalidOperationException>(
                () => pool.CreatePool("Other", template, 0));
        }
    }
}
