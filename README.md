# Description
Experimental project that enables non-official runtime worldspace Unity's UIToolkit with XR interaction.

This project is born from [this reddit post](https://www.reddit.com/r/Unity3D/comments/qh4fe4/here_is_a_script_to_use_uitoolkit_in_runtime/) where you can find some detailed information on how to setup a new project with the *WorldSpaceUIDocument* script.

I also [posted some feedback](https://forum.unity.com/threads/you-didnt-account-for-some-use-cases-in-the-event-dispatcher-vr-ar.1187116/) on the official Unity's UIToolkit forums about my findings on the XR usecase for the package.

# Documentation

## *XRInteractionExample* scene
This scene just contains an example of how to setup the *XR Interaction Toolkit* package with a traditional worldspace canvas.

## *WorldSpace_UIToolkit* scene
This scene contains the experimental worldspace UIToolkit panel that can be interacted within VR.

The first thing to notice is the **WorldSpaceUIDocument** script, attached to the WorldSpace_UIToolkit_Panel0 and WorldSpace_UIToolkit_Panel1 game objects. This script takes advantage of the UIToolkit's feature that outputs to a render texture to create a worldspace panel, and also handles some required operations to make this panel work with the EventSystem. This script can be used alone in any non-XR project.

The second part of the experimental work are the modifications of some original scripts from the *XR Interaction Toolkit* package:
* The **XRUIInputModuleFix** component that must be added to the EventSystem object instead of the original **XRUIInputModule**. This fixes the dragging events that doesn't work with the original module.
* The **IUIInteractorRegisterer** component that must be added to any ray interactor object that you want to enable interactions with your UI panel. This works together with the **XRUIInputModuleFix** module.

# Conclusions
The current Unity's EventSystem for UI interactions (either with the traditional canvas or the new UIToolkit) looks too complex and convoluted for me. Also it is definitelly not well suited for new use cases like VR, where we can have multiple mouse-like pointers that can trigger hover events. The main pain point of the whole system is the fact that all the events are hardcoded to be projections from the screenspace pixels, which is definitelly not the case with some kinds of worldspace interactions.

 I hoped for the new UIToolkit project to refactor the old Unity's EventSystem but it is not the case right now. I hope that they refactor it some day and make some better documentation so it is actually feasible to make your own custom UI input modules.