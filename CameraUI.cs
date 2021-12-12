using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria.UI;
using Terraria;
using Terraria.ModLoader;
using Terraria.GameContent.UI.Elements;
using ReLogic.Content;
using Terraria.Audio;

namespace MPSpectate.UI
{

    class UIHelper {

        /// <summary>
        /// Sets the size of an element according to a fraction of the total screensize
        /// It first calculates either the width or height based on the given fraction of total screensize
        /// And then calculates the other side based on aspect ratio. (1:1 is 1f. 16:9 is 1.77f)
        /// </summary>
        /// <param name="element"> The element to be resized</param>
        /// <param name="fraction"> the target fraction of the total screen resize</param>
        /// <param name="aspect">The desired aspect ratio</param>
        /// <param name="widthFraction">Whether to calculate a fraction of the width or Height. Leave true for width.</param>
        public static void SetPercentAspectSize(UIElement element, float fraction, float aspect, bool widthFraction=true)
        {
            int xSize;
            int ySize;
            if (widthFraction)
            {
                xSize = (int) System.Math.Round(fraction * Main.screenWidth);
                ySize = (int)System.Math.Round(1 / aspect * xSize);
            }
            else {
                ySize = (int)System.Math.Round(fraction * Main.screenHeight);
                xSize = (int)System.Math.Round(aspect * ySize);
            }
            element.Width.Set(xSize, 0f);
            element.Height.Set(ySize, 0f);
        }

        /// <summary>
        /// Same as SetPercentAspectSize, but using parent size as relative base/max.
        /// </summary>
        /// <param name="element"> The element to be resized</param>
        /// <param name="parent">The parent element of the one to be resized</param>
        /// <param name="fraction"> the target fraction of the total screen resize</param>
        /// <param name="aspect">The desired aspect ratio</param>
        /// <param name="widthFraction">Whether to calculate a fraction of the width or Height. Leave true for width.</param>
        public static void SetPercentAspectSizeOnParent(UIElement element, UIElement parent, float fraction, float aspect, bool widthFraction = true)
        {
            //element.Width.GetValue
            int xSize;
            int ySize;
            if (widthFraction)
            {
                xSize = (int)System.Math.Round(fraction * parent.Width.GetValue(1f));
                ySize = (int)System.Math.Round(1 / aspect * xSize);
            }
            else
            {
                ySize = (int)System.Math.Round(fraction * parent.Height.GetValue(1f));
                xSize = (int)System.Math.Round(aspect * ySize);
            }
            element.Width.Set(xSize, 0f);
            element.Height.Set(ySize, 0f);
        }

        /// <summary>
        /// Sets the size of an element according to a fraction of the total screensize
        /// Ignores Original Aspect Ratio.
        /// </summary>
        /// <param name="element"> The element to be resized</param>
        /// <param name="fraction"> the target fraction of the total screen resize</param>
        /// <param name="aspect">The desired aspect ratio</param>
        /// <param name="widthFraction">Whether to calculate a fraction of the width or Height. Leave true for width.</param>
        public static void SetPercentSize(UIElement element, float widthFraction, float heightFraction)
        {
            element.Width.Set(widthFraction * Main.screenWidth, 0f);
            element.Height.Set(heightFraction * Main.screenHeight, 0f);
        }
    }

    class ScalableIconButton : UIImageButton
    {
        Asset<Texture2D> _texture;
        private float _scale;
        private float _visibilityActive = 1f;
        private float _visibilityInactive = 0.4f;
        public ScalableIconButton(Asset<Texture2D> texture, float scale) : base(texture)
        {
            _scale = scale;
            _texture = texture;
            UpdateSize();
        }

        private void UpdateSize()
        {
            Width.Set(_texture.Width() * _scale, 0f);
            Height.Set(_texture.Height() * _scale, 0f);
        }
        protected override void DrawSelf(SpriteBatch spriteBatch)
        {
            CalculatedStyle dimensions = GetDimensions();
            spriteBatch.Draw(_texture.Value, dimensions.Position(), null, Color.White * (base.IsMouseHovering ? _visibilityActive : _visibilityInactive), 0f, new Vector2(0, 0), _scale, SpriteEffects.None, 0f);
        }

        new public void SetVisibility(float whenActive, float whenInactive)
        {
            _visibilityActive = MathHelper.Clamp(whenActive, 0f, 1f);
            _visibilityInactive = MathHelper.Clamp(whenInactive, 0f, 1f);
        }

        public void setScale(float val)
        {
            _scale = val;
            UpdateSize();
        }

        public void setScaleToParent(float scaleToParent, float parentSize, bool fitWidth = true)
        {
            // Scales button to fit specified size, fitWidth true, will fit size up to width, else will fit height.
            float targetSize = parentSize * scaleToParent;
            float x = fitWidth ? _texture.Width() : _texture.Height();
            float scaleFactor = targetSize / x;
            setScale(scaleFactor);
        }

        public void ScaleToSize(float pixelSz, bool fitWidth = true) {
            // Scales button to fit specified size, fitWidth true, will fit size up to width, else will fit height.
            float x = fitWidth ? _texture.Width() : _texture.Height();
            float scaleFactor = pixelSz / x;
            setScale(scaleFactor);   
        }

        
    }

    class CameraUIState : UIState
    {
        private UIPanel _displayPanel;
        private UIText _displayText;
        private ScalableIconButton cycleLeftButton;
        private ScalableIconButton cycleRightButton;
        private bool InGameInitialized = false;

        public bool hidden = false;
        //private bool _initialize = false;
        //private UIText _userDisplayText;

        //private ScalableIconButton debugButton;

        private void RecalculateSizes() {
            // Recalculates dynamic sizes based on screen size.
            UIHelper.SetPercentSize(_displayPanel, 0.5f, 0.05f);
            _displayPanel.VAlign = 0.9f;
            _displayPanel.HAlign = 0.5f;

            _displayText.VAlign = 0.5f;
            _displayText.HAlign = 0.5f;

            cycleLeftButton.setScaleToParent(0.5f, _displayPanel.Height.GetValue(1f), false); // set to half of parent height
            cycleLeftButton.HAlign = 0f;
            cycleLeftButton.VAlign = 0.5f;

            cycleRightButton.setScaleToParent(0.5f, _displayPanel.Height.GetValue(1f), false); // set to half of parent height
            cycleRightButton.HAlign = 1f;
            cycleRightButton.VAlign = 0.5f;

            _displayPanel.Recalculate();
        }
        public override void OnInitialize()
        {
            _displayPanel = new UIPanel();
            _displayPanel.MaxWidth = new StyleDimension(500, 0f);

            Append(_displayPanel);

            _displayText = new UIText("DisplayColor");
            _displayPanel.Append(_displayText);

            Asset<Texture2D> leftCycleTexture = ModContent.Request<Texture2D>("MPSpectate/cycleLeft");
            Asset<Texture2D> rightCycleTexture = ModContent.Request<Texture2D>("MPSpectate/cycleRight");

            cycleLeftButton = new ScalableIconButton(leftCycleTexture, 1f);
            cycleLeftButton.OnClick += cycleLeft;

            cycleRightButton = new ScalableIconButton(rightCycleTexture, 1f);
            cycleRightButton.OnClick += cycleRight;

            
            _displayPanel.Append(cycleLeftButton);
            _displayPanel.Append(cycleRightButton);
        }

        public void setText(string userName, Color teamColor) {
            _displayText.TextColor = teamColor;
            _displayText.SetText(userName);
            _displayPanel.Recalculate();
        }

        public void OnGameInitialize() {
            // Called to initialized assured inside the game. Use this to adjust sizes on startup.
            InGameInitialized = true;
            RecalculateSizes();
            ModContent.GetInstance<MPSpectateModSystem>().HideMyUI();
        }

        
        new public void Update(GameTime gameTime)
        {
            if (!InGameInitialized) {
                OnGameInitialize();
                InGameInitialized = true;
            }

            if (!Main.player[Main.myPlayer].dead && !hidden)
            {
                ModContent.GetInstance<MPSpectateModSystem>().HideMyUI();
            }

        }

        private void cycleLeft(UIMouseEvent evt, UIElement listeningElement) {
            ModContent.GetInstance<MPSpectateModSystem>().performCycle(true);
        }

        private void cycleRight(UIMouseEvent evt, UIElement listeningElement) {
            ModContent.GetInstance<MPSpectateModSystem>().performCycle(false);
        }


        /// <summary>
        /// On resize hook. Will be called everytime the screen is resized.
        /// </summary>
        public void onResize() {
            if (cycleLeftButton != null)
            {
                RecalculateSizes();
            }
        }
    }
}
