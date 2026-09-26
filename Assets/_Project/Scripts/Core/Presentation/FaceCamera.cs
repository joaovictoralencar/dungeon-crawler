using Unity.Cinemachine;
using UnityEngine;

namespace DungeonCrawler.Core.Presentation
{
    public sealed class FaceCamera : MonoBehaviour
    {
        private enum FaceAxis
        {
            Up,
            Forward
        }

        [SerializeField] private FaceAxis faceAxis = FaceAxis.Up;
        [SerializeField] private bool invertFacingDirection;

        private void OnEnable()
        {
            CinemachineCore.CameraUpdatedEvent.AddListener(OnCameraUpdated);
        }

        private void OnDisable()
        {
            CinemachineCore.CameraUpdatedEvent.RemoveListener(OnCameraUpdated);
        }

        private void OnCameraUpdated(CinemachineBrain brain)
        {
            if (brain == null)
                return;

            Vector3 direction = -brain.transform.forward;
            if (direction.sqrMagnitude <= 0.0001f)
                return;

            direction.Normalize();
            if (invertFacingDirection)
                direction = -direction;

            Vector3 up = Vector3.ProjectOnPlane(Vector3.up, direction);
            if (up.sqrMagnitude <= 0.0001f)
                up = Vector3.ProjectOnPlane(brain.transform.up, direction);

            if (faceAxis == FaceAxis.Forward)
            {
                transform.rotation = Quaternion.LookRotation(direction, up);
                return;
            }

            Vector3 forward = Vector3.ProjectOnPlane(Vector3.up, direction);
            if (forward.sqrMagnitude <= 0.0001f)
                forward = Vector3.ProjectOnPlane(brain.transform.forward, direction);

            transform.rotation = Quaternion.LookRotation(forward, direction);
        }
    }
}