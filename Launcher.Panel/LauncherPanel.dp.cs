// =-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-
// Launcher.Panel/LauncherPanel.dp.cs
// --------------------------------------------------------------------------------
// Copyright (c) 2014, Jieni Luchijinzhou a.k.a Aragorn Wyvernzora
// 
// This file is a part of Launcher.Panel.
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy 
// of this software and associated documentation files (the "Software"), to deal 
// in the Software without restriction, including without limitation the rights 
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies 
// of the Software, and to permit persons to whom the Software is furnished to do 
// so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all 
// copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, 
// INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A 
// PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT 
// HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION 
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE 
// SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
// =-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-

using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Animation;

namespace Launcher.Panel
{
    // This is file for dependency property definitions.
    /// <summary>
    ///     Paged wrap panel witth fluid drag-and-drop behavior.
    /// </summary>
    public partial class LauncherPanel
    {
        // Default Values
        protected const Double DefaultScale = 1.0;
        protected const Double DefaultOpacity = 1.0;
        protected const Double MinimumOpacity = 0.1;

        // Default Drag Values
        protected const Double DefaultDragScale = 1.0;
        protected const Double DefaultDragOpacity = 1.0;

        // Z-Index
        protected const Int32 DefaultZ = 0;
        protected const Int32 TransitionZ = 1;
        protected const Int32 DragZ = Int32.MaxValue;

        // Default Transition Durations
        protected const Int32 DefaultTransitionDuration = 300;

        #region Easing

        /// <summary>
        ///     Easing Dependency Property
        /// </summary>
        public static readonly DependencyProperty EasingProperty =
            DependencyProperty.Register("Easing", typeof(EasingFunctionBase), typeof(LauncherPanel),
                new FrameworkPropertyMetadata(OnEasingChanged));

        /// <summary>
        ///     Gets or sets the easing function for all transitions within the panel.
        /// </summary>
        public EasingFunctionBase Easing
        {
            get { return (EasingFunctionBase) GetValue(EasingProperty); }
            set { SetValue(EasingProperty, value); }
        }

        /// <summary>
        ///     Handles changes to the Easing property.
        /// </summary>
        /// <param name="d"></param>
        /// <param name="e"></param>
        private static void OnEasingChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            LauncherPanel panel = d as LauncherPanel;
            if (panel != null)
                panel.OnEasingChanged((EasingFunctionBase) e.OldValue, (EasingFunctionBase) e.NewValue);
        }

        /// <summary>
        ///     Derived classes can override this method to handle changes to the Easing property.
        /// </summary>
        /// <param name="oldValue"></param>
        /// <param name="newValue"></param>
        protected virtual void OnEasingChanged(EasingFunctionBase oldValue, EasingFunctionBase newValue)
        {
        }

        #endregion

        #region DragScale

        /// <summary>
        ///     DragScale Dependency Property
        /// </summary>
        public static readonly DependencyProperty DragScaleProperty =
            DependencyProperty.Register("DragScale", typeof(double), typeof(LauncherPanel),
                new FrameworkPropertyMetadata(DefaultDragScale, OnDragScaleChanged));

        /// <summary>
        ///     Gets or sets the scale of the dragged item.
        /// </summary>
        public double DragScale
        {
            get { return (double) GetValue(DragScaleProperty); }
            set { SetValue(DragScaleProperty, value); }
        }

        /// <summary>
        ///     Handles changes to the DragScale property.
        /// </summary>
        /// <param name="d">LauncherPanel</param>
        /// <param name="e">DependencyProperty changed event arguments</param>
        private static void OnDragScaleChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            LauncherPanel panel = (LauncherPanel) d;
            double oldDragScale = (double) e.OldValue;
            double newDragScale = panel.DragScale;
            panel.OnDragScaleChanged(oldDragScale, newDragScale);
        }

        /// <summary>
        ///     Provides derived classes an opportunity to handle changes to the DragScale property.
        /// </summary>
        /// <param name="oldDragScale">Old Value</param>
        /// <param name="newDragScale">New Value</param>
        protected virtual void OnDragScaleChanged(double oldDragScale, double newDragScale)
        {
        }

        #endregion

        #region DragOpacity

        /// <summary>
        ///     DragOpacity Dependency Property
        /// </summary>
        public static readonly DependencyProperty DragOpacityProperty =
            DependencyProperty.Register("DragOpacity", typeof(double), typeof(LauncherPanel),
                new FrameworkPropertyMetadata(DefaultDragOpacity, OnDragOpacityChanged, CoerceDragOpacity));

        /// <summary>
        ///     Gets or sets the opacity of the dragged item.
        /// </summary>
        public double DragOpacity
        {
            get { return (double) GetValue(DragOpacityProperty); }
            set { SetValue(DragOpacityProperty, value); }
        }

        /// <summary>
        ///     Coerces the opacity of the dragged item to an acceptable value
        /// </summary>
        /// <param name="d">Dependency Object</param>
        /// <param name="value">Value</param>
        /// <returns>Coerced Value</returns>
        private static object CoerceDragOpacity(DependencyObject d, object value)
        {
            double opacity = (double) value;

            if (opacity < MinimumOpacity)
                opacity = MinimumOpacity;
            else if (opacity > DefaultOpacity)
                opacity = DefaultOpacity;

            return opacity;
        }

        /// <summary>
        ///     Handles changes to the DragOpacity property.
        /// </summary>
        /// <param name="d">LauncherPanel</param>
        /// <param name="e">DependencyProperty changed event arguments</param>
        private static void OnDragOpacityChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            LauncherPanel panel = (LauncherPanel) d;
            double oldDragOpacity = (double) e.OldValue;
            double newDragOpacity = panel.DragOpacity;
            panel.OnDragOpacityChanged(oldDragOpacity, newDragOpacity);
        }

        /// <summary>
        ///     Provides derived classes an opportunity to handle changes to the DragOpacity property.
        /// </summary>
        /// <param name="oldDragOpacity">Old Value</param>
        /// <param name="newDragOpacity">New Value</param>
        protected virtual void OnDragOpacityChanged(double oldDragOpacity, double newDragOpacity)
        {
        }

        #endregion

        #region Orientation

        /// <summary>
        ///     Orientation Dependency Property
        /// </summary>
        public static readonly DependencyProperty OrientationProperty =
            DependencyProperty.Register("Orientation", typeof(Orientation), typeof(LauncherPanel),
                new FrameworkPropertyMetadata(Orientation.Horizontal, OnOrientationChanged));

        /// <summary>
        ///     Gets or sets the Orientation property. This dependency property
        ///     indicates the orientation of arrangement of items in the panel.
        /// </summary>
        public Orientation Orientation
        {
            get { return (Orientation) GetValue(OrientationProperty); }
            set { SetValue(OrientationProperty, value); }
        }

        /// <summary>
        ///     Handles changes to the Orientation property.
        /// </summary>
        /// <param name="d">LauncherPanel</param>
        /// <param name="e">DependencyProperty changed event arguments</param>
        private static void OnOrientationChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            LauncherPanel panel = (LauncherPanel) d;
            Orientation oldOrientation = (Orientation) e.OldValue;
            Orientation newOrientation = panel.Orientation;
            panel.OnOrientationChanged(oldOrientation, newOrientation);
        }

        /// <summary>
        ///     Provides derived classes an opportunity to handle changes to the Orientation property.
        /// </summary>
        /// <param name="oldOrientation">Old Value</param>
        /// <param name="newOrientation">New Value</param>
        protected virtual void OnOrientationChanged(Orientation oldOrientation, Orientation newOrientation)
        {
            InvalidateVisual();
        }

        #endregion

        #region ActivePage

        /// <summary>
        /// ActivePage Dependency Property.
        /// </summary>
        public static readonly DependencyProperty ActivePageProperty =
            DependencyProperty.Register("ActivePage", typeof(Int32), typeof(LauncherPanel),
                new FrameworkPropertyMetadata(0, OnActivePageChanged));

        /// <summary>
        /// Gets or sets the index of the active page.
        /// </summary>
        public Int32 ActivePage
        {
            get { return (Int32) GetValue(ActivePageProperty); }
            set { SetValue(ActivePageProperty, value);}
        }

        /// <summary>
        /// Handles changes to the ActivePages property.
        /// </summary>
        /// <param name="o"></param>
        /// <param name="e"></param>
        private static void OnActivePageChanged(DependencyObject o, DependencyPropertyChangedEventArgs e)
        {
            var panel = o as LauncherPanel;
            if (panel != null)
                panel.OnActivePageChanged((Int32)e.OldValue, (Int32)e.NewValue);
        }

        /// <summary>
        /// Derived types can override this method to handle changes to the ActivePage property.
        /// </summary>
        /// <param name="oldValue"></param>
        /// <param name="newValue"></param>
        protected virtual void OnActivePageChanged(Int32 oldValue, Int32 newValue)
        { }

        #endregion
    }
}