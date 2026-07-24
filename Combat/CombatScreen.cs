using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System.Collections.Generic;

namespace Ecalpon.Combat
{
    public class CombatScreen
    {

        private CombatManager Manager;
        private SpriteBatch SpriteBatch;
        private SpriteFont Font;
        private Texture2D Pixel;
        public CombatState CurrentState => Manager.CurrentState;

        private KeyboardState CurrentKeys;
        private KeyboardState PreviousKeys;

        private const int GRID_COLS = 16;
        private const int GRID_ROWS = 16;
        private const int TILE_SIZE = 32;
        private const int GRID_ORIGIN_X = 16;
        private const int GRID_ORIGIN_Y = 16;
        private const int PANEL_X = GRID_ORIGIN_X + (GRID_COLS * TILE_SIZE) + 16;
        private const int PANEL_Y = 16;

        public CombatScreen(SpriteBatch spriteBatch, SpriteFont font,
                            GraphicsDevice graphicsDevice)
        {
            SpriteBatch = spriteBatch;
            Font = font;
            Manager = new CombatManager();

            Pixel = new Texture2D(graphicsDevice, 1, 1);
            Pixel.SetData(new Color[] { Color.White });
        }

        // =====================================================
        // PUBLIC ENTRY POINT
        // =====================================================

        public void BeginCombat(List<Combatant> playerParty,
                                List<Combatant> enemies)
        {
            Manager.StartCombat(playerParty, enemies);
        }

        private void DrawTargetCursor()
        {
            if (Manager.CurrentState != CombatState.SelectingTarget)
                return;

            Rectangle cursorRect = new Rectangle(
                GRID_ORIGIN_X + (Manager.CursorCol * TILE_SIZE),
                GRID_ORIGIN_Y + (Manager.CursorRow * TILE_SIZE),
                TILE_SIZE - 1,
                TILE_SIZE - 1
            );

            DrawCursorOutline(cursorRect, Color.Red);
        }

        private void DrawCursorOutline(Rectangle rect, Color color)
        {
            int thickness = 2;

            DrawFilledRect(new Rectangle(rect.X, rect.Y, rect.Width, thickness), color);
            DrawFilledRect(new Rectangle(rect.X, rect.Y + rect.Height - thickness, rect.Width, thickness), color);
            DrawFilledRect(new Rectangle(rect.X, rect.Y, thickness, rect.Height), color);
            DrawFilledRect(new Rectangle(rect.X + rect.Width - thickness, rect.Y, thickness, rect.Height), color);
        }

        public void Update(GameTime gameTime)
        {
            PreviousKeys = CurrentKeys;
            CurrentKeys = Keyboard.GetState();

            // The game only responds to input on the player's turn
            // On the enemy turn we resolve immediately and wait
            // for the player again — no timing, no delays

            if (Manager.CurrentState == CombatState.PlayerTurn)
                HandlePlayerInput();

            if (Manager.CurrentState == CombatState.EnemyTurn)
                ResolveEnemyTurn();

            if (Manager.CurrentState == CombatState.SelectingTarget)
                HandlePlayerTargeting();
        }

        public void Draw(GameTime gameTime)
        {
            DrawGrid();
            DrawCombatants();
            DrawTargetCursor();
            DrawMessagePanel();
            DrawActionMenu();
        }

        private void HandlePlayerTargeting()
        {
            if (WasKeyJustPressed(Keys.Up))
            {
                Manager.MoveCursor(-1, 0);
                return;
            }

            if (WasKeyJustPressed(Keys.Down))
            {
                Manager.MoveCursor(1, 0);
                return;
            }

            if (WasKeyJustPressed(Keys.Left))
            {
                Manager.MoveCursor(0, -1);
                return;
            }

            if (WasKeyJustPressed(Keys.Right))
            {
                Manager.MoveCursor(0, 1);
                return;
            }

            if (WasKeyJustPressed(Keys.Enter))
            {
                if (Manager.TryConfirmTarget(out Combatant target))
                {
                    Combatant attacker = Manager.CurrentCombatant();

                    if (Manager.RollToHit(attacker, target))
                    {
                        int damage = Manager.RollDamage(attacker);
                        Manager.ApplyDamage(target, damage);
                    }

                    attacker.AttacksRemainingThisTurn--;

                    if (attacker.HasActedThisTurn)
                        Manager.EndCurrentTurn();
                    else
                        Manager.CancelTargeting();
                }

                return;
            }

            if (WasKeyJustPressed(Keys.Escape))
            {
                Manager.CancelTargeting();
                return;
            }
        }

        private void HandlePlayerInput()
        {
            if (WasKeyJustPressed(Keys.A))
            {
                Manager.TransitionTo(CombatState.SelectingTarget);
                return;
            }

            if (WasKeyJustPressed(Keys.M))
            {
                Manager.TransitionTo(CombatState.SelectingMove);
                return;
            }

            if (WasKeyJustPressed(Keys.P))
            {
                Manager.TransitionTo(CombatState.UsingPower);
                return;
            }

            if (WasKeyJustPressed(Keys.Space))
                Manager.EndCurrentTurn();
        }

        private void ResolveEnemyTurn()
        {
            // Enemy does nothing yet — just passes their turn
            // AI behavior gets built here later, one piece at a time
            Manager.EndCurrentTurn();
        }

        private bool WasKeyJustPressed(Keys key)
        {
            return CurrentKeys.IsKeyDown(key)
                && PreviousKeys.IsKeyUp(key);
        }

        private void DrawGrid()
        {
            for (int row = 0; row < GRID_ROWS; row++)
            {
                for (int col = 0; col < GRID_COLS; col++)
                {
                    Rectangle tileRect = new Rectangle(
                        GRID_ORIGIN_X + (col * TILE_SIZE),
                        GRID_ORIGIN_Y + (row * TILE_SIZE),
                        TILE_SIZE - 1,
                        TILE_SIZE - 1
                    );

                    Color tileColor;
                    if ((row + col) % 2 == 0)
                        tileColor = new Color(60, 50, 40);
                    else
                        tileColor = new Color(70, 60, 48);

                    DrawFilledRect(tileRect, tileColor);
                }
            }
        }

        // =====================================================
        // DRAW COMBATANTS
        // =====================================================

        private void DrawCombatants()
        {
            Combatant current = Manager.CurrentCombatant();

            foreach (Combatant combatant in Manager.GetAliveCombatants())
            {
                Rectangle combatantRect = new Rectangle(
                    GRID_ORIGIN_X + (combatant.GridCol * TILE_SIZE) + 4,
                    GRID_ORIGIN_Y + (combatant.GridRow * TILE_SIZE) + 4,
                    TILE_SIZE - 8,
                    TILE_SIZE - 8
                );

                Color combatantColor;
                if (combatant.Type == CombatantType.PlayerCharacter)
                    combatantColor = Color.CornflowerBlue;
                else if (combatant.Type == CombatantType.Companion)
                    combatantColor = Color.MediumSeaGreen;
                else
                    combatantColor = Color.Crimson;

                // Draw highlight behind active combatant
                if (current != null && combatant == current)
                {
                    Rectangle highlight = new Rectangle(
                        combatantRect.X - 3,
                        combatantRect.Y - 3,
                        combatantRect.Width + 6,
                        combatantRect.Height + 6
                    );
                    DrawFilledRect(highlight, Color.Yellow);
                }

                DrawFilledRect(combatantRect, combatantColor);

                // Draw first initial
                SpriteBatch.DrawString(
                    Font,
                    combatant.Name.Substring(0, 1),
                    new Vector2(combatantRect.X + 6, combatantRect.Y + 4),
                    Color.White
                );
            }
        }

        // =====================================================
        // DRAW MESSAGE PANEL
        // =====================================================

        private void DrawMessagePanel()
        {
            Rectangle panelRect = new Rectangle(PANEL_X, PANEL_Y, 280, 300);
            DrawFilledRect(panelRect, new Color(20, 20, 30));

            SpriteBatch.DrawString(
                Font,
                "-- Combat Log --",
                new Vector2(PANEL_X + 8, PANEL_Y + 8),
                Color.Gold
            );

            int lineY = PANEL_Y + 30;
            foreach (string message in Manager.RecentMessages)
            {
                SpriteBatch.DrawString(
                    Font,
                    message,
                    new Vector2(PANEL_X + 8, lineY),
                    Color.LightGray
                );
                lineY += 20;
            }
        }

        // =====================================================
        // DRAW ACTION MENU
        // =====================================================

        private void DrawActionMenu()
        {
            int menuX = PANEL_X;
            int menuY = PANEL_Y + 320;

            Rectangle menuRect = new Rectangle(menuX, menuY, 280, 160);
            DrawFilledRect(menuRect, new Color(20, 20, 30));

            SpriteBatch.DrawString(
                Font,
                "-- Actions --",
                new Vector2(menuX + 8, menuY + 8),
                Color.Gold
            );

            if (Manager.CurrentState == CombatState.PlayerTurn)
            {
                SpriteBatch.DrawString(Font, "[A] Attack",
                    new Vector2(menuX + 8, menuY + 30), Color.White);

                SpriteBatch.DrawString(Font, "[M] Move",
                    new Vector2(menuX + 8, menuY + 52), Color.White);

                SpriteBatch.DrawString(Font, "[P] Use Power",
                    new Vector2(menuX + 8, menuY + 74), Color.White);

                SpriteBatch.DrawString(Font, "[Space] End Turn",
                    new Vector2(menuX + 8, menuY + 96), Color.White);
            }
            else
            {
                SpriteBatch.DrawString(
                    Font,
                    Manager.CurrentState.ToString(),
                    new Vector2(menuX + 8, menuY + 30),
                    Color.Yellow
                );
            }
        }

        // =====================================================
        // DRAW HELPER
        // =====================================================

        private void DrawFilledRect(Rectangle rect, Color color)
        {
            SpriteBatch.Draw(Pixel, rect, color);
        }
    }
}