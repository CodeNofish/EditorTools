using System;
using UnityEngine;
using UnityEngine.UIElements;

namespace EditorUI.Manipulators {

  /// <summary>
  /// 滚轮缩放操纵器。
  /// 默认对 <see cref="Content"/> 应用 <c>style.scale</c>，
  /// 并通过 <c>style.translate</c> 补偿位移，使指针下的内容点保持不动。
  /// 为让补偿公式成立，缩放原点会被设为内容元素的左上角。
  /// </summary>
  public sealed class CanvasZoomer : Manipulator {
    private Vector2 _translation;
    private bool _originApplied;

    /// <summary>
    /// 被缩放的内容元素；未设置时缩放操纵器所附加的元素
    /// </summary>
    public VisualElement? Content { get; set; }

    /// <summary>
    /// 当前缩放比例
    /// </summary>
    public float Zoom { get; private set; } = 1f;

    /// <summary>
    /// 最小缩放比例
    /// </summary>
    public float MinZoom { get; set; } = 0.1f;

    /// <summary>
    /// 最大缩放比例
    /// </summary>
    public float MaxZoom { get; set; } = 10f;

    /// <summary>
    /// 每次滚轮滚动的缩放步长（相对比例）
    /// </summary>
    public float ZoomStep { get; set; } = 0.1f;

    /// <summary>
    /// 是否以指针位置为缩放中心
    /// </summary>
    public bool AnchorToPointer { get; set; } = true;

    /// <summary>
    /// 缩放变化时触发，参数为新比例与相对target的指针位置
    /// </summary>
    public event Action<float, Vector2>? ZoomChanged;

    protected override void RegisterCallbacksOnTarget() {
      target.RegisterCallback<WheelEvent>(OnWheel);
    }

    protected override void UnregisterCallbacksFromTarget() {
      target.UnregisterCallback<WheelEvent>(OnWheel);
    }

    /// <summary>
    /// 直接设置缩放比例，不改动指针锚点补偿
    /// </summary>
    public void SetZoom(float value) {
      float next = Mathf.Clamp(value, MinZoom, MaxZoom);
      if (Mathf.Approximately(next, Zoom)) {
        return;
      }

      VisualElement content = GetContent();
      ApplyOrigin(content);
      content.style.scale = new Scale(new Vector3(next, next, 1f));
      Zoom = next;
      ZoomChanged?.Invoke(next, Vector2.zero);
    }

    /// <summary>
    /// 恢复为 1 倍缩放并清除平移补偿
    /// </summary>
    public void Reset() {
      Zoom = 1f;
      _translation = Vector2.zero;
      VisualElement content = GetContent();
      content.style.scale = new Scale(Vector3.one);
      content.style.translate = new Translate(0f, 0f);
    }

    private void OnWheel(WheelEvent e) {
      float direction = e.delta.y < 0f ? 1f : -1f;
      float next = Mathf.Clamp(Zoom * (1f + ZoomStep * direction), MinZoom, MaxZoom);
      if (Mathf.Approximately(next, Zoom)) {
        return;
      }

      VisualElement content = GetContent();
      ApplyOrigin(content);
      Vector2 pointer = ResolvePointer(e);
      float ratio = next / Zoom;

      if (AnchorToPointer) {
        Vector2 relative = pointer - content.layout.position;
        _translation = ratio * _translation + (1f - ratio) * relative;
        content.style.translate = new Translate(_translation.x, _translation.y);
      }

      content.style.scale = new Scale(new Vector3(next, next, 1f));
      Zoom = next;
      ZoomChanged?.Invoke(next, pointer);
      e.StopPropagation();
    }

    private void ApplyOrigin(VisualElement content) {
      if (_originApplied) {
        return;
      }

      content.style.transformOrigin = new TransformOrigin(
        new Length(0f, LengthUnit.Pixel), new Length(0f, LengthUnit.Pixel), 0f);
      _originApplied = true;
    }

    private Vector2 ResolvePointer(WheelEvent e) {
      VisualElement source = e.target as VisualElement ?? target;
      Vector2 local = e.localMousePosition;
      return source == target ? local : source.ChangeCoordinatesTo(target, local);
    }

    private VisualElement GetContent() {
      return Content ?? target;
    }

  }

}
