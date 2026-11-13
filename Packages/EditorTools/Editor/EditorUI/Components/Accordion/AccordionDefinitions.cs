using UnityEngine;
using UnityEngine.UIElements;

namespace EditorUI.Components {

  /// <summary>
  /// 手风琴的视觉变体
  /// </summary>
  public enum AccordionVariant {
    Default,
    Surface,
    Outline,
  }

  /// <summary>
  /// Accordion 使用的 BEM 类名常量<br/>
  /// 可用 UQuery 定位内部结构，也可在自定义 USS 中覆盖
  /// </summary>
  public static class AccordionClasses {
    public const string Root = "accordion";
    public const string RootSurface = "accordion--surface";
    public const string RootOutline = "accordion--outline";
    public const string RootCompact = "accordion--compact";
    public const string RootHideSeparator = "accordion--hide-separator";
    public const string RootHideIndicator = "accordion--hide-indicator";
    public const string RootNoAnimation = "accordion--no-animation";
    public const string RootDisabled = "accordion--disabled";

    public const string Item = "accordion__item";
    public const string ItemExpanded = "accordion__item--expanded";
    public const string ItemDisabled = "accordion__item--disabled";
    public const string ItemLast = "accordion__item--last";
    public const string ItemHideIndicator = "accordion__item--hide-indicator";

    public const string Separator = "accordion__separator";
    public const string Heading = "accordion__heading";
    public const string Trigger = "accordion__trigger";
    public const string TriggerExpanded = "accordion__trigger--expanded";
    public const string TriggerFocus = "accordion__trigger--focus-visible";
    public const string TriggerPressed = "accordion__trigger--pressed";
    public const string TriggerDisabled = "accordion__trigger--disabled";
    public const string StartContent = "accordion__start-content";
    public const string EndContent = "accordion__end-content";
    public const string Title = "accordion__title";
    public const string Subtitle = "accordion__subtitle";

    public const string Indicator = "accordion__indicator";
    public const string IndicatorExpanded = "accordion__indicator--expanded";
    public const string IndicatorChevron = "accordion__indicator-chevron";

    public const string Panel = "accordion__panel";
    public const string PanelExpanded = "accordion__panel--expanded";
    public const string Body = "accordion__body";
    public const string TitleGroup = "accordion__title-group";
  }

  /// <summary>
  /// 加载包内默认变量表与组件样式表
  /// </summary>
  internal static class AccordionStyle {
    internal const string StyleSheetResource = Const.StyleSheetPathPrefix + "EditorUI.Accordion";
    internal const string VariablesStyleSheetResource = Const.StyleSheetPathPrefix + "EditorUI.Accordion.Variables";

    internal static void Attach(VisualElement element, StyleSheet? overrideSheet, StyleSheet? variablesOverride) {
      Helper.AttachSheet(element, variablesOverride, VariablesStyleSheetResource);
      Helper.AttachSheet(element, overrideSheet, StyleSheetResource);
    }
  }

}
