using System;
using UnityEngine;
using UnityEngine.UIElements;

namespace EditorUI.Manipulators {

  /// <summary>
  /// 悬停提示操纵器。
  /// 延迟在父容器中插入一个绝对定位的浮层，
  /// 元素没有父级时退化为内置 <c>VisualElement.tooltip</c>。
  /// </summary>
  public sealed class TooltipManipulator : Manipulator {
    private Label? _tooltip;
    private bool _hovering;
    private bool _usingBuiltinTooltip;

    /// <summary>
    /// 提示文本；<see cref="TextProvider"/> 优先
    /// </summary>
    public string Text { get; set; } = string.Empty;

    /// <summary>
    /// 动态提示文本，适合随状态变化的场景
    /// </summary>
    public Func<string>? TextProvider { get; set; }

    /// <summary>
    /// 悬停多久后显示（毫秒）
    /// </summary>
    public long DelayMs { get; set; } = 400L;

    /// <summary>
    /// 浮层的 USS class，可用于自定义样式
    /// </summary>
    public string TooltipClass { get; set; } = "editorui-tooltip";

    /// <summary>
    /// 浮层相对元素左下角的偏移
    /// </summary>
    public Vector2 Offset { get; set; } = new(0f, 8f);

    protected override void RegisterCallbacksOnTarget() {
      target.RegisterCallback<PointerEnterEvent>(OnPointerEnter);
      target.RegisterCallback<PointerLeaveEvent>(OnPointerLeave);
      target.RegisterCallback<DetachFromPanelEvent>(OnDetachFromPanel);
    }

    protected override void UnregisterCallbacksFromTarget() {
      target.UnregisterCallback<PointerEnterEvent>(OnPointerEnter);
      target.UnregisterCallback<PointerLeaveEvent>(OnPointerLeave);
      target.UnregisterCallback<DetachFromPanelEvent>(OnDetachFromPanel);
      HideTooltip();
    }

    /// <summary>
    /// 立即隐藏当前提示
    /// </summary>
    public void Hide() {
      HideTooltip();
    }

    private void OnPointerEnter(PointerEnterEvent e) {
      if (_hovering) {
        return;
      }

      _hovering = true;
      target.schedule.Execute(() => {
        if (_hovering) {
          ShowTooltip();
        }
      }).StartingIn(Math.Max(1L, DelayMs));
    }

    private void OnPointerLeave(PointerLeaveEvent e) {
      HideTooltip();
    }

    private void OnDetachFromPanel(DetachFromPanelEvent e) {
      HideTooltip();
    }

    private void ShowTooltip() {
      if (_tooltip != null || _usingBuiltinTooltip) {
        return;
      }

      string text = TextProvider != null ? TextProvider() : Text;
      if (string.IsNullOrEmpty(text)) {
        return;
      }

      VisualElement? host = target.parent;
      if (host == null) {
        target.tooltip = text;
        _usingBuiltinTooltip = true;
        return;
      }

      var label = new Label(text);
      label.AddToClassList(TooltipClass);
      label.pickingMode = PickingMode.Ignore;
      label.style.position = Position.Absolute;
      label.style.backgroundColor = new Color(0.13f, 0.13f, 0.13f, 0.95f);
      label.style.color = Color.white;
      label.style.paddingLeft = 6f;
      label.style.paddingRight = 6f;
      label.style.paddingTop = 3f;
      label.style.paddingBottom = 3f;
      label.style.maxWidth = 320f;
      label.style.whiteSpace = WhiteSpace.Normal;

      Vector2 origin = target.worldBound.position - host.worldBound.position;
      Vector2 position = origin + new Vector2(0f, target.layout.height) + Offset;
      label.style.left = position.x;
      label.style.top = position.y;
      host.Add(label);
      _tooltip = label;
    }

    private void HideTooltip() {
      _hovering = false;
      if (_tooltip != null) {
        _tooltip.RemoveFromHierarchy();
        _tooltip = null;
      }

      if (_usingBuiltinTooltip) {
        target.tooltip = string.Empty;
        _usingBuiltinTooltip = false;
      }
    }

  }

}
