using FluentAssertions;
using Microsoft.Xna.Framework;

namespace Slate.Client.UI.Tests
{
    public class LayoutElementMeasureTests
    {
        public class BasicLayoutElement : LayoutElement {}

        [Fact]
        public void LayoutElement_MeasureConsidersWidthAndHeight()
        {
            var sut = new BasicLayoutElement
            {
                Width = 400,
                Height = 200
            };

            sut.Measure();
            sut.DesiredSize.Should().Be(new Vector2(400, 200));
        }

        [Fact]
        public void LayoutElement_MeasureConsidersMargin()
        {
            var sut = new BasicLayoutElement
            {
                Margin = new Thickness(10, 20)
            };

            sut.Measure();
            sut.DesiredSize.Should().Be(new Vector2(20, 40));
        }

        [Fact]
        public void LayoutElement_MeasureConsidersPadding()
        {
            var sut = new BasicLayoutElement
            {
                Padding = new Thickness(15, 25)
            };

            sut.Measure();
            sut.DesiredSize.Should().Be(new Vector2(30, 50));
        }

        [Fact]
        public void LayoutElement_MeasureConsidersWidthHeightMarginAndPadding()
        {
            var sut = new BasicLayoutElement
            {
                Width = 400,
                Height = 200,
                Margin = new Thickness(10, 20),
                Padding = new Thickness(15, 25)
            };

            sut.Measure();
            sut.DesiredSize.Should().Be(new Vector2(450, 290));
        }
    }
}
