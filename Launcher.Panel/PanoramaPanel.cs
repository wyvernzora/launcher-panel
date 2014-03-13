using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Animation;
using CPanel = System.Windows.Controls.Panel;

namespace Launcher.Panel
{
    /// <summary>
    /// Paged wrap panel with fluid drag-and-drop behavior.
    /// Does not include scrolling functionality.
    /// </summary>
    /// <remarks>
    /// PanoramaPanel is not designed for use as an ItemPanel.
    /// </remarks>
    [ContentProperty("Pages")]
    public partial class PanoramaPanel : CPanel
    {
        #region Nested Types

        public sealed class PageCollection : UIElementCollection
        {
            private readonly PanoramaPanel parent;

            public PageCollection(PanoramaPanel panel)
                : base(panel, panel)
            {
                parent = panel;
            }

            public override int Add(UIElement element)
            {
                var page = element as PanoramaPanelPage;
                if (page == null) throw new Exception("LauncherPanelPage expected!");

                parent.AddPage(page);
                return base.Add(element);
            }

            public override void Remove(UIElement element)
            {
                var page = element as PanoramaPanelPage;
                if (page == null) throw new Exception("LauncherPanelPage expected!");

                parent.RemovePage(page);
                base.Remove(element);
            }

            public new PanoramaPanelPage this[Int32 index]
            {
                get { return (PanoramaPanelPage) base[index]; }
            }
        }

        #endregion

        protected Int32 rowSize;
        protected Int32 columnSize;
        protected Boolean disableAnimation;
        protected PageCollection pages;
        
        public PanoramaPanel()
        {
            pages = new PageCollection(this);
        }

        #region Properties

        /// <summary>
        /// Gets the number of pages in the PanoramaPanel.
        /// </summary>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public Int32 PageCount
        { get { return pages.Count; } }

        /// <summary>
        /// Gets the list of pages in the PanoramaPanel.
        /// </summary>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public PageCollection Pages
        {
            get { return pages; }
        }

        #endregion

        protected override Size MeasureOverride(Size availableSize)
        {
            // There are no size constrains
            var size = new Size(Double.PositiveInfinity, Double.PositiveInfinity);
            
            // Measure children
            foreach (UIElement child in InternalChildren)
                child.Measure(size);
            
            // Calculate the desired size
            return Orientation == Orientation.Horizontal ?
                new Size(PageWidth * PageCount, PageHeight) :
                new Size(PageWidth, PageHeight * PageCount);
        }

        protected override Size ArrangeOverride(Size finalSize)
        {
            var needEasing = !disableAnimation;

            // If animation is disabled, reinitialize the layout
            if (disableAnimation && Children.Count > 0)
            {
                foreach (var page in pages.Cast<PanoramaPanelPage>())
                    for (int i = 0; i < page.Count; i++)
                    {
                        // Get the cell position
                        var cell = GetCellRect(page, i);

                        // Arrange the element at (0, 0)...
                        page[i].Arrange(new Rect(0, 0, CellWidth, CellHeight));

                        // ...then move it to the correct location
                        page[i].RenderTransform = CreateTransform(cell.X, cell.Y, DefaultScale, DefaultScale);
                    }

                if (!DesignerProperties.GetIsInDesignMode(this))
                    disableAnimation = false;
            }

            // Update the layout
            UpdateFluidLayout(needEasing);

            // Return parent panel size
            return Orientation == Orientation.Horizontal
                ? new Size(PageWidth * PageCount, PageHeight)
                : new Size(PageWidth, PageHeight * PageCount);
        }

        protected void UpdateFluidLayout(Boolean ease)
        {
            InvalidateVisual();
        }

        #region Layout

        protected Rect GetPageRect(PanoramaPanelPage page)
        {
            var index = pages.IndexOf(page);
            if (index < 0) throw new Exception("Page not found.");

            return Orientation == Orientation.Horizontal ? 
                new Rect(index * PageWidth, 0, PageWidth, PageHeight) : 
                new Rect(0, index * PageHeight, PageWidth, PageHeight);
        }

        protected Rect GetGridRect(PanoramaPanelPage page)
        {
            // Get the available rect
            var pageRect = GetPageRect(page);

            rowSize = (Int32) Math.Floor(PageWidth / CellWidth);
            columnSize = (Int32) Math.Floor(PageHeight / CellHeight);
            if (rowSize <= 0)
                rowSize = 1;
            if (columnSize <= 0)
                columnSize = 1;

            var x = pageRect.X + (PageWidth - rowSize * CellWidth) / 2.0;
            var y = pageRect.Y + (PageHeight - columnSize * CellHeight) / 2.0;

            return new Rect(x, y, rowSize * CellWidth, columnSize * CellHeight);
        }

        protected Rect GetCellRect(PanoramaPanelPage page, Int32 index)
        {
            // Get the rectangle of the grid for the page
            var gridRect = GetGridRect(page);

            // Get x and y positions of the cell
            var x = gridRect.X + index % rowSize * CellWidth;
            var y = gridRect.Y + Math.Floor((Double)index / columnSize) * CellHeight;

            return new Rect(x, y, CellWidth, CellHeight);
        }


        protected Int32 GetPageIndex(Point point)
        {
            if (Orientation == Orientation.Horizontal)
                return (Int32) Math.Floor(point.X / PageWidth);
            return (Int32) Math.Floor(point.Y / PageHeight);
        }

        protected Int32 GetCellIndex(Point point)
        {
            // Get the page index
            var pageIndex = GetPageIndex(point);

            // Convert to point relative to the page grid
            var pageGrid = GetGridRect(pages[pageIndex]);
            point.X -= pageGrid.X;
            point.Y -= pageGrid.Y;
            
            return (Int32) (Math.Floor(point.X / CellWidth) + Math.Floor(point.Y / CellHeight) * rowSize);
        }

        #endregion

        #region Layout & Transitions

        /// <summary>
        /// Creates tranfrorm for a UI element.
        /// </summary>
        /// <param name="translateX">Translation offset along the X axis.</param>
        /// <param name="translateY">Translation offset along the Y axis.</param>
        /// <param name="scaleX">Scale along the X axis.</param>
        /// <param name="scaleY">Scale along the Y axis.</param>
        /// <param name="rotationAngle">Rotation angle.</param>
        /// <returns></returns>
        protected TransformGroup CreateTransform(double translateX, double translateY, double scaleX, double scaleY)
        {
            var translate = new TranslateTransform { X = translateX, Y = translateY };
            var scale = new ScaleTransform(scaleX, scaleY);

            var group = new TransformGroup();
            group.Children.Add(scale);
            group.Children.Add(translate);

            return group;
        }

        /// <summary>
        /// Creates the transition animation for an element.
        /// </summary>
        /// <param name="element">Child element to move.</param>
        /// <param name="position">New position of the child element.</param>
        /// <param name="duration">Duration of the animation.</param>
        /// <returns></returns>
        protected Storyboard CreateTransition(UIElement element, Point position, TimeSpan duration, EasingFunctionBase easing)
        {
            // Animate along X axis
            var tx = new DoubleAnimation { To = position.X, Duration = duration };
            if (Easing != null)
                tx.EasingFunction = easing;
            Storyboard.SetTarget(tx, element);
            Storyboard.SetTargetProperty(tx, new PropertyPath("(UIElement.RenderTransform).(TransformGroup.Children)[1].(TranslateTransform.X)"));

            // Animate along Y axis
            var ty = new DoubleAnimation { To = position.Y, Duration = duration };
            if (Easing != null)
                ty.EasingFunction = easing;
            Storyboard.SetTarget(ty, element);
            Storyboard.SetTargetProperty(ty, new PropertyPath("(UIElement.RenderTransform).(TransformGroup.Children)[1].(TranslateTransform.Y)"));

            // Animate X axis scale
            var sx = new DoubleAnimation { To = 1.0D, Duration = duration };
            if (Easing != null)
                sx.EasingFunction = easing;
            Storyboard.SetTarget(sx, element);
            Storyboard.SetTargetProperty(sx, new PropertyPath("(UIElement.RenderTransform).(TransformGroup.Children)[0].(ScaleTransform.ScaleX)"));

            // Animate Y axis scale
            var sy = new DoubleAnimation { To = 1.0D, Duration = duration };
            if (Easing != null)
                sy.EasingFunction = easing;
            Storyboard.SetTarget(sy, element);
            Storyboard.SetTargetProperty(sy, new PropertyPath("(UIElement.RenderTransform).(TransformGroup.Children)[0].(ScaleTransform.ScaleY)"));

            // Assemble animation
            var board = new Storyboard { Duration = duration };
            board.Children.Add(tx);
            board.Children.Add(ty);
            board.Children.Add(sx);
            board.Children.Add(sy);

            board.Completed += (s, e) =>
                element.SetValue(ZIndexProperty, DefaultZ);

            return board;
        }

        #endregion

        #region Child Management

        internal void AddPage(PanoramaPanelPage page)
        {
            // Set page's parent
            page.Parent = this;

            // Add all children to the panel
            foreach (var child in page.Children)
                Children.Add(child);

            // Refresh layout
            disableAnimation = true;
            InvalidateVisual();
        }

        internal void RemovePage(PanoramaPanelPage page)
        {
            // Remove all children
            foreach (var child in page.Children)
                Children.Remove(child);

            // Refresh layoutt
            disableAnimation = true;
            InvalidateVisual();
        }

        #endregion

        #region Drag & Drop

        private Point dragStart;
        private UIElement dragging;
        private UIElement dragged;

        internal void OnDragStart(FrameworkElement child, Point origin)
        {
            if (child == null)
                return;

            Dispatcher.Invoke(() =>
            {
                child.Opacity = DragOpacity;
                child.SetValue(ZIndexProperty, DragZ);
                // Dragging point within the moving element
                dragStart = new Point(origin.X * DragScale, origin.Y * DragScale);
                // Apply transform without moving the element
                var translatePosition = child.TranslatePoint(new Point(-child.Margin.Left, -child.Margin.Top), this);
                child.RenderTransform = CreateTransform(translatePosition.X, translatePosition.Y, DragScale, DragScale);
                // Capture further mouse events
                child.CaptureMouse();
                dragging = child;
                dragged = null;
            });
        }

        internal void OnDragMove(FrameworkElement child, Point origin, Point position)
        {
            if (child == null)
                return;

            Dispatcher.Invoke(() =>
            {
                // Set up render transform to move the element
                child.RenderTransform = CreateTransform(position.X - dragStart.X, position.Y - dragStart.Y, DragScale,
                    DragScale);

                // TODO Update Layout on move
                
            });
        }

        internal void OnDragEnd(FrameworkElement child, Point origin, Point position)
        {
            if (child == null)
                return;

            Dispatcher.Invoke(() =>
            {
                // Get current page/cell
                var page = GetPageIndex(position);
                var cell = GetCellIndex(position);
                

                // Reset opacity
                child.Opacity = DefaultOpacity;
                child.SetValue(ZIndexProperty, TransitionZ);
                child.ReleaseMouseCapture();

                // Keep a reference
                dragged = dragging;
                dragging = null;

                
            });
        }

        #endregion
    }
}
