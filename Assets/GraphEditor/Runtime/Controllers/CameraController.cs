using System.Collections;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.EventSystems;

namespace GraphEditor.Runtime
{
    public class CameraController : MonoBehaviour
    {
        [Header("Drag options")] 
        [SerializeField] private float speedDampening = 15f;
        [SerializeField] private Vector2 leftBottomBorderCorner;
        [SerializeField] private Vector2 rightTopBorderCorner;

        [Header("Zoom options")] 
        [SerializeField] private float zoomDampening = 6f;
        [SerializeField] private float zoomStepSize = 2f;
        [SerializeField] private float minHeight = 3f;
        [SerializeField] private float maxHeight = 16f;

        [Header("Paddings")] 
        [SerializeField] private float paddingsTop;
        [SerializeField] private float paddingsRight;
        [SerializeField] private float paddingsBottom;
        [SerializeField] private float paddingsLeft;

        [Header("Move")] 
        [SerializeField] private float timeToMove;
        [SerializeField] private float heightPadding = 1f;

        private Canvas canvas;
        private Transform cameraTransform;
        
        private Vector2 horizontalVelocity;
        private Vector2 pivotPoint;
        private Vector3 lastPosition;
        private bool isDragging;
        private bool isMoving;

        private float zoomValue;

        public static Camera MainCamera => Camera.main;

        public void Initialize()
        {
            if (MainCamera != null)
                cameraTransform = MainCamera.GetComponent<Transform>();

            canvas = GameObject.Find("Canvas").GetComponent<Canvas>();
            MainCamera.orthographicSize = Mathf.Clamp(MainCamera.orthographicSize, minHeight, maxHeight);
            zoomValue = MainCamera.orthographicSize;
        }

        private void Update()
        {
            if ((Input.GetMouseButtonDown(2) || Input.touches.Any(touch => touch.phase == TouchPhase.Began)) &&
                !PointerIsOverUI() && !PointerIsOverLayer("Graph") && !UI.Instance.IsWaitingMouseClick)
            {
                pivotPoint = MainCamera.ScreenToWorldPoint(Input.mousePosition);
                isDragging = true;
            }

            if (Input.GetMouseButton(2) && isDragging)
            {
                if (Input.touches.Any(touch => touch.phase == TouchPhase.Ended))
                    pivotPoint = MainCamera.ScreenToWorldPoint(GetMidpointBetweenTouches());
                else
                {
                    var currentPosition = MainCamera.ScreenToWorldPoint(Input.mousePosition);
                    DragCamera(currentPosition);
                }
            }
            else
                isDragging = false;

            MouseZoom();
            TouchZoom();

            UpdateOrthographicSize();
            UpdateVelocity();
        }

        public static bool PointerIsOverUI()
        {
            var results = new List<RaycastResult>();
            var pointerData = new PointerEventData(EventSystem.current) { position = Input.mousePosition };
            EventSystem.current.RaycastAll(pointerData, results);

            var hitObject = results.Count < 1 ? null : results[0].gameObject;

            return hitObject != null && hitObject.layer == LayerMask.NameToLayer("UI");
        }

        public static bool PointerIsOverAnyObject()
        {
            return Physics.Raycast(MainCamera.ScreenPointToRay(Input.mousePosition));
        }
        
        public static bool PointerIsOverLayer([CanBeNull]string layerName)
        {
            var ray = MainCamera.ScreenPointToRay(Input.mousePosition);
        
            if(Physics.Raycast(ray, out var hit) && layerName != null)
                return hit.transform.gameObject.layer == LayerMask.NameToLayer(layerName);

            return false;
        }
        
        public void MoveTo(Bounds bounds)
        {
            StopAllCoroutines();
            StartCoroutine(MoveToPoint(bounds));
        }
        
        private IEnumerator MoveToPoint(Bounds bounds)
        {
            var startPoint = cameraTransform.position;
            var targetPoint = new Vector3(bounds.center.x, bounds.center.y, startPoint.z);
            float completionPercentage = 0;
            
            var newOrthographicSize = Mathf.Max(bounds.size.x / (2 * MainCamera.aspect), bounds.size.y / 2) + heightPadding;
            
            zoomValue = Mathf.Clamp(newOrthographicSize, minHeight, maxHeight);

            while (completionPercentage < 1)
            {
                completionPercentage += Time.deltaTime / timeToMove;
                var easeOutQuart = 1 - Mathf.Pow(1 - completionPercentage, 4);
                cameraTransform.position = ClampCameraPosition(Vector3.Lerp(startPoint, targetPoint, easeOutQuart));
                yield return null;
            }
        }

        private Vector2 GetMidpointBetweenTouches()
        {
            var activeTouches =
                Input.touches.Where(touch => touch.phase != TouchPhase.Canceled && touch.phase != TouchPhase.Ended)
                    .Select(touch => touch.position).ToArray();
            return activeTouches.Aggregate((touch1, touch2) => touch1 + touch2) / activeTouches.Count();
        }

        private void DragCamera(Vector2 position)
        {
            var resultPosition = cameraTransform.position + (Vector3)(position - pivotPoint) * -1;
            cameraTransform.position = ClampCameraPosition(resultPosition);
        }

        private void UpdateVelocity()
        {
            if (isDragging)
            {
                var cameraPosition = cameraTransform.position;
                horizontalVelocity = (cameraPosition - lastPosition) / Time.deltaTime;
                lastPosition = cameraPosition;
            }
            else
            {
                horizontalVelocity = Vector3.Lerp(horizontalVelocity, Vector3.zero, speedDampening * Time.deltaTime);
                var resultPosition = cameraTransform.position + (Vector3)horizontalVelocity * Time.deltaTime;
                cameraTransform.position = ClampCameraPosition(resultPosition);
            }
        }

        private void MouseZoom()
        {
            var inputValue = Input.mouseScrollDelta.y;
            if (Mathf.Abs(inputValue) > 0f)
                zoomValue = MainCamera.orthographicSize - inputValue * zoomStepSize;
        }

        private void TouchZoom()
        {
            if (Input.touchCount > 1)
            {
                var touch0 = Input.GetTouch(0);
                var touch1 = Input.GetTouch(1);
                var previousMagnitude =
                    ((touch0.position - touch0.deltaPosition) - (touch1.position - touch1.deltaPosition)).magnitude;
                var currentMagnitude = (touch0.position - touch1.position).magnitude;

                var difference = currentMagnitude - previousMagnitude;

                zoomValue = MainCamera.orthographicSize - difference * 0.1f;
            }
        }

        private void UpdateOrthographicSize()
        {
            zoomValue = Mathf.Clamp(zoomValue, minHeight, maxHeight);

            if (Mathf.Abs(zoomValue - MainCamera.orthographicSize) < 0.01)
                return;

            MainCamera.orthographicSize = Mathf.Lerp(MainCamera.orthographicSize, zoomValue,
                zoomDampening * Time.deltaTime);
            cameraTransform.position = ClampCameraPosition(cameraTransform.position);
        }

        private Vector3 ClampCameraPosition(Vector3 position)
        {
            var maxPaddingCorner = new Vector2(FromScreenToWorld(paddingsRight), FromScreenToWorld(paddingsTop)) *
                                   canvas.scaleFactor;
            var minPaddingCorner = new Vector2(FromScreenToWorld(paddingsLeft), FromScreenToWorld(paddingsBottom)) *
                                   canvas.scaleFactor;

            var halfCameraSize = GetCameraSize() / 2;
            var maxLimitPoint = rightTopBorderCorner - halfCameraSize + maxPaddingCorner;
            var minLimitPoint = leftBottomBorderCorner + halfCameraSize - minPaddingCorner;

            return new Vector3(Mathf.Clamp(position.x, minLimitPoint.x, maxLimitPoint.x),
                Mathf.Clamp(position.y, minLimitPoint.y, maxLimitPoint.y),
                cameraTransform.position.z);
        }

        private Vector2 GetCameraSize()
        {
            var height = MainCamera.orthographicSize * 2;
            var width = height * MainCamera.aspect;
            return new Vector2(width, height);
        }

        private float FromScreenToWorld(float value)
        {
            var valueInWorld = MainCamera.ScreenToWorldPoint(new Vector3(0, value)).y;
            return -(cameraTransform.position.y - MainCamera.orthographicSize - valueInWorld);
        }
    }
}