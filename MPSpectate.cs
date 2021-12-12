using Terraria;
using Terraria.ModLoader;
using Terraria.Chat;
using Terraria.ID;
using Terraria.Localization;

using System.IO;
using Terraria.ModLoader;

namespace MPSpectate
{
	public class MPSpectate : Mod
	{
		 public override void HandlePacket(BinaryReader reader, int whoAmI)
        {
            byte msg = reader.ReadByte();
            if (msg == 0) // serves requested tile section for player.
            {
                //ChatHelper.BroadcastChatMessage(NetworkText.FromLiteral("Received Tile Request: "), Colors.RarityGreen);
                RemoteClient.CheckSection(whoAmI, Utils.ReadVector2(reader), 1);
            }
        }
	}
}