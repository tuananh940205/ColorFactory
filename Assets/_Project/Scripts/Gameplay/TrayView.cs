using UnityEngine;

namespace ColorFactory.Gameplay
{
    public sealed class TrayView : MonoBehaviour
    {
        [SerializeField] private Transform toyContainer;
        [SerializeField] private float firstToyLocalY = -1.1f;
        [SerializeField] private float toySpacing = 0.7f;

        public int TrayIndex { get; private set; }

        public Transform ToyContainer =>
            toyContainer != null ? toyContainer : transform;

        public void Setup(int trayIndex)
        {
            TrayIndex = trayIndex;
            name = $"Tray_{trayIndex + 1:00}";
        }

        public Vector3 GetToyLocalPosition(int stackIndex)
        {
            return new Vector3(
                0f,
                firstToyLocalY + toySpacing * stackIndex,
                -0.1f);
        }
    }
}