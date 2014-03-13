using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interactivity;
using System.Windows.Media;

namespace Launcher.Panel
{
    /// <summary>
    /// 
    /// </summary>
    public class PanoramaPanelDragBehavior : Behavior<FrameworkElement>
    {
        #region Fields

        private DelayScheduler scheduler;
        private PanoramaPanel panel;

        #endregion

        public PanoramaPanelDragBehavior()
        {
        }

        #region Dependency Properties

        /// <summary>
        /// DragButton Dependency Property.
        /// </summary>
        public static readonly DependencyProperty DragButtonProperty =
            DependencyProperty.Register("DragButton", typeof(MouseButton), typeof(PanoramaPanelDragBehavior),
                new PropertyMetadata(MouseButton.Left));

        /// <summary>
        /// Gets or sets the mouse button that initiates the drag operation.
        /// </summary>
        public MouseButton DragButton
        {
            get { return (MouseButton) GetValue(DragButtonProperty); }
            set { SetValue(DragButtonProperty, value); }
        }

        /// <summary>
        /// DragDelay Dependency Property.
        /// </summary>
        public static readonly DependencyProperty DragDelayProperty =
            DependencyProperty.Register("DragDelay", typeof(TimeSpan), typeof(PanoramaPanelDragBehavior),
                new PropertyMetadata(TimeSpan.FromMilliseconds(50)));

        /// <summary>
        /// Gets or sets the delay before the drag operation is initialized.
        /// In other words, the "click-and-hold" delay.
        /// </summary>
        public TimeSpan DragDelay
        {
            get { return (TimeSpan) GetValue(DragDelayProperty); }
            set { SetValue(DragDelayProperty, value); }
        }

        #endregion

        #region Attach/Detach

        private void Loaded(Object sender, EventArgs e)
        {
            // Set up the delay scheduler
            scheduler = new DelayScheduler(AssociatedObject.Dispatcher);

            // Get the parent PanoramaPanel
            var ancestor = AssociatedObject as FrameworkElement;
            while (panel == null && ancestor != null)
            {
                panel = ancestor as PanoramaPanel;
                ancestor = VisualTreeHelper.GetParent(ancestor) as FrameworkElement;
            }

            // Subscribe to related events
            AssociatedObject.PreviewMouseDown += PreviewMouseDown;
            AssociatedObject.PreviewMouseMove += PreviewMouseMove;
            AssociatedObject.PreviewMouseUp += PreviewMouseUp;
        }

        protected override void OnAttached()
        {
            var @object = AssociatedObject as FrameworkElement;
            if (@object == null) throw new Exception("Unexpected AssociatedObject type: Expected FrameworkElement but received " + AssociatedObject.GetType().Name);

            @object.Loaded += Loaded;
        }

        protected override void OnDetaching()
        {
            var @object = AssociatedObject as FrameworkElement;
            if (@object == null)
                throw new Exception("Unexpected AssociatedObject type: Expected FrameworkElement but received " + AssociatedObject.GetType().Name);

            @object.Loaded -= Loaded;

            AssociatedObject.PreviewMouseDown -= PreviewMouseDown;
            AssociatedObject.PreviewMouseMove -= PreviewMouseMove;
            AssociatedObject.PreviewMouseUp -= PreviewMouseUp;
        }

        #endregion

        #region Event Handlers

        private void PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == DragButton)
            {
                scheduler.Schedule(DragDelay, () =>
                {
                    // Check if mouse is still in bounds
                    if (!AssociatedObject.IsMouseOver)
                        return;

                    // Get mouse position
                    var position = Mouse.GetPosition(AssociatedObject);
                    if (panel != null)
                        panel.OnDragStart(AssociatedObject, position);
                });
            }
        }
        private void PreviewMouseMove(object sender, MouseEventArgs e)
        {
            // Test if mouse moved out of range
            if (!AssociatedObject.IsMouseOver)
            {
                //...if so, cancel the scheduled delay
                scheduler.Cancel();
                return;
            }

            // Determine whether the appropriate mouse button is down.
            // this way is so messy... come on .Net
            var isDragging = false;
            switch (DragButton)
            {
                case MouseButton.Left:
                    isDragging = e.LeftButton == MouseButtonState.Pressed;
                    break;
                case MouseButton.Right:
                    isDragging = e.RightButton == MouseButtonState.Pressed;
                    break;
                case MouseButton.Middle:
                    isDragging = e.MiddleButton == MouseButtonState.Pressed;
                    break;
                case MouseButton.XButton1:
                    isDragging = e.XButton1 == MouseButtonState.Pressed;
                    break;
                case MouseButton.XButton2:
                    isDragging = e.XButton2 == MouseButtonState.Pressed;
                    break;
            }

            // Notify PagedPanel when needed
            if (isDragging)
            {
                var position = e.GetPosition(AssociatedObject);

                if (panel != null)
                {
                    var positionInParent = e.GetPosition(panel);
                    panel.OnDragMove(AssociatedObject, position, positionInParent);
                }
            }
        }

        private void PreviewMouseUp(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == DragButton)
            {
                // Cancel scheduled response
                scheduler.Cancel();
                
                // Notify PanoramaPanel
                var position = Mouse.GetPosition(AssociatedObject);
                if (panel != null)
                {
                    var positionInParent = e.GetPosition(panel);
                    panel.OnDragEnd(AssociatedObject, position, positionInParent);
                }
            }
        }

        #endregion
    }
}
