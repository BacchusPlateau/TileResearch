using Ecalpon.Combat;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Ecalpon
{
    public class TheGame : Game
    {
        private bool InCombat = true;  // true for now to test combat screen
        private GraphicsDeviceManager Graphics;
        private SpriteBatch SpriteBatch;
        private Texture2D RogueSword;
        private Texture2D Tileset;
        private int TilesPerRow = 8;
        private float Scale = 2.0f;
        private Vector2 SpritePosition;
        private int PlayerGridX = 7;  // Starting position on the map
        private int PlayerGridY = 5;
        private float TileSize = 32.0f;  // Raw world tile size (matches sprite)
        private KeyboardState PreviousKeyboardState;
		private CombatScreen CombatScreen;
		private SpriteFont CombatFont;

		private int[,] map = new int[,]
        {   
        
        //15x10
        //                       1
        //   0 1 2 3 4 5 6 7 8 9 0 1 2 3 4 5
            {0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0},  //0
            {0,2,2,2,2,2,2,2,2,2,2,2,2,2,2,0},  //1
            {0,2,2,2,2,2,2,2,2,2,2,2,2,2,2,0},  //2
            {0,2,2,2,2,2,2,2,2,2,2,2,2,2,2,0},  //3
            {0,2,2,2,2,2,2,2,2,2,2,2,2,2,2,0},  //4
            {0,2,2,2,2,2,2,2,2,2,2,2,2,2,2,0},  //5
            {0,2,2,2,2,2,2,2,2,2,2,2,2,2,2,0},  //6
            {0,2,2,2,2,2,2,2,2,2,2,2,2,2,2,0},  //7
            {0,2,2,2,2,2,2,2,2,2,2,2,2,2,2,0},  //8
            {0,2,2,2,2,2,2,2,2,2,2,2,2,2,2,0},  //9
            {0,2,2,2,2,2,2,2,2,2,2,2,2,2,2,0},  //10
            {0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,1}   //11
        };


        public TheGame()
        {
            Graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;

            //have grok explain the two below settings
            Graphics.PreferredBackBufferWidth = 1080;
            Graphics.PreferredBackBufferHeight = 800;

            Graphics.ApplyChanges();
        }

        protected override void Initialize()
        {
            base.Initialize();
        }

        protected override void LoadContent()
        {
            SpriteBatch = new SpriteBatch(GraphicsDevice);

            // Original assets
            RogueSword = Content.Load<Texture2D>("sprites/rogueSword");
            Tileset = Content.Load<Texture2D>("sheets/t1");

            // Combat screen setup
            CombatFont = Content.Load<SpriteFont>("fonts/CombatFont");
            CombatScreen = new CombatScreen(SpriteBatch, CombatFont, GraphicsDevice);

            // Test combat encounter
            List<Combatant> party = new List<Combatant>
            {
                new Combatant
                {
                    Name = "Scientist",
                    Type = CombatantType.PlayerCharacter,
                    HitPoints = 20,
                    MaxHitPoints = 20,
                    ArmorClass = 6,
                    ThacO = 20,
                    DamageMin = 1,
                    DamageMax = 6,
                    DamageBonus = 1,
                    Level = 1,
                    MaxMoves = 3,
                    GridRow = 12,
                    GridCol = 7
                }
            };

            List<Combatant> enemies = new List<Combatant>
            {
                new Combatant
                {
                    Name = "Legionary",
                    Type = CombatantType.Enemy,
                    HitPoints = 12,
                    MaxHitPoints = 12,
                    ArmorClass = 4,
                    ThacO = 20,
                    DamageMin = 1,
                    DamageMax = 8,
                    DamageBonus = 0,
                    Level = 1,
                    MaxMoves = 2,
                    GridRow = 11,
                    GridCol = 7
                },
                new Combatant
                {
                    Name = "Jackal",
                    Type = CombatantType.Enemy,
                    HitPoints = 6,
                    MaxHitPoints = 6,
                    ArmorClass = 7,
                    ThacO = 19,
                    DamageMin = 1,
                    DamageMax = 4,
                    DamageBonus = 0,
                    Level = 1,
                    MaxMoves = 4,
                    GridRow = 3,
                    GridCol = 9
                }
            };

            CombatScreen.BeginCombat(party, enemies);
        }

        protected override void Update(GameTime gameTime)
        {
            KeyboardState currentKeyboardState = Keyboard.GetState();

            bool escapeJustPressed = currentKeyboardState.IsKeyDown(Keys.Escape)
                && PreviousKeyboardState.IsKeyUp(Keys.Escape);

            bool escapeShouldQuit = !(InCombat && CombatScreen.CurrentState == CombatState.SelectingTarget);

            bool backButtonPressed = GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed;

            if (backButtonPressed || (escapeJustPressed && escapeShouldQuit))
                Exit();

            if (InCombat)
            {
                CombatScreen.Update(gameTime);
            }
            else
            {
                if (currentKeyboardState.IsKeyDown(Keys.W) && PreviousKeyboardState.IsKeyUp(Keys.W))
                    PlayerGridY--;

                if (currentKeyboardState.IsKeyDown(Keys.S) && PreviousKeyboardState.IsKeyUp(Keys.S))
                    PlayerGridY++;

                if (currentKeyboardState.IsKeyDown(Keys.A) && PreviousKeyboardState.IsKeyUp(Keys.A))
                    PlayerGridX--;

                if (currentKeyboardState.IsKeyDown(Keys.D) && PreviousKeyboardState.IsKeyUp(Keys.D))
                    PlayerGridX++;

                float logicalWidth = GraphicsDevice.Viewport.Width / Scale;
                float logicalHeight = GraphicsDevice.Viewport.Height / Scale;

                int maxGridX = (int)((logicalWidth - TileSize) / TileSize);
                int maxGridY = (int)((logicalHeight - TileSize) / TileSize);

                PlayerGridX = MathHelper.Clamp(PlayerGridX, 0, maxGridX);
                PlayerGridY = MathHelper.Clamp(PlayerGridY, 0, maxGridY);

                UpdateSpritePosition();
            }

            PreviousKeyboardState = currentKeyboardState;

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.Black);

            if (InCombat)
            {
                SpriteBatch.Begin();
                CombatScreen.Draw(gameTime);
                SpriteBatch.End();
            }
            else
            {
                Matrix scaleMatrix = Matrix.CreateScale(Scale);

                SpriteBatch.Begin(
                    transformMatrix: scaleMatrix,
                    samplerState: SamplerState.PointClamp
                );

                float logicalWidth = GraphicsDevice.Viewport.Width / Scale;
                float logicalHeight = GraphicsDevice.Viewport.Height / Scale;

                int tilesX = (int)(logicalWidth / TileSize) + 2;
                int tilesY = (int)(logicalHeight / TileSize) + 2;

                int startX = Math.Max(0, PlayerGridX - tilesX / 2);
                int startY = Math.Max(0, PlayerGridY - tilesY / 2);
                int endX = Math.Min(map.GetLength(1), startX + tilesX);
                int endY = Math.Min(map.GetLength(0), startY + tilesY);

                Vector2 cameraOffset = new Vector2(
                    (GraphicsDevice.Viewport.Width / Scale / 2) - TileSize / 2 - (PlayerGridX * TileSize),
                    (GraphicsDevice.Viewport.Height / Scale / 2) - TileSize / 2 - (PlayerGridY * TileSize)
                );

                for (int y = startY; y < endY; y++)
                {
                    for (int x = startX; x < endX; x++)
                    {
                        int tileId = map[y, x];

                        Rectangle sourceRect = new Rectangle(
                            (tileId % TilesPerRow) * (int)TileSize,
                            (tileId / TilesPerRow) * (int)TileSize,
                            (int)TileSize,
                            (int)TileSize
                        );

                        Vector2 pos = new Vector2(x * TileSize, y * TileSize) + cameraOffset;
                        SpriteBatch.Draw(Tileset, pos, sourceRect, Color.White);
                    }
                }

                SpriteBatch.Draw(
                    RogueSword,
                    SpritePosition + cameraOffset,
                    null,
                    Color.White,
                    0f,
                    Vector2.Zero,
                    1.0f,
                    SpriteEffects.None,
                    0f
                );

                SpriteBatch.End();
            }

            base.Draw(gameTime);
        }

        private void UpdateSpritePosition()
        {
            SpritePosition = new Vector2(PlayerGridX * TileSize, PlayerGridY * TileSize);
        }
    }
}
