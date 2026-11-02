using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ColorFactory.UI
{
    /// <summary>
    /// Một button level trong màn hình chọn level.
    /// </summary>
    public sealed class LevelSelectButton : MonoBehaviour
    {
        [SerializeField] private Button button;
        [SerializeField] private TMP_Text levelLabel;
        [SerializeField] private TMP_Text lockLabel;

        /// <summary>
        /// Gán số level, trạng thái mở khóa và callback khi người chơi bấm.
        /// </summary>
        public void Setup(
            int levelId,
            bool isUnlocked,
            Action<int> onSelected)
        {
            levelLabel.text = $"LEVEL {levelId:00}";

            // Level khóa thì hiện chữ LOCKED.
            lockLabel.gameObject.SetActive(!isUnlocked);

            // isUnlocked = false sẽ tương đương button.interactable = false.
            button.interactable = isUnlocked;

            // Xóa listener cũ để prefab được tái sử dụng an toàn.
            button.onClick.RemoveAllListeners();

            // Chỉ level mở mới có thể chuyển sang Gameplay scene.
            if (isUnlocked)
                button.onClick.AddListener(() => onSelected?.Invoke(levelId));
        }
    }
}