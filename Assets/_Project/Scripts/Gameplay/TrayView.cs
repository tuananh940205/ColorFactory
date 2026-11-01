using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace ColorFactory.Gameplay
{
    /// <summary>
    /// View của một khay: hiển thị chọn/bỏ chọn và phát sự kiện khi người chơi chạm.
    /// </summary>
    public sealed class TrayView : MonoBehaviour, IPointerClickHandler
    {
        [SerializeField] private Transform toyContainer;
        [SerializeField] private SpriteRenderer trayRenderer;
        [SerializeField] private float firstToyLocalY = -1.1f;
        [SerializeField] private float toySpacing = 0.7f;
        [SerializeField] private Color selectedColor = new(1f, 0.82f, 0.2f);

        private Color normalColor = Color.white;

        public int TrayIndex { get; private set; }

        /// <summary>
        /// BoardView đăng ký callback này để biết người chơi đã chọn khay nào.
        /// </summary>
        public event Action<int> Clicked;

        /// <summary>
        /// Transform chứa toy; nếu không gán thì dùng chính root của prefab.
        /// </summary>
        public Transform ToyContainer => toyContainer != null ? toyContainer : transform;

        /// <summary>
        /// Tự tìm SpriteRenderer khi prefab chưa được gán thủ công.
        /// </summary>
        private void Awake()
        {
            if (trayRenderer == null)
                trayRenderer = GetComponentInChildren<SpriteRenderer>();

            if (trayRenderer != null)
                normalColor = trayRenderer.color;
        }

        /// <summary>
        /// Gán index của khay dựa theo dữ liệu LevelData.
        /// </summary>
        public void Setup(int trayIndex)
        {
            TrayIndex = trayIndex;
            name = $"Tray_{trayIndex + 1:00}";
            SetSelected(false);
        }

        /// <summary>
        /// Trả vị trí local của toy theo thứ tự từ đáy lên đỉnh.
        /// </summary>
        public Vector3 GetToyLocalPosition(int stackIndex)
        {
            return new Vector3(
                0f,
                firstToyLocalY + toySpacing * stackIndex,
                -0.1f);
        }

        /// <summary>
        /// Đổi màu khay để phản hồi trạng thái được người chơi chọn.
        /// </summary>
        public void SetSelected(bool selected)
        {
            if (trayRenderer != null)
                trayRenderer.color = selected ? selectedColor : normalColor;
        }

        /// <summary>
        /// Nhận click/touch thông qua EventSystem và Physics2DRaycaster.
        /// </summary>
        public void OnPointerClick(PointerEventData eventData)
        {
            Clicked?.Invoke(TrayIndex);
        }
    }
}