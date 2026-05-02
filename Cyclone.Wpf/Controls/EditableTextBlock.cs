using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Cyclone.Wpf.Controls;

[TemplatePart(Name = PART_TextBlock, Type = typeof(TextBlock))]
[TemplatePart(Name = PART_TextBox, Type = typeof(TextBox))]
public class EditableTextBlock : Control
{
    private const string PART_TextBlock = "PART_TextBlock";
    private const string PART_TextBox = "PART_TextBox";

    private TextBlock _textBlock;
    private TextBox _textBox;

    private void TextBlock_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (e.ClickCount == 2)  // 检查点击次数是否为双击
        {
            IsEditing = true;
            UpdateVisualState();
            e.Handled = true;
        }
    }

    private void TextBox_LostFocus(object sender, RoutedEventArgs e)
    {
        CommitEdit();
    }

    private void TextBox_KeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter)
        {
            CommitEdit();
            e.Handled = true;
        }
        else if (e.Key == Key.Escape)
        {
            CancelEdit();
            e.Handled = true;
        }
    }

    private void CommitEdit()
    {
        if (_textBox != null)
        {
            Text = _textBox.Text;
        }
        IsEditing = false;
        UpdateVisualState();
    }

    private void CancelEdit()
    {
        IsEditing = false;
        UpdateVisualState();
    }

    private void UpdateVisualState()
    {
        if (_textBlock != null && _textBox != null)
        {
            if (IsEditing)
            {
                _textBlock.Visibility = Visibility.Collapsed;
                _textBox.Visibility = Visibility.Visible;
                _textBox.Focus();
                _textBox.SelectAll();
            }
            else
            {
                _textBlock.Visibility = Visibility.Visible;
                _textBox.Visibility = Visibility.Collapsed;
            }
        }
    }

    public override void OnApplyTemplate()
    {
        base.OnApplyTemplate();

        _textBlock = GetTemplateChild(PART_TextBlock) as TextBlock;
        _textBox = GetTemplateChild(PART_TextBox) as TextBox;

        if (_textBlock != null)
        {
            _textBlock.MouseLeftButtonDown += TextBlock_MouseLeftButtonDown;
        }

        if (_textBox != null)
        {
            _textBox.LostFocus += TextBox_LostFocus;
            _textBox.KeyDown += TextBox_KeyDown;
        }

        UpdateVisualState();
    }

    static EditableTextBlock()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(EditableTextBlock), new FrameworkPropertyMetadata(typeof(EditableTextBlock)));
    }

    #region Text

    public static readonly DependencyProperty TextProperty =
        DependencyProperty.Register(nameof(Text), typeof(string), typeof(EditableTextBlock), new FrameworkPropertyMetadata(string.Empty, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

    public string Text
    {
        get => (string)GetValue(TextProperty);
        set => SetValue(TextProperty, value);
    }

    #endregion Text

    #region IsEditing

    public static readonly DependencyProperty IsEditingProperty =
        DependencyProperty.Register(nameof(IsEditing), typeof(bool), typeof(EditableTextBlock), new FrameworkPropertyMetadata(false));

    public bool IsEditing
    {
        get => (bool)GetValue(IsEditingProperty);
        set => SetValue(IsEditingProperty, value);
    }

    #endregion IsEditing

    #region TextWrapping

    public static readonly DependencyProperty TextWrappingProperty =
        DependencyProperty.Register(nameof(TextWrapping), typeof(TextWrapping), typeof(EditableTextBlock), new PropertyMetadata(default(TextWrapping)));

    public TextWrapping TextWrapping
    {
        get => (TextWrapping)GetValue(TextWrappingProperty);
        set => SetValue(TextWrappingProperty, value);
    }

    #endregion TextWrapping

    #region IsReadOnly

    public static readonly DependencyProperty IsReadOnlyProperty =
        DependencyProperty.Register(nameof(IsReadOnly), typeof(bool), typeof(EditableTextBlock), new PropertyMetadata(default(bool)));

    public bool IsReadOnly
    {
        get => (bool)GetValue(IsReadOnlyProperty);
        set => SetValue(IsReadOnlyProperty, value);
    }

    #endregion IsReadOnly
}