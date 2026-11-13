using System;
using UnityEngine;
using UnityEngine.UIElements;

namespace EditorUI.Manipulators {

  /// <summary>
  /// 拖拽事件参数。
  /// 所有坐标均为面板坐标，不会随被拖拽元素的位置变化而改变。
  /// </summary>
  public readonly struct DragEventArgs {
    /// <summary>
    /// 按下指针时的位置
    /// </summary>
    public Vector2 StartPosition { get; }

    /// <summary>
    /// 当前指针位置
    /// </summary>
    public Vector2 Position { get; }

    /// <summary>
    /// 相对上一次拖拽更新的位移
    /// </summary>
    public Vector2 Delta { get; }

    /// <summary>
    /// 相对按下位置的累计位移
    /// </summary>
    public Vector2 TotalDelta { get; }

    /// <summary>
    /// 当前拖拽使用的指针 ID
    /// </summary>
    public int PointerId { get; }

    /// <summary>
    /// 事件触发时的修饰键
    /// </summary>
    public EventModifiers Modifiers { get; }

    public DragEventArgs(
      Vector2 startPosition,
      Vector2 position,
      Vector2 delta,
      Vector2 totalDelta,
      int pointerId,
      EventModifiers modifiers) {
      StartPosition = startPosition;
      Position = position;
      Delta = delta;
      TotalDelta = totalDelta;
      PointerId = pointerId;
      Modifiers = modifiers;
    }
  }

  /// <summary>
  /// 连续拖拽类操纵器的基类：统一处理激活器、拖拽阈值、指针捕获以及异常中断后的状态清理。
  /// 子类只需重写 <see cref="OnDragStarted"/>、<see cref="OnDragUpdated"/>、<see cref="OnDragEnded"/> 等虚方法。
  /// </summary>
  public abstract class PointerDragManipulator : PointerManipulator {
    private int _pointerId = -1;
    private bool _pressed;
    private bool _dragging;
    private Vector2 _startPosition;
    private Vector2 _lastPosition;

    /// <summary>
    /// 超过该距离（像素）后才视为拖拽，用于区分点击与拖拽；设为 0 表示按下即开始。
    /// </summary>
    public float DragThreshold { get; set; } = 4f;

    /// <summary>
    /// 当前是否正在拖拽
    /// </summary>
    public bool IsDragging => _dragging;

    /// <summary>
    /// 拖拽越过阈值时触发
    /// </summary>
    public event Action<DragEventArgs>? DragStarted;

    /// <summary>
    /// 拖拽过程中每次指针移动时触发
    /// </summary>
    public event Action<DragEventArgs>? DragUpdated;

    /// <summary>
    /// 指针正常抬起、拖拽结束时触发
    /// </summary>
    public event Action<DragEventArgs>? DragEnded;

    /// <summary>
    /// 指针捕获丢失、指针取消或元素脱离面板导致拖拽中断时触发
    /// </summary>
    public event Action<DragEventArgs>? DragCancelled;

    protected PointerDragManipulator(MouseButton button = MouseButton.LeftMouse) {
      AddActivator(button);
    }

    /// <summary>
    /// 追加一个激活器，例如让画布同时支持中键与左键平移
    /// </summary>
    protected void AddActivator(MouseButton button, EventModifiers modifiers = EventModifiers.None) {
      activators.Add(new ManipulatorActivationFilter { button = button, modifiers = modifiers });
    }

    protected override void RegisterCallbacksOnTarget() {
      target.RegisterCallback<PointerDownEvent>(OnPointerDown);
      target.RegisterCallback<PointerMoveEvent>(OnPointerMove);
      target.RegisterCallback<PointerUpEvent>(OnPointerUp);
      target.RegisterCallback<PointerCancelEvent>(OnPointerCancel);
      target.RegisterCallback<PointerCaptureOutEvent>(OnPointerCaptureOut);
      target.RegisterCallback<DetachFromPanelEvent>(OnDetachFromPanel);
    }

    protected override void UnregisterCallbacksFromTarget() {
      target.UnregisterCallback<PointerDownEvent>(OnPointerDown);
      target.UnregisterCallback<PointerMoveEvent>(OnPointerMove);
      target.UnregisterCallback<PointerUpEvent>(OnPointerUp);
      target.UnregisterCallback<PointerCancelEvent>(OnPointerCancel);
      target.UnregisterCallback<PointerCaptureOutEvent>(OnPointerCaptureOut);
      target.UnregisterCallback<DetachFromPanelEvent>(OnDetachFromPanel);
      ResetState();
    }

    /// <summary>
    /// 从外部中断当前拖拽（例如按下 Esc）
    /// </summary>
    public void CancelDrag() {
      if (!_pressed) {
        return;
      }

      FinishDrag(true, _lastPosition, EventModifiers.None, null);
    }

    /// <summary>
    /// 拖拽越过阈值时调用，可用于记录元素的初始位置或尺寸
    /// </summary>
    protected virtual void OnDragStarted(DragEventArgs args) { }

    /// <summary>
    /// 拖拽过程中调用，子类在此应用位移
    /// </summary>
    protected virtual void OnDragUpdated(DragEventArgs args) { }

    /// <summary>
    /// 拖拽正常结束时调用
    /// </summary>
    protected virtual void OnDragEnded(DragEventArgs args) { }

    /// <summary>
    /// 拖拽被中断时调用，可用于回滚
    /// </summary>
    protected virtual void OnDragCancelled(DragEventArgs args) { }

    private void OnPointerDown(PointerDownEvent e) {
      if (_pressed) {
        e.StopImmediatePropagation();
        return;
      }

      if (!CanStartManipulation(e)) {
        return;
      }

      _pressed = true;
      _dragging = false;
      _pointerId = e.pointerId;
      _startPosition = ToVector2(e.position);
      _lastPosition = _startPosition;
      target.CapturePointer(_pointerId);
    }

    private void OnPointerMove(PointerMoveEvent e) {
      if (!_pressed || e.pointerId != _pointerId || !target.HasPointerCapture(_pointerId)) {
        return;
      }

      Vector2 position = ToVector2(e.position);
      Vector2 total = position - _startPosition;
      if (!_dragging) {
        if (DragThreshold > 0f && total.sqrMagnitude < DragThreshold * DragThreshold) {
          return;
        }

        _dragging = true;
        var startArgs = new DragEventArgs(_startPosition, position, total, total, _pointerId, e.modifiers);
        OnDragStarted(startArgs);
        DragStarted?.Invoke(startArgs);
      }

      Vector2 delta = position - _lastPosition;
      var args = new DragEventArgs(_startPosition, position, delta, total, _pointerId, e.modifiers);
      OnDragUpdated(args);
      DragUpdated?.Invoke(args);
      _lastPosition = position;
      e.StopPropagation();
    }

    private void OnPointerUp(PointerUpEvent e) {
      if (!_pressed || e.pointerId != _pointerId || !CanStopManipulation(e)) {
        return;
      }

      FinishDrag(false, ToVector2(e.position), e.modifiers, e);
    }

    private void OnPointerCancel(PointerCancelEvent e) {
      if (_pressed && e.pointerId == _pointerId) {
        FinishDrag(true, _lastPosition, EventModifiers.None, null);
      }
    }

    private void OnPointerCaptureOut(PointerCaptureOutEvent e) {
      if (_pressed && e.pointerId == _pointerId) {
        FinishDrag(true, _lastPosition, EventModifiers.None, null);
      }
    }

    private void OnDetachFromPanel(DetachFromPanelEvent e) {
      FinishDrag(true, _lastPosition, EventModifiers.None, null);
    }

    private void FinishDrag(bool cancelled, Vector2 position, EventModifiers modifiers, EventBase? source) {
      bool wasDragging = _dragging;
      var args = new DragEventArgs(_startPosition, position, Vector2.zero, position - _startPosition,
        _pointerId, modifiers);
      ResetState();

      if (!wasDragging) {
        return;
      }

      if (cancelled) {
        OnDragCancelled(args);
        DragCancelled?.Invoke(args);
      } else {
        OnDragEnded(args);
        DragEnded?.Invoke(args);
        source?.StopPropagation();
      }
    }

    private void ResetState() {
      int pointerId = _pointerId;
      _pressed = false;
      _dragging = false;
      _pointerId = -1;

      if (pointerId >= 0 && target.HasPointerCapture(pointerId)) {
        target.ReleasePointer(pointerId);
      }
    }

    private static Vector2 ToVector2(Vector3 position) {
      return new Vector2(position.x, position.y);
    }
  }

}
