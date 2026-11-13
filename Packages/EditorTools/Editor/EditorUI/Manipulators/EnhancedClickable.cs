using System;
using UnityEngine;
using UnityEngine.UIElements;

namespace EditorUI.Manipulators {

  /// <summary>
  /// 点击事件参数，不持有 UI Toolkit 事件对象，可在事件分发结束后安全使用
  /// </summary>
  public readonly struct ClickEventArgs {
    /// <summary>
    /// 触发点击的鼠标按键
    /// </summary>
    public MouseButton Button { get; }

    /// <summary>
    /// 连击次数，大于等于 2 表示双击
    /// </summary>
    public int ClickCount { get; }

    /// <summary>
    /// 事件触发时的修饰键
    /// </summary>
    public EventModifiers Modifiers { get; }

    /// <summary>
    /// 面板坐标下的指针位置
    /// </summary>
    public Vector2 Position { get; }

    public ClickEventArgs(MouseButton button, int clickCount, EventModifiers modifiers, Vector2 position) {
      Button = button;
      ClickCount = clickCount;
      Modifiers = modifiers;
      Position = position;
    }
  }

  /// <summary>
  /// 单击、双击、长按、右键点击的统一入口。
  /// 内置 <c>Clickable</c> 只识别单击，该操纵器在同一套指针状态机中补齐其余交互。
  /// 它不捕获指针（捕获交给拖拽类操纵器），并用位移阈值区分点击与拖拽，便于多个操纵器组合使用。
  /// </summary>
  public sealed class EnhancedClickable : PointerManipulator {
    private int _pointerId = -1;
    private bool _pressed;
    private bool _longPressFired;
    private Vector2 _startPosition;

    /// <summary>
    /// 长按判定时间（毫秒）；设为负数可禁用长按
    /// </summary>
    public long LongPressDelayMs { get; set; } = 500L;

    /// <summary>
    /// 按下与抬起之间的最大位移（像素），超过后视为拖拽而非点击；设为 0 表示不限制
    /// </summary>
    public float ClickThreshold { get; set; } = 4f;

    /// <summary>
    /// 长按触发后是否抑制随后的单击
    /// </summary>
    public bool SuppressClickAfterLongPress { get; set; } = true;

    /// <summary>左键单击时触发
    /// </summary>
    public event Action<ClickEventArgs>? Clicked;

    /// <summary>
    /// 连击次数大于等于 2 时触发，不再触发 <see cref="Clicked"/>
    /// </summary>
    public event Action<ClickEventArgs>? DoubleClicked;

    /// <summary>
    /// 按住超过 <see cref="LongPressDelayMs"/> 时触发
    /// </summary>
    public event Action<ClickEventArgs>? LongPressed;

    /// <summary>
    /// 右键按下时触发；不会阻止内置上下文菜单操纵器工作
    /// </summary>
    public event Action<ClickEventArgs>? RightClicked;

    public EnhancedClickable() {
      activators.Add(new ManipulatorActivationFilter { button = MouseButton.LeftMouse });
    }

    protected override void RegisterCallbacksOnTarget() {
      target.RegisterCallback<PointerDownEvent>(OnPointerDown);
      target.RegisterCallback<PointerUpEvent>(OnPointerUp);
      target.RegisterCallback<PointerLeaveEvent>(OnPointerLeave);
      target.RegisterCallback<PointerCancelEvent>(OnPointerCancel);
      target.RegisterCallback<DetachFromPanelEvent>(OnDetachFromPanel);
    }

    protected override void UnregisterCallbacksFromTarget() {
      target.UnregisterCallback<PointerDownEvent>(OnPointerDown);
      target.UnregisterCallback<PointerUpEvent>(OnPointerUp);
      target.UnregisterCallback<PointerLeaveEvent>(OnPointerLeave);
      target.UnregisterCallback<PointerCancelEvent>(OnPointerCancel);
      target.UnregisterCallback<DetachFromPanelEvent>(OnDetachFromPanel);
      ResetState();
    }

    private void OnPointerDown(PointerDownEvent e) {
      if (e.button == (int)MouseButton.RightMouse) {
        RightClicked?.Invoke(CreateArgs(e));
        return;
      }

      if (_pressed) {
        e.StopImmediatePropagation();
        return;
      }

      if (!CanStartManipulation(e)) {
        return;
      }

      _pressed = true;
      _longPressFired = false;
      _pointerId = e.pointerId;
      _startPosition = ToVector2(e.position);
      ScheduleLongPress(CreateArgs(e));
    }

    private void OnPointerUp(PointerUpEvent e) {
      if (!_pressed || e.pointerId != _pointerId || !CanStopManipulation(e)) {
        return;
      }

      var args = CreateArgs(e);
      bool longPressFired = _longPressFired;
      Vector2 offset = ToVector2(e.position) - _startPosition;
      bool moved = ClickThreshold > 0f && offset.sqrMagnitude > ClickThreshold * ClickThreshold;
      ResetState();

      if (moved) {
        return;
      }

      if (args.ClickCount >= 2) {
        DoubleClicked?.Invoke(args);
      } else if (!longPressFired || !SuppressClickAfterLongPress) {
        Clicked?.Invoke(args);
      }

      e.StopPropagation();
    }

    private void OnPointerLeave(PointerLeaveEvent e) {
      if (_pressed && e.pointerId == _pointerId) {
        ResetState();
      }
    }

    private void OnPointerCancel(PointerCancelEvent e) {
      if (_pressed && e.pointerId == _pointerId) {
        ResetState();
      }
    }

    private void OnDetachFromPanel(DetachFromPanelEvent e) {
      ResetState();
    }

    private void ScheduleLongPress(ClickEventArgs args) {
      if (LongPressDelayMs < 0L) {
        return;
      }

      int pointerId = _pointerId;
      target.schedule.Execute(() => {
        if (!_pressed || _longPressFired || _pointerId != pointerId) {
          return;
        }

        _longPressFired = true;
        LongPressed?.Invoke(args);
      }).StartingIn(Math.Max(1L, LongPressDelayMs));
    }

    private void ResetState() {
      _pressed = false;
      _longPressFired = false;
      _pointerId = -1;
    }

    private static ClickEventArgs CreateArgs(PointerDownEvent e) {
      return CreateArgs(e.button, e.clickCount, e.modifiers, e.position);
    }

    private static ClickEventArgs CreateArgs(PointerUpEvent e) {
      return CreateArgs(e.button, e.clickCount, e.modifiers, e.position);
    }

    private static ClickEventArgs CreateArgs(
      int button,
      int clickCount,
      EventModifiers modifiers,
      Vector3 position) {
      return new ClickEventArgs((MouseButton)button, clickCount, modifiers, new Vector2(position.x, position.y));
    }

    private static Vector2 ToVector2(Vector3 position) {
      return new Vector2(position.x, position.y);
    }
  }

}
