using System;
using UnityEngine;
using UnityEngine.UIElements;

namespace EditorUI.Manipulators {

  /// <summary>
  /// 可拖拽的轴向
  /// </summary>
  public enum DragAxis {
    Both,
    Horizontal,
    Vertical,
  }

  /// <summary>
  /// 基础拖拽操纵器。
  /// 默认通过 <c>style.left</c> / <c>style.top</c> 移动所附加的元素，
  /// 适用于 <c>position: absolute</c> 的元素；也可设置 <see cref="ApplyDrag"/> 自行处理位移。
  /// </summary>
  public sealed class Draggable : PointerDragManipulator {
    private Vector2 _startOffset;

    /// <summary>
    /// 限制拖拽方向
    /// </summary>
    public DragAxis Axis { get; set; } = DragAxis.Both;

    /// <summary>
    /// 大于0时按该间距吸附位置
    /// </summary>
    public float Snap { get; set; }

    /// <summary>
    /// 自定义位移处理；设置后不再修改元素的样式
    /// </summary>
    public Action<VisualElement, DragEventArgs>? ApplyDrag { get; set; }

    protected override void OnDragStarted(DragEventArgs args) {
      _startOffset = new Vector2(target.layout.x, target.layout.y);
    }

    protected override void OnDragUpdated(DragEventArgs args) {
      if (ApplyDrag != null) {
        ApplyDrag(target, args);
        return;
      }

      Vector2 total = args.TotalDelta;
      float x = Axis == DragAxis.Vertical ? 0f : total.x;
      float y = Axis == DragAxis.Horizontal ? 0f : total.y;
      float left = _startOffset.x + x;
      float top = _startOffset.y + y;

      if (Snap > 0f) {
        left = Mathf.Round(left / Snap) * Snap;
        top = Mathf.Round(top / Snap) * Snap;
      }

      target.style.left = left;
      target.style.top = top;
    }

  }

}
