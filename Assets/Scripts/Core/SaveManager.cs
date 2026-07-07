using UnityEngine;

namespace RPGDragon.Core
{
    /// <summary>
    /// Stub for the save/load system. Implement serialization logic (PlayerPrefs,
    /// JSON, binary) as needed.
    /// </summary>
    public static class SaveManager
    {
        /// <summary>
        /// Persist the current game state.
        /// </summary>
        public static void Save()
        {
            Debug.Log("[SaveManager] Game saved.");
            // TODO: Implement actual persistence.
        }

        /// <summary>
        /// Load a previously saved game state.
        /// </summary>
        public static void Load()
        {
            Debug.Log("[SaveManager] Game loaded.");
            // TODO: Implement actual deserialization.
        }
    }
}
