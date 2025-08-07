using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;

namespace EscapeSinRetorno.Source.Core
{
    public class GameStateManager
    {
        private Dictionary<string, IGameState> states;
        private IGameState currentState;
        private ContentManager content;
        private GraphicsDevice graphicsDevice;

        public string CurrentStateName { get; private set; }

        public GameStateManager(ContentManager content, GraphicsDevice graphicsDevice)
        {
            states = new Dictionary<string, IGameState>();
            this.content = content;
            this.graphicsDevice = graphicsDevice;
        }

        public void AddState(string name, IGameState state)
        {
            states[name] = state;
            state.LoadContent(content, graphicsDevice);
        }

        public void ChangeState(string stateName)
        {
            if (states.ContainsKey(stateName))
            {
                currentState = states[stateName];
                CurrentStateName = stateName;
            }
        }

        public void Update(GameTime gameTime)
        {
            currentState?.Update(gameTime);
            currentState?.HandleInput();
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            currentState?.Draw(spriteBatch);
        }
    }
}