using UnityEngine;

namespace QuizGame.Character
{
    /// <summary>
    /// Scene marker component. Place this in scenes where the player character should appear.
    /// The PlayerCharacterManager will find all PlayerSpawnPoints in the loaded scene and
    /// instantiate the player at the first active one.
    /// </summary>
    public class PlayerSpawnPoint : MonoBehaviour
    {
        [SerializeField]
        private bool spawnOnSceneLoad = true;

        [SerializeField]
        private bool faceRight = true;

        [SerializeField, Tooltip("Spawn the player as a child of this point so it shows/hides with its parent (e.g. a room background). Keep this point's world scale at 1.")]
        private bool parentToSpawnPoint;

        public bool SpawnOnSceneLoad => spawnOnSceneLoad;
        public bool FaceRight => faceRight;
        public bool ParentToSpawnPoint => parentToSpawnPoint;
    }
}
