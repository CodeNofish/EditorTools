using System;
using UnityEngine;
using UnityEngine.UIElements;

namespace EditorUI.Manipulators {

  /// <summary>
  /// 画布平移操纵器。
  /// 默认按住中键拖拽，可通过构造参数追加左键平移。
  /// 默认修改 <see cref="Content"/> 的 <c>style.left</c> / <c>style.top</c>；
  /// 平移量与拖拽阈值、指针捕获均由 <see cref="PointerDragManipulator"/> 统一管理。
  /// </summary>
  public sealed class CanvasPanner : PointerDragManipulator {
    private Vector2 _startOffset;

    /// <summary>
    /// 被平移的内容元素；未设置时平移操纵器所附加的元素
    /// </summary>
    public VisualElement? Content { get; set; }

    /// <summary>
    /// 自定义平移处理；设置后不再修改元素的样式
    /// </summary>
    public Action<VisualElement, DragEventArgs>? ApplyPan { get; set; }

    /// <param name="panWithLeftButton">是否同时允许左键拖拽平移。</param>
    public CanvasPanner(bool panWithLeftButton = false) : base(MouseButton.MiddleMouse) {
      if (panWithLeftButton) {
        AddActivator(MouseButton.LeftMouse);
      }
    }

    protected override void OnDragStarted(DragEventArgs args) {
      VisualElement content = GetContent();
      _startOffset = new Vector2(content.layout.x, content.layout.y);
    }

    protected override void OnDragUpdated(DragEventArgs args) {
      VisualElement content = GetContent();
      if (ApplyPan != null) {
        ApplyPan(content, args);
        return;
      }

      content.style.left = _startOffset.x + args.TotalDelta.x;
      content.style.top = _startOffset.y + args.TotalDelta.y;
    }

    private VisualElement GetContent() {
      return Content ?? target;
    }

  }

}
