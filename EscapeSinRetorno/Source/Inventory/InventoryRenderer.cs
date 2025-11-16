using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace EscapeSinRetorno.Source.Inventory
{
    public sealed class InventoryRenderer
    {
        private readonly SpriteFont _font;
        private readonly Texture2D _pixel;

        public bool IsOpen { get; private set; }
        public int SelectedIndex { get; private set; }

        private const int Cols = 8;
        private const int Rows = 4;
        private const int SlotSize = 64;
        private const int Padding = 10;

        // Colores inspirados en tu menú
        private readonly Color _darkPurple = new Color(25, 15, 35);
        private readonly Color _deepBlack = new Color(10, 5, 15);
        private readonly Color _glowPurple = new Color(120, 80, 150);
        private readonly Color _bloodRed = new Color(120, 20, 30);
        private readonly Color _ghostWhite = new Color(220, 220, 230);

        public InventoryRenderer(GraphicsDevice gd, SpriteFont font)
        {
            _font = font;
            _pixel = new Texture2D(gd, 1, 1);
            _pixel.SetData(new[] { Color.White });
            SelectedIndex = 0;
        }

        public void Toggle() => IsOpen = !IsOpen;
        public void Open() => IsOpen = true;
        public void Close() => IsOpen = false;

        public void Draw(SpriteBatch sb, PlayerInventory inv, Viewport vp)
        {
            if (!IsOpen || inv == null) return;

            int panelWidth = Cols * SlotSize + Padding * 2;
            int panelHeight = Rows * SlotSize + Padding * 2 + 48 + 26;

            int px = (vp.Width - panelWidth) / 2;
            int py = (vp.Height - panelHeight) / 2;

            // Overlay que oscurece el fondo
            sb.Draw(_pixel, new Rectangle(0, 0, vp.Width, vp.Height), _deepBlack * 0.7f);

            // Panel principal
            var panelRect = new Rectangle(px, py, panelWidth, panelHeight);
            sb.Draw(_pixel, panelRect, _darkPurple * 0.95f);
            DrawRect(sb, panelRect, 2, _glowPurple);

            // Título
            var title = "Inventario";
            var tSize = _font.MeasureString(title);
            sb.DrawString(_font, title, new Vector2(px + (panelWidth - tSize.X) / 2, py + 8), _ghostWhite);

            int gridY = py + 40;

            // Slots
            for (int i = 0; i < inv.Slots.Length; i++)
            {
                int cx = i % Cols;
                int cy = i / Cols;

                int sx = px + Padding + cx * SlotSize;
                int sy = gridY + Padding + cy * SlotSize;

                var slotRect = new Rectangle(sx, sy, SlotSize - 6, SlotSize - 6);

                sb.Draw(_pixel, slotRect, _deepBlack * 0.85f);
                DrawRect(sb, slotRect, 1, new Color(60, 50, 80));

                if (i == SelectedIndex)
                {
                    var selRect = new Rectangle(slotRect.X - 2, slotRect.Y - 2, slotRect.Width + 4, slotRect.Height + 4);
                    sb.Draw(_pixel, selRect, _bloodRed * 0.35f);
                    DrawRect(sb, selRect, 2, _glowPurple);
                }

                var slot = inv.Slots[i];

                if (!slot.IsEmpty && slot.Item != null)
                {
                    if (slot.Item.Icon != null)
                    {
                        int iconSize = SlotSize - 16;
                        var iconRect = new Rectangle(
                            slotRect.X + (slotRect.Width - iconSize) / 2,
                            slotRect.Y + (slotRect.Height - iconSize) / 2,
                            iconSize,
                            iconSize);
                        sb.Draw(slot.Item.Icon, iconRect, Color.White);
                    }

                    if (slot.Item.Stackable && slot.Count > 1)
                    {
                        var txt = slot.Count.ToString();
                        var ts = _font.MeasureString(txt);
                        sb.DrawString(_font, txt,
                            new Vector2(slotRect.Right - ts.X - 3, slotRect.Bottom - ts.Y - 3),
                            _ghostWhite);
                    }
                }
            }

            // Tooltip
            var sel = inv.Slots[SelectedIndex];
            if (!sel.IsEmpty && sel.Item != null)
            {
                string tip = sel.Item.Name;
                var tipSize = _font.MeasureString(tip);
                var tipRect = new Rectangle(px + Padding,
                    gridY + Rows * SlotSize + 4,
                    (int)tipSize.X + 12, (int)tipSize.Y + 8);

                sb.Draw(_pixel, tipRect, _deepBlack * 0.95f);
                DrawRect(sb, tipRect, 1, _glowPurple);
                sb.DrawString(_font, tip, new Vector2(tipRect.X + 6, tipRect.Y + 3), _ghostWhite);
            }

            // Help / controles
            string help = "Enter: usar   Del: descartar 1   Backspace: descartar todo   Q: soltar 1   Shift+Q: soltar todo";
            var hs = _font.MeasureString(help);
            sb.DrawString(_font, help,
                new Vector2(px + (panelWidth - hs.X) / 2, py + panelHeight - hs.Y - 4),
                _ghostWhite * 0.9f);
        }

        public void HandleInput(
            KeyboardState prev,
            KeyboardState cur,
            PlayerInventory inv,
            System.Func<Item, bool> canUse,
            System.Action<Item> onUse,
            System.Action<Item, int> onDrop = null)
        {
            if (!IsOpen || inv == null) return;

            bool Pressed(Keys k) => cur.IsKeyDown(k) && !prev.IsKeyDown(k);
            bool Shift() => cur.IsKeyDown(Keys.LeftShift) || cur.IsKeyDown(Keys.RightShift);

            int col = SelectedIndex % Cols;
            int row = SelectedIndex / Cols;

            if (Pressed(Keys.Right) && col < Cols - 1) SelectedIndex++;
            if (Pressed(Keys.Left) && col > 0) SelectedIndex--;
            if (Pressed(Keys.Down) && row < Rows - 1) SelectedIndex += Cols;
            if (Pressed(Keys.Up) && row > 0) SelectedIndex -= Cols;

            if (Pressed(Keys.Enter))
                inv.UseAt(SelectedIndex, canUse, onUse);

            if (Pressed(Keys.Delete))
            {
                var slot = inv.Slots[SelectedIndex];
                if (!slot.IsEmpty)
                {
                    slot.Count -= 1;
                    if (slot.Count <= 0)
                    {
                        onDrop?.Invoke(slot.Item, 0);
                        slot.Item = null;
                        slot.Count = 0;
                    }
                }
            }

            if (Pressed(Keys.Back))
            {
                var slot = inv.Slots[SelectedIndex];
                if (!slot.IsEmpty)
                {
                    onDrop?.Invoke(slot.Item, slot.Count);
                    slot.Item = null;
                    slot.Count = 0;
                }
            }

            if (Pressed(Keys.Q))
            {
                var slot = inv.Slots[SelectedIndex];
                if (!slot.IsEmpty)
                {
                    int amount = Shift() ? slot.Count : 1;
                    onDrop?.Invoke(slot.Item, amount);

                    slot.Count -= amount;
                    if (slot.Count <= 0)
                    {
                        slot.Item = null;
                        slot.Count = 0;
                    }
                }
            }
        }

        private void DrawRect(SpriteBatch sb, Rectangle r, int thickness, Color color)
        {
            sb.Draw(_pixel, new Rectangle(r.Left, r.Top, r.Width, thickness), color);
            sb.Draw(_pixel, new Rectangle(r.Left, r.Bottom - thickness, r.Width, thickness), color);
            sb.Draw(_pixel, new Rectangle(r.Left, r.Top, thickness, r.Height), color);
            sb.Draw(_pixel, new Rectangle(r.Right - thickness, r.Top, thickness, r.Height), color);
        }
    }
}