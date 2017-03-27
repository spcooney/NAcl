using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.Linq;
using System.Collections.Generic;

namespace NAcl.View
{
    /// <summary>
    /// Container of the Acl property which will enable or not an access to a resource
    /// </summary>
    public class Acls : DependencyObject
    {
        public string Acl
        {
            get { return (string)GetValue(AclProperty); }
            set { SetValue(AclProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Resource.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty AclProperty =
            DependencyProperty.RegisterAttached("Acl", typeof(string), typeof(Acls), new PropertyMetadata(AclChanged));

        private static void AclChanged(DependencyObject target, DependencyPropertyChangedEventArgs e)
        {
            SetAcl(target, (string)e.NewValue);
        }

        private static IList<WeakReference> controls = new List<WeakReference>();

        public static void RefreshAcls()
        {
            Application.Current.RootVisual.Dispatcher.BeginInvoke(RefreshAclsSafe);
        }
        private static void RefreshAclsSafe()
        {
            for (int i = controls.Count - 1; i >= 0; i--)
            {
                if (controls[i].IsAlive)
                {
                    if (AclRefreshed != null)
                        AclRefreshed((DependencyObject)controls[i].Target, GetAcl((DependencyObject)controls[i].Target), GetSubjects());
                }
                else
                    controls.RemoveAt(i);
            }
        }

        public static void SetAcl(DependencyObject target, string resourceKey)
        {
            target.SetValue(AclProperty, resourceKey);

            controls.Add(new WeakReference(target));

            if (AclRefreshed != null)
                AclRefreshed(target, resourceKey, GetSubjects());
        }

        public static Func<string[]> GetSubjects;

        public static event Action<DependencyObject, string, string[]> AclRefreshed;

        public static string GetAcl(DependencyObject b)
        {
            return (string)b.GetValue(AclProperty);
        }

        static Acls()
        {
            AclRefreshed += HandleAclRefreshedForFrameworkElement;
        }

        private static void HandleAclRefreshedForFrameworkElement(DependencyObject target, string resourceKey, string[] subjects)
        {
            FrameworkElement targetFrameworkElement = target as FrameworkElement;
            if (targetFrameworkElement != null)
            {
                targetFrameworkElement.Visibility = AclManager.IsAllowed(resourceKey, Verbs.Visible.ToString(), subjects) ? Visibility.Visible : Visibility.Collapsed;

                Control targetControl = target as Control;

                if (targetControl != null)
                    targetControl.IsEnabled = AclManager.IsAllowed(resourceKey, Verbs.Enabled.ToString(), subjects);
            }
        }
    }
}
