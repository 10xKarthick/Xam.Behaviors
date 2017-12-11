using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Windows.Input;
using Xamarin.Forms;

namespace Xam.Behaviors
{
    /// <summary>
    /// Invoked a command when an event raises
    /// </summary>
    public class EventToCommand : Behavior
    {
        public static readonly BindableProperty EventNameProperty = BindableProperty.Create("EventName", typeof(string), typeof(EventToCommand));
        public static readonly BindableProperty CommandProperty = BindableProperty.Create("Command", typeof(ICommand), typeof(EventToCommand));
        public static readonly BindableProperty CommandParameterProperty = BindableProperty.Create("CommandParameter", typeof(object), typeof(EventToCommand));
        public static readonly BindableProperty CommandNameProperty = BindableProperty.Create("CommandName", typeof(string), typeof(EventToCommand));
        public static readonly BindableProperty CommandNameContextProperty = BindableProperty.Create("CommandNameContext", typeof(object), typeof(EventToCommand));

        private Delegate _handler;
        private EventInfo _eventInfo;

        /// <summary>
        /// Gets or sets a value indicating whether event argument will be passed to bound command.
        /// </summary>
        /// <value>
        ///   <c>true</c> if [pass event argument]; otherwise, <c>false</c>.
        /// </value>
        public bool PassEventArgument { get; set; }

        /// <summary>
        /// Gets or sets the name of the event to subscribe
        /// </summary>
        /// <value>
        /// The name of the event.
        /// </value>
        public string EventName
        {
            get => (string)GetValue(EventNameProperty);
            set => SetValue(EventNameProperty, value);
        }

        /// <summary>
        /// Gets or sets the command to invoke when event raised
        /// </summary>
        /// <value>
        /// The command.
        /// </value>
        public ICommand Command
        {
            get => (ICommand)GetValue(CommandProperty);
            set => SetValue(CommandProperty, value);
        }


        /// <summary>
        /// Gets or sets the optional command parameter.
        /// </summary>
        /// <value>
        /// The command parameter.
        /// </value>
        public object CommandParameter
        {
            get => GetValue(CommandParameterProperty);
            set => SetValue(CommandParameterProperty, value);
        }

        /// <summary>
        /// Gets or sets the name of the relative command.
        /// </summary>
        /// <value>
        /// The name of the command.
        /// </value>
        public string CommandName
        {
            get => (string)GetValue(CommandNameProperty);
            set { SetValue(CommandNameProperty, value); }
        }

        /// <summary>
        /// Gets or sets the relative context used with command name.
        /// </summary>
        /// <value>
        /// The command name context.
        /// </value>
        public object CommandNameContext
        {
            get => GetValue(CommandNameContextProperty);
            set => SetValue(CommandNameContextProperty, value);
        }


        protected override void OnAttach()
        {
            var events = AssociatedObject.GetType().GetRuntimeEvents();
            if (events.Any())
            {
                _eventInfo = events.FirstOrDefault(e => e.Name == EventName);
                if (_eventInfo == null) throw new ArgumentException(string.Format("EventToCommand: Can't find any event named '{0}' on attached type"));
                AddEventHandler(_eventInfo, AssociatedObject, OnFired);
            }
        }


        protected override void OnDetach()
        {
            if (_handler != null) _eventInfo.RemoveEventHandler(AssociatedObject, _handler);
        }

        /// <summary>
        /// Subscribes the event handler.
        /// </summary>
        /// <param name="eventInfo">The event information.</param>
        /// <param name="item">The item.</param>
        /// <param name="action">The action.</param>
        private void AddEventHandler(EventInfo eventInfo, object item, Action<EventArgs> action)
        {
            //Got inspiration from here: http://stackoverflow.com/questions/9753366/subscribing-an-action-to-any-event-type-via-reflection
            //Maybe it is possible to pass Event arguments as CommanParameter

            var mi = eventInfo.EventHandlerType.GetRuntimeMethods().First(rtm => rtm.Name == "Invoke");
            var parameters = mi.GetParameters().Select(p => Expression.Parameter(p.ParameterType)).ToList();
            var actionMethodInfo = action.GetMethodInfo();
            Expression exp = Expression.Call(Expression.Constant(this), actionMethodInfo, parameters.Last());
            _handler = Expression.Lambda(eventInfo.EventHandlerType, exp, parameters).Compile();
            eventInfo.AddEventHandler(item, _handler);
        }

        /// <summary>
        /// Called when subscribed event fires
        /// 
        /// If a CommandParameter isn't assigned, the EventArgs parameter to the Event you're attaching to will be sent instead.
        /// You will want to have your Command to accept a parameter type of EventArgs for this to work correctly.
        /// 
        /// </summary>
        /// <example>This is an example of using a Command and accepting an object of the ItemVisibilityEventArgs Type
        /// <code>
        /// ICommand ItemAppearingCommand
        /// {
        ///     get
        ///     {
        ///         return new Command&lt;ItemVisibilityEventArgs&gt;(async args => 
        ///         {
        ///             if(viewModel.Items != null &amp;&amp; e.Item == viewModel.Items[viewModel.Items.Count -1])
        ///             {
        ///                 await viewModel.RetrieveNextItemSet(viewModel.Items.Count).ConfigureAwait(false);
        ///             }
        ///         }
        ///     }    
        /// }
        /// </code>
        /// </example>
        /// <param name="e">The EventArgs value accompanying the Event</param>
        private void OnFired(EventArgs e)
        {
            var param = PassEventArgument ? e : CommandParameter;

            if (!string.IsNullOrEmpty(CommandName))
            {
                if (Command == null) CreateRelativeBinding();
            }

            if (Command == null) throw new InvalidOperationException("No command available, Is Command properly set up?");

            if (e == null && CommandParameter == null) throw new InvalidOperationException("You need a CommandParameter");

            if (Command != null && Command.CanExecute(param))
            {
                Command.Execute(param);
            }
        }

        /// <summary>
        /// Cretes a binding between relative context and provided Command name
        /// </summary>
        private void CreateRelativeBinding()
        {
            if (CommandNameContext == null) throw new ArgumentNullException("CommandNameContext property cannot be null when using CommandName property, consider using CommandNameContext={b:RelativeContext [ElementName]} markup markup extension.");
            if (Command != null) throw new InvalidOperationException("Both Command and CommandName properties specified, only one mode supported.");

            var pi = CommandNameContext.GetType().GetRuntimeProperty(CommandName);
            if (pi == null) throw new ArgumentNullException($"Can't find a command named '{CommandName}'");
            Command = pi.GetValue(CommandNameContext) as ICommand;
            if (Command == null) throw new ArgumentNullException($"Can't create binding with CommandName '{CommandName}'");
        }
    }
}
