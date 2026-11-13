using System;
using System.Collections.Generic;
using UnityEngine.UIElements;

namespace EditorUI.Manipulators {

  /// <summary>
  /// 上下文菜单条目；使用 <see cref="CreateSeparator"/> 创建分隔线。
  /// </summary>
  public sealed class ContextMenuItem {
    /// <summary>
    /// 菜单显示名称
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// 点击时执行的操作，分隔线为 null
    /// </summary>
    public Action? Action { get; }

    /// <summary>
    /// 动态决定条目是否可用，null 表示始终可用
    /// </summary>
    public Func<bool>? IsEnabled { get; }

    /// <summary>
    /// 是否为分隔线
    /// </summary>
    public bool IsSeparator { get; }

    private ContextMenuItem(string name, Action? action, Func<bool>? isEnabled, bool isSeparator) {
      Name = name;
      Action = action;
      IsEnabled = isEnabled;
      IsSeparator = isSeparator;
    }

    /// <summary>
    /// 创建一个可点击的菜单条目
    /// </summary>
    public static ContextMenuItem Create(string name, Action action, Func<bool>? isEnabled = null) {
      return new ContextMenuItem(name, action, isEnabled, false);
    }

    /// <summary>
    /// 创建一条分隔线
    /// </summary>
    public static ContextMenuItem CreateSeparator() {
      return new ContextMenuItem(string.Empty, null, null, true);
    }
  }

  /// <summary>
  /// 内置 <c>ContextualMenuManipulator</c> 的轻量封装：以条目列表统一描述右键菜单，
  /// 条目可以在运行时增删，避免为每个元素重复编写菜单构建回调。
  /// </summary>
  public sealed class ContextMenuManipulator : ContextualMenuManipulator {
    private readonly List<ContextMenuItem> _items;

    public ContextMenuManipulator(params ContextMenuItem[]? items) : this(new List<ContextMenuItem>(items ?? Array.Empty<ContextMenuItem>())) { }

    private ContextMenuManipulator(List<ContextMenuItem> items) : base(evt => Populate(evt, items)) {
      _items = items;
    }

    /// <summary>
    /// 追加一个菜单条目
    /// </summary>
    public void AddItem(ContextMenuItem item) {
      _items.Add(item);
    }

    /// <summary>
    /// 追加一个可点击的菜单条目
    /// </summary>
    public void AddItem(string name, Action action, Func<bool>? isEnabled = null) {
      AddItem(ContextMenuItem.Create(name, action, isEnabled));
    }

    /// <summary>
    /// 追加一条分隔线
    /// </summary>
    public void AddSeparator() {
      AddItem(ContextMenuItem.CreateSeparator());
    }

    /// <summary>
    /// 清空所有菜单条目
    /// </summary>
    public void ClearItems() {
      _items.Clear();
    }

    private static void Populate(ContextualMenuPopulateEvent evt, List<ContextMenuItem> items) {
      foreach (ContextMenuItem item in items) {
        if (item.IsSeparator) {
          evt.menu.AppendSeparator();
          continue;
        }

        if (item.Action == null) {
          continue;
        }

        Action action = item.Action;
        Func<bool>? isEnabled = item.IsEnabled;
        evt.menu.AppendAction(item.Name, _ => action(),
          _ => isEnabled == null || isEnabled()
            ? DropdownMenuAction.Status.Normal
            : DropdownMenuAction.Status.Disabled);
      }
    }
  }

}
