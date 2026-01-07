using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;

namespace TileResearch
{
    public class Game1 : Game
    {
        private GraphicsDeviceManager graphics;
        private SpriteBatch spriteBatch;
        private Texture2D rogueSword;
        private Texture2D tileset;
        private int TilesPerRow = 8;
        private float scale = 2.0f;
        private Vector2 spritePosition;
        private int playerGridX = 7;  // Starting position on the map
        private int playerGridY = 5;
        private float tileSize = 32.0f;  // Raw world tile size (matches sprite)
        private KeyboardState previousKeyboardState;

        private int[,] map = new int[,]
        {   
        
        //15x10
        //                       1
        //   0 1 2 3 4 5 6 7 8 9 0 1 2 3 4
            {0,0,0,0,0,0,0,0,0,0,0,0,0,0,0},  //0
            {0,2,2,2,2,2,2,2,2,2,2,2,2,2,0},  //1
            {0,2,2,2,2,2,2,2,2,2,2,2,2,2,0},  //3
            {0,2,2,2,2,2,2,2,2,2,2,2,2,2,0},  //4
            {0,2,2,2,2,2,2,2,2,2,2,2,2,2,0},  //5
            {0,2,2,2,2,2,2,2,2,2,2,2,2,2,0},  //6
            {0,2,2,2,2,2,2,2,2,2,2,2,2,2,0},  //7
            {0,2,2,2,2,2,2,2,2,2,2,2,2,2,0},  //8
            {0,0,0,0,0,0,0,0,0,0,0,0,0,0,1}   //9
        };


        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;

            //have grok explain the two below settings
            graphics.PreferredBackBufferWidth = 960;
            graphics.PreferredBackBufferHeight = 640;

            graphics.ApplyChanges();
        }

        protected override void Initialize()
        {
            base.Initialize();
        }

        protected override void LoadContent()
        {
            spriteBatch = new SpriteBatch(GraphicsDevice);
            spriteBatch.GraphicsDevice.SamplerStates[0] = SamplerState.PointClamp;

            tileset = Content.Load<Texture2D>("sheets/t1");
            rogueSword = Content.Load<Texture2D>("sprites/rogueSword");

            UpdateSpritePosition();
        }

        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed ||
                    Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            KeyboardState currentKeyboardState = Keyboard.GetState();

            if (currentKeyboardState.IsKeyDown(Keys.W) && previousKeyboardState.IsKeyUp(Keys.W))
            {
                playerGridY--;  // Move up one tile
            }
            if (currentKeyboardState.IsKeyDown(Keys.S) && previousKeyboardState.IsKeyUp(Keys.S))
            {
                playerGridY++;  // Down
            }
            if (currentKeyboardState.IsKeyDown(Keys.A) && previousKeyboardState.IsKeyUp(Keys.A))
            {
                playerGridX--;  // Left
            }
            if (currentKeyboardState.IsKeyDown(Keys.D) && previousKeyboardState.IsKeyUp(Keys.D))
            {
                playerGridX++;  // Right
            }

            // Clamp to screen (adjust for your scaled sprite size)
            float logicalWidth = GraphicsDevice.Viewport.Width / scale;
            float logicalHeight = GraphicsDevice.Viewport.Height / scale;
          
            int maxGridX = (int)((logicalWidth - tileSize) / tileSize);
            int maxGridY = (int)((logicalHeight - tileSize) / tileSize);

            playerGridX = MathHelper.Clamp(playerGridX, 0, maxGridX);
            playerGridY = MathHelper.Clamp(playerGridY, 0, maxGridY);

            UpdateSpritePosition();

            previousKeyboardState = currentKeyboardState;


            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {

            GraphicsDevice.Clear(Color.Black);

            Matrix scaleMatrix = Matrix.CreateScale(scale);

            spriteBatch.Begin(
                transformMatrix: scaleMatrix,
                samplerState: SamplerState.PointClamp
            );

            // Calculate visible tile range (camera centered on player)
            float logicalWidth = GraphicsDevice.Viewport.Width / scale;
            float logicalHeight = GraphicsDevice.Viewport.Height / scale;

            int tilesX = (int)(logicalWidth / tileSize) + 2;
            int tilesY = (int)(logicalHeight / tileSize) + 2;

            int startX = Math.Max(0, playerGridX - tilesX / 2);
            int startY = Math.Max(0, playerGridY - tilesY / 2);
            int endX = Math.Min(map.GetLength(1), startX + tilesX);
            int endY = Math.Min(map.GetLength(0), startY + tilesY);

            // Offset to center player
            Vector2 cameraOffset = new Vector2(
                (GraphicsDevice.Viewport.Width / scale / 2) - tileSize / 2 - (playerGridX * tileSize),
                (GraphicsDevice.Viewport.Height / scale / 2) - tileSize / 2 - (playerGridY * tileSize)
            );

            // Draw tiles
            for (int y = startY; y < endY; y++)
            {
                for (int x = startX; x < endX; x++)
                {
                    int tileId = map[y, x];

                    Rectangle sourceRect = new Rectangle(
                        (tileId % TilesPerRow) * (int)tileSize,
                        (tileId / TilesPerRow) * (int)tileSize,
                        (int)tileSize,
                        (int)tileSize
                    );

                    Vector2 pos = new Vector2(x * tileSize, y * tileSize) + cameraOffset;

                    spriteBatch.Draw(tileset, pos, sourceRect, Color.White);
                }
            }

            // Draw player on top
            spriteBatch.Draw(
                rogueSword,
                spritePosition + cameraOffset,
                null,
                Color.White,
                0f,
                Vector2.Zero,
                1.0f,
                SpriteEffects.None,
                0f
            );

            spriteBatch.End();

            base.Draw(gameTime);
        }

        private void UpdateSpritePosition()
        {
            spritePosition = new Vector2(playerGridX * tileSize, playerGridY * tileSize);
        }
    }
}
