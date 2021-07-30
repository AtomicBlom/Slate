using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace CastIron.Engine.Debugging
{
    public class DebugInfoSink : DrawableGameComponent, IDebugInfoSink, ILoadContent
    {
        private readonly Dictionary<DebugInfoCorner, DebugInfoSection> _debugInfoCorner = new()
        {
            {DebugInfoCorner.TopLeft, new DebugInfoSection(false)},
            {DebugInfoCorner.TopRight, new DebugInfoSection(false)},
            {DebugInfoCorner.BottomLeft, new DebugInfoSection(true)},
            {DebugInfoCorner.BottomRight, new DebugInfoSection(true)}
        };

        private readonly SpriteBatch _spriteBatch;
        private SpriteFont? _font;
        private Vector2 _spaceSize;

        public DebugInfoLine AddDebugInfo(DebugInfoCorner corner, string header, Vector3 vector)
        {
            return AddDebugInfo(corner, header, $"(x: {vector.X:0.000}, y: {vector.Y:0.000}, Z: {vector.Z:0.000})");
        }

        public DebugInfoLine AddDebugInfo(DebugInfoCorner corner, string header)
        {
	        return AddDebugInfo(corner, header, (string?)null);
        }

        public DebugInfoLine AddDebugInfo<T>(DebugInfoCorner corner, string header, T item)
        {
            string? text = item as string;
	        if (!(item is null) && !(item is string))
	        {
		        text = item?.ToString();
	        }

	        var section = string.IsNullOrEmpty(text) ? new DebugInfoLine(header) : new DebugInfoLine().Add(header, text);
	        var collection = _debugInfoCorner[corner];
	        collection.Add(section);
	        
            return section;
        }

        public DebugInfoSink(Game game) : base(game)
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);
            Enabled = true;
        }

        public override void Draw(GameTime gameTime)
        {
            if (_font == null) return;
            _spriteBatch.Begin();

			// Originally I made this all very generic, however all the branching code removed a good 200fps out of 3,000,
			// so I unrolled it.
            TopLeft();
			TopRight();
            BottomLeft();
            BottomRight();

            _spriteBatch.End();
        }

        private Vector2 DrawStringForLeftAlignment(string text, Color color, Vector2 pos)
        {
			Debug.Assert(_font != null);
	        _spriteBatch.DrawString(_font, text, pos, color);
	        var (x, y) = pos;
	        return new Vector2(x + _font.MeasureString(text).X, y);
        }

        private Vector2 DrawStringForRightAligned(string text, Color color, Vector2 pos)
        {
	        var (x, y) = pos;

            Debug.Assert(_font != null);
            var f = _font.MeasureString(text).X;
	        pos.X -= f;
	        _spriteBatch.DrawString(_font, text, pos, color);

	        return new Vector2(x - f, y);
        }

        private void TopLeft()
        {
	        var sections = _debugInfoCorner[DebugInfoCorner.TopLeft];
	        var textPos = new Vector2();
			
	        foreach (var section in sections)
	        {
		        textPos.X = 0;
		        var subHeaderColor = Color.Gray;
		        if (!string.IsNullOrEmpty(section.SectionHeader))
		        {
			        textPos = DrawStringForLeftAlignment($"{section.SectionHeader}: ", subHeaderColor, textPos);
			        subHeaderColor = Color.LightGray;
		        }

		        foreach (var debugInfo in section)
		        {
			        textPos = DrawStringForLeftAlignment($"{debugInfo.Header}: ", subHeaderColor, textPos);
			        textPos = DrawStringForLeftAlignment(debugInfo.Text, Color.White, textPos);
			        textPos.X += _spaceSize.X;
		        }

		        textPos.Y += _spaceSize.Y;
	        }

	        sections.Clear();
        }

        private void TopRight()
        {
	        var sections = _debugInfoCorner[DebugInfoCorner.TopRight];
	        var textPos = new Vector2();

	        foreach (var section in sections)
	        {
		        textPos.X = GraphicsDevice.Viewport.Width;

		        var subHeaderColor = !string.IsNullOrEmpty(section.SectionHeader) ? Color.LightGray : Color.Gray;
                foreach (var debugInfo in section.Reverse())
		        {
			        textPos = DrawStringForRightAligned(debugInfo.Text, Color.White, textPos);
			        textPos = DrawStringForRightAligned($"{debugInfo.Header}: ", subHeaderColor, textPos);
			        textPos.X -= _spaceSize.X;
		        }

		        if (!string.IsNullOrEmpty(section.SectionHeader))
		        {
			        textPos = DrawStringForRightAligned($"{section.SectionHeader}: ", Color.Gray, textPos);
		        }

                textPos.Y += _spaceSize.Y;
	        }

	        sections.Clear();
        }

        private void BottomLeft()
        {
	        var sections = _debugInfoCorner[DebugInfoCorner.BottomLeft];
	        var textPos = new Vector2 {Y = GraphicsDevice.Viewport.Height - _spaceSize.Y};

	        foreach (var section in sections)
	        {
		        textPos.X = 0;
		        var subHeaderColor = Color.Gray;
		        if (!string.IsNullOrEmpty(section.SectionHeader))
		        {
			        textPos = DrawStringForLeftAlignment($"{section.SectionHeader}: ", subHeaderColor, textPos);
			        subHeaderColor = Color.LightGray;
		        }

		        foreach (var debugInfo in section)
		        {
			        textPos = DrawStringForLeftAlignment($"{debugInfo.Header}: ", subHeaderColor, textPos);
			        textPos = DrawStringForLeftAlignment(debugInfo.Text, Color.White, textPos);
			        textPos.X += _spaceSize.X;
		        }

		        textPos.Y -= _spaceSize.Y;
	        }

	        sections.Clear();
        }

        private void BottomRight()
        {
	        var sections = _debugInfoCorner[DebugInfoCorner.BottomRight];
	        var textPos = new Vector2
	        {
		        Y = GraphicsDevice.Viewport.Height - _spaceSize.Y
	        };

	        foreach (var section in sections)
	        {
		        textPos.X = GraphicsDevice.Viewport.Width;
		        var subHeaderColor = !string.IsNullOrEmpty(section.SectionHeader) ? Color.LightGray : Color.Gray;

                foreach (var debugInfo in section.Reverse())
		        {
			        textPos = DrawStringForRightAligned(debugInfo.Text, Color.White, textPos);
                    textPos = DrawStringForRightAligned($"{debugInfo.Header}: ", subHeaderColor, textPos);
                    textPos.X -= _spaceSize.X;
		        }

		        if (!string.IsNullOrEmpty(section.SectionHeader))
		        {
			        textPos = DrawStringForRightAligned($"{section.SectionHeader}: ", Color.Gray, textPos);
		        }

                textPos.Y -= _spaceSize.Y;
	        }

	        sections.Clear();
        }

        public void LoadContent(ContentManager content)
        {
            _font = content.Load<SpriteFont>("CascadiaCode");
            _spaceSize = _font.MeasureString(" ");
        }
    }
}