using UnityEngine;
using Fusion;
using Slates.Camera;
using Slates.Networking.Input;
using Slates.Book;
using Slates.Networking;

namespace Slates.XRPlayer
{
    public class XRPlayerController : NetworkBehaviour
    {
        private const float InteractionDistance = 500.0f;

        // Networked values
        [Networked] private NetworkButtons PreviousButtons { get; set; }

        // TODO - This will likely need to be more clever in future (i.e., select from many books, etc)
        [Header("References")]
        [SerializeField] private BookController _book;
        private bool _bookSelected = false;

        [Header("Camera Settings (Non-VR)")]
        [SerializeField] private Transform _head;
        [SerializeField] private float _sensitivity = 40.0f;

        private float _xRotation;

        public override void Spawned()
        {
            if (!HasInputAuthority) return;

            // TODO - Attempt to initialise XR/VR controllers

            _book = FindAnyObjectByType<BookController>();
            CameraController.Instance.BindToHead(_head);

            _book.gameObject.SetActive(false);
        }

        public override void Despawned(NetworkRunner runner, bool hasState)
        {
            CameraController.Instance.UnbindFromHead(_head);
        }

        public override void FixedUpdateNetwork()
        {
            if (GetInput(out NetworkXRInputData data))
            {
                // Prevent wild input by normalising
                data.interactionDirection.Normalize();

                // Pause/unpause (TODO - in VR this likely needs to be different)
                if (data.buttons.WasPressed(PreviousButtons, (int)XRInputButtons.Pause))
                {
                    if (NetworkGameManager.Instance.IsPaused)
                    {
                        Cursor.lockState = CursorLockMode.None;
                        Cursor.visible = true;

                        NetworkGameManager.Instance.UnpauseGame();
                    }
                    else
                    {
                        Cursor.lockState = CursorLockMode.Locked;
                        Cursor.visible = false;

                        NetworkGameManager.Instance.PauseGame();
                    }
                }

                // TODO - Skip this camera rotation information if in VR - and use VR tracking to update camera position
                transform.Rotate(Vector3.up * data.look.x * _sensitivity * Runner.DeltaTime);

                _xRotation -= data.look.y * _sensitivity * Runner.DeltaTime;
                _xRotation = Mathf.Clamp(_xRotation, -80.0f, 85.0f);
                UpdateCameraRotation();

                // Book summoning
                if (data.buttons.WasPressed(PreviousButtons, (int)XRInputButtons.Summon))
                {
                    // TODO - This should probably be animated/prettier
                    _book.gameObject.SetActive(!_book.gameObject.activeSelf);
                    if (!_book.gameObject.activeSelf) _bookSelected = false;
                }

                // Pinch interactions
                RaycastHit hit;
                if (data.buttons.IsSet((int)XRInputButtons.Pinch))
                {
                    // Try to begin turning the page
                    if (_bookSelected)
                    {
                        if (Physics.Raycast(data.interactionOrigin, data.interactionDirection, out hit, InteractionDistance))
                        {
                            _book.UpdateDrag(hit.point);
                        }
                        else
                        {
                            _book.UpdateDrag(data.interactionOrigin + data.interactionDirection * InteractionDistance);
                        }
                    }
                    else
                    {
                        if (Physics.Raycast(data.interactionOrigin, data.interactionDirection, out hit, InteractionDistance))
                        {
                            if (hit.collider?.GetComponentInParent<BookController>() is BookController book)
                            {
                                if (book == _book && _book.CanBeginPageTurn)
                                {
                                    _bookSelected = _book.StartDrag(hit.point);
                                }
                            }
                        }
                    }
                }
                else if (data.buttons.WasReleased(PreviousButtons, (int)XRInputButtons.Pinch))
                {
                    if (_bookSelected)
                    {
                        // End the page turn
                        if (Physics.Raycast(data.interactionOrigin, data.interactionDirection, out hit, InteractionDistance)) _book.EndDrag(hit.point);
                        else _book.EndDrag(data.interactionOrigin + data.interactionDirection * InteractionDistance);

                        _bookSelected = false;
                    }
                }

                // Update button values
                PreviousButtons = data.buttons;
            }
        }

        public override void Render()
        {
            // TODO - Skip this if not it VR, replace w/ VR tracking
            UpdateCameraRotation();
        }

        private void UpdateCameraRotation()
        {
            _head.transform.localRotation = Quaternion.Euler(_xRotation, 0.0f, 0.0f);
        }
    }
}