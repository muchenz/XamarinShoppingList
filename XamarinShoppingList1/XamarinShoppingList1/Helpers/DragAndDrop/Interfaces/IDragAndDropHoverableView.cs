using System.Collections.Generic;

namespace XamarinShoppingList1.Helpers.DragAndDrop.Interfaces
{
    public interface IDragAndDropHoverableView
    {
        void OnHovered(List<IDraggableAndDroppable> views);
    }
}