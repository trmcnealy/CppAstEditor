using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;

namespace CppAstEditor
{
    /// <summary>
    /// Interaction logic for EditableTextBlock.xaml
    /// </summary>
    public partial class EditableTextBlock : UserControl
    {
        public static readonly DependencyProperty TextAlignmentProperty
            = DependencyProperty.Register(
                "TextAlignment",
                typeof(TextAlignment),
                typeof(EditableTextBlock));

        public static readonly DependencyProperty TextDecorationsProperty
            = DependencyProperty.Register(
                "TextDecorations",
                typeof(TextDecorationCollection),
                typeof(EditableTextBlock));

        public static readonly DependencyProperty TextEffectsProperty
            = DependencyProperty.Register(
                "TextEffects",
                typeof(TextEffectCollection),
                typeof(EditableTextBlock));

        public static readonly DependencyProperty TextProperty
            = DependencyProperty.Register(
                "Text",
                typeof(string),
                typeof(EditableTextBlock));


        public static readonly DependencyProperty TextTrimmingProperty
            = DependencyProperty.Register(
                "TextTrimming",
                typeof(TextTrimming),
                typeof(EditableTextBlock));

        public static readonly DependencyProperty TextWrappingProperty
            = DependencyProperty.Register(
                "TextWrapping",
                typeof(TextWrapping),
                typeof(EditableTextBlock));
        
        public string Text
        {
            get { return (string)GetValue(TextProperty); }
            set { SetValue(TextProperty, value); }
        }

        public static readonly RoutedEvent TextChangedEvent = EventManager.RegisterRoutedEvent("TextChanged",
                                                                                               RoutingStrategy.Bubble,
                                                                                               typeof(TextChangedEventHandler),
                                                                                               typeof(EditableTextBlock));

        public event TextChangedEventHandler TextChanged
        {
            add
            {
                AddHandler(TextChangedEvent, value);
            }

            remove
            {
                RemoveHandler(TextChangedEvent, value);
            }
        }

        //private static void OnTextPropertyChanged(DependencyObject d,
        //                                          DependencyPropertyChangedEventArgs e)
        //{
        //    EditableTextBlock textBox = (EditableTextBlock)d;

        //    textBox.OnTextPropertyChanged((string)e.OldValue, (string)e.NewValue);
        //}

        //private void OnTextPropertyChanged(string oldText, string newText)
        //{
        //}

        private void EditBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            e.Handled = true;

            foreach (var change in e.Changes)
            {
                Changes.Add(change);
            }
        }

        protected virtual void OnTextChanged(TextChangedEventArgs e)
        {
            RaiseEvent(e);
        }

        public TextAlignment TextAlignment
        {
            get { return (TextAlignment)GetValue(TextAlignmentProperty); }
            set { SetValue(TextAlignmentProperty, value); }
        }

        public TextDecorationCollection TextDecorations
        {
            get { return (TextDecorationCollection)GetValue(TextDecorationsProperty); }
            set { SetValue(TextDecorationsProperty, value); }
        }

        public TextEffectCollection TextEffects
        {
            get { return (TextEffectCollection)GetValue(TextEffectsProperty); }
            set { SetValue(TextEffectsProperty, value); }
        }

        public TextTrimming TextTrimming
        {
            get { return (TextTrimming)GetValue(TextTrimmingProperty); }
            set { SetValue(TextTrimmingProperty, value); }
        }

        public TextWrapping TextWrapping
        {
            get { return (TextWrapping)GetValue(TextWrappingProperty); }
            set { SetValue(TextWrappingProperty, value); }
        }

        public EditableTextBlock()
        {
            InitializeComponent();
        }

        public void Edit()
        {
            DisplayBox.Visibility = Visibility.Collapsed;
            EditBox.Visibility = Visibility.Visible;
        }

        public void CancelEdit()
        {
            EditBox.Visibility = Visibility.Collapsed;
            DisplayBox.Visibility = Visibility.Visible;
        }

        private void DisplayBox_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ButtonState == MouseButtonState.Pressed && e.ChangedButton == MouseButton.Left)
            {
                Edit();
                EditBox.Focus();
                EditBox.UpdateLayout();

                int pos = EditBox.GetCharacterIndexFromPoint(e.GetPosition(EditBox), true);
                EditBox.CaretIndex = pos + 1;
            }
        }


        internal List<TextChange> Changes { get; } = new List<TextChange>();

        private void EditBox_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Return || e.Key == Key.Enter)
            {
                e.Handled = true;
                EditBox.GetBindingExpression(TextBox.TextProperty)?.UpdateSource();
                DisplayBox.GetBindingExpression(TextBlock.TextProperty)?.UpdateTarget();
                GetBindingExpression(TextProperty)?.UpdateSource();
                CancelEdit();

                OnTextChanged(new TextChangedEventArgs(TextChangedEvent, UndoAction.None, new ReadOnlyCollection<TextChange>(Changes)));
                Changes.Clear();
            }
            else if(e.Key == Key.Escape)
            {
                e.Handled = true;
                EditBox.Undo();
                EditBox.GetBindingExpression(TextBox.TextProperty)?.UpdateSource();
                DisplayBox.GetBindingExpression(TextBlock.TextProperty)?.UpdateTarget();
                GetBindingExpression(TextProperty)?.UpdateSource();
                CancelEdit();
            }
        }
    }
}
