namespace Cyclone.Wpf.Controls;

/// <summary>
/// <see cref="Card"/> 的 Header / Footer 分隔线可见性配置。
/// 命名风格参照 <c>DataGrid.GridLinesVisibility</c>。
/// </summary>
public enum CardSeparatorVisibility
{
    /// <summary>都不显示。</summary>
    None,

    /// <summary>仅显示 Header 下方分隔线。</summary>
    HeaderOnly,

    /// <summary>仅显示 Footer 上方分隔线。</summary>
    FooterOnly,

    /// <summary>同时显示 Header 和 Footer 的分隔线（默认）。</summary>
    Both,
}
