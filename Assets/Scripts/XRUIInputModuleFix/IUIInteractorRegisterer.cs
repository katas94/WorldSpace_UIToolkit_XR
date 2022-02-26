using UnityEngine.EventSystems;

namespace UnityEngine.XR.Interaction.Toolkit.UI
{
    [RequireComponent(typeof(IUIInteractor))]
    public class IUIInteractorRegisterer : MonoBehaviour
    {
        void OnEnable ()
        {
            XRUIInputModuleFix module = EventSystem.current?.GetComponent<XRUIInputModuleFix>();

            if (module != null)
            {
                IUIInteractor[] interactors = GetComponents<IUIInteractor>();

                foreach (IUIInteractor interactor in interactors)
                    module.RegisterInteractor(interactor);
            }
        }

        void OnDisable ()
        {
            XRUIInputModuleFix module = EventSystem.current?.GetComponent<XRUIInputModuleFix>();

            if (module != null)
            {
                IUIInteractor[] interactors = GetComponents<IUIInteractor>();

                foreach (IUIInteractor interactor in interactors)
                    module.UnregisterInteractor(interactor);
            }
        }
    }
}