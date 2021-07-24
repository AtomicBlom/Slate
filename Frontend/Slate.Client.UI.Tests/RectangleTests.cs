using FluentAssertions;

namespace Slate.Client.UI.Tests
{
    public class RectangleTests
    {
        [Fact]
        public void Rectangle_InflateWithXY_InflatesCorrectly()
        {
            var rect = new Rectangle(50, 100, 400, 500);

            var result = rect.Inflate(10, 10);
            result.Left.Should().Be(40);
            result.Top.Should().Be(90);
            result.Right.Should().Be(460);
            result.Bottom.Should().Be(610);
            result.Width.Should().Be(420);
            result.Height.Should().Be(520);
        }

        [Fact]
        public void Rectangle_InflateWithThickness_InflatesCorrectly()
        {
            var rect = new Rectangle(50, 100, 400, 500);
            var inflation = new Thickness(10, 10, 20, 20);
            var result = rect.Inflate(inflation);
            result.Left.Should().Be(40);
            result.Top.Should().Be(90);
            result.Right.Should().Be(470);
            result.Bottom.Should().Be(620);
            result.Width.Should().Be(430);
            result.Height.Should().Be(530);
        }

        [Fact]
        public void Rectangle_DeflateWithThickness_DeflatesCorrectly()
        {
            var rect = new Rectangle(50, 100, 400, 500);
            var inflation = new Thickness(10, 10, 20, 20);
            var result = rect.Deflate(inflation);
            result.Left.Should().Be(60);
            result.Top.Should().Be(110);
            result.Right.Should().Be(430);
            result.Bottom.Should().Be(580);
            result.Width.Should().Be(370);
            result.Height.Should().Be(470);
        }
    }
}
