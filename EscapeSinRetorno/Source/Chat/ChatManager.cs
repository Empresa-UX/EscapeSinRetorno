using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace EscapeSinRetorno.Source.Chat
{
    public sealed class ChatManager
    {
        private const float MessageLifetime = 15f;
        private readonly List<ChatMessage> _messages = new();

        private string _input = string.Empty;
        private bool _isOpen = false;
        private bool _suppressFirstChar = false;

        private float _time = 0f;
        private float _caretTimer = 0f;

        private KeyboardState _prevKeyboard;

        // --- BACKSPACE HOLD ---
        private float _backspaceTimer = 0f;
        private const float BackspaceInitialDelay = 0.35f;
        private const float BackspaceRepeatDelay = 0.04f;

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

        // ================================================================
        // OPEN/CLOSE
        // ================================================================
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
            _suppressFirstChar = true;
        }

        public void Close()
        {
            _isOpen = false;
        }

        // ================================================================
        // ADD MESSAGES
        // ================================================================
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

        // ================================================================
        // UPDATE
        // ================================================================
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
                HandleOpenChatInput(ks, dt);

            _prevKeyboard = ks;
        }

        // ================================================================
        // MANEJO DEL TEXTO POR EVENTO (EL REAL, COMPLETO)
        // ================================================================
        public void ReceiveTextInput(TextInputEventArgs e)
        {
            if (!_isOpen)
                return;

            // Ignorar teclas especiales: enter, escape, backspace
            if (e.Key == Keys.Enter || e.Key == Keys.Escape || e.Key == Keys.Back)
                return;

            // Agregar caracter tal cual (respeta layout, acentos, símbolos, SHIFT…)
            _input += e.Character;
        }

        // ================================================================
        // INPUT CUANDO EL CHAT ESTÁ ABIERTO
        // ================================================================
        private void HandleOpenChatInput(KeyboardState ks, float dt)
        {
            // Cerrar con ESC
            if (JustPressed(ks, Keys.Escape))
            {
                Close();
                return;
            }

            // Enviar mensaje con ENTER
            if (JustPressed(ks, Keys.Enter))
            {
                SendCurrentInput();
                Close();
                return;
            }

            // Ignorar teclas justo después de abrir el chat
            if (_suppressFirstChar)
            {
                _suppressFirstChar = false;
                return;
            }

            // ===== BACKSPACE SOSTENIDO =====
            if (ks.IsKeyDown(Keys.Back))
            {
                if (_input.Length > 0)
                {
                    if (JustPressed(ks, Keys.Back))
                    {
                        _input = _input[..^1];
                        _backspaceTimer = BackspaceInitialDelay;
                    }
                    else
                    {
                        _backspaceTimer -= dt;
                        if (_backspaceTimer <= 0f)
                        {
                            _input = _input[..^1];
                            _backspaceTimer = BackspaceRepeatDelay;
                        }
                    }
                }
            }
            else
            {
                _backspaceTimer = 0f;
            }
        }

        private bool JustPressed(KeyboardState ks, Keys key)
        {
            return ks.IsKeyDown(key) && !_prevKeyboard.IsKeyDown(key);
        }

        // ================================================================
        // SEND MESSAGE / COMMAND
        // ================================================================
        private void SendCurrentInput()
        {
            string trimmed = _input.Trim();
            if (string.IsNullOrEmpty(trimmed))
            {
                _input = string.Empty;
                return;
            }

            // COMANDO
            if (trimmed.StartsWith("/"))
            {
                string[] parts = trimmed.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                string cmd = parts[0];
                string[] args = parts.Length > 1 ? parts[1..] : Array.Empty<string>();

                AddSystemMessage(trimmed);
                CommandRequested?.Invoke(cmd, args);
            }
            else
            {
                // MENSAJE NORMAL → Añadir remitente
                string formatted = $"[Player]: {trimmed}";
                AddPlayerMessage(formatted);
                MessageSent?.Invoke(trimmed);
            }

            _input = string.Empty;
        }
        public void ClearMessages()
        {
            _messages.Clear();
        }

    }
}
