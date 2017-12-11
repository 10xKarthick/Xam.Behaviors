using Xamarin.Forms;

namespace Xam.Behaviors
{
    /// <summary>
    /// Updates text while Entry text changes
    /// </summary>
    public class TextChangedBehavior : Behavior<Entry>
    {
        public static readonly BindableProperty TextProperty = BindableProperty.Create<TextChangedBehavior, string>(p => p.Text, null, propertyChanged: OnTextChanged);

        private static void OnTextChanged(BindableObject bindable, string oldvalue, string newvalue)
        {
            ((TextChangedBehavior) bindable).AssociatedObject.Text = newvalue;
        }

        public string Text
        {
            get => (string)GetValue(TextProperty);
            set => SetValue(TextProperty, value);
        }

        protected override void OnAttach() => AssociatedObject.TextChanged += OnTextChanged;

        private void OnTextChanged(object sender, TextChangedEventArgs e) => Text = e.NewTextValue;

        protected override void OnDetach() => AssociatedObject.TextChanged -= OnTextChanged;
    }
}
