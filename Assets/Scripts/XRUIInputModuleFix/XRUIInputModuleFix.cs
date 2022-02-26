using System;
using System.Collections.Generic;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;
using UnityEngine.UIElements;

namespace UnityEngine.XR.Interaction.Toolkit.UI
{
    /// <summary>
    /// Custom class for input modules that send UI input in XR. Adapted for UIToolkit runtime worldspace panels.
    /// </summary>
    public class XRUIInputModuleFix : UIInputModuleFix
    {
        struct RegisteredInteractor
        {
            public IUIInteractor interactor;
            public TrackedDeviceModel model;

            public RegisteredInteractor(IUIInteractor interactor, int deviceIndex)
            {
                this.interactor = interactor;
                model = new TrackedDeviceModel(deviceIndex);
            }
        }

        struct RegisteredTouch
        {
            public bool isValid;
            public int touchId;
            public TouchModel model;

            public RegisteredTouch(Touch touch, int deviceIndex)
            {
                touchId = touch.fingerId;
                model = new TouchModel(deviceIndex);
                isValid = true;
            }
        }

        [SerializeField, HideInInspector]
        [Tooltip("The maximum distance to raycast with tracked devices to find hit objects.")]
        float m_MaxTrackedDeviceRaycastDistance = 1000f;

        /// <summary>
        /// The maximum distance to raycast with tracked devices to find hit objects.
        /// </summary>
        [Obsolete("maxRaycastDistance has been deprecated. Its value was unused, calling this property is unnecessary and should be removed.")]
        public float maxRaycastDistance
        {
            get => m_MaxTrackedDeviceRaycastDistance;
            set => m_MaxTrackedDeviceRaycastDistance = value;
        }

        [SerializeField]
        [Tooltip("If true, will forward 3D tracked device data to UI elements.")]
        bool m_EnableXRInput = true;

        /// <summary>
        /// If <see langword="true"/>, will forward 3D tracked device data to UI elements.
        /// </summary>
        public bool enableXRInput
        {
            get => m_EnableXRInput;
            set => m_EnableXRInput = value;
        }

        [SerializeField]
        [Tooltip("If true, will forward 2D mouse data to UI elements.")]
        bool m_EnableMouseInput = true;

        /// <summary>
        /// If <see langword="true"/>, will forward 2D mouse data to UI elements.
        /// </summary>
        public bool enableMouseInput
        {
            get => m_EnableMouseInput;
            set => m_EnableMouseInput = value;
        }

        [SerializeField]
        [Tooltip("If true, will forward 2D touch data to UI elements.")]
        bool m_EnableTouchInput = true;

        [SerializeField]
        protected bool usePenPointerIdBase = false;

        /// <summary>
        /// If <see langword="true"/>, will forward 2D touch data to UI elements.
        /// </summary>
        public bool enableTouchInput
        {
            get => m_EnableTouchInput;
            set => m_EnableTouchInput = value;
        }

        MouseModel m_Mouse;

        readonly List<RegisteredTouch> m_RegisteredTouches = new List<RegisteredTouch>();

        int m_RollingInteractorIndex = PointerId.touchPointerIdBase;
        bool m_penBaseIdAssigned = false;

        readonly List<RegisteredInteractor> m_RegisteredInteractors = new List<RegisteredInteractor>();

        /// <inheritdoc />
        protected override void OnEnable()
        {
            base.OnEnable();
            m_Mouse = new MouseModel(0);
        }

        public override int ConvertUIToolkitPointerId(PointerEventData sourcePointerData)
        {
            return sourcePointerData.pointerId;
        }

        /// <summary>
        /// Register an <see cref="IUIInteractor"/> with the UI system.
        /// Calling this will enable it to start interacting with UI.
        /// </summary>
        /// <param name="interactor">The <see cref="IUIInteractor"/> to use.</param>
        public void RegisterInteractor(IUIInteractor interactor)
        {
            for (var i = 0; i < m_RegisteredInteractors.Count; i++)
            {
                if (m_RegisteredInteractors[i].interactor == interactor)
                    return;
            }

            if (usePenPointerIdBase && !m_penBaseIdAssigned)
            {
                m_penBaseIdAssigned = true;
                m_RegisteredInteractors.Add(new RegisteredInteractor(interactor, PointerId.penPointerIdBase));
            }
            else
            {
                if (usePenPointerIdBase && m_RollingInteractorIndex == PointerId.penPointerIdBase)
                    ++m_RollingInteractorIndex;
                
                m_RegisteredInteractors.Add(new RegisteredInteractor(interactor, m_RollingInteractorIndex++));
            }
        }

        /// <summary>
        /// Unregisters an <see cref="IUIInteractor"/> with the UI system.
        /// This cancels all UI Interaction and makes the <see cref="IUIInteractor"/> no longer able to affect UI.
        /// </summary>
        /// <param name="interactor">The <see cref="IUIInteractor"/> to stop using.</param>
        public void UnregisterInteractor(IUIInteractor interactor)
        {
            for (var i = 0; i < m_RegisteredInteractors.Count; i++)
            {
                if (m_RegisteredInteractors[i].interactor == interactor)
                {
                    var registeredInteractor = m_RegisteredInteractors[i];
                    registeredInteractor.interactor = null;
                    m_RegisteredInteractors[i] = registeredInteractor;
                    return;
                }
            }
        }

        /// <summary>
        /// Gets an <see cref="IUIInteractor"/> from its corresponding Unity UI Pointer Id.
        /// This can be used to identify individual Interactors from the underlying UI Events.
        /// </summary>
        /// <param name="pointerId">A unique integer representing an object that can point at UI.</param>
        /// <returns>Returns the interactor associated with <paramref name="pointerId"/>.
        /// Returns <see langword="null"/> if no Interactor is associated (e.g. if it's a mouse event).</returns>
        public IUIInteractor GetInteractor(int pointerId)
        {
            for (var i = 0; i < m_RegisteredInteractors.Count; i++)
            {
                if (m_RegisteredInteractors[i].model.pointerId == pointerId)
                {
                    return m_RegisteredInteractors[i].interactor;
                }
            }

            return null;
        }

        /// <summary>Retrieves the UI Model for a selected <see cref="IUIInteractor"/>.</summary>
        /// <param name="interactor">The <see cref="IUIInteractor"/> you want the model for.</param>
        /// <param name="model">The returned model that reflects the UI state of the <paramref name="interactor"/>.</param>
        /// <returns>Returns <see langword="true"/> if the model was able to retrieved. Otherwise, returns <see langword="false"/>.</returns>
        public bool GetTrackedDeviceModel(IUIInteractor interactor, out TrackedDeviceModel model)
        {
            for (var i = 0; i < m_RegisteredInteractors.Count; i++)
            {
                if (m_RegisteredInteractors[i].interactor == interactor)
                {
                    model = m_RegisteredInteractors[i].model;
                    return true;
                }
            }

            model = new TrackedDeviceModel(-1);
            return false;
        }

        /// <inheritdoc />
        protected override void DoProcess()
        {
            base.DoProcess();

            if (m_EnableXRInput)
            {
                for (var i = 0; i < m_RegisteredInteractors.Count; i++)
                {
                    var registeredInteractor = m_RegisteredInteractors[i];

                    // If device is removed, we send a default state to unclick any UI
                    if (registeredInteractor.interactor == null)
                    {
                        registeredInteractor.model.Reset(false);
                        ProcessTrackedDevice(ref registeredInteractor.model, true);
                        m_RegisteredInteractors.RemoveAt(i--);
                    }
                    else
                    {
                        registeredInteractor.interactor.UpdateUIModel(ref registeredInteractor.model);
                        ProcessTrackedDevice(ref registeredInteractor.model);
                        m_RegisteredInteractors[i] = registeredInteractor;
                    }
                }
            }

            if (m_EnableMouseInput)
                ProcessMouse();

            if (m_EnableTouchInput)
                ProcessTouches();
        }

        void ProcessMouse()
        {
            if (Mouse.current != null)
            {
                // The Input System reports scroll in pixels, whereas the old Input class reported in lines.
                // Example, scrolling down by one notch of a mouse wheel for Input would be (0, -1),
                // but would be (0, -120) from Input System.
                // For consistency between the two Active Input Handling modes and with StandaloneInputModule,
                // scale the scroll value to the range expected by UI.
                const float kPixelsPerLine = 120f;
                m_Mouse.position = Mouse.current.position.ReadValue();
                m_Mouse.scrollDelta = Mouse.current.scroll.ReadValue() * (1 / kPixelsPerLine);
                m_Mouse.leftButtonPressed = Mouse.current.leftButton.isPressed;
                m_Mouse.rightButtonPressed = Mouse.current.rightButton.isPressed;
                m_Mouse.middleButtonPressed = Mouse.current.middleButton.isPressed;

                ProcessMouse(ref m_Mouse);
            }
            else if (Input.mousePresent)
            {
                m_Mouse.position = Input.mousePosition;
                m_Mouse.scrollDelta = Input.mouseScrollDelta;
                m_Mouse.leftButtonPressed = Input.GetMouseButton(0);
                m_Mouse.rightButtonPressed = Input.GetMouseButton(1);
                m_Mouse.middleButtonPressed = Input.GetMouseButton(2);

                ProcessMouse(ref m_Mouse);
            }
        }

        void ProcessTouches()
        {
            if (Input.touchCount > 0)
            {
                var touches = Input.touches;
                foreach (var touch in touches)
                {
                    var registeredTouchIndex = -1;

                    // Find if touch already exists
                    for (var j = 0; j < m_RegisteredTouches.Count; j++)
                    {
                        if (touch.fingerId == m_RegisteredTouches[j].touchId)
                        {
                            registeredTouchIndex = j;
                            break;
                        }
                    }

                    if (registeredTouchIndex < 0)
                    {
                        // Not found, search empty pool
                        for (var j = 0; j < m_RegisteredTouches.Count; j++)
                        {
                            if (!m_RegisteredTouches[j].isValid)
                            {
                                // Re-use the Id
                                var pointerId = m_RegisteredTouches[j].model.pointerId;
                                m_RegisteredTouches[j] = new RegisteredTouch(touch, pointerId);
                                registeredTouchIndex = j;
                                break;
                            }
                        }

                        if (registeredTouchIndex < 0)
                        {
                            // No Empty slots, add one
                            registeredTouchIndex = m_RegisteredTouches.Count;
                            m_RegisteredTouches.Add(new RegisteredTouch(touch, m_RollingInteractorIndex++));
                        }
                    }

                    var registeredTouch = m_RegisteredTouches[registeredTouchIndex];
                    registeredTouch.model.selectPhase = touch.phase;
                    registeredTouch.model.position = touch.position;
                    m_RegisteredTouches[registeredTouchIndex] = registeredTouch;
                }

                for (var i = 0; i < m_RegisteredTouches.Count; i++)
                {
                    var registeredTouch = m_RegisteredTouches[i];
                    ProcessTouch(ref registeredTouch.model);
                    if (registeredTouch.model.selectPhase == TouchPhase.Ended || registeredTouch.model.selectPhase == TouchPhase.Canceled)
                        registeredTouch.isValid = false;
                    m_RegisteredTouches[i] = registeredTouch;
                }
            }
        }
    }
}
