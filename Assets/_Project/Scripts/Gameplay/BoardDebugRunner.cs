using ColorFactory.Levels;
using UnityEngine;

namespace ColorFactory.Gameplay
{
    public class BoardDebugRunner : MonoBehaviour
    {
        [SerializeField] private LevelData level;

        private void Start()
        {
            if (level == null)
            {
                Debug.LogError("Level Data is missing");
                return;
            }

            var board = new BoardModel(level);

            Debug.Log($"Loaded {board.TrayCount} trays");

            bool moved = board.TryMove(0, 2);

            Debug.Log($"Move 0 -> 2: {moved}");
            Debug.Log($"Completed: {board.IsCompleted()}");
        }
    }
}