using System.Collections.Generic;
using System.Linq;
using Xamarin.Forms;
using XamarinShoppingList1.Helpers.DragAndDrop.Extensions;
using XamarinShoppingList1.Helpers.DragAndDrop.Interfaces;

namespace XamarinShoppingList1.Helpers.DragAndDrop.Components
{
    public class DragAndDropSample3MovingView : Frame, IDraggableAndDroppable, IDragAndDropHoverableView, IDraggable
    {
        

        public void OnHovered(List<IDraggableAndDroppable> views)
        {
            Opacity = views.Any() ? .3 : 1;
        }

        public void OnDropReceived(IDraggableAndDroppable view)
        {
            if (view is IDraggable sender)

                BackgroundColor = sender.BackgroundColor;
        }
        /// <summary>
        /// ///////////////////////////////////////////////////////////////////
        /// </summary>

        public bool IsDrag { get; set; }


        public double ScreenX { get; set; }
        public double ScreenY { get; set; }

        protected override void OnParentSet()
        {
            base.OnParentSet();
            this.InitializeDragAndDrop();
        }
    }
}