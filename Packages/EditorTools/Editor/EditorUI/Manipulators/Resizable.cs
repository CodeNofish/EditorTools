using System;
using UnityEngine;
using UnityEngine.UIElements;

namespace EditorUI.Manipulators {

  /// <summary>
  /// 参与尺寸调整的边，可组合表示四角
  /// </summary>
  [Flags]
  public enum ResizeEdges {
    None = 0,
    Left = 1 << 0,
    Right = 1 << 1,
    Top = 1 << 2,
    Bottom = 1 << 3,
    BottomRight = Right | Bottom,
    BottomLeft = Left | Bottom,
    TopRight = Right | Top,
    TopLeft = Left | Top,
  }

  /// <summary>
  /// 尺寸调整操纵器。
  /// 把操纵器附加到作为调整手柄的元素上，拖拽时修改<see cref="Target"/> 的 <c>style.left/top/width/height</c>。
  /// </summary>
  public sealed class Resizable : PointerDragManipulator {
    private Rect _startRect;

    /// <summary>
    /// 要调整尺寸的元素；未设置时调整操纵器所附加的元素
    /// </summary>
    public VisualElement? Target { get; set; }

    /// <summary>
    /// 本次拖拽影响哪些边；水平或垂直方向上同时指定两边时，Left / Top 优先
    /// </summary>
    public ResizeEdges Edges { get; set; }

    /// <summary>
    /// 最小尺寸
    /// </summary>
    public Vector2 MinSize { get; set; } = new Vector2(16f, 16f);

    /// <summary>
    /// 最大尺寸，使用 <c>float.PositiveInfinity</c> 表示不限制
    /// </summary>
    public Vector2 MaxSize { get; set; } = new Vector2(float.PositiveInfinity, float.PositiveInfinity);

    /// <summary>
    /// 自定义尺寸处理；设置后不再修改元素的样式
    /// </summary>
    public Action<VisualElement, Rect, DragEventArgs>? ApplyResize { get; set; }

    public Resizable(ResizeEdges edges = ResizeEdges.BottomRight) {
      Edges = edges;
    }

    protected override void OnDragStarted(DragEventArgs args) {
      _startRect = GetTarget().layout;
    }

    protected override void OnDragUpdated(DragEventArgs args) {
      VisualElement element = GetTarget();
      float left = _startRect.x;
      float top = _startRect.y;
      float width = _startRect.width;
      float height = _startRect.height;
      float right = _startRect.xMax;
      float bottom = _startRect.yMax;

      if (Edges.HasFlag(ResizeEdges.Left)) {
        left = Mathf.Clamp(_startRect.x + args.TotalDelta.x, right - MaxSize.x, right - MinSize.x);
        width = right - left;
      } else if (Edges.HasFlag(ResizeEdges.Right)) {
        width = Mathf.Clamp(_startRect.width + args.TotalDelta.x, MinSize.x, MaxSize.x);
      }

      if (Edges.HasFlag(ResizeEdges.Top)) {
        top = Mathf.Clamp(_startRect.y + args.TotalDelta.y, bottom - MaxSize.y, bottom - MinSize.y);
        height = bottom - top;
      } else if (Edges.HasFlag(ResizeEdges.Bottom)) {
        height = Mathf.Clamp(_startRect.height + args.TotalDelta.y, MinSize.y, MaxSize.y);
      }

      var rect = new Rect(left, top, width, height);
      if (ApplyResize != null) {
        ApplyResize(element, rect, args);
        return;
      }

      element.style.left = rect.x;
      element.style.top = rect.y;
      element.style.width = rect.width;
      element.style.height = rect.height;
    }

    private VisualElement GetTarget() {
      return Target ?? target;
    }

  }

}
