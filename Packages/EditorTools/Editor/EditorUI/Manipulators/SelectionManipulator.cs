using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace EditorUI.Manipulators {

  /// <summary>
  /// 选择操纵器。
  /// 附加到容器上，从冒泡的 <c>PointerDownEvent</c> 中解析可选项，
  /// 支持单选、Ctrl/Cmd 切换、Shift 连续选择，并通过 USS class 标记选中状态。
  /// </summary>
  public sealed class SelectionManipulator : Manipulator {
    private readonly List<VisualElement> _selection = new();
    private VisualElement? _anchor;

    /// <summary>
    /// 可选项的 USS class
    /// </summary>
    public string ItemClass { get; set; } = "editorui-selectable";

    /// <summary>
    /// 选中项附加的 USS class
    /// </summary>
    public string SelectedClass { get; set; } = "editorui-selected";

    /// <summary>
    /// 是否允许多选
    /// </summary>
    public bool AllowMultiple { get; set; } = true;

    /// <summary>
    /// 是否允许清空选择
    /// </summary>
    public bool AllowEmpty { get; set; } = true;

    /// <summary>
    /// 自定义解析：从事件目标得到可选项，返回 null 表示不参与选择
    /// </summary>
    public Func<VisualElement, VisualElement?>? ItemResolver { get; set; }

    /// <summary>
    /// 自定义索引解析，用于 Shift 连续选择；默认按容器的直接子级顺序
    /// </summary>
    public Func<VisualElement, int>? IndexResolver { get; set; }

    /// <summary>
    /// 当前选中项，按选择顺序排列
    /// </summary>
    public IReadOnlyList<VisualElement> SelectedItems => _selection;

    /// <summary>
    /// 选中集合发生变化时触发
    /// </summary>
    public event Action<IReadOnlyList<VisualElement>>? SelectionChanged;

    protected override void RegisterCallbacksOnTarget() {
      target.RegisterCallback<PointerDownEvent>(OnPointerDown);
    }

    protected override void UnregisterCallbacksFromTarget() {
      target.UnregisterCallback<PointerDownEvent>(OnPointerDown);
      RemoveSelectionClasses();
      _selection.Clear();
      _anchor = null;
    }

    /// <summary>
    /// 选中单个元素并清空原有选择
    /// </summary>
    public void Select(VisualElement? item) {
      if (item == null) {
        return;
      }

      SetSelection(new[] { item });
      _anchor = item;
    }

    /// <summary>
    /// 从选中集合中移除一个元素
    /// </summary>
    public void Deselect(VisualElement? item) {
      if (item == null || !_selection.Contains(item)) {
        return;
      }

      if (!AllowEmpty && _selection.Count == 1) {
        return;
      }

      _selection.Remove(item);
      item.RemoveFromClassList(SelectedClass);
      SelectionChanged?.Invoke(_selection);
    }

    /// <summary>
    /// 清空选择
    /// </summary>
    public void Clear() {
      SetSelection(Array.Empty<VisualElement>(), false);
      _anchor = null;
    }

    private void OnPointerDown(PointerDownEvent e) {
      if (e.button != (int)MouseButton.LeftMouse) {
        return;
      }

      VisualElement? item = ResolveItem(e.target as VisualElement);
      if (item == null) {
        return;
      }

      bool toggle = (e.modifiers & (EventModifiers.Control | EventModifiers.Command)) != 0;
      bool range = (e.modifiers & EventModifiers.Shift) != 0;
      if (AllowMultiple && toggle) {
        Toggle(item);
      } else if (AllowMultiple && range) {
        SelectRange(item);
      } else {
        Select(item);
      }
    }

    private void Toggle(VisualElement item) {
      if (_selection.Contains(item)) {
        if (!AllowEmpty && _selection.Count == 1) {
          return;
        }

        _selection.Remove(item);
        item.RemoveFromClassList(SelectedClass);
      } else {
        _selection.Add(item);
        item.AddToClassList(SelectedClass);
      }

      _anchor = item;
      SelectionChanged?.Invoke(_selection);
    }

    private void SelectRange(VisualElement item) {
      if (_anchor == null) {
        Select(item);
        return;
      }

      int start = GetIndex(_anchor);
      int end = GetIndex(item);
      if (start < 0 || end < 0) {
        Select(item);
        return;
      }

      int min = Math.Min(start, end);
      int max = Math.Max(start, end);
      var items = new List<VisualElement>();
      int index = 0;
      foreach (VisualElement child in target.Children()) {
        if (index >= min && index <= max && child.ClassListContains(ItemClass)) {
          items.Add(child);
        }

        index++;
      }

      SetSelection(items);
    }

    private void SetSelection(IEnumerable<VisualElement> items, bool enforceAllowEmpty = true) {
      RemoveSelectionClasses();
      _selection.Clear();
      foreach (VisualElement item in items) {
        if (item == null || _selection.Contains(item)) {
          continue;
        }

        _selection.Add(item);
        item.AddToClassList(SelectedClass);
      }

      if (enforceAllowEmpty && !AllowEmpty && _selection.Count == 0 && _anchor != null) {
        _selection.Add(_anchor);
        _anchor.AddToClassList(SelectedClass);
      }

      SelectionChanged?.Invoke(_selection);
    }

    private void RemoveSelectionClasses() {
      foreach (VisualElement item in _selection) {
        item.RemoveFromClassList(SelectedClass);
      }
    }

    private VisualElement? ResolveItem(VisualElement? start) {
      if (start == null) {
        return null;
      }

      if (ItemResolver != null) {
        return ItemResolver(start);
      }

      VisualElement? current = start;
      while (current != null && current != target) {
        if (current.ClassListContains(ItemClass)) {
          return current;
        }

        current = current.parent;
      }

      return current != null && current.ClassListContains(ItemClass) ? current : null;
    }

    private int GetIndex(VisualElement item) {
      if (IndexResolver != null) {
        return IndexResolver(item);
      }

      int index = 0;
      foreach (VisualElement child in target.Children()) {
        if (child == item) {
          return index;
        }

        index++;
      }

      return -1;
    }

  }

}
