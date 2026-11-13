using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace EditorUI.Components {

  /// <summary>
  /// 手风琴容器：管理一组 <see cref="AccordionItem"/> 的展开状态、禁用状态与键盘导航<br/>
  /// 对应 HeroUI 的 Accordion（allowsMultipleExpanded / defaultExpandedKeys / onExpandedChange 等接口）
  /// </summary>
  /// <remarks>
  /// UXML 用法：<br/>
  /// <code>
  /// &lt;ui:UXML xmlns:ui="UnityEngine.UIElements" xmlns:EditorUI="EditorUI.Components"&gt;
  ///   &lt;EditorUI:Accordion allows-multiple-expanded="true" variant="Surface"&gt;
  ///     &lt;EditorUI:AccordionItem key="one" title="Section One" subtitle="Description"&gt;
  ///       &lt;ui:Label text="Content" /&gt;
  ///     &lt;/EditorUI:AccordionItem&gt;
  ///   &lt;/EditorUI:Accordion&gt;
  /// &lt;/ui:UXML&gt;
  /// </code>
  /// </remarks>
  [UxmlElement(nameof(Accordion), libraryPath = Const.LibraryPath)]
  public partial class Accordion : VisualElement {
    private readonly List<AccordionItem> _items = new();
    private readonly List<string> _expandedKeys = new();
    private readonly HashSet<string> _defaultExpandedKeys = new();
    private readonly HashSet<string> _disabledKeys = new();
    private bool _allowMultipleExpanded;
    private bool _isDisabled;
    private AccordionVariant _variant;
    private bool _hideSeparator;
    private bool _compact;
    private bool _hideIndicator;
    private bool _disableAnimation;
    private int _nextItemId;

    /// <summary>
    /// 替换默认组件样式表，为 null 时从包内 Resources 加载
    /// </summary>
    public static StyleSheet? StyleSheetOverride { get; set; }

    /// <summary>
    /// 替换默认变量表，为 null 时从包内 Resources 加载<br/>
    /// 只覆写 <c>--editorui-accordion-*</c> 变量即可完成定制，无需改动组件 USS
    /// </summary>
    public static StyleSheet? VariablesStyleSheetOverride { get; set; }

    /// <summary>
    /// 当前展开的 key 集合
    /// </summary>
    public IReadOnlyList<string> ExpandedKeys => _expandedKeys;

    /// <summary>
    /// 当前注册的子项，按视觉树顺序排列
    /// </summary>
    public IReadOnlyList<AccordionItem> Items => _items;

    /// <summary>
    /// 是否允许同时展开多个项
    /// </summary>
    [Tooltip("是否允许同时展开多个项")]
    [UxmlAttribute("allows-multiple-expanded")]
    public bool AllowMultipleExpanded {
      get => _allowMultipleExpanded;
      set {
        if (_allowMultipleExpanded == value) {
          return;
        }

        _allowMultipleExpanded = value;
        if (!value && _expandedKeys.Count > 1) {
          string first = _expandedKeys[0];
          _expandedKeys.Clear();
          _expandedKeys.Add(first);
        }

        Refresh();
      }
    }

    /// <summary>
    /// 初始展开的 key，UXML 中以逗号分隔；仅作为非受控初始值
    /// </summary>
    [Tooltip("初始展开的 key，UXML 中以逗号分隔；仅作为非受控初始值")]
    [UxmlAttribute("default-expanded-keys")]
    public string DefaultExpandedKeys {
      get => string.Join(",", _defaultExpandedKeys);
      set {
        _defaultExpandedKeys.Clear();
        foreach (string key in ParseKeys(value)) {
          _defaultExpandedKeys.Add(key);
        }
      }
    }

    /// <summary>
    /// 禁用整个手风琴
    /// </summary>
    [Tooltip("禁用整个手风琴")]
    [UxmlAttribute("is-disabled")]
    public bool IsDisabled {
      get => _isDisabled;
      set => SetOption(ref _isDisabled, value);
    }

    /// <summary>
    /// 视觉变体
    /// </summary>
    [Tooltip("视觉变体")]
    [UxmlAttribute("variant")]
    public AccordionVariant Variant {
      get => _variant;
      set => SetOption(ref _variant, value);
    }

    /// <summary>
    /// 隐藏项之间的分隔线
    /// </summary>
    [Tooltip("隐藏项之间的分隔线")]
    [UxmlAttribute("hide-separator")]
    public bool HideSeparator {
      get => _hideSeparator;
      set => SetOption(ref _hideSeparator, value);
    }

    /// <summary>
    /// 使用紧凑间距
    /// </summary>
    [Tooltip("使用紧凑间距")]
    [UxmlAttribute("compact")]
    public bool Compact {
      get => _compact;
      set => SetOption(ref _compact, value);
    }

    /// <summary>
    /// 隐藏所有项的展开指示器
    /// </summary>
    [Tooltip("隐藏所有项的展开指示器")]
    [UxmlAttribute("hide-indicator")]
    public bool HideIndicator {
      get => _hideIndicator;
      set => SetOption(ref _hideIndicator, value);
    }

    /// <summary>
    /// 关闭 USS 过渡动画（指示器旋转、悬停变色）
    /// </summary>
    [Tooltip("关闭 USS 过渡动画（指示器旋转、悬停变色）")]
    [UxmlAttribute("disable-animation")]
    public bool DisableAnimation {
      get => _disableAnimation;
      set => SetOption(ref _disableAnimation, value);
    }

    public Accordion() {
      AddToClassList(AccordionClasses.Root);
      RegisterCallback<AttachToPanelEvent>(OnAttachToPanel);
      RegisterCallback<DetachFromPanelEvent>(OnDetachFromPanel);
      RegisterCallback<KeyDownEvent>(OnKeyDown);
    }

    /// <summary>
    /// 设置被禁用的 key（对应 HeroUI 的 disabledKeys）
    /// </summary>
    public void SetDisabledKeys(IEnumerable<string>? keys) {
      _disabledKeys.Clear();
      if (keys != null) {
        foreach (string key in keys) {
          if (!string.IsNullOrEmpty(key)) {
            _disabledKeys.Add(key);
          }
        }
      }

      Refresh();
    }

    /// <summary>
    /// 以受控方式设置展开集合（对应 HeroUI 的 expandedKeys）
    /// </summary>
    public void SetExpandedKeys(IEnumerable<string>? keys, bool notify = true) {
      _expandedKeys.Clear();
      if (keys != null) {
        foreach (string key in keys) {
          if (!string.IsNullOrEmpty(key) && !_expandedKeys.Contains(key)) {
            _expandedKeys.Add(key);
          }
        }
      }

      if (!_allowMultipleExpanded && _expandedKeys.Count > 1) {
        string first = _expandedKeys[0];
        _expandedKeys.Clear();
        _expandedKeys.Add(first);
      }

      Refresh();
      if (notify) {
        Notify(null, false);
      }
    }

    /// <summary>
    /// 展开指定项
    /// </summary>
    public void Expand(string key) {
      if (!CanToggle(key) || _expandedKeys.Contains(key)) {
        return;
      }

      if (!_allowMultipleExpanded) {
        _expandedKeys.Clear();
      }

      _expandedKeys.Add(key);
      Refresh();
      Notify(key, true);
    }

    /// <summary>
    /// 折叠指定项
    /// </summary>
    public void Collapse(string key) {
      if (!CanToggle(key) || !_expandedKeys.Remove(key)) {
        return;
      }

      Refresh();
      Notify(key, false);
    }

    /// <summary>
    /// 切换指定项的展开状态
    /// </summary>
    public void Toggle(string key) {
      if (!CanToggle(key)) {
        return;
      }

      if (_expandedKeys.Contains(key)) {
        Collapse(key);
      } else {
        Expand(key);
      }
    }

    /// <summary>
    /// 展开所有可交互项；单选模式下只展开第一项
    /// </summary>
    public void ExpandAll() {
      var keys = new List<string>();
      foreach (AccordionItem item in _items) {
        if (!CanToggle(item.Key)) {
          continue;
        }

        keys.Add(item.Key);
        if (!_allowMultipleExpanded) {
          break;
        }
      }

      SetExpandedKeys(keys);
    }

    /// <summary>
    /// 折叠所有项
    /// </summary>
    public void CollapseAll() {
      SetExpandedKeys(Array.Empty<string>());
    }

    /// <summary>
    /// 按 key 查找已注册的子项
    /// </summary>
    public AccordionItem? FindItem(string key) {
      foreach (AccordionItem item in _items) {
        if (item.Key == key) {
          return item;
        }
      }

      return null;
    }

    internal void RegisterItem(AccordionItem item) {
      if (_items.Contains(item)) {
        return;
      }

      if (string.IsNullOrEmpty(item.Key)) {
        item.SetKey("accordion-item-" + _nextItemId++);
      }

      _items.Add(item);
      if (item.DefaultExpanded || _defaultExpandedKeys.Contains(item.Key)) {
        if (!_allowMultipleExpanded) {
          _expandedKeys.Clear();
        }

        if (!_expandedKeys.Contains(item.Key)) {
          _expandedKeys.Add(item.Key);
        }
      }

      Refresh();
    }

    internal void UnregisterItem(AccordionItem item) {
      if (!_items.Remove(item)) {
        return;
      }

      Refresh();
    }

    internal void ActivateItem(AccordionItem item) {
      Toggle(item.Key);
    }

    internal void Refresh() {
      RefreshRootClasses();
      RefreshItems();
    }

    private void OnAttachToPanel(AttachToPanelEvent e) {
      if (e.target != this) {
        return;
      }

      AccordionStyle.Attach(this, StyleSheetOverride, VariablesStyleSheetOverride);
      Refresh();
    }

    private void OnDetachFromPanel(DetachFromPanelEvent e) {
      if (e.target != this) {
        return;
      }
    }

    private void OnKeyDown(KeyDownEvent e) {
      AccordionItem? item = FindFocusedItem();
      if (item == null) {
        return;
      }

      switch (e.keyCode) {
      case KeyCode.Return:
      case KeyCode.KeypadEnter:
      case KeyCode.Space:
        item.Activate();
        e.StopPropagation();
        break;
      case KeyCode.DownArrow:
        FocusRelative(item, 1);
        e.StopPropagation();
        break;
      case KeyCode.UpArrow:
        FocusRelative(item, -1);
        e.StopPropagation();
        break;
      case KeyCode.Home:
        FocusAt(0);
        e.StopPropagation();
        break;
      case KeyCode.End:
        FocusAt(_items.Count - 1);
        e.StopPropagation();
        break;
      }
    }

    private void RefreshRootClasses() {
      EnableClass(AccordionClasses.RootSurface, _variant == AccordionVariant.Surface);
      EnableClass(AccordionClasses.RootOutline, _variant == AccordionVariant.Outline);
      EnableClass(AccordionClasses.RootCompact, _compact);
      EnableClass(AccordionClasses.RootHideSeparator, _hideSeparator);
      EnableClass(AccordionClasses.RootHideIndicator, _hideIndicator);
      EnableClass(AccordionClasses.RootNoAnimation, _disableAnimation);
      EnableClass(AccordionClasses.RootDisabled, _isDisabled);
      SetEnabled(!_isDisabled);
    }

    private void RefreshItems() {
      for (int i = 0; i < _items.Count; i++) {
        AccordionItem item = _items[i];
        bool expanded = _expandedKeys.Contains(item.Key);
        bool disabled = _isDisabled || item.IsDisabled || _disabledKeys.Contains(item.Key);
        item.ApplyState(expanded, disabled, _hideIndicator || item.HideIndicator,
          _disableAnimation || item.DisableAnimation);
        item.EnableClass(AccordionClasses.ItemLast, i == _items.Count - 1);
      }
    }

    private void Notify(string? changedKey, bool isExpanded) {
      using (var evt = AccordionExpandedChangedEvent.GetPooled(_expandedKeys.ToArray(), changedKey, isExpanded)) {
        SendEvent(evt);
      }

      if (changedKey == null) {
        return;
      }

      AccordionItem? item = FindItem(changedKey);
      if (item == null) {
        return;
      }

      using (var evt = AccordionItemExpandedChangedEvent.GetPooled(item, changedKey, isExpanded)) {
        item.SendEvent(evt);
      }
    }

    private bool CanToggle(string key) {
      if (_isDisabled || string.IsNullOrEmpty(key) || _disabledKeys.Contains(key)) {
        return false;
      }

      AccordionItem? item = FindItem(key);
      return item != null && !item.IsDisabled;
    }

    private void SetOption<T>(ref T field, T value) where T : struct {
      if (EqualityComparer<T>.Default.Equals(field, value)) {
        return;
      }

      field = value;
      Refresh();
    }

    private void EnableClass(string className, bool enabled) {
      if (enabled) {
        AddToClassList(className);
      } else {
        RemoveFromClassList(className);
      }
    }

    private static IEnumerable<string> ParseKeys(string? value) {
      if (string.IsNullOrEmpty(value)) {
        yield break;
      }

      foreach (string part in value.Split(',')) {
        string key = part.Trim();
        if (key.Length > 0) {
          yield return key;
        }
      }
    }

    private AccordionItem? FindFocusedItem() {
      VisualElement? focused = focusController?.focusedElement as VisualElement;
      if (focused == null || !focused.ClassListContains(AccordionClasses.Trigger)) {
        return null;
      }

      VisualElement? current = focused.parent;
      while (current != null && current != this) {
        if (current is AccordionItem item) {
          return _items.Contains(item) ? item : null;
        }

        current = current.parent;
      }

      return null;
    }

    private void FocusRelative(AccordionItem current, int direction) {
      if (_items.Count == 0) {
        return;
      }

      int index = _items.IndexOf(current);
      for (int step = 1; step <= _items.Count; step++) {
        int next = (index + direction * step + _items.Count * _items.Count) % _items.Count;
        AccordionItem item = _items[next];
        if (CanToggle(item.Key)) {
          item.FocusTrigger();
          return;
        }
      }
    }

    private void FocusAt(int index) {
      if (index >= 0 && index < _items.Count && CanToggle(_items[index].Key)) {
        _items[index].FocusTrigger();
      }
    }
  }

}
