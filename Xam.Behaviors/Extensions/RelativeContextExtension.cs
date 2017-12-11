using System;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Xam.Behaviors
{
    /// <summary>
    /// Custom markup extension that gets the BindingContext of a UI element
    /// </summary>
    [ContentProperty("Name")]
    public class RelativeContextExtension : IMarkupExtension
    {
        private BindableObject attachedObject;
        private Element rootElement;

        /// <summary>
        /// Gets or sets the element name.
        /// </summary>
        /// <value>
        /// The name.
        /// </value>
        public string Name { get; set; }

        public object ProvideValue(IServiceProvider serviceProvider)
        {
            if (serviceProvider == null) throw new ArgumentNullException("serviceProvider");
            var rootObjectProvider = serviceProvider.GetService(typeof(IRootObjectProvider)) as IRootObjectProvider;
            if (rootObjectProvider == null) throw new ArgumentException("serviceProvider does not provide an IRootObjectProvider");
            if (string.IsNullOrEmpty(Name)) throw new ArgumentNullException("Name");


            var nameScope = rootObjectProvider.RootObject as Element;
            var element = nameScope.FindByName<Element>(Name);
            if (element == null) throw new ArgumentNullException(string.Format("Can't find element named '{0}'", Name));
            var context = element.BindingContext;

            rootElement = element;
            var ipvt = (IProvideValueTarget)serviceProvider.GetService(typeof(IProvideValueTarget));
            attachedObject = ipvt.TargetObject as BindableObject;
            attachedObject.BindingContextChanged += OnContextChanged;

            return context ?? new object();
        }

        private void OnContextChanged(object sender, EventArgs e)
        {
            //If used with EventToCommand, markup extension automatically acts on CommandNameContext
            if (attachedObject is EventToCommand command)
            {
                command.CommandNameContext = rootElement.BindingContext;
            }
            else
            {
                attachedObject.BindingContext = rootElement.BindingContext;
            }


        }
    }
}
