using Slates.PuzzleInteractions.Controllers;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Slates.DebugUtils
{
    public class DebugCamera : MonoBehaviour
    {
        private InputAction _look, _select;

        [SerializeField] private UnityEngine.Camera _camera;
        private float _xRotation;

        [SerializeField] private BookController _book;
        private bool _bookSelected = false;

        private void Awake()
        {
            _look = InputSystem.actions.FindAction("Look");
            _select = InputSystem.actions.FindAction("Select");

            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        private void Update()
        {
            Vector2 look = _look.ReadValue<Vector2>();

            _xRotation -= look.y * 40.0f * Time.fixedDeltaTime;
            _xRotation = Mathf.Clamp(_xRotation, -80.0f, 80.0f);

            transform.Rotate(Vector3.up * look.x * 40.0f * Time.fixedDeltaTime);
            _camera.transform.localRotation = Quaternion.Euler(Vector3.right * _xRotation);

            if (_select.IsPressed())
            {
                if (_bookSelected)
                {
                    RaycastHit hit;
                    if (!Physics.Raycast(transform.position, _camera.transform.forward, out hit, 10.0f))
                    {
                        _book.UpdateDrag(transform.position + _camera.transform.forward * 10.0f);
                        return;
                    }

                    _book.UpdateDrag(hit.point);
                }
                else
                {
                    RaycastHit hit;
                    if (!Physics.Raycast(transform.position, _camera.transform.forward, out hit, 10.0f)) return;

                    if (hit.collider?.GetComponentInParent<BookController>() is not BookController book) return;
                    if (book != _book || !_book.CanBeginPageTurn) return;

                    // Start dragging
                    _bookSelected = _book.StartDrag(hit.point);
                }
            }

            if (_select.WasReleasedThisFrame())
            {
                if (!_bookSelected) return;

                RaycastHit hit;
                if (!Physics.Raycast(transform.position, _camera.transform.forward, out hit, 10.0f))
                {
                    _book.EndDrag(transform.position + _camera.transform.forward * 10.0f);
                    return;
                }

                _book.EndDrag(hit.point);
                _bookSelected = false;
            }
        }
    }
}