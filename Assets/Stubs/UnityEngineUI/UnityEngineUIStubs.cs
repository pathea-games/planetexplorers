// Stub implementations for UnityEngine.UI and UnityEngine.EventSystems namespaces.
// These are provided by Unity's UI module which isn't captured in a separate module DLL
// in the current unity-stubs set. These stubs exist solely so the Roslyn analyzer
// project compiles.
// THESE TYPES MUST NOT BE USED IN RUNTIME CODE.

using UnityEngine;

namespace UnityEngine.UI
{
    public class Graphic : UnityEngine.MonoBehaviour
    {
        public Color color { get; set; }
    }

    public class Image : Graphic
    {
        public Sprite sprite { get; set; }
        public enum Type { Simple, Sliced, Tiled, Filled }
        public Type type { get; set; }
        public bool fillCenter { get; set; }
        public float fillAmount { get; set; }
    }

    public class Text : Graphic
    {
        public string text { get; set; }
        public int fontSize { get; set; }
        public Font font { get; set; }
    }

    public class RawImage : Graphic
    {
        public Texture texture { get; set; }
        public UnityEngine.Rect uvRect { get; set; }
    }

    public class Button : UnityEngine.MonoBehaviour
    {
        public ButtonClickedEvent onClick { get; set; }

        public class ButtonClickedEvent : UnityEngine.Events.UnityEvent { }
    }

    public class Toggle : UnityEngine.MonoBehaviour
    {
        public bool isOn { get; set; }
        public ToggleEvent onValueChanged { get; set; }

        public class ToggleEvent : UnityEngine.Events.UnityEvent<bool> { }
    }

    public class Slider : UnityEngine.MonoBehaviour
    {
        public float value { get; set; }
        public float minValue { get; set; }
        public float maxValue { get; set; }
        public SliderEvent onValueChanged { get; set; }

        public class SliderEvent : UnityEngine.Events.UnityEvent<float> { }
    }

    public class Scrollbar : UnityEngine.MonoBehaviour
    {
        public float value { get; set; }
        public ScrollEvent onValueChanged { get; set; }

        public class ScrollEvent : UnityEngine.Events.UnityEvent<float> { }
    }

    public class ScrollRect : UnityEngine.MonoBehaviour
    {
        public UnityEngine.RectTransform content { get; set; }
        public bool horizontal { get; set; }
        public bool vertical { get; set; }
    }

    public class InputField : UnityEngine.MonoBehaviour
    {
        public string text { get; set; }
        public OnChangeEvent onValueChanged { get; set; }
        public SubmitEvent onEndEdit { get; set; }

        public class OnChangeEvent : UnityEngine.Events.UnityEvent<string> { }
        public class SubmitEvent : UnityEngine.Events.UnityEvent<string> { }
    }

    public class Dropdown : UnityEngine.MonoBehaviour
    {
        public int value { get; set; }
        public DropdownEvent onValueChanged { get; set; }

        public class DropdownEvent : UnityEngine.Events.UnityEvent<int> { }
    }

    // Note: CanvasGroup is defined in UnityEngine module DLLs — do not redefine here.

    public class LayoutElement : UnityEngine.MonoBehaviour
    {
        public float preferredWidth { get; set; }
        public float preferredHeight { get; set; }
        public float minWidth { get; set; }
        public float minHeight { get; set; }
    }

    public class ContentSizeFitter : UnityEngine.MonoBehaviour
    {
        public enum FitMode { Unconstrained, MinSize, PreferredSize }
        public FitMode horizontalFit { get; set; }
        public FitMode verticalFit { get; set; }
    }
}

namespace UnityEngine
{
    // Network: legacy Unity networking API (removed in Unity 5.4).
    // Referenced by UILobbyMainWndCtrl.cs via UnityEngine.Network.HavePublicAddress().
    public static class Network
    {
        public static bool HavePublicAddress() => false;
    }
}

namespace UnityEngine.EventSystems
{
    public interface IPointerEnterHandler { void OnPointerEnter(PointerEventData eventData); }
    public interface IPointerExitHandler { void OnPointerExit(PointerEventData eventData); }
    public interface IPointerDownHandler { void OnPointerDown(PointerEventData eventData); }
    public interface IPointerUpHandler { void OnPointerUp(PointerEventData eventData); }
    public interface IPointerClickHandler { void OnPointerClick(PointerEventData eventData); }
    public interface IDragHandler { void OnDrag(PointerEventData eventData); }
    public interface IBeginDragHandler { void OnBeginDrag(PointerEventData eventData); }
    public interface IEndDragHandler { void OnEndDrag(PointerEventData eventData); }
    public interface IScrollHandler { void OnScroll(PointerEventData eventData); }

    public class PointerEventData : BaseEventData
    {
        public PointerEventData(EventSystem eventSystem) : base(eventSystem) { }
        public UnityEngine.Vector2 position { get; set; }
        public UnityEngine.Vector2 delta { get; set; }
        public UnityEngine.GameObject pointerEnter { get; set; }
        public UnityEngine.GameObject pointerPress { get; set; }
    }

    public class BaseEventData
    {
        public BaseEventData(EventSystem eventSystem) { }
        public bool used { get; set; }
        public void Use() { }
    }

    public class EventSystem : UnityEngine.MonoBehaviour
    {
        public static EventSystem current => null;
        public bool IsPointerOverGameObject() => false;
        public bool IsPointerOverGameObject(int pointerId) => false;
    }

    public class UIBehaviour : UnityEngine.MonoBehaviour { }

    public static class ExecuteEvents
    {
        public delegate void EventFunction<T1>(T1 handler, BaseEventData eventData);
        public static void Execute<T>(UnityEngine.GameObject target, BaseEventData eventData, EventFunction<T> functor) where T : IEventSystemHandler { }
        public static EventFunction<IPointerClickHandler> pointerClickHandler => null;
    }

    public interface IEventSystemHandler { }
}
