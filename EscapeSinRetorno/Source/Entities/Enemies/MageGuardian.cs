using EscapeSinRetorno.Source.World;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using EscapeSinRetorno.Source.Inventory;
using EscapeSinRetorno.Source.Items;
using EscapeSinRetorno.Source.Chat;

namespace EscapeSinRetorno.Source.Entities.Enemies
{
    public class MageGuardian : Enemy
    {
        private readonly string _variant;     // "red", "blue", "magenta"
        private readonly string _keyItemId;   // "red_key", "cyan_key", "purple_key"
        private bool _keyGiven;
        public override string TypeId => "mg";

        public bool KeyAlreadyGiven => _keyGiven;
        public string KeyItemId => _keyItemId;

        public MageGuardian(Vector2 startPosition, string variant) : base(startPosition)
        {
            _variant = variant.ToLowerInvariant();

            speed = 50f;
            health = 120;

            // Mapeo variante → id de ítem
            _keyItemId = _variant switch
            {
                "red" => "red_key",
                "blue" => "cyan_key",
                "magenta" => "purple_key",
                _ => null
            };
        }

        public void MarkKeyGiven()
        {
            _keyGiven = true;
        }

        public override void LoadContent(ContentManager content)
        {
            // Usa la misma variante para elegir el sprite
            string path = $"Characters/MageGuardian/Idle_{_variant}";

            animations["Idle"] = new AnimationClip
            {
                Texture = content.Load<Texture2D>(path),
                FrameWidth = 64,
                FrameHeight = 64,
            };

            currentAnimation = "Idle";

            hitboxWidth = (int)(animations["Idle"].FrameWidth * 0.4f);
            hitboxHeight = (int)(animations["Idle"].FrameHeight * 0.6f);
        }

        public override void Update(GameTime gameTime, Player player, TileMap tileMap)
        {
            currentAnimation = "Idle";
            UpdateAnimation(gameTime);
        }

        public override void TakeDamage(int dmg){}
    }
}
