using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace EscapeSinRetorno.Source.Chat
{
    public sealed class ChatManager
    {
        private const float MessageLifetime = 10f; // segundos
        private readonly List<ChatMessage> _messages = new();

        private string _input = string.Empty;
        private bool _isOpen = false;
        private bool _suppressFirstChar = false;
        private float _time = 0f;
        private float _caretTimer = 0f;

        private KeyboardState _prevKeyboard;

        public bool IsOpen => _isOpen;
        public string CurrentInput => _input;
        public float CaretTime => _caretTimer;

        public IReadOnlyList<ChatMessage> Messages => _messages;

        public event Action<string, string[]> CommandRequested;
        public event Action<string> MessageSent;

        public ChatManager()
        {
            _prevKeyboard = Keyboard.GetState();
        }

        // ----------------- API externa -----------------

        public void Toggle()
        {
            if (_isOpen) Close();
            else Open();
        }

        public void Open()
        {
            if (_isOpen) return;
            _isOpen = true;
            _input = string.Empty;
            _suppressFirstChar = true; // para no meter la tecla T que lo abrió
        }

        public void Close()
        {
            _isOpen = false;
        }

        public void AddSystemMessage(string text)
        {
            AddMessage(text, ChatMessageType.System);
        }

        public void AddErrorMessage(string text)
        {
            AddMessage(text, ChatMessageType.Error);
        }

        public void AddPlayerMessage(string text)
        {
            AddMessage(text, ChatMessageType.Player);
        }

        private void AddMessage(string text, ChatMessageType type)
        {
            _messages.Add(new ChatMessage(text, type, _time));
        }

        // ----------------- Update / input -----------------

        public void Update(GameTime gameTime)
        {
            float dt = (float)gameTime.ElapsedGameTime.TotalSeconds;
            _time += dt;
            _caretTimer += dt;

            // Expirar mensajes viejos
            for (int i = _messages.Count - 1; i >= 0; i--)
            {
                if (_time - _messages[i].Time > MessageLifetime)
                    _messages.RemoveAt(i);
            }

            var ks = Keyboard.GetState();

            if (_isOpen)
            {
                HandleOpenChatInput(ks);
            }

            _prevKeyboard = ks;
        }

        private void HandleOpenChatInput(KeyboardState ks)
        {
            bool shift = ks.IsKeyDown(Keys.LeftShift) || ks.IsKeyDown(Keys.RightShift);

            // Cerrar con ESC
            if (JustPressed(ks, Keys.Escape))
            {
                Close();
                return;
            }

            // Enviar con ENTER (y cerrar)
            if (JustPressed(ks, Keys.Enter))
            {
                SendCurrentInput();
                Close();
                return;
            }

            // Primera frame tras abrir → ignorar todas las teclas
            if (_suppressFirstChar)
            {
                _suppressFirstChar = false;
                return;
            }

            // Backspace
            if (JustPressed(ks, Keys.Back) && _input.Length > 0)
            {
                _input = _input[..^1];
                return;
            }

            // Texto
            HandleTextInput(ks, shift);
        }

        private bool JustPressed(KeyboardState ks, Keys key)
        {
            return ks.IsKeyDown(key) && !_prevKeyboard.IsKeyDown(key);
        }

        private void HandleTextInput(KeyboardState ks, bool shift)
        {
            // Letras A..Z
            if (JustPressed(ks, Keys.A)) _input += shift ? 'A' : 'a';
            if (JustPressed(ks, Keys.B)) _input += shift ? 'B' : 'b';
            if (JustPressed(ks, Keys.C)) _input += shift ? 'C' : 'c';
            if (JustPressed(ks, Keys.D)) _input += shift ? 'D' : 'd';
            if (JustPressed(ks, Keys.E)) _input += shift ? 'E' : 'e';
            if (JustPressed(ks, Keys.F)) _input += shift ? 'F' : 'f';
            if (JustPressed(ks, Keys.G)) _input += shift ? 'G' : 'g';
            if (JustPressed(ks, Keys.H)) _input += shift ? 'H' : 'h';
            if (JustPressed(ks, Keys.I)) _input += shift ? 'I' : 'i';
            if (JustPressed(ks, Keys.J)) _input += shift ? 'J' : 'j';
            if (JustPressed(ks, Keys.K)) _input += shift ? 'K' : 'k';
            if (JustPressed(ks, Keys.L)) _input += shift ? 'L' : 'l';
            if (JustPressed(ks, Keys.M)) _input += shift ? 'M' : 'm';
            if (JustPressed(ks, Keys.N)) _input += shift ? 'N' : 'n';
            if (JustPressed(ks, Keys.O)) _input += shift ? 'O' : 'o';
            if (JustPressed(ks, Keys.P)) _input += shift ? 'P' : 'p';
            if (JustPressed(ks, Keys.Q)) _input += shift ? 'Q' : 'q';
            if (JustPressed(ks, Keys.R)) _input += shift ? 'R' : 'r';
            if (JustPressed(ks, Keys.S)) _input += shift ? 'S' : 's';
            if (JustPressed(ks, Keys.T)) _input += shift ? 'T' : 't';
            if (JustPressed(ks, Keys.U)) _input += shift ? 'U' : 'u';
            if (JustPressed(ks, Keys.V)) _input += shift ? 'V' : 'v';
            if (JustPressed(ks, Keys.W)) _input += shift ? 'W' : 'w';
            if (JustPressed(ks, Keys.X)) _input += shift ? 'X' : 'x';
            if (JustPressed(ks, Keys.Y)) _input += shift ? 'Y' : 'y';
            if (JustPressed(ks, Keys.Z)) _input += shift ? 'Z' : 'z';

            // Números (fila superior)
            if (JustPressed(ks, Keys.D0)) _input += '0';
            if (JustPressed(ks, Keys.D1)) _input += '1';
            if (JustPressed(ks, Keys.D2)) _input += '2';
            if (JustPressed(ks, Keys.D3)) _input += '3';
            if (JustPressed(ks, Keys.D4)) _input += '4';
            if (JustPressed(ks, Keys.D5)) _input += '5';
            if (JustPressed(ks, Keys.D6)) _input += '6';

            // Aquí viene el truco para tu layout: Shift+7 = '/'
            if (JustPressed(ks, Keys.D7))
            {
                _input += shift ? '/' : '7';
            }

            if (JustPressed(ks, Keys.D8)) _input += '8';
            if (JustPressed(ks, Keys.D9)) _input += '9';

            // Números keypad
            if (JustPressed(ks, Keys.NumPad0)) _input += '0';
            if (JustPressed(ks, Keys.NumPad1)) _input += '1';
            if (JustPressed(ks, Keys.NumPad2)) _input += '2';
            if (JustPressed(ks, Keys.NumPad3)) _input += '3';
            if (JustPressed(ks, Keys.NumPad4)) _input += '4';
            if (JustPressed(ks, Keys.NumPad5)) _input += '5';
            if (JustPressed(ks, Keys.NumPad6)) _input += '6';
            if (JustPressed(ks, Keys.NumPad7)) _input += '7';
            if (JustPressed(ks, Keys.NumPad8)) _input += '8';
            if (JustPressed(ks, Keys.NumPad9)) _input += '9';

            // Espacio
            if (JustPressed(ks, Keys.Space)) _input += ' ';

            // Algunos signos básicos
            if (JustPressed(ks, Keys.OemPeriod) || JustPressed(ks, Keys.Decimal)) _input += '.';
            if (JustPressed(ks, Keys.OemComma)) _input += ',';
            if (JustPressed(ks, Keys.OemMinus) || JustPressed(ks, Keys.Subtract)) _input += '-';
        }

        private void SendCurrentInput()
        {
            string trimmed = _input.Trim();
            if (string.IsNullOrEmpty(trimmed))
            {
                _input = string.Empty;
                return;
            }

            // Comando
            if (trimmed.StartsWith("/"))
            {
                string[] parts = trimmed.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                string cmd = parts[0];
                string[] args = parts.Length > 1 ? parts[1..] : Array.Empty<string>();

                // Lo mostramos también en el chat
                AddSystemMessage(trimmed);
                CommandRequested?.Invoke(cmd, args);
            }
            else
            {
                // Mensaje normal
                AddPlayerMessage(trimmed);
                MessageSent?.Invoke(trimmed);   // 👈 Notificamos a Game1 para mandarlo al server

            }

            _input = string.Empty;
        }
    }
}
