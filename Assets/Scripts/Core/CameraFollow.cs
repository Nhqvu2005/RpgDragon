using UnityEngine;

namespace RPGDragon.Core
{
    /// <summary>
    /// Makes the camera follow a target (usually the player).
    /// Attach to Main Camera and assign followTarget in Inspector.
    /// </summary>
    public class CameraFollow : MonoBehaviour
    {
        [Header("Settings")]
        [SerializeField] private Transform followTarget;
        [SerializeField] private float smoothSpeed = 5f;
        [SerializeField] private Vector3 offset = new Vector3(0, 0, -10);

        private void LateUpdate()
        {
            if (followTarget == null)
            {
                // Auto-find player if not assigned
                if (GameManager.Instance?.Player != null)
                    followTarget = GameManager.Instance.Player.transform;
                return;
            }

            Vector3 targetPosition = followTarget.position + offset;
            transform.position = Vector3.Lerp(transform.position, targetPosition, smoothSpeed * Time.deltaTime);
        }

        public void SetTarget(Transform target)
        {
            followTarget = target;
            if (target != null)
                transform.position = target.position + offset;
        }
    }
}
