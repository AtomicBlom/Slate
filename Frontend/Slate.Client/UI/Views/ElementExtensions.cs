using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows.Input;
using MLEM.Misc;
using MLEM.Ui;
using MLEM.Ui.Elements;

namespace Slate.Client.UI.Views
{
	public static class RootElementExtensions
	{
		public static RootElement FadeOut(this RootElement element, TimeSpan? duration = null, Easings.Easing? easing = null, bool remove = false)
		{
			var time = duration ?? TimeSpan.FromSeconds(1);
			easing ??= Easings.InCubic;

			var startOpacity = element.Element.DrawAlpha;
			var sw = Stopwatch.StartNew();
			Task.Run(async () =>
			{
				while (sw.ElapsedMilliseconds < time.TotalMilliseconds)
				{
					var progress = (float)(sw.ElapsedMilliseconds / time.TotalMilliseconds);

					var easedProgress = easing(progress);
					var opacity = startOpacity * easedProgress;

					element.Element.DrawAlpha = opacity;
					await RudeEngineGame.NextUpdate;
				}

				if (remove)
				{
					element.System.Remove(element.Name);
				}
			});
			return element;
		}
	}

	public static class ElementExtensions
	{
		public static TElement AddChildren<TElement>(this TElement parent, params Element[] children)
			where TElement : Element
		{
			foreach (var element in children) parent.AddChild(element);

			return parent;
		}

		public static T BindPressed<T>(this T element, ICommand command, Func<object>? resolveParameter = null)
			where T : Element
		{
			command.CanExecuteChanged += CommandOnCanExecuteChanged;
			element.OnPressed += OnPressed;
			element.OnDisposed += OnDispose;

			CommandOnCanExecuteChanged(element, EventArgs.Empty);

			return element;

			void OnDispose(Element e)
			{
				command.CanExecuteChanged -= CommandOnCanExecuteChanged;
				e.OnPressed -= OnPressed;
				e.OnDisposed -= OnDispose;
			}

			void CommandOnCanExecuteChanged(object? sender, EventArgs e)
			{
				if (element is Button b) b.IsDisabled = !command.CanExecute(resolveParameter?.Invoke());
			}

			void OnPressed(Element e)
			{
				var parameter = resolveParameter?.Invoke();
				if (command.CanExecute(parameter)) command.Execute(parameter);
			}
		}

		//FIXME: Make a source generator so we can avoid the Expression crap.
		public static TElement BindText<TElement, TViewModel>(this TElement textField, TViewModel viewModel,
			Expression<Func<TViewModel, string>> property)
			where TViewModel : INotifyPropertyChanged
			where TElement : TextField
		{
			if (property.Body is not MemberExpression propertyExpression)
				throw new ArgumentException("Expected a property expression", nameof(property));
			if (propertyExpression.Member is not PropertyInfo propertyInfo)
				throw new ArgumentException("Expected a property expression", nameof(property));
			var setMethod = propertyInfo.SetMethod;
			var canSetOnViewModel = setMethod is not null && setMethod.IsPublic;

			var compiledProperty = property.Compile();

			if (canSetOnViewModel) textField.OnTextChange += OnTextChange;

			textField.OnDisposed += OnDispose;
			viewModel.PropertyChanged += OnViewModelPropertyChanged;

			OnViewModelPropertyChanged(textField, new PropertyChangedEventArgs(propertyInfo.Name));

			return textField;

			void OnDispose(Element e)
			{
				textField.OnTextChange -= OnTextChange;
				e.OnDisposed -= OnDispose;
				viewModel.PropertyChanged -= OnViewModelPropertyChanged;
			}

			void OnViewModelPropertyChanged(object? sender, PropertyChangedEventArgs e)
			{
				if (string.IsNullOrEmpty(e.PropertyName) || e.PropertyName == propertyInfo.Name)
				{
					var newText = compiledProperty.Invoke(viewModel);
					if (textField.Text != newText) textField.SetText(newText);
				}
			}

			void OnTextChange(TextField field, string text)
			{
				setMethod?.Invoke(viewModel, new object?[] { text });
			}
		}

		public static TElement BindCollection<TElement, TViewModel, TItemType>(this TElement group,
			TViewModel viewModel,
			Expression<Func<TViewModel, IEnumerable<TItemType>>> property,
			Func<TItemType, Element> elementBuilder)
			where TElement : Element
			where TViewModel : INotifyPropertyChanged
		{
			//TODO: ObservableCollection
			if (property.Body is not MemberExpression propertyExpression)
				throw new ArgumentException("Expected a property expression", nameof(property));
			if (propertyExpression.Member is not PropertyInfo propertyInfo)
				throw new ArgumentException("Expected a property expression", nameof(property));

			var compiledProperty = property.Compile();
			group.OnDisposed += OnDispose;
			viewModel.PropertyChanged += OnViewModelPropertyChanged;

			OnViewModelPropertyChanged(group, new PropertyChangedEventArgs(propertyInfo.Name));

			return group;

			void OnDispose(Element e)
			{
				e.OnDisposed -= OnDispose;
				viewModel.PropertyChanged -= OnViewModelPropertyChanged;
			}

			void OnViewModelPropertyChanged(object? sender, PropertyChangedEventArgs e)
			{
				if (string.IsNullOrEmpty(e.PropertyName) || e.PropertyName == propertyInfo.Name)
				{
					//FIXME: Detect if the collection has actually changed.
					group.RemoveChildren();
					var items = compiledProperty.Invoke(viewModel);
					foreach (var item in items) @group.AddChild(elementBuilder(item));
				}
			}
		}

		public static TElement BindParagraph<TElement, TViewModel>(
			this TElement element,
			TViewModel viewModel,
			Expression<Func<TViewModel, string?>> property)
			where TElement : Paragraph

		{
			return BindParagraph(element, viewModel, property, new NoOpConverter<string?>());
		}

		public static TElement BindParagraph<TElement, TViewModel, TPropertyType>(
			this TElement paragraph,
			TViewModel viewModel,
			Expression<Func<TViewModel, TPropertyType>> property,
			IConverter<TPropertyType, string?> converter)
			where TElement : Paragraph
		{
			Expression expression;
			if (property.Body.NodeType == ExpressionType.Convert)
			{
				if (property.Body is not UnaryExpression unaryExpression)
					throw new ArgumentException("Could not get unaryExpression from Convert node type");
				expression = unaryExpression.Operand;
			}
			else
			{
				expression = property.Body;
			}
			if (expression is not MemberExpression propertyExpression)
				throw new ArgumentException("Expected a property expression", nameof(property));
			if (propertyExpression.Member is not PropertyInfo propertyInfo)
				throw new ArgumentException("Expected a property expression", nameof(property));


			var compiledProperty = property.Compile();
			paragraph.OnDisposed += OnDispose;
			if (viewModel is INotifyPropertyChanged inpcSetter)
			{
				inpcSetter.PropertyChanged += OnViewModelPropertyChanged;
			}

			OnViewModelPropertyChanged(paragraph, new PropertyChangedEventArgs(propertyInfo.Name));

			return paragraph;

			void OnDispose(Element e)
			{
				e.OnDisposed -= OnDispose;
				if (viewModel is INotifyPropertyChanged inpcUnsetter)
				{
					inpcUnsetter.PropertyChanged += OnViewModelPropertyChanged;
				}
			}

			void OnViewModelPropertyChanged(object? sender, PropertyChangedEventArgs e)
			{
				if (string.IsNullOrEmpty(e.PropertyName) || e.PropertyName == propertyInfo.Name)
				{
					paragraph.Text = converter.Convert(compiledProperty.Invoke(viewModel));
				}
			}
		}

		private class NoOpConverter<T> : IConverter<T, T>
		{
			public T Convert(T value) => value;
		}
	}

	public interface IConverter<in TSource, out TDestination>
	{
		public TDestination Convert(TSource value);
	}

	public class ToStringConverter : IConverter<object?, string?>
	{
		public string? Convert(object? value)
		{
			return value?.ToString();
		}
	}
}