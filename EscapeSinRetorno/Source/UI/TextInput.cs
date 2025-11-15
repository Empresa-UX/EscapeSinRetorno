using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace EscapeSinRetorno.Source.UI
{
    public sealed class TextInput
    {
        private readonly Rectangle _bounds;
        private readonly SpriteFont _font;
        private readonly Texture2D _px;

        private string _text = "";
        private bool _focused;

        // nuevo: edge-trigger (transición de tecla)
        private KeyboardState _prevKs;

        public string Placeholder { get; set; } = "127.0.0.1";
        public string Text
        {
            get => string.IsNullOrWhiteSpace(_text) ? "" : _text.Trim();
            set => _text = value ?? "";
        }

        public TextInput(Rectangle bounds, SpriteFont font, GraphicsDevice gd)
        {
            _bounds = bounds;
            _font = font;
            _px = new Texture2D(gd, 1, 1);
            _px.SetData(new[] { Color.White });
        }

        public void Update(GameTime gt)
        {
            var mouse = InputManager.MousePosition;
            if (InputManager.IsLeftMouseButtonPressed())
                _focused = _bounds.Contains(mouse);

            var ks = Keyboard.GetState();
            if (_focused)
            {
                // procesa solo teclas recién presionadas (edge)
                foreach (var key in ks.GetPressedKeys())
                {
                    if (!_prevKs.IsKeyDown(key)) // recién presionada
                    {
                        if (key == Keys.Back && _text.Length > 0)
                        {
                            _text = _text[..^1];
                            continue;
                        }

                        char? ch = null;

                        // dígitos
                        if (key >= Keys.D0 && key <= Keys.D9) ch = (char)('0' + (key - Keys.D0));
                        else if (key >= Keys.NumPad0 && key <= Keys.NumPad9) ch = (char)('0' + (key - Keys.NumPad0));
                        // punto
                        else if (key == Keys.OemPeriod) ch = '.';
                        // dos puntos (para ip:puerto si luego lo usamos)
                        else if (key == Keys.OemSemicolon) ch = ':'; // normalizamos a ':'

                        // Si quieres permitir letras para DNS, descomenta:
                        // else if (key >= Keys.A && key <= Keys.Z)
                        // {
                        //     bool shift = ks.IsKeyDown(Keys.LeftShift) || ks.IsKeyDown(Keys.RightShift);
                        //     ch = (char)((shift ? 'A' : 'a') + (key - Keys.A));
                        // }

                        if (ch.HasValue && _text.Length < 32)
                            _text += ch.Value;
                    }
                }
            }

            _prevKs = ks;
        }

        public void Draw(SpriteBatch sb)
        {
            sb.Draw(_px, _bounds, _focused ? new Color(40, 40, 60) : new Color(30, 30, 45));

            // borde
            sb.Draw(_px, new Rectangle(_bounds.X, _bounds.Y, _bounds.Width, 2), Color.Black);
            sb.Draw(_px, new Rectangle(_bounds.X, _bounds.Bottom - 2, _bounds.Width, 2), Color.Black);
            sb.Draw(_px, new Rectangle(_bounds.X, _bounds.Y, 2, _bounds.Height), Color.Black);
            sb.Draw(_px, new Rectangle(_bounds.Right - 2, _bounds.Y, 2, _bounds.Height), Color.Black);

            string show = string.IsNullOrEmpty(_text) ? Placeholder : _text;
            var color = string.IsNullOrEmpty(_text) ? new Color(180, 180, 200) * 0.7f : Color.White;

            var size = _font.MeasureString(show);
            var pos = new Vector2(_bounds.X + 10, _bounds.Y + (_bounds.Height - size.Y) / 2f);
            sb.DrawString(_font, show, pos, color);
        }
    }
}
