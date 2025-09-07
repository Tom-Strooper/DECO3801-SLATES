using System;
using UnityEngine;

namespace Slates.PuzzleInteractions.Controllers
{
    public class BookController : MonoBehaviour
    {
        [Header("Transform References")]
        [SerializeField, Tooltip("Point at the base of the spine of the book")] private Transform _spine;
        [SerializeField, Tooltip("Bottom left corner of left side of book")] private Transform _leftSideCorner;
        [SerializeField, Tooltip("Bottom right corner of right side of book")] private Transform _rightSideCorner;
        [SerializeField] private Transform _leftPagePivot;
        [SerializeField] private Transform _rightPagePivot;

        private Vector3 BookRight => _rightSideCorner.position - _leftSideCorner.position;
        private Vector3 BookLeft => _leftSideCorner.position - _rightSideCorner.position;

        private Vector3 SpineRight => _rightSideCorner.position - _spine.position;
        private Vector3 SpineLeft => _leftSideCorner.position - _spine.position;

        private float MaxAngleLeft => Mathf.Atan2(Vector3.Dot(SpineLeft, _spine.forward), Vector3.Dot(SpineLeft, _spine.right));
        private float MaxAngleRight => Mathf.Atan2(Vector3.Dot(SpineRight, _spine.forward), Vector3.Dot(SpineRight, _spine.right));

        [Header("Renderer References")]
        [SerializeField] private Renderer _left;
        [SerializeField] private Renderer _leftPageLeft;
        [SerializeField] private Renderer _leftPageRight;
        [SerializeField] private Renderer _rightPageLeft;
        [SerializeField] private Renderer _rightPageRight;
        [SerializeField] private Renderer _right;

        [Header("Page Textures")]
        [SerializeField] private Texture2D _blankPageTexture;
        [SerializeField] private Texture2D[] _pages;

        [Header("Page UIs")]
        [SerializeField] private GameObject[] _pageUIs;

        [Header("Animation Settings")]
        [SerializeField, Range(0.0f, 180.0f)] private float _autoTurnAngularSpeed;

        public bool CanBeginPageTurn => _dragStatus == DragStatus.NoDrag;

        private DragStatus _dragStatus = DragStatus.NoDrag;
        private Vector3 _dragStartPoint;
        private bool _didDragFromRight;
        private float _dragProgress;
        private bool _committed;

        private int _leftIndex = 0;

        private void Start()
        {
            // Initialise page visuals
            UpdateVisuals();
        }

        private void Update()
        {
            if (!_committed) return;

            bool sameDirectionCommit = (_didDragFromRight && _dragStatus == DragStatus.DraggingFromRight)
                                    || (!_didDragFromRight && _dragStatus == DragStatus.DraggingFromLeft);
            _dragProgress += (sameDirectionCommit ? 1.0f : -1.0f) * _autoTurnAngularSpeed / 180.0f * Time.deltaTime;
            if ((sameDirectionCommit && _dragProgress + float.Epsilon >= 1.0f)
             || (!sameDirectionCommit && _dragProgress - float.Epsilon <= 0.0f))
            {
                ResolveDrag();
            }
            else
            {
                UpdateDrag();
            }
        }

        public void UpdateDrag(Vector3 at)
        {
            switch (_dragStatus)
            {
                case DragStatus.DraggingFromLeft:
                    _dragProgress = LeftProgress(at);
                    break;
                case DragStatus.DraggingFromRight:
                    _dragProgress = RightProgress(at);
                    break;
            }
            UpdateDrag();
        }
        private void UpdateDrag()
        {
            float minAngle, maxAngle;

            if (_didDragFromRight)
            {
                minAngle = MaxAngleRight;
                maxAngle = MaxAngleLeft;
            }
            else
            {
                minAngle = MaxAngleLeft;
                maxAngle = MaxAngleRight;
            }

            float angleFromInitial = Mathf.Lerp(0.0f, maxAngle - minAngle, _dragProgress);

            if (_didDragFromRight) MoveRightPage(angleFromInitial);
            else MoveLeftPage(angleFromInitial);
        }

        public void StartDrag(Vector3 from)
        {
            // Don't start dragging until page has completely flipped
            if (_dragStatus != DragStatus.NoDrag) return;

            Debug.Log($"Drag starting from {from}");
            _dragStartPoint = from;

            if (IsOnRightSide(from))
            {
                if (_leftIndex >= LastLeftPageIndex) return;

                _dragStatus = DragStatus.DraggingFromRight;
                _didDragFromRight = true;
            }
            else
            {
                if (_leftIndex <= 0) return;

                _dragStatus = DragStatus.DraggingFromLeft;
                _didDragFromRight = false;
            }

            UpdateDrag(from);
        }
        public void EndDrag(Vector3 to)
        {
            Debug.Log($"Drag ending to {to}");
            // We shouldn't do anything if the page is not being dragged
            if (_dragStatus == DragStatus.NoDrag) return;

            // We only commit to the side on which the pointer currently is
            _dragStatus = IsOnRightSide(to) ? DragStatus.DraggingFromLeft : DragStatus.DraggingFromRight;

            // Indicate that the remainder of the drag should occur automatically
            _committed = true;
        }

        /// <summary>
        /// Resolves the drag by updating the visuals and placing the pivots in the correct positions/rotations.
        /// </summary>
        private void ResolveDrag()
        {
            if (_didDragFromRight && _dragStatus == DragStatus.DraggingFromRight)
            {
                _leftIndex += 2;
                UpdateVisuals();
            }
            if (!_didDragFromRight && _dragStatus == DragStatus.DraggingFromLeft)
            {
                _leftIndex -= 2;
                UpdateVisuals();
            }

            _dragStatus = DragStatus.NoDrag;
            _leftIndex = Mathf.Clamp(_leftIndex, 0, LastLeftPageIndex);

            _committed = false;

            // Reset positions of moving pages
            MoveLeftPage();
            MoveRightPage();
        }

        private void MoveLeftPage() => MoveLeftPage(0.0f);
        private void MoveLeftPage(float angleFromInitial)
        {
            _leftPagePivot.localRotation = Quaternion.identity * Quaternion.Euler(Vector3.up * -angleFromInitial * Mathf.Rad2Deg);
        }
        private void MoveRightPage() => MoveRightPage(0.0f);
        private void MoveRightPage(float angleFromInitial)
        {
            _rightPagePivot.localRotation = Quaternion.identity * Quaternion.Euler(Vector3.up * -angleFromInitial * Mathf.Rad2Deg);
        }

        private void UpdateVisuals()
        {
            _left.material.mainTexture = Page(_leftIndex - 2);
            _leftPageLeft.material.mainTexture = Page(_leftIndex - 1);
            _leftPageLeft.material.mainTexture = Page(_leftIndex - 1);

            _leftPageRight.material.mainTexture = Page(_leftIndex);
            _rightPageLeft.material.mainTexture = Page(_leftIndex + 1);

            _rightPageRight.material.mainTexture = Page(_leftIndex + 2);
            _right.material.mainTexture = Page(_leftIndex + 3);

            // TODO - Spawn/Despawn Page UI Elements
        }

        private int LastLeftPageIndex => _pages.Length - (_pages.Length % 2);

        private bool IsOnRightSide(Vector3 point) => Vector3.Dot(point - _spine.position, SpineRight) >= 0.0f;

        // 0.0f when point on at drag start point, 1.0f when point on left edge of book
        private float RightProgress(Vector3 point) => Mathf.Clamp01(Vector3.Dot(point - _dragStartPoint, BookLeft)
                                                                  / Vector3.Dot(_leftSideCorner.position - _dragStartPoint, BookLeft));
        // 0.0f when point on at drag start point, 1.0f when point on right edge of book
        private float LeftProgress(Vector3 point) => Mathf.Clamp01(Vector3.Dot(point - _dragStartPoint, BookRight)
                                                                 / Vector3.Dot(_rightSideCorner.position - _dragStartPoint, BookRight));

        private Texture2D Page(int index)
        {
            if (_pages is null || index < 0 || index >= _pages.Length) return _blankPageTexture;
            return _pages[index] ?? _blankPageTexture;
        }
        private GameObject PageUI(int index)
        {
            if (_pageUIs is null || index < 0 || index >= _pageUIs.Length) return null;
            return _pageUIs[index];
        }
    }

    public enum DragStatus
    {
        NoDrag,
        DraggingFromLeft,
        DraggingFromRight,
    }
}