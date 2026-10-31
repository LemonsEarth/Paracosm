using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.GameContent;
using Terraria.ModLoader;

namespace Paracosm.Common.Utils;

public static class ArenaBorderUtils
{
    const int SphereCount = 40;
    const float SphereSpacing = MathHelper.TwoPi / SphereCount;

    public static void DrawBorder(Vector2 center, float radius, float rotation, float timer, Color color)
    {
        Texture2D texture = ModContent.Request<Texture2D>("Paracosm/Content/Projectiles/Hostile/BorderSphere").Value;
        Rectangle frame = texture.Frame(1, 2, 0, (int)(timer / 6f) % 2);
        Vector2 origin = new Vector2(frame.Width * 0.5f, frame.Height * 0.5f);

        for (int i = 0; i < SphereCount; i++)
        {
            Vector2 position = center + (-Vector2.UnitY * radius).RotatedBy(rotation + i * SphereSpacing);
            Main.EntitySpriteDraw(texture, position - Main.screenPosition, frame, color, timer, origin, 1f, SpriteEffects.None);
        }
    }
}
