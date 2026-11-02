using UnityEngine;

namespace ColorFactory.Gameplay
{
    /// <summary>
    /// Phát particle phản hồi tái sử dụng cho lúc toy đáp và lúc hoàn thành level.
    /// </summary>
    public sealed class BoardFeedbackView : MonoBehaviour
    {
        [SerializeField, Min(1)] private int landingParticleCount = 8;
        [SerializeField, Min(1)] private int completionParticleCount = 36;

        private ParticleSystem.EmitParams emitParams = new();
        private ParticleSystem landingParticles;
        private ParticleSystem completionParticles;

        /// <summary>
        /// Tạo sẵn hai ParticleSystem built-in để không Instantiate effect theo từng nước đi.
        /// </summary>
        private void Awake()
        {
            landingParticles = CreateParticleSystem("LandingSparkles", 6);
            completionParticles = CreateParticleSystem("CompletionSparkles", 7);
        }

        /// <summary>
        /// Dừng particle còn sống trước khi BoardRoot bị hủy cùng scene.
        /// </summary>
        private void OnDestroy()
        {
            if (landingParticles != null)
                landingParticles.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);

            if (completionParticles != null)
            {
                completionParticles.Stop(
                    true,
                    ParticleSystemStopBehavior.StopEmittingAndClear);
            }
        }

        /// <summary>
        /// Phát sparkle nhỏ tại vị trí toy vừa đáp với màu toy tương ứng.
        /// </summary>
        public void PlayLanding(Vector3 worldPosition, Color color)
        {
            if (landingParticles == null)
                return;

            landingParticles.transform.position = worldPosition;
            emitParams.startColor = color;
            landingParticles.Emit(emitParams, landingParticleCount);
        }

        /// <summary>
        /// Phát sparkle vàng nhẹ ở giữa board sau khi move hoàn thành level.
        /// </summary>
        public void PlayCompletion(Vector3 worldPosition)
        {
            if (completionParticles == null)
                return;

            completionParticles.transform.position = worldPosition;
            emitParams.startColor = new Color(1f, 0.82f, 0.25f, 1f);
            completionParticles.Emit(emitParams, completionParticleCount);
        }

        /// <summary>
        /// Cấu hình một ParticleSystem world-space nhỏ dùng chung cho toàn bộ feedback.
        /// </summary>
        private ParticleSystem CreateParticleSystem(string effectName, int sortingOrder)
        {
            var effectObject = new GameObject(effectName, typeof(ParticleSystem));
            effectObject.transform.SetParent(transform, false);

            ParticleSystem particles = effectObject.GetComponent<ParticleSystem>();
            ParticleSystem.MainModule main = particles.main;
            main.loop = false;
            main.playOnAwake = false;
            main.simulationSpace = ParticleSystemSimulationSpace.World;
            main.startLifetime = 0.42f;
            main.startSpeed = 1.7f;
            main.startSize = 0.12f;
            main.maxParticles = 48;

            ParticleSystem.EmissionModule emission = particles.emission;
            emission.enabled = false;

            ParticleSystem.ShapeModule shape = particles.shape;
            shape.enabled = false;

            ParticleSystem.ColorOverLifetimeModule colorOverLifetime =
                particles.colorOverLifetime;
            colorOverLifetime.enabled = true;

            var alphaGradient = new Gradient();
            alphaGradient.SetKeys(
                new[]
                {
                    new GradientColorKey(Color.white, 0f),
                    new GradientColorKey(Color.white, 1f)
                },
                new[]
                {
                    new GradientAlphaKey(0.9f, 0f),
                    new GradientAlphaKey(0f, 1f)
                });
            colorOverLifetime.color = new ParticleSystem.MinMaxGradient(alphaGradient);

            ParticleSystemRenderer renderer = particles.GetComponent<ParticleSystemRenderer>();
            renderer.sortingOrder = sortingOrder;

            return particles;
        }
    }
}
