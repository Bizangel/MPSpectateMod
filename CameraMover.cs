
using Terraria;
using Microsoft.Xna.Framework;
using Terraria.Net;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.DataStructures;

namespace MPSpectate.Camera
{
	class CameraMover : ModPlayer
	{

		public int _spectatingPlayer = -1;
		public static bool allowAliveSpectate = false; //  if True allows spectating while alive
		public static bool disallowNoTeamSpectate = true; // if True, doesn't allow spectating while not in a team, team 0 (No team)

		public void changeTeam(int newTeam) {
			Main.player[Main.myPlayer].team = newTeam;
			//player[myPlayer].team = newTeam;
			NetMessage.SendData(45, -1, -1, null, Main.myPlayer);
		}

		public override void ModifyScreenPosition()
		{
			// Called Every Frame

			if (Main.player[Main.myPlayer].dead || allowAliveSpectate) // Don't spectate if no team
			{
				//Verify that it is spectating a player
				if (_spectatingPlayer == -1)
				{
					return;
				}

				//Check that player that was previously spectating is still ready for spectation
				if (Main.player[_spectatingPlayer].dead || !Main.player[_spectatingPlayer].active || 
					(Main.player[_spectatingPlayer].team != Main.player[Main.myPlayer].team && Main.player[Main.myPlayer].team != 0))
				{
					_spectatingPlayer = -1; // No longer spectating, retry
					findNextTeamPlayerIndex();
					return;
				}

				if (Main.GameUpdateCount % 120u == 0) // Every two seconds request tile data, so that no chunks are unloaded
				{
					RequestTileSection();
				}

				// We have an active player, just track him
				/* I for real don't even remember where i got this code from, just copied it from an old similar project */

				Vector3 customVector = new Vector3(1f, 1f, 1f);
				Vector3 customVector2 = Vector3.One / customVector;
				int customNum = 21;

				if (Main.cameraX != 0f && !Main.player[_spectatingPlayer].pulley)
				{
					Main.cameraX = 0f;
				}
				if (Main.cameraX > 0f)
				{
					Main.cameraX -= 1f;
					if (Main.cameraX < 0f)
					{
						Main.cameraX = 0f;
					}
				}
				if (Main.cameraX < 0f)
				{
					Main.cameraX += 1f;
					if (Main.cameraX > 0f)
					{
						Main.cameraX = 0f;
					}
				}
				Main.screenPosition.X = Main.player[_spectatingPlayer].position.X + (float)Main.player[_spectatingPlayer].width * 0.5f - (float)Main.screenWidth * 0.5f * customVector2.X + Main.cameraX;
				Main.screenPosition.Y = Main.player[_spectatingPlayer].position.Y + (float)Main.player[_spectatingPlayer].height - (float)customNum - (float)Main.screenHeight * 0.5f * customVector2.Y + Main.player[_spectatingPlayer].gfxOffY;
			}
			else { _spectatingPlayer = -1; } // if not dead, not spectating

		}

		public int getSpectatingIndex() { return _spectatingPlayer; }


        public override void Kill(double damage, int hitDirection, bool pvp, PlayerDeathReason damageSource)
        {
			ModContent.GetInstance<MPSpectateModSystem>().performCycle(false); // Attempt to spectate
			if (_spectatingPlayer != -1) { ModContent.GetInstance<MPSpectateModSystem>().ShowMyUI(); }
        }

        public void RequestTileSection() // request current spectating player tile section
		{
			ModPacket coolpacket = Mod.GetPacket();
			coolpacket.Write((byte)0);
            coolpacket.WriteVector2(Main.player[_spectatingPlayer].position);
			coolpacket.Send();
		}

		public static bool IsBossActive()
		{
			// Returns True if a boss is active. Does not include mini-bosses. Does not include event-bossess.
			foreach (NPC mob in Main.npc)
			{
				if (mob.active && mob.boss)
				{
					return true;
				}
			}
			if (NPC.AnyNPCs(NPCID.EaterofWorldsBody) || NPC.AnyNPCs(NPCID.EaterofWorldsHead) || NPC.AnyNPCs(NPCID.EaterofWorldsTail))
			{
				return true;
			}

			return false;
		}

		/// <summary>
		/// Searches for next index of player within the same Team that is NOT DEAD, return -1 if can't find one (WRAPS AROUND)
		/// Additionally, automatically sets it as spectating player.
		/// </summary>
		public void findNextTeamPlayerIndex(bool reversed = false){
			
			if (Main.player[Main.myPlayer].team == 0 && IsBossActive()) {
				// make an exception and find any player
				int step = reversed ? -1 : 1;
				for (int j = 0; j < 256; j += step)
				{ // Look for another player
					if (j == Main.myPlayer)
					{
						continue;
					}

					if (j == _spectatingPlayer) {
						continue;
					}

					if (!Main.player[j].active || Main.player[j].dead)
					{
						continue;
					}

					_spectatingPlayer = j;
					return;
				}
				_spectatingPlayer = -1;
				return;
			}

			if (Main.player[Main.myPlayer].team == 0 && disallowNoTeamSpectate)
			{
				return; // don't spectate on team if disallowed.
			}

			//Find first player
			if (_spectatingPlayer == -1) {
				for (int j = 0; j < 256; j++)
				{ // Look for another player
					if (j == Main.myPlayer)
					{
						continue;
					}

					if (!Main.player[j].active || Main.player[j].dead || Main.player[Main.myPlayer].team != Main.player[j].team)
					{
						continue;
					}

					_spectatingPlayer = j;
					return;
				}
				_spectatingPlayer = -1;
				return;
			}

			// If already has an spectating player, find next
			int i;
			if (reversed)
			{
				i = _spectatingPlayer - 1;
			}
			else
			{
				i = _spectatingPlayer + 1;
			}

			while (i != _spectatingPlayer) {
				if (i == 256) { i = 0; } //wrap around
				if (i == -1) { i = 255; }

				if (i == Main.myPlayer)
				{
					if (reversed) { i--; }
					else { i++; }
					if (i == 256) { i = 0; } //wrap around
					if (i == -1) { i = 255; }

					continue;
				}

				if (!Main.player[i].active || Main.player[i].dead || Main.player[i].team != Main.player[Main.myPlayer].team)
				{
					if (reversed) { i--; }
					else { i++; }
					if (i == 256) { i = 0; } //wrap around
					if (i == -1) { i = 255; }

					continue;
				}

				_spectatingPlayer = i;
				return;
			}

			//_spectatingPlayer = -1;
			return;
		}

	}
}
