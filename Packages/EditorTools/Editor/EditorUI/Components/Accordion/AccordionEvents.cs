using System;
using System.Collections.Generic;
using EditorUI.Components;
using UnityEngine.UIElements;

namespace EditorUI {

  /// <summary>
  /// Accordion 展开集合发生变化时派发的事件<br/>
  /// 事件同时支持向下传递与冒泡，可在 Accordion 本身、其祖先或后代上注册回调
  /// </summary>
  public class AccordionExpandedChangedEvent : EventBase<AccordionExpandedChangedEvent> {
    /// <summary>
    /// 变化后的展开 key 快照
    /// </summary>
    public IReadOnlyList<string> ExpandedKeys { get; private set; } = Array.Empty<string>();

    /// <summary>
    /// 本次变化的 key，整体设置时为 null
    /// </summary>
    public string? ChangedKey { get; private set; }

    /// <summary>
    /// <see cref="ChangedKey"/> 变化后是否处于展开状态
    /// </summary>
    public bool IsExpanded { get; private set; }

    protected override void Init() {
      base.Init();
      ExpandedKeys = Array.Empty<string>();
      ChangedKey = null;
      IsExpanded = false;
      bubbles = true;
      tricklesDown = true;
    }

    /// <summary>
    /// 从事件池中获取一个已填充数据的事件<br/>
    /// 使用完毕后应调用 Dispose 或放入 using 语句
    /// </summary>
    public static AccordionExpandedChangedEvent GetPooled(
      IReadOnlyList<string> expandedKeys,
      string? changedKey,
      bool isExpanded) {
      var evt = GetPooled();
      evt.ExpandedKeys = expandedKeys;
      evt.ChangedKey = changedKey;
      evt.IsExpanded = isExpanded;
      return evt;
    }
  }

  /// <summary>
  /// 单个 AccordionItem 展开或折叠时派发的事件<br/>
  /// 事件同时支持向下传递与冒泡
  /// </summary>
  public class AccordionItemExpandedChangedEvent : EventBase<AccordionItemExpandedChangedEvent> {
    /// <summary>
    /// 状态发生变化的项
    /// </summary>
    public AccordionItem? Item { get; private set; }

    /// <summary>
    /// 状态发生变化的项的 key
    /// </summary>
    public string Key { get; private set; } = string.Empty;

    /// <summary>
    /// 变化后是否处于展开状态
    /// </summary>
    public bool IsExpanded { get; private set; }

    protected override void Init() {
      base.Init();
      Item = null;
      Key = string.Empty;
      IsExpanded = false;
      bubbles = true;
      tricklesDown = true;
    }

    /// <summary>
    /// 从事件池中获取一个已填充数据的事件<br/>
    /// 使用完毕后应调用 Dispose 或放入 using 语句
    /// </summary>
    public static AccordionItemExpandedChangedEvent GetPooled(
      AccordionItem item,
      string key,
      bool isExpanded) {
      var evt = GetPooled();
      evt.Item = item;
      evt.Key = key;
      evt.IsExpanded = isExpanded;
      return evt;
    }
  }

  /// <summary>
  /// AccordionItem 的 trigger 被激活时派发的事件<br/>
  /// 由项自身发出并冒泡到 Accordion，对应 HeroUI 的 Trigger.onPress
  /// </summary>
  public class AccordionTriggerPressedEvent : EventBase<AccordionTriggerPressedEvent> {
    /// <summary>
    /// 触发交互的项
    /// </summary>
    public AccordionItem? Item { get; private set; }

    /// <summary>
    /// 触发交互的项的 key
    /// </summary>
    public string Key { get; private set; } = string.Empty;

    protected override void Init() {
      base.Init();
      Item = null;
      Key = string.Empty;
      bubbles = true;
      tricklesDown = true;
    }

    /// <summary>
    /// 从事件池中获取一个已填充数据的事件<br/>
    /// 使用完毕后应调用 Dispose 或放入 using 语句
    /// </summary>
    public static AccordionTriggerPressedEvent GetPooled(AccordionItem item, string key) {
      var evt = GetPooled();
      evt.Item = item;
      evt.Key = key;
      return evt;
    }
  }

}
