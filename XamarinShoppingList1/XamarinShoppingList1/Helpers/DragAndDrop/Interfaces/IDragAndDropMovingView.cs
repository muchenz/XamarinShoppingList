namespace XamarinShoppingList1.Helpers.DragAndDrop.Interfaces
{
    public interface IDraggableAndDroppable
    {
        double ScreenX { get; set; }
        double ScreenY { get; set; }

        bool IsDrag { get; set; }

        void OnDropReceived(IDraggableAndDroppable view);
    }
}