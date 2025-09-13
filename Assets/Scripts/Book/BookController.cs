using System;
using Slates.Book.Pages;
using UnityEngine;

namespace Slates.Book
{
    public class BookController : MonoBehaviour
    {
        private const float MoveAngleThreshold = 2.0f * Mathf.Deg2Rad;

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

        [Header("Page UI References")]
        [SerializeField] private Canvas _leftPage;
        [SerializeField] private Canvas _leftFoldingPageLeft;
        [SerializeField] private Canvas _leftFoldingPageRight;
        [SerializeField] private Canvas _rightFoldingPageLeft;
        [SerializeField] private Canvas _rightFoldingPageRight;
        [SerializeField] private Canvas _rightPage;

        private PageController _leftPageController = null;
        private PageController _leftFoldingPageLeftController = null;
        private PageController _leftFoldingPageRightController = null;
        private PageController _rightFoldingPageLeftController = null;
        private PageController _rightFoldingPageRightController = null;
        private PageController _rightPageController = null;

        [Header("Pages")]
        [SerializeField] private GameObject _blankPage;
        [SerializeField] private GameObject[] _pages;

        [Header("Animation Settings")]
        [SerializeField, Range(0.0f, 720.0f)] private float _autoTurnAngularSpeed;

        public bool CanBeginPageTurn => _dragStatus == DragStatus.NoDrag;

        private DragStatus _dragStatus = DragStatus.NoDrag;
        private Vector3 _dragStartPoint;
        private bool _didDragFromRight;
        private float _dragProgress;
        private bool _committed;

        private int _leftIndex = 0;

        private void OnEnable()
        {
            // Initialise page visuals
            UpdateVisuals();
        }

        private void Update()
        {
            if (!_committed) return;

            bool sameDirectionCommit = (_didDragFromRight && _dragStatus == DragStatus.DraggingFromRight)
                                    || (!_didDragFromRight && _dragStatus == DragStatus.DraggingFromLeft);
            _dragProgress += (sameDirectionCommit ? 1.0f : -1.0f) * _autoTurnAngularSpeed / 360.0f * Time.deltaTime; // This isn't exactly angular velocity, but i can't be bothered to figure this out before sprint end
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
                minAngle = MaxAngleRight - MoveAngleThreshold;
                maxAngle = MaxAngleLeft + MoveAngleThreshold;
            }
            else
            {
                minAngle = MaxAngleLeft + MoveAngleThreshold;
                maxAngle = MaxAngleRight - MoveAngleThreshold;
            }

            float angleFromInitial = Mathf.Lerp(0.0f, maxAngle - minAngle, Mathf.Clamp01(_dragProgress));

            if (_didDragFromRight) MoveRightPage(angleFromInitial);
            else MoveLeftPage(angleFromInitial);
        }

        public bool StartDrag(Vector3 from)
        {
            // Don't start dragging until page has completely flipped
            if (_dragStatus != DragStatus.NoDrag) return false;

            if (IsOnRightSide(from))
            {
                if (_leftIndex >= LastLeftPageIndex) return false;

                _dragStatus = DragStatus.DraggingFromRight;
                _didDragFromRight = true;
            }
            else
            {
                if (_leftIndex <= 0) return false;

                _dragStatus = DragStatus.DraggingFromLeft;
                _didDragFromRight = false;
            }

            _dragStartPoint = from;
            UpdateDrag(from);

            return true;
        }
        public void EndDrag(Vector3 to)
        {
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
            ClearUIs();

            _leftPageController = Instantiate(Page(_leftIndex - 2), _leftPage.transform).GetComponent<PageController>();
            _leftFoldingPageLeftController = Instantiate(Page(_leftIndex - 1), _leftFoldingPageLeft.transform).GetComponent<PageController>();
            _leftFoldingPageRightController = Instantiate(Page(_leftIndex), _leftFoldingPageRight.transform).GetComponent<PageController>();
            _rightFoldingPageLeftController = Instantiate(Page(_leftIndex + 1), _rightFoldingPageLeft.transform).GetComponent<PageController>();
            _rightFoldingPageRightController = Instantiate(Page(_leftIndex + 2), _rightFoldingPageRight.transform).GetComponent<PageController>();
            _rightPageController = Instantiate(Page(_leftIndex + 3), _rightPage.transform).GetComponent<PageController>();

            _leftPageController.Initialise();
            _leftFoldingPageLeftController.Initialise();
            _leftFoldingPageRightController.Initialise();
            _rightFoldingPageLeftController.Initialise();
            _rightFoldingPageRightController.Initialise();
            _rightPageController.Initialise();
        }
        private void ClearUIs()
        {
            if (_leftPageController is not null)
            {
                Destroy(_leftPageController.gameObject);
                _leftPageController = null;
            }
            if (_leftFoldingPageLeftController is not null)
            {
                Destroy(_leftFoldingPageLeftController.gameObject);
                _leftFoldingPageLeftController = null;
            }
            if (_leftFoldingPageRightController is not null)
            {
                Destroy(_leftFoldingPageRightController.gameObject);
                _leftFoldingPageRightController = null;
            }
            if (_rightFoldingPageLeftController is not null)
            {
                Destroy(_rightFoldingPageLeftController.gameObject);
                _rightFoldingPageLeftController = null;
            }
            if (_rightFoldingPageRightController is not null)
            {
                Destroy(_rightFoldingPageRightController.gameObject);
                _rightFoldingPageRightController = null;
            }
            if (_rightPageController is not null)
            {
                Destroy(_rightPageController.gameObject);
                _rightPageController = null;
            }
        }

        private int LastLeftPageIndex => _pages.Length - (_pages.Length % 2);

        private bool IsOnRightSide(Vector3 point) => Vector3.Dot(point - _spine.position, BookRight) >= 0.0f;

        // 0.0f when point on at drag start point, 1.0f when point on left edge of book
        private float RightProgress(Vector3 point) => Mathf.Clamp01(Vector3.Dot(point - _dragStartPoint, BookLeft)
                                                                  / Vector3.Dot(_leftSideCorner.position - _dragStartPoint, BookLeft));
        // 0.0f when point on at drag start point, 1.0f when point on right edge of book
        private float LeftProgress(Vector3 point) => Mathf.Clamp01(Vector3.Dot(point - _dragStartPoint, BookRight)
                                                                 / Vector3.Dot(_rightSideCorner.position - _dragStartPoint, BookRight));

        private GameObject Page(int index)
        {
            if (_pages is null || index < 0 || index >= _pages.Length) return _blankPage;
            return _pages[index] ?? _blankPage;
        }
    }

    public enum DragStatus
    {
        NoDrag,
        DraggingFromLeft,
        DraggingFromRight,
    }
}