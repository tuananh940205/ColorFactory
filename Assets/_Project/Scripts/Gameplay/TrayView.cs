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
        [SerializeField] private Color hintColor = new(0.28f, 0.88f, 0.62f);

        private bool isSelected;
        private bool isHinted;
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
        /// Bật hoặc tắt highlight khay nguồn do người chơi chọn.
        /// </summary>
        public void SetSelected(bool selected)
        {
            isSelected = selected;
            RefreshColor();
        }

        /// <summary>
        /// Nhận click/touch thông qua EventSystem và Physics2DRaycaster.
        /// </summary>
        public void OnPointerClick(PointerEventData eventData)
        {
            Clicked?.Invoke(TrayIndex);
        }
        
        /// <summary>
        /// Bật hoặc tắt highlight xanh cho khay được Hint đề xuất.
        /// </summary>
        public void SetHint(bool hinted)
        {
            isHinted = hinted;
            RefreshColor();
        }

        /// <summary>
        /// Cập nhật màu hiển thị, trong đó selection ưu tiên hơn hint.
        /// </summary>
        private void RefreshColor()
        {
            if (trayRenderer == null)
                return;

            if (isSelected)
                trayRenderer.color = selectedColor;
            else if (isHinted)
                trayRenderer.color = hintColor;
            else
                trayRenderer.color = normalColor;
        }
    }
}