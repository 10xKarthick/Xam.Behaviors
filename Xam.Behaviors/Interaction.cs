namespace Xam.Behaviors
{
    using Xamarin.Forms;

    /// <summary>
    /// Manages Behavior infrastructure
    /// </summary>
    public class Interaction
    {
        public static readonly BindableProperty BehaviorsProperty = BindableProperty.CreateAttached("Behaviors", typeof(BehaviorCollection),
            typeof(Interaction),
            null,
            BindingMode.OneWay,
            null,
            OnBehaviorsChanged);


        private static void OnBehaviorsChanged(BindableObject target, object oldvalue, object newvalue)
        {
            var value = (BehaviorCollection)newvalue;
            if (value == null) return;
            var t = (Element) target;
            if (t != null)
            {
                t.BindingContextChanged += (s, e) =>
                {
                    var behaviors = GetBehaviors(target);

                    foreach (var behavior in behaviors)
                    {
                        var el = (BindableObject) behavior;
                        if (el != null)
                        {
                            el.BindingContext = target.BindingContext;
                        }
                    }
                };

            }
            value.CollectionChanged += (sender, args) =>
            {
                switch (args.Action)
                {
                    case System.Collections.Specialized.NotifyCollectionChangedAction.Add:
                        foreach (IBehavior behavior in args.NewItems)
                        {
                            behavior.Attach(target);
                        }
                        break;
                    case System.Collections.Specialized.NotifyCollectionChangedAction.Remove:
                        foreach (IBehavior behavior in args.OldItems)
                        {
                            behavior.Detach();
                        }
                        break;
                }
            };

            foreach (var behavior in value)
            {
                behavior.Attach(target);
            }

            SetBehaviors(target, value);
        }

        public static BehaviorCollection GetBehaviors(BindableObject target) => (BehaviorCollection)target.GetValue(BehaviorsProperty);

        public static void SetBehaviors(BindableObject target, BehaviorCollection value) => target.SetValue(BehaviorsProperty, value);

    }
}
