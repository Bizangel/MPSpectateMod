using Terraria;
using Terraria.ModLoader;
using Microsoft.Xna.Framework.Input;
using Terraria.ID;
using Terraria.Localization;
using Microsoft.Xna.Framework;
using Terraria.UI;
using MPSpectate.UI;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;

using MPSpectate.Camera;

namespace MPSpectate
{
	class MPSpectateModSystem : ModSystem
	{

		private int storedWidth;
		private int storedHeight;

		internal bool UIToggleFlag = true;

		internal CameraUIState cameraUIState;
		private UserInterface _cameraUI;

		public override void Load()
		{
			if (!Main.dedServ)
			{
				cameraUIState = new CameraUIState();
				cameraUIState.Activate();
				_cameraUI = new UserInterface();
				_cameraUI.SetState(cameraUIState);

				storedWidth = Main.screenWidth;
				storedHeight = Main.screenHeight;
			}
		}

		internal void ShowMyUI()
		{
			_cameraUI?.SetState(cameraUIState);
			cameraUIState.hidden = false;
		}

		internal void HideMyUI()
		{
			_cameraUI?.SetState(null);
			cameraUIState.hidden = true;
		}

		internal void ToggleUI()
		{
			if (UIToggleFlag)
			{
				HideMyUI(); UIToggleFlag = false;
			}
			else
			{
				ShowMyUI(); UIToggleFlag = true;
			}
		}

		public override void UpdateUI(GameTime gameTime)
		{
			if (Main.screenHeight != storedHeight || Main.screenWidth != storedWidth) {
				storedWidth = Main.screenWidth;
				storedHeight = Main.screenHeight;
				cameraUIState.onResize();
            }
			cameraUIState?.Update(gameTime);
            _cameraUI?.Update(gameTime);
		}



		public override void ModifyInterfaceLayers(List<GameInterfaceLayer> layers)
		{
			int mouseTextIndex = layers.FindIndex(layer => layer.Name.Equals("Vanilla: Mouse Text"));
			if (mouseTextIndex != -1)
			{
				layers.Insert(mouseTextIndex, new LegacyGameInterfaceLayer(
					"YourMod: A Description",
					delegate
					{
						_cameraUI.Draw(Main.spriteBatch, new GameTime());
						return true;
					},
					InterfaceScaleType.UI)
				);
			}

		}

		public static bool JustPressed(Keys key)
		{
			return Main.keyState.IsKeyDown(key) && !Main.oldKeyState.IsKeyDown(key);
		}

		// Attempt to perform the cycle both left and right.
		public void performCycle(bool reverse) {
			if (Main.player[Main.myPlayer].dead) { // only cycle if dead
				CameraMover mover = Main.player[Main.myPlayer].GetModPlayer<CameraMover>();

				mover.findNextTeamPlayerIndex(reverse);

				int spectIndex = mover.getSpectatingIndex();
				if (spectIndex == -1)
				{
					return; // can't spectate don't do anything.
				}
				else {
					cameraUIState.setText(Main.player[spectIndex].name, Main.teamColor[Main.player[spectIndex].team]);
				}
			}
		}

		public override void PostUpdateEverything()
		{

            if (JustPressed(Keys.Z))
                TestMethod();

            //if (JustPressed(Keys.X))
            //	ToggleUI();

            if (JustPressed(Keys.Left) && Main.player[Main.myPlayer].dead)
                performCycle(true);

			if (JustPressed(Keys.Right) && Main.player[Main.myPlayer].dead)
				performCycle(false);

        }

		private void TestMethod()
		{
			//Main.player[Main.myPlayer].GetModPlayer<CameraMover>().changeTeam(1);
		}

		private void CheckCurrentlyTracking()
		{

			int index = Main.player[Main.myPlayer].GetModPlayer<Camera.CameraMover>().getSpectatingIndex();
			if (index == -1)
			{
				Main.NewText("Not tracking anybody!");
			}
			else
			{
				Main.NewText("Currently Tracking: Index: " + index + " Player: " + Main.player[index].name);
			}

		}
	}
}
