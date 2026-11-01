using ColorFactory.Core;
using UnityEngine;

namespace ColorFactory.Gameplay
{
    public sealed class ToyView : MonoBehaviour
    {
        [SerializeField] private SpriteRenderer spriteRenderer;

        private void Awake()
        {
            if (spriteRenderer == null)
                spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        }

        public void SetColor(ToyColor color)
        {
            if (spriteRenderer == null)
            {
                Debug.LogError("ToyView needs a SpriteRenderer.", this);
                return;
            }

            spriteRenderer.color = color switch
            {
                ToyColor.Red => new Color(0.95f, 0.25f, 0.28f),
                ToyColor.Blue => new Color(0.22f, 0.52f, 0.95f),
                ToyColor.Yellow => new Color(1.00f, 0.78f, 0.18f),
                ToyColor.Green => new Color(0.25f, 0.77f, 0.43f),
                ToyColor.Purple => new Color(0.62f, 0.38f, 0.90f),
                _ => Color.white
            };
        }
    }
}