using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System.Diagnostics;

namespace ShadowSky;

public class Game1 : Game
{
    private GraphicsDeviceManager _graphics;
    private World.World world;
    private Texture2D playerTexture;
    private Vector2 playerPosition;
    private Texture2D backgroundTexture;

    public Game1()
    {
        _graphics = new GraphicsDeviceManager(this);
        Content.RootDirectory = "Content";
        IsMouseVisible = true;

        _graphics.PreferredBackBufferWidth = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Width;
        _graphics.PreferredBackBufferHeight = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Height;
        _graphics.ApplyChanges();

        Debug.WriteLine("Game1: Constructor ejecutado.");
    }

    protected override void Initialize()
    {
        playerPosition = new Vector2(100, 100);
        Debug.WriteLine("Game1: Initialize ejecutado. Posición inicial del jugador: " + playerPosition);
        base.Initialize();
    }

    protected override void LoadContent()
    {
        Globals.SpriteBatch = new SpriteBatch(GraphicsDevice);

        playerTexture = Content.Load<Texture2D>("Images/Player/PersonajeUno/player");
        backgroundTexture = Content.Load<Texture2D>("Images/World/background3");

        Debug.WriteLine("Game1: Texturas del jugador y fondo cargadas.");

        world = new World.World(50, 50);
        Debug.WriteLine("Game1: Mundo creado con tamaño 50x50.");

        world.LoadContent(Content);
        Debug.WriteLine("Game1: Contenido del mundo cargado.");
    }

    protected override void Update(GameTime gameTime)
    {
        if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed ||
            Keyboard.GetState().IsKeyDown(Keys.Escape))
        {
            Debug.WriteLine("Game1: Saliendo del juego por input.");
            Exit();
        }

        var keyboardState = Keyboard.GetState();
        if (keyboardState.IsKeyDown(Keys.W))
        {
            playerPosition.Y -= 2;
            Debug.WriteLine("Game1: Jugador movido hacia arriba. Nueva posición: " + playerPosition);
        }
        if (keyboardState.IsKeyDown(Keys.S))
        {
            playerPosition.Y += 2;
            Debug.WriteLine("Game1: Jugador movido hacia abajo. Nueva posición: " + playerPosition);
        }
        if (keyboardState.IsKeyDown(Keys.A))
        {
            playerPosition.X -= 2;
            Debug.WriteLine("Game1: Jugador movido hacia la izquierda. Nueva posición: " + playerPosition);
        }
        if (keyboardState.IsKeyDown(Keys.D))
        {
            playerPosition.X += 2;
            Debug.WriteLine("Game1: Jugador movido hacia la derecha. Nueva posición: " + playerPosition);
        }

        base.Update(gameTime);
    }

    protected override void Draw(GameTime gameTime)
    {
        GraphicsDevice.Clear(Color.CornflowerBlue);

        Globals.SpriteBatch.Begin();

        // Dibujar el fondo
        Globals.SpriteBatch.Draw(
            backgroundTexture,
            new Rectangle(0, 0, _graphics.PreferredBackBufferWidth, _graphics.PreferredBackBufferHeight),
            Color.White
        );

        // Dibujar el mundo
        world.Draw();

        // Dibujar el jugador
        float scale = 0.1f;
        int newWidth = (int)(playerTexture.Width * scale);
        int newHeight = (int)(playerTexture.Height * scale);

        Globals.SpriteBatch.Draw(
            playerTexture,
            new Rectangle((int)playerPosition.X, (int)playerPosition.Y, newWidth, newHeight),
            Color.White
        );

        Globals.SpriteBatch.End();

        Debug.WriteLine("Game1: Draw ejecutado.");

        base.Draw(gameTime);
    }
}
