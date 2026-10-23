using Microsoft.Xna.Framework;
using RTS;

public abstract class UIAnchor
{
    public abstract Point GetScreenOffset(UIElement element);
    public virtual Point GetScreenPosition(UIElement element) => GetScreenOffset(element) + element.GetScreenUnanchoredPosition();

}

public class SideAnchor(SideAnchor.SideHorizontal horizontalSide, SideAnchor.SideVerical verticalSide) : UIAnchor
{
    public enum SideHorizontal
    {
        Left,
        Middle,
        Right
    }
    public enum SideVerical
    {
        Top,
        Middle,
        Bottom
    }

    public static int GetXOffset(UIElement element, SideHorizontal side)
    {
        int numSide = (int)side;
        Point parentSize = element.Parent == null ? Main.Dimensions.Bounds.Size : element.Parent.Size;

        return numSide switch
        {
            0 => 0,
            1 => (parentSize.X / 2) - element.Size.X,
            2 => parentSize.X - element.Size.X,
            _ => 0,
        };
    }
    public static int GetYOffset(UIElement element, SideVerical side)
    {
        int numSide = (int)side;
        Point parentSize = element.Parent == null ? Main.Dimensions.Bounds.Size : element.Parent.Size;

        return numSide switch
        {
            0 => 0,
            1 => (parentSize.Y / 2) - element.Size.Y,
            2 => parentSize.Y - element.Size.Y,
            _ => 0,
        };
    }

    public SideHorizontal HorizontalSide = horizontalSide;
    public SideVerical VerticalSide = verticalSide;

    public override Point GetScreenOffset(UIElement element) => new Point(GetXOffset(element, HorizontalSide), GetYOffset(element, VerticalSide));
}