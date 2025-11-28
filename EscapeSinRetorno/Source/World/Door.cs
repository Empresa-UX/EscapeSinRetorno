using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;
using EscapeSinRetorno.Source.Inventory;
using EscapeSinRetorno.Source.Items;
using EscapeSinRetorno.Source.Chat;

namespace EscapeSinRetorno.Source.World
{
    public enum DoorType
    {
        Cyan,
        Purple,
        Red,
        Final
    }

    public enum DoorState
    {
        Closed,
        Opening,
        Open
    }

    public sealed class Door
    {
        private readonly Texture2D _texture;
        private readonly float _frameDuration;
        private readonly float _scale;
        private Texture2D _debugPixel;

        private int _currentFrame;
        private float _frameTimer;

        public DoorType Type { get; }
        public DoorState State { get; private set; }

        // Posición en mundo = centro inferior de la puerta
        public Vector2 Position { get; }

        public int FrameWidth => _texture.Width / 3;
        public int FrameHeight => _texture.Height;

        private int ScaledWidth => (int)(FrameWidth * _scale);
        private int ScaledHeight => (int)(FrameHeight * _scale);

        public Rectangle Bounds
        {
            get
            {
                int w = ScaledWidth;
                int h = ScaledHeight;
                int x = (int)(Position.X - w / 2f);
                int y = (int)(Position.Y - h);
                return new Rectangle(x, y, w, h);
            }
        }

        public bool IsOpen => State == DoorState.Open;

        public Door(DoorType type, Texture2D tex, Vector2 position, float scale, float frameDuration = 0.12f)
        {
            Type = type;
            _texture = tex;
            Position = position;
            _scale = scale;
            _frameDuration = frameDuration;

            State = DoorState.Closed;
            _currentFrame = 0;
            _frameTimer = 0f;
        }

        public void Update(GameTime gameTime)
        {
            if (State != DoorState.Opening) return;

            _frameTimer += (float)gameTime.ElapsedGameTime.TotalSeconds;
            if (_frameTimer >= _frameDuration)
            {
                _frameTimer -= _frameDuration;
                _currentFrame++;

                if (_currentFrame >= 3)
                {

                    _currentFrame = 2; // frame final = abierto
                    State = DoorState.Open;
                }
            }
        }

        public void Draw(SpriteBatch sb)
        {
            int fw = FrameWidth;
            var src = new Rectangle(fw * _currentFrame, 0, fw, FrameHeight);

            int w = ScaledWidth;
            int h = ScaledHeight;
            var dest = new Rectangle(
                (int)(Position.X - w / 2f),
                (int)(Position.Y - h),
                w,
                h
            );

            sb.Draw(_texture, dest, src, Color.White);
        }

        public void ForceOpen()
        {
            State = DoorState.Open;
            _currentFrame = 2;
            _frameTimer = 0f;
        }

        public void StartOpening()
        {
            if (State == DoorState.Closed)
            {
                State = DoorState.Opening;
                _currentFrame = 0;
                _frameTimer = 0f;
            }
        }
    }

    public sealed class DoorSpawn
    {
        public DoorType Type { get; }
        public Vector2 Position { get; }

        public DoorSpawn(DoorType type, Vector2 pos)
        {
            Type = type;
            Position = pos; // ya viene en los coords que definimos en TileMap
        }
    }

    public sealed class DoorManager
    {
        private readonly List<Door> _doors = new();

        private Texture2D _cyanTex;
        private Texture2D _purpleTex;
        private Texture2D _redTex;
        private Texture2D _finalTex;
        private Texture2D _debugPixel;

        public IReadOnlyList<Door> Doors => _doors;

        public void LoadContent(ContentManager content, GraphicsDevice graphicsDevice)
        {
            _cyanTex = content.Load<Texture2D>("Doors/cyan_door");
            _purpleTex = content.Load<Texture2D>("Doors/purple_door");
            _redTex = content.Load<Texture2D>("Doors/red_door");
            _finalTex = content.Load<Texture2D>("Doors/final_door");

            _debugPixel = new Texture2D(graphicsDevice, 1, 1);
            _debugPixel.SetData(new[] { Color.White });
        }


        public void ClearDoors() => _doors.Clear();

        public void SpawnFromMapData(IEnumerable<DoorSpawn> spawns, int tileSize)
        {
            _doors.Clear();
            if (spawns == null) return;

            const int DoorTilesWide = 2;
            const int DoorTilesHigh = 3;

            foreach (var s in spawns)
            {
                Texture2D tex = s.Type switch
                {
                    DoorType.Cyan => _cyanTex,
                    DoorType.Purple => _purpleTex,
                    DoorType.Red => _redTex,
                    DoorType.Final => _finalTex,
                    _ => null
                };

                if (tex == null) continue;

                int frameWidth = tex.Width / 3;
                int frameHeight = tex.Height;

                float targetW = DoorTilesWide * tileSize;
                float targetH = DoorTilesHigh * tileSize;

                float sx = targetW / frameWidth;
                float sy = targetH / frameHeight;
                float scale = MathF.Min(sx, sy);

                _doors.Add(new Door(s.Type, tex, s.Position, scale));
            }
        }

        public void Update(GameTime gameTime)
        {
            foreach (var d in _doors)
                d.Update(gameTime);
        }

        public void Draw(SpriteBatch sb)
        {
            foreach (var d in _doors)
            {
                // Debug de la caja de colisión de la puerta
                Color debugColor = d.State switch
                {
                    DoorState.Closed => Color.Red * 0.4f,
                    DoorState.Opening => Color.Yellow * 0.4f,
                    DoorState.Open => Color.Green * 0.25f,
                    _ => Color.Blue * 0.3f
                };

                sb.Draw(_debugPixel, d.Bounds, debugColor);

                // Sprite real de la puerta
                d.Draw(sb);
            }
        }
        public bool IsCollidingClosedDoor(Rectangle rect)
        {
            foreach (var d in _doors)
            {
                if (!d.IsOpen)
                {
                    var b = d.Bounds;
                    // achicamos la colisión real de la puerta un poco a los costados
                    b.Inflate(-2, 0); // 2 px menos por lado
                    if (b.Intersects(rect))
                        return true;
                }
            }
            return false;
        }


        public bool TryOpenNearbyDoor(Rectangle playerHitbox, PlayerInventory inv, ChatManager chat)
        {
            if (inv == null) return false;

            const int interactPadding = 12;
            var area = playerHitbox;
            area.Inflate(interactPadding, interactPadding);

            Door closest = null;
            float bestDist = float.MaxValue;

            foreach (var d in _doors)
            {
                if (d.IsOpen) continue;
                if (!area.Intersects(d.Bounds)) continue;

                float dist = DistanceCenter(playerHitbox, d.Bounds);
                if (dist < bestDist)
                {
                    bestDist = dist;
                    closest = d;
                }
            }

            if (closest == null) return false;

            string neededKeyId = closest.Type switch
            {
                DoorType.Cyan => "cyan_key",
                DoorType.Purple => "purple_key",
                DoorType.Red => "red_key",
                DoorType.Final => "main_key",
                _ => null
            };

            if (neededKeyId == null) return false;

            if (!inv.RemoveItem(neededKeyId, 1))
            {
                chat?.AddSystemMessage("Te falta la llave adecuada para esta puerta.");
                return false;
            }

            closest.StartOpening();

            if (ItemDatabase.Items.TryGetValue(neededKeyId, out var item))
                chat?.AddSystemMessage($"Usaste {item.Name} para abrir la puerta.");
            else
                chat?.AddSystemMessage("Abriste la puerta.");

            return true;
        }

        private static float DistanceCenter(Rectangle a, Rectangle b)
        {
            var ac = new Vector2(a.Center.X, a.Center.Y);
            var bc = new Vector2(b.Center.X, b.Center.Y);
            return Vector2.Distance(ac, bc);
        }

        public bool IntersectsOpenDoor(Rectangle rect)
        {
            foreach (var d in _doors)
            {
                if (!d.IsOpen)
                    continue;

                var portal = GetDoorPortalArea(d);
                if (portal.Intersects(rect))
                    return true;
            }

            return false;
        }

        // Helper privado para NO duplicar lógica
        private Rectangle GetDoorPortalArea(Door d)
        {
            var portal = d.Bounds;

            const float portalRatio = 0.50f; // lo que ya ajustaste
            int h = (int)(portal.Height * portalRatio);

            portal.Y = portal.Bottom - h;
            portal.Height = h;

            // mismo inflate que usaste
            portal.Inflate(-20, 0);

            return portal;
        }
        public void GetDoorStats(out int total, out int open, out int closed)
        {
            total = Doors.Count;
            open = 0;
            closed = 0;

            foreach (var d in Doors)
            {
                if (d.IsOpen) open++;
                else closed++;
            }
        }

    }
}