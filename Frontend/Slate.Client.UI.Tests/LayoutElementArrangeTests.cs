using FluentAssertions;

namespace Slate.Client.UI.Tests
{
    public class LayoutElementArrangeTests
    {
        class BasicLayoutElement : LayoutElement { }

        [Fact]
        public void LayoutElement_Arrange_ArrangesHorizontalStretch()
        {
            var sut = new BasicLayoutElement
            {
                Width = 40,
                HorizontalAlignment = HorizontalAlignment.Stretch
            };
            sut.Measure();
            sut.Arrange(new Rectangle(0, 0, 400, 0));

            sut.VisualOffset.X.Should().Be(0);
            sut.ActualSize.X.Should().Be(400);
        }

        [Fact]
        public void LayoutElement_Arrange_ArrangesHorizontalLeft()
        {
            var sut = new BasicLayoutElement
            {
                Width = 40,
                HorizontalAlignment = HorizontalAlignment.Left
            };
            sut.Measure();
            sut.Arrange(new Rectangle(0, 0, 400, 0));

            sut.VisualOffset.X.Should().Be(0);
            sut.ActualSize.X.Should().Be(40);
        }

        [Fact]
        public void LayoutElement_Arrange_ArrangesHorizontalRight()
        {
            var sut = new BasicLayoutElement
            {
                Width = 40,
                HorizontalAlignment = HorizontalAlignment.Right
            };
            sut.Measure();
            sut.Arrange(new Rectangle(0, 0, 400, 0));

            sut.VisualOffset.X.Should().Be(360);
            sut.ActualSize.X.Should().Be(40);
        }

        [Fact]
        public void LayoutElement_Arrange_ArrangesHorizontalCenter()
        {
            var sut = new BasicLayoutElement
            {
                Width = 40,
                HorizontalAlignment = HorizontalAlignment.Center
            };
            sut.Measure();
            sut.Arrange(new Rectangle(0, 0, 400, 0));

            sut.VisualOffset.X.Should().Be(180);
            sut.ActualSize.X.Should().Be(40);
        }

        [Fact]
        public void LayoutElement_Arrange_ArrangesVerticalStretch()
        {
            var sut = new BasicLayoutElement
            {
                Height = 40,
                VerticalAlignment = VerticalAlignment.Stretch
            };
            sut.Measure();
            sut.Arrange(new Rectangle(0, 0, 0, 400));

            sut.VisualOffset.Y.Should().Be(0);
            sut.ActualSize.Y.Should().Be(400);
        }

        [Fact]
        public void LayoutElement_Arrange_ArrangesVerticalTop()
        {
            var sut = new BasicLayoutElement
            {
                Height = 40,
                VerticalAlignment = VerticalAlignment.Top
            };
            sut.Measure();
            sut.Arrange(new Rectangle(0, 0, 0, 400));

            sut.VisualOffset.Y.Should().Be(0);
            sut.ActualSize.Y.Should().Be(40);
        }

        [Fact]
        public void LayoutElement_Arrange_ArrangesVerticalBottom()
        {
            var sut = new BasicLayoutElement
            {
                Height = 40,
                VerticalAlignment = VerticalAlignment.Bottom
            };
            sut.Measure();
            sut.Arrange(new Rectangle(0, 0, 0, 400));

            sut.VisualOffset.Y.Should().Be(360);
            sut.ActualSize.Y.Should().Be(40);
        }

        [Fact]
        public void LayoutElement_Arrange_ArrangesVerticalCenter()
        {
            var sut = new BasicLayoutElement
            {
                Height = 40,
                VerticalAlignment = VerticalAlignment.Center
            };
            sut.Measure();
            sut.Arrange(new Rectangle(0, 0, 0, 400));

            sut.VisualOffset.Y.Should().Be(180);
            sut.ActualSize.Y.Should().Be(40);
        }

        [Fact]
        public void LayoutElement_ArrangeWithMargin_ArrangesStretched()
        {
            var sut = new BasicLayoutElement
            {
                Width = 80,
                Height = 40,
                VerticalAlignment = VerticalAlignment.Stretch,
                HorizontalAlignment = HorizontalAlignment.Stretch,
                Margin = new Thickness(4, 2, 8, 4)
            };
            sut.Measure();
            sut.Arrange(new Rectangle(0, 0, 800, 400));

            sut.VisualOffset.X.Should().Be(4);
            sut.VisualOffset.Y.Should().Be(2);
            sut.ActualSize.X.Should().Be(800 - 8 - 4);
            sut.ActualSize.Y.Should().Be(400 - 4 - 2);
        }

        [Fact]
        public void LayoutElement_ArrangeWithMargin_ArrangesTopLeft()
        {
            var sut = new BasicLayoutElement
            {
                Width = 80,
                Height = 40,
                VerticalAlignment = VerticalAlignment.Top,
                HorizontalAlignment = HorizontalAlignment.Left,
                Margin = new Thickness(4, 2, 8, 4)
            };
            sut.Measure();
            sut.Arrange(new Rectangle(0, 0, 800, 400));

            sut.VisualOffset.X.Should().Be(4);
            sut.VisualOffset.Y.Should().Be(2);
            sut.ActualSize.X.Should().Be(80);
            sut.ActualSize.Y.Should().Be(40);
        }

        [Fact]
        public void LayoutElement_ArrangeWithMargin_ArrangesBottomRight()
        {
            var sut = new BasicLayoutElement
            {
                Width = 80,
                Height = 40,
                VerticalAlignment = VerticalAlignment.Bottom,
                HorizontalAlignment = HorizontalAlignment.Right,
                Margin = new Thickness(4, 2, 8, 4)
            };
            sut.Measure();
            sut.Arrange(new Rectangle(0, 0, 800, 400));

            sut.VisualOffset.X.Should().Be(712);
            sut.VisualOffset.Y.Should().Be(356);
            sut.ActualSize.X.Should().Be(80);
            sut.ActualSize.Y.Should().Be(40);
        }

        [Fact]
        public void LayoutElement_ArrangeWithMargin_ArrangesCentered()
        {
            var sut = new BasicLayoutElement
            {
                Width = 80,
                Height = 40,
                VerticalAlignment = VerticalAlignment.Center,
                HorizontalAlignment = HorizontalAlignment.Center,
                Margin = new Thickness(4, 2, 8, 4)
            };
            sut.Measure();
            var available = new Rectangle(0, 0, 800, 400);
            sut.Arrange(available);

            sut.VisualOffset.X.Should().Be(358);
            sut.VisualOffset.Y.Should().Be(179);
            sut.ActualSize.X.Should().Be(80);
            sut.ActualSize.Y.Should().Be(40);
        }
    }
}