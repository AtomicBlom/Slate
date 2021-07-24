using System.Collections.Generic;
using System.Threading.Tasks;
using FluentAssertions;

namespace Slate.Client.UI.Tests
{
    public class UIElementTests
    {
        internal class BasicUIElement : UIElement {}

        [Fact]
        public void UIElement_GetValue_ReturnsDefaultIfNotSet()
        {
            var sut = new BasicUIElement();
            var property = new UIProperty<string>("Test", typeof(UIElementTests), () => "test string");

            sut.GetProperty(property).Should().Be("test string");
        }

        [Fact]
        public void UIElement_SetValue_SetsTheValue()
        {
            var sut = new BasicUIElement();
            var property = new UIProperty<string>("Test", typeof(UIElementTests), () => "test string");
            sut.SetProperty(property, "custom string");

            sut.GetProperty(property).Should().Be("custom string");
        }

        [Fact]
        public void UIElement_SetValueTwice_OverwritesPreviousValue()
        {
            var sut = new BasicUIElement();
            var property = new UIProperty<string>("Test", typeof(UIElementTests), () => "test string");
            sut.SetProperty(property, "custom string");
            sut.SetProperty(property, "updated string");

            sut.GetProperty(property).Should().Be("updated string");
        }

        [Fact]
        public void UIElement_ClearValue_RevertsToDefault()
        {
            var sut = new BasicUIElement();
            var property = new UIProperty<string>("Test", typeof(UIElementTests), () => "test string");
            sut.SetProperty(property, "custom string");
            sut.ClearProperty(property);

            sut.GetProperty(property).Should().Be("test string");
        }

        [Fact]
        public void UIElement_GetValue_FiresPropertyChangedForDefaultValue()
        {
            var invocations = 0;
            var sut = new BasicUIElement();
            var property = new UIProperty<string>("Test", typeof(UIElementTests), () => "test string", OnChangedAction);

            sut.GetProperty(property).Should().Be("test string");
            invocations.Should().Be(1);

            void OnChangedAction(UIElement uielement, string? oldvalue, string newvalue)
            {
                invocations++;
            }
        }

        [Fact]
        public void UIElement_GetValue_ShouldNotCallPropertyChangedForDefaultOnFirstAccess()
        {
            var invocations = 0;
            var sut = new BasicUIElement();
            var property = new UIProperty<string>("Test", typeof(UIElementTests), () => "test string", OnChangedAction);
            sut.SetProperty(property, "rawr");

            sut.GetProperty(property).Should().Be("rawr");
            invocations.Should().Be(1);

            void OnChangedAction(UIElement uielement, string? oldvalue, string newvalue)
            {
                oldvalue.Should().BeNull();
                invocations++;
            }
        }

        [Fact]
        public void UIElement_SetValueTwice_ShouldCallPropertyChangedTwice()
        {
            var newValues = new List<string>();
            var oldValues = new List<string?>();
            var sut = new BasicUIElement();
            var property = new UIProperty<string>("Test", typeof(UIElementTests), () => "test string", OnChangedAction);
            sut.SetProperty(property, "rawr");
            sut.SetProperty(property, "beep");

            sut.GetProperty(property).Should().Be("beep");
            newValues.Should().HaveCount(2);
            newValues.Should().ContainInOrder("rawr", "beep");
            oldValues.Should().HaveCount(2);
            oldValues.Should().ContainInOrder((string?)null, "rawr");

            void OnChangedAction(UIElement uielement, string? oldvalue, string newvalue)
            {
                newValues.Add(newvalue);
                oldValues.Add(oldvalue);
            }
        }

        [Fact]
        public void UIElement_WithUIPropertiesWithOnlyDifferentOwners_DistinguishesValues()
        {
            var sut = new BasicUIElement();
            var property = new UIProperty<string>("Test", typeof(UIElementTests), () => "UIElementTests string");
            var property2 = new UIProperty<string>("Test", typeof(UIElement), () => "UIElement string");

            sut.GetProperty(property).Should().Be("UIElementTests string");
            sut.GetProperty(property2).Should().Be("UIElement string");
        }
    }
}
