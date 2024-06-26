using UnityEngine;
using UnityEngine.Events;

namespace OneHamsa.Dexterity.Builtins
{
	public class DragAndDropListener : MonoBehaviour
	{
		public RaycastListener raycastListener;
		public float maxClickDuration = 0;

		private IRaycastController _controller;
		private bool _pressed;
		private float _pressDuration;

		public IRaycastController draggingRaycastController => _controller;

		public bool isActuallyDragged => _pressed && _pressDuration > maxClickDuration;

		public UnityEvent<IRaycastController> onDragStart;
		public UnityEvent<Ray> onDrag;
		public UnityEvent onDragEnd;
		public UnityEvent onDragCancel;
		public UnityEvent onClick;

		private void FindRaycastListener()
		{
			if (raycastListener != null) return;

			raycastListener = GetComponentInChildren<RaycastListener>();
			if (raycastListener != null) return;

			raycastListener = GetComponentInParent<RaycastListener>();
			if (raycastListener != null) return;

			Debug.LogError("Unable to find RaycastListener");
			enabled = false;
		}

		public void StartDrag(IRaycastController controller) {
			raycastListener.SetPressing(controller);
		}

		public void EndDrag() {
			raycastListener.SetPressing(null);
		}

		private void OnPress()
		{
			_pressed = true;
			_pressDuration = 0;
			_controller = raycastListener.pressingController;
			onDragStart?.Invoke(raycastListener.pressingController);
			InvokeOnDrag();
		}

		private void OnRelease()
		{
			if (_pressDuration < maxClickDuration)
			{
				onDragCancel?.Invoke();
				onClick?.Invoke();
			}
			else
			{
				onDragEnd?.Invoke();
			}

			_pressed = false;
			_controller = null;
		}

		private void InvokeOnDrag()
		{
			Ray ray = new Ray(_controller.position, _controller.forward);
			onDrag?.Invoke(ray);
		}

		private void Awake()
		{
			FindRaycastListener();
		}

		private void OnEnable()
		{
			if (raycastListener == null) return;
			raycastListener.onPress += OnPress;
			raycastListener.onRelease += OnRelease;
		}

		private void OnDisable()
		{
			if (raycastListener == null) return;
			raycastListener.onPress -= OnPress;
			raycastListener.onRelease -= OnRelease;

			if (_pressed) {
				_pressed = false;
				_controller = null;
				onDragCancel?.Invoke();
			}
		}

		private void Update()
		{
			if (_pressed == false) return;

			_pressDuration += Time.unscaledDeltaTime;
			InvokeOnDrag();
		}
	}
}
