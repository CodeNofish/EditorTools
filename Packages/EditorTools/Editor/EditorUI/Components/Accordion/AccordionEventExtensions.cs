using UnityEngine.UIElements;

namespace EditorUI {

  /// <summary>
  /// Accordion 事件的注册快捷方法<br/>
  /// 用法与 Unity 内置控件的 RegisterValueChangedCallback 保持一致
  /// </summary>
  public static class AccordionEventExtensions {
    /// <summary>
    /// 注册展开集合变化事件
    /// </summary>
    public static void RegisterExpandedChangedCallback(
      this VisualElement element,
      EventCallback<AccordionExpandedChangedEvent> callback,
      TrickleDown trickleDown = TrickleDown.NoTrickleDown) {
      element.RegisterCallback(callback, trickleDown);
    }

    /// <summary>
    /// 注销展开集合变化事件
    /// </summary>
    public static void UnregisterExpandedChangedCallback(
      this VisualElement element,
      EventCallback<AccordionExpandedChangedEvent> callback,
      TrickleDown trickleDown = TrickleDown.NoTrickleDown) {
      element.UnregisterCallback(callback, trickleDown);
    }

    /// <summary>
    /// 注册单个项展开状态变化事件
    /// </summary>
    public static void RegisterItemExpandedChangedCallback(
      this VisualElement element,
      EventCallback<AccordionItemExpandedChangedEvent> callback,
      TrickleDown trickleDown = TrickleDown.NoTrickleDown) {
      element.RegisterCallback(callback, trickleDown);
    }

    /// <summary>
    /// 注销单个项展开状态变化事件
    /// </summary>
    public static void UnregisterItemExpandedChangedCallback(
      this VisualElement element,
      EventCallback<AccordionItemExpandedChangedEvent> callback,
      TrickleDown trickleDown = TrickleDown.NoTrickleDown) {
      element.UnregisterCallback(callback, trickleDown);
    }

    /// <summary>
    /// 注册 trigger 激活事件
    /// </summary>
    public static void RegisterTriggerPressedCallback(
      this VisualElement element,
      EventCallback<AccordionTriggerPressedEvent> callback,
      TrickleDown trickleDown = TrickleDown.NoTrickleDown) {
      element.RegisterCallback(callback, trickleDown);
    }

    /// <summary>
    /// 注销 trigger 激活事件
    /// </summary>
    public static void UnregisterTriggerPressedCallback(
      this VisualElement element,
      EventCallback<AccordionTriggerPressedEvent> callback,
      TrickleDown trickleDown = TrickleDown.NoTrickleDown) {
      element.UnregisterCallback(callback, trickleDown);
    }
  }

}
