using UnityEngine;

namespace Gilzoide.UpdateManager.Sample.FollowTarget
{
    public class MoveTarget : AManagedBehaviour, IUpdatable
    {
        [SerializeField] private Camera _camera;

        void Awake()
        {
            if (_camera == null)
            {
                _camera = Camera.main;
            }
        }

        public void ManagedUpdate()
        {
            if (!Input.GetMouseButton(0))
            {
                return;
            }

            Ray ray = _camera.ScreenPointToRay(Input.mousePosition);
            Plane xz = new Plane(transform.up, transform.position);
            if (xz.Raycast(ray, out float z))
            {
                transform.position = ray.GetPoint(z);
            }
        }
    }
}
