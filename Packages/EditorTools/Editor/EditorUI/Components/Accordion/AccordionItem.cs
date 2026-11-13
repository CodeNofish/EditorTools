using UnityEngine;
using UnityEngine.UIElements;

namespace EditorUI.Components {

  /// <summary>
  /// 手风琴的单个项，对应 HeroUI 的 Accordion.Item / Heading / Trigger / Indicator / Panel / Body 复合结构<br/>
  /// UXML 中该元素的子元素会自动放入 Body（内容容器），例如：<br/>
  /// <c>&lt;EditorUI:AccordionItem key="a" title="Title" subtitle="Subtitle"&gt;&lt;ui:Label text="Content" /&gt;&lt;/EditorUI:AccordionItem&gt;</c><br/>
  /// 视觉参数由 <c>EditorUI.Accordion.Variables.uss</c> 的 <c>--editorui-accordion-*</c> 变量提供<br/>
  /// 可注册监听的事件：<see cref="AccordionTriggerPressedEvent"/> 与 <see cref="AccordionItemExpandedChangedEvent"/>
  /// </summary>
  [UxmlElement(nameof(AccordionItem), libraryPath = Const.LibraryPath)]
  public partial class AccordionItem : VisualElement {
    private static readonly CustomStyleProperty<float> ChevronLineWidth = new("--editorui-accordion-chevron-line-width");
    private readonly VisualElement _heading;
    private readonly VisualElement _trigger;
    private readonly VisualElement _startContent;
    private readonly VisualElement _endContent;
    private readonly VisualElement _titleGroup;
    private readonly Label _title;
    private readonly Label _subtitle;
    private readonly VisualElement _indicator;
    private readonly VisualElement _chevron;
    private readonly VisualElement _panel;
    private readonly VisualElement _body;
    private readonly VisualElement _separator;

    private Accordion? _accordion;
    private VisualElement? _customIndicator;
    private bool _pressed;
    private int _pointerId = -1;
    private bool _initialized;
    private bool _expanded;
    private bool _isDisabled;
    private bool _disabledByGroup;
    private bool _hideIndicator;
    private bool _disableAnimation;
    private float _chevronLineWidth = 1.5f;

    /// <summary>
    /// item 的唯一标识，为空时由所属 Accordion 自动生成
    /// </summary>
    [Tooltip("item 的唯一标识，为空时由所属 Accordion 自动生成")]
    [UxmlAttribute("key")]
    public string Key { get; set; } = string.Empty;

    /// <summary>
    /// 标题文本
    /// </summary>
    [Tooltip("标题文本")]
    [UxmlAttribute("title")]
    public string Title {
      get => _title.text;
      set => _title.text = string.IsNullOrEmpty(value) ? string.Empty : value;
    }

    /// <summary>
    /// 副标题文本，为空时隐藏
    /// </summary>
    [Tooltip("副标题文本，为空时隐藏")]
    [UxmlAttribute("subtitle")]
    public string Subtitle {
      get => _subtitle.text;
      set {
        _subtitle.text = string.IsNullOrEmpty(value) ? string.Empty : value;
        _subtitle.style.display = string.IsNullOrEmpty(_subtitle.text)
          ? DisplayStyle.None
          : DisplayStyle.Flex;
      }
    }

    /// <summary>
    /// 初始是否展开，仅作为非受控初始值
    /// </summary>
    [Tooltip("初始是否展开，仅作为非受控初始值")]
    [UxmlAttribute("default-expanded")]
    public bool DefaultExpanded { get; set; }

    /// <summary>
    /// 禁用该项的交互
    /// </summary>
    [Tooltip("禁用该项的交互")]
    [UxmlAttribute("is-disabled")]
    public bool IsDisabled {
      get => _isDisabled;
      set {
        if (_isDisabled == value) {
          return;
        }

        _isDisabled = value;
        ApplyState(_expanded, _isDisabled || _disabledByGroup, _hideIndicator, _disableAnimation);
        _accordion?.Refresh();
      }
    }

    /// <summary>
    /// 隐藏该项的指示器
    /// </summary>
    [Tooltip("隐藏该项的指示器")]
    [UxmlAttribute("hide-indicator")]
    public bool HideIndicator {
      get => _hideIndicator;
      set {
        if (_hideIndicator == value) {
          return;
        }

        _hideIndicator = value;
        ApplyState(_expanded, _isDisabled || _disabledByGroup, _hideIndicator, _disableAnimation);
        _accordion?.Refresh();
      }
    }

    /// <summary>
    /// 关闭该项的过渡动画，独立使用（不在 Accordion 内）时有效
    /// </summary>
    [Tooltip("关闭该项的过渡动画，独立使用时有效")]
    [UxmlAttribute("disable-animation")]
    public bool DisableAnimation {
      get => _disableAnimation;
      set {
        if (_disableAnimation == value) {
          return;
        }

        _disableAnimation = value;
        ApplyState(_expanded, _isDisabled || _disabledByGroup, _hideIndicator, _disableAnimation);
      }
    }

    /// <summary>
    /// 当前是否展开，状态由所属 Accordion 统一管理
    /// </summary>
    public bool IsExpanded => _expanded;

    /// <summary>
    /// 标题区域，可添加自定义内容
    /// </summary>
    public VisualElement Heading => _heading;

    /// <summary>
    /// 标题与副标题所在的容器
    /// </summary>
    public VisualElement TitleGroup => _titleGroup;

    /// <summary>
    /// 项底部的分隔线，可用 USS 或代码自定义
    /// </summary>
    public VisualElement Separator => _separator;

    /// <summary>
    /// 可点击、可获得焦点的触发区域
    /// </summary>
    public VisualElement Trigger => _trigger;

    /// <summary>
    /// 标题前的插槽，适合放图标
    /// </summary>
    public VisualElement StartContent => _startContent;

    /// <summary>
    /// 标题后的插槽，适合放操作按钮
    /// </summary>
    public VisualElement EndContent => _endContent;

    /// <summary>
    /// 指示器插槽，可用 <see cref="SetIndicator"/> 换成自定义图标
    /// </summary>
    public VisualElement Indicator => _indicator;

    /// <summary>
    /// 折叠面板容器
    /// </summary>
    public VisualElement Panel => _panel;

    /// <summary>
    /// 内容容器，也是 UXML 子元素的默认父级
    /// </summary>
    public VisualElement Body => _body;

    /// <summary>
    /// UXML 子元素与 Add 方法使用的默认父级为 <see cref="Body"/>
    /// </summary>
    public override VisualElement contentContainer => _body;

    public AccordionItem() {
      AddToClassList(AccordionClasses.Item);

      _heading = CreateContainer(AccordionClasses.Heading);
      _trigger = new VisualElement { focusable = true, tabIndex = 0 };
      _trigger.AddToClassList(AccordionClasses.Trigger);
      _startContent = CreateContainer(AccordionClasses.StartContent);
      _endContent = CreateContainer(AccordionClasses.EndContent);
      _titleGroup = CreateContainer(AccordionClasses.TitleGroup);
      _title = CreateLabel(AccordionClasses.Title);
      _subtitle = CreateLabel(AccordionClasses.Subtitle);
      _indicator = CreateContainer(AccordionClasses.Indicator);
      _chevron = new VisualElement { pickingMode = PickingMode.Ignore };
      _chevron.AddToClassList(AccordionClasses.IndicatorChevron);
      _chevron.generateVisualContent += DrawChevron;
      _chevron.RegisterCallback<CustomStyleResolvedEvent>(OnChevronCustomStyleResolved);

      _panel = CreateContainer(AccordionClasses.Panel);
      _body = CreateContainer(AccordionClasses.Body);
      _separator = CreateContainer(AccordionClasses.Separator);

      _titleGroup.Add(_title);
      _titleGroup.Add(_subtitle);
      _indicator.Add(_chevron);
      _trigger.Add(_startContent);
      _trigger.Add(_titleGroup);
      _trigger.Add(_endContent);
      _trigger.Add(_indicator);
      _heading.Add(_trigger);
      _panel.Add(_body);

      hierarchy.Add(_heading);
      hierarchy.Add(_panel);
      hierarchy.Add(_separator);

      _trigger.RegisterCallback<PointerDownEvent>(OnTriggerPointerDown);
      _trigger.RegisterCallback<PointerUpEvent>(OnTriggerPointerUp);
      _trigger.RegisterCallback<PointerCancelEvent>(OnTriggerPointerCancel);
      _trigger.RegisterCallback<FocusInEvent>(OnFocusIn);
      _trigger.RegisterCallback<FocusOutEvent>(OnFocusOut);
      RegisterCallback<AttachToPanelEvent>(OnAttachToPanel);
      RegisterCallback<DetachFromPanelEvent>(OnDetachFromPanel);

      Subtitle = string.Empty;
      ApplyState(false, false, false, false);
    }

    /// <summary>
    /// 用自定义元素替换默认的箭头指示器，传入 null 恢复默认
    /// </summary>
    public void SetIndicator(VisualElement? indicator) {
      if (_customIndicator == indicator) {
        return;
      }

      if (_customIndicator != null) {
        _customIndicator.RemoveFromHierarchy();
      }

      _customIndicator = indicator;
      _chevron.style.display = indicator == null ? DisplayStyle.Flex : DisplayStyle.None;
      if (indicator != null) {
        indicator.pickingMode = PickingMode.Ignore;
        _indicator.Add(indicator);
      }
    }

    /// <summary>
    /// 展开该项
    /// </summary>
    public void Expand() {
      SetExpanded(true);
    }

    /// <summary>
    /// 折叠该项
    /// </summary>
    public void Collapse() {
      SetExpanded(false);
    }

    /// <summary>
    /// 切换该项的展开状态
    /// </summary>
    public void Toggle() {
      if (_accordion != null) {
        _accordion.ActivateItem(this);
        return;
      }

      if (!_isDisabled && !_disabledByGroup) {
        ApplyLocalExpanded(!_expanded);
      }
    }

    /// <summary>
    /// 设置展开状态，在 Accordion 内时转为对容器的受控请求
    /// </summary>
    public void SetExpanded(bool value) {
      if (_accordion != null) {
        if (value) {
          _accordion.Expand(Key);
        } else {
          _accordion.Collapse(Key);
        }

        return;
      }

      ApplyLocalExpanded(value);
    }

    internal void SetKey(string key) {
      Key = key;
    }

    internal void FocusTrigger() {
      _trigger.Focus();
    }

    internal void Activate() {
      if (_isDisabled || _disabledByGroup) {
        return;
      }

      using (var evt = AccordionTriggerPressedEvent.GetPooled(this, Key)) {
        SendEvent(evt);
      }

      Toggle();
    }

    internal void EnableClass(string className, bool enabled) {
      SetClass(this, className, enabled);
    }

    internal void ApplyState(bool expanded, bool disabled, bool hideIndicator, bool disableAnimation) {
      _expanded = expanded;
      _disabledByGroup = disabled;
      EnableClass(AccordionClasses.ItemExpanded, expanded);
      EnableClass(AccordionClasses.ItemDisabled, disabled);
      EnableClass(AccordionClasses.ItemHideIndicator, hideIndicator);
      EnableClass(AccordionClasses.RootNoAnimation, disableAnimation);
      SetClass(_trigger, AccordionClasses.TriggerExpanded, expanded);
      SetClass(_trigger, AccordionClasses.TriggerDisabled, disabled);
      SetClass(_indicator, AccordionClasses.IndicatorExpanded, expanded);
      SetClass(_panel, AccordionClasses.PanelExpanded, expanded);
      _trigger.SetEnabled(!disabled);
      _panel.style.display = expanded ? DisplayStyle.Flex : DisplayStyle.None;
      _indicator.style.display = hideIndicator ? DisplayStyle.None : DisplayStyle.Flex;
    }

    private void ApplyLocalExpanded(bool value) {
      if (_expanded == value) {
        return;
      }

      _expanded = value;
      ApplyState(_expanded, _isDisabled || _disabledByGroup, _hideIndicator, _disableAnimation);
      using (var evt = AccordionItemExpandedChangedEvent.GetPooled(this, Key, _expanded)) {
        SendEvent(evt);
      }
    }

    private void OnTriggerPointerDown(PointerDownEvent e) {
      if (e.button != (int)MouseButton.LeftMouse || e.target != _trigger) {
        return;
      }

      _pressed = true;
      _pointerId = e.pointerId;
      SetClass(_trigger, AccordionClasses.TriggerPressed, true);
    }

    private void OnTriggerPointerUp(PointerUpEvent e) {
      bool valid = _pressed && e.pointerId == _pointerId
        && e.button == (int)MouseButton.LeftMouse && e.target == _trigger;
      _pressed = false;
      _pointerId = -1;
      SetClass(_trigger, AccordionClasses.TriggerPressed, false);

      if (!valid) {
        return;
      }

      Activate();
      e.StopPropagation();
    }

    private void OnTriggerPointerCancel(PointerCancelEvent e) {
      _pressed = false;
      _pointerId = -1;
      SetClass(_trigger, AccordionClasses.TriggerPressed, false);
    }

    private void OnFocusIn(FocusInEvent e) {
      SetClass(_trigger, AccordionClasses.TriggerFocus, true);
    }

    private void OnFocusOut(FocusOutEvent e) {
      SetClass(_trigger, AccordionClasses.TriggerFocus, false);
    }

    private void OnAttachToPanel(AttachToPanelEvent e) {
      if (e.target != this) {
        return;
      }

      AccordionStyle.Attach(this, Accordion.StyleSheetOverride,
        Accordion.VariablesStyleSheetOverride);
      if (!_initialized) {
        _initialized = true;
        _expanded = DefaultExpanded;
      }

      _accordion = parent as Accordion;
      if (_accordion != null) {
        _accordion.RegisterItem(this);
      } else {
        ApplyState(_expanded, _isDisabled, _hideIndicator, _disableAnimation);
      }
    }

    private void OnDetachFromPanel(DetachFromPanelEvent e) {
      if (e.target != this && parent is Accordion) {
        return;
      }

      _accordion?.UnregisterItem(this);
      _accordion = null;
      _pressed = false;
      _pointerId = -1;
      SetClass(_trigger, AccordionClasses.TriggerPressed, false);
    }

    private void OnChevronCustomStyleResolved(CustomStyleResolvedEvent e) {
      _chevronLineWidth = e.customStyle.TryGetValue(ChevronLineWidth, out float width)
        && width > 0f ? width : 1.5f;
      _chevron.MarkDirtyRepaint();
    }

    private void DrawChevron(MeshGenerationContext context) {
      Vector2 size = _chevron.layout.size;
      if (size.x <= 0f || size.y <= 0f) {
        return;
      }

      var painter = context.painter2D;
      painter.strokeColor = _chevron.resolvedStyle.color;
      painter.lineWidth = _chevronLineWidth;
      painter.BeginPath();
      painter.MoveTo(new Vector2(size.x * 0.35f, size.y * 0.25f));
      painter.LineTo(new Vector2(size.x * 0.65f, size.y * 0.5f));
      painter.LineTo(new Vector2(size.x * 0.35f, size.y * 0.75f));
      painter.Stroke();
    }

    private static VisualElement CreateContainer(string className) {
      var element = new VisualElement { pickingMode = PickingMode.Ignore };
      element.AddToClassList(className);
      return element;
    }

    private static Label CreateLabel(string className) {
      var label = new Label { pickingMode = PickingMode.Ignore };
      label.AddToClassList(className);
      return label;
    }

    private static void SetClass(VisualElement element, string className, bool enabled) {
      if (enabled) {
        element.AddToClassList(className);
      } else {
        element.RemoveFromClassList(className);
      }
    }

  }

}
