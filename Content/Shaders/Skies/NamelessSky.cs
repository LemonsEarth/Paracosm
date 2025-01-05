using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Graphics.Effects;
using Terraria.ModLoader;

namespace Paracosm.Content.Shaders.Skies
{
    public class NamelessSky : CustomSky
    {
        public bool _isActive = false;

        public override void Update(GameTime gameTime)
        {
  
        }

        public override void Draw(SpriteBatch spriteBatch, float minDepth, float maxDepth)
        {
            Texture2D sky = ModContent.Request<Texture2D>("Paracosm/Assets/Textures/Backgrounds/Void/NamelessSky").Value;
            spriteBatch.Draw(sky, new Rectangle(0, 0, Main.screenWidth, Main.screenHeight), new Color(0.5f, 0f, 0f));
        }

        public override bool IsActive()
        {
            return _isActive;
        }

        public override void Activate(Vector2 position, params object[] args)
        {
            _isActive = true;
        }

        public override void Deactivate(params object[] args)
        {
            _isActive = false;
        }

        public override void Reset()
        {
            _isActive = false;
        }
    }
}
