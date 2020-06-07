using System;
using System.Linq;
using Xamarin.Forms;
using XamarinShoppingList1.Helpers.DragAndDrop.Interfaces;

namespace XamarinShoppingList1.Helpers.DragAndDrop.Extensions
{
    public static class DragAndDropExtensions
    {
        #region IDragAndDropMovingView

        private static VisualElement GetContainer(this IDraggableAndDroppable view)
        {
            if (!(view is VisualElement visualElement))
                throw new Exception($"{nameof(IDraggableAndDroppable)} can only be an interface on a {nameof(View)}");
            var dropContainer = visualElement.GetFirstParentOfType<IDragAndDropContainer>();
            if (dropContainer is VisualElement output) return output;
            return visualElement.GetFirstParentOfType<Page>();
        }

        public static void InitializeDragAndDrop(this IDraggableAndDroppable view)
        {
            if (!(view is View output))
                throw new Exception($"{nameof(IDraggableAndDroppable)} can only be an interface on a {nameof(View)}");

            var panGestureRecognizer = new PanGestureRecognizer
            {
                TouchPoints = 1
            };
            panGestureRecognizer.PanUpdated += PanGestureRecognizer_PanUpdated;
            //output.GestureRecognizers.Clear();
            output.GestureRecognizers.Add(panGestureRecognizer);
        }

        private static void PanGestureRecognizer_PanUpdated(object sender, PanUpdatedEventArgs e)
        {
            if (!(sender is VisualElement visualElement) ||
                !(sender is IDraggableAndDroppable dragAndDropMovingView))
                throw new Exception($"{nameof(IDraggableAndDroppable)} can only be an interface on a {nameof(View)}");

            var dropContainer = dragAndDropMovingView.GetContainer();
            // Update on Drop
            if (e.StatusType == GestureStatus.Completed)
                dropContainer?.DragAndDroppableViewDropped(dragAndDropMovingView);

            // Coordinate of center = Initial position of top left corner + Pan transformation + Size / 2 
            var screenCoordinates = visualElement.GetScreenCoordinates();

            switch (e.StatusType)
            {
                case GestureStatus.Running:
                    dragAndDropMovingView.IsDrag = true;
                    visualElement.TranslationX = (Device.RuntimePlatform == Device.Android ? visualElement.TranslationX : 0) + e.TotalX;
                    visualElement.TranslationY = (Device.RuntimePlatform == Device.Android ? visualElement.TranslationY : 0) + e.TotalY;
                    dragAndDropMovingView.ScreenX = screenCoordinates.X + (Device.RuntimePlatform == Device.Android ? visualElement.TranslationX : 0) + e.TotalX + visualElement.Width / 2;
                    dragAndDropMovingView.ScreenY = screenCoordinates.Y + (Device.RuntimePlatform == Device.Android ? visualElement.TranslationY : 0) + e.TotalY + visualElement.Height / 2;
                    break;
                case GestureStatus.Completed:
                case GestureStatus.Canceled:
                    visualElement.TranslateTo(0, 0, 200);
                    dragAndDropMovingView.ScreenX = 0;
                    dragAndDropMovingView.ScreenY = 0;
                    dragAndDropMovingView.IsDrag = false;
                    break;
            }

            // Update on Hover
            dropContainer?.UpdateHoverStatuses();
        }

        #endregion

        #region IDragAndDropContainer

        private static void UpdateHoverStatuses(this VisualElement view)
        {
            var allReceivers = view.GetAllChildrenOfType<IDragAndDropHoverableView>();

            var reveiver = allReceivers.Cast<IDraggableAndDroppable>().Where(a => a.ScreenX > 0 || a.ScreenY > 0).Cast<IDragAndDropHoverableView>().FirstOrDefault();
            if (reveiver != null)
            {
                allReceivers.Remove(reveiver);
            }

            var allSenders = view.GetAllChildrenOfType<IDraggableAndDroppable>();
            //  allSenders.Remove(reveiver as IDragAndDropMovingView);   

            foreach (var receiver in allReceivers)
            {
                if (!(receiver is VisualElement veReceiver)) continue;

                var x = veReceiver.GetScreenCoordinates().X;
                var y = veReceiver.GetScreenCoordinates().Y;
                var width = veReceiver.Width;
                var height = veReceiver.Height;
                receiver.OnHovered(allSenders.Where(sender => sender.ScreenX >= x &&
                                                              sender.ScreenX <= x + width &&
                                                              sender.ScreenY >= y &&
                                                              sender.ScreenY <= y + height
                ).ToList());
            }
        }

        private static void DragAndDroppableViewDropped(this VisualElement view, IDraggableAndDroppable sender)
        {
            var allReceivers = view.GetAllChildrenOfType<IDraggableAndDroppable>();

            allReceivers.Reverse();

            foreach (var receiver in allReceivers)
            {
                if (!(receiver is VisualElement veReceiver)) continue;
                //if (receiver == sender) { 

                //    continue; 
                //}
                var x = veReceiver.GetScreenCoordinates().X;
                var y = veReceiver.GetScreenCoordinates().Y;
                var width = veReceiver.Width;
                var height = veReceiver.Height;
                if (sender.ScreenX >= x &&
                    sender.ScreenX <= x + width &&
                    sender.ScreenY >= y &&
                    sender.ScreenY <= y + height)
                    receiver.OnDropReceived(sender);
            }
        }

        #endregion
    }
}