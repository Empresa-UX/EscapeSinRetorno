# 🧩 Escape sin Retorno — Juego de Terror, Supervivencia y Puzles 2D

> **"Despiertas sin recuerdos en un complejo subterráneo. Los pasillos están vacíos… pero no estás solo."**

**Escape sin Retorno** es un videojuego 2D de terror psicológico, exploración y supervivencia desarrollado en **C# + MonoGame**. Encarnas a un personaje sin memoria atrapado en una instalación oscura, perseguido por entidades hostiles, con recursos limitados y una sola meta: **escapar con vida**.

---

## 📚 Índice

- [✨ Características principales](#-características-principales)
- [🧠 Historia y ambientación](#-historia-y-ambientación)
- [🎮 Cómo se juega](#-cómo-se-juega)
  - [Estadísticas de supervivencia](#estadísticas-de-supervivencia)
  - [Inventario y objetos](#inventario-y-objetos)
  - [Puertas, llaves y progresión](#puertas-llaves-y-progresión)
  - [Enemigos e IA](#enemigos-e-ia)
  - [Chat interno y comandos](#chat-interno-y-comandos)
- [🎛 Controles](#-controles)
- [🧪 Modo desarrollador / debug](#-modo-desarrollador--debug)
- [🛠 Stack técnico](#-stack-técnico)
- [💻 Requisitos](#-requisitos)
- [📦 Instalación y ejecución](#-instalación-y-ejecución)
- [📁 Estructura del proyecto](#-estructura-del-proyecto)
- [🗺 Roadmap](#-roadmap)
- [🤝 Contribuciones](#-contribuciones)
- [👤 Autor / Créditos](#-autor--créditos)
- [📄 Licencia](#-licencia)

---

## ✨ Características principales

- 🕹 **Gameplay 2D top-down** con movimiento libre y combate cuerpo a cuerpo.
- 🧩 **Puzles interconectados**: el progreso en una zona afecta otras áreas del mapa.
- 🗝 **Sistema de llaves y puertas** (incluyendo puertas especiales con requisitos concretos).
- 📦 **Inventario con uso de ítems** (pociones, comida, agua, llaves, etc.).
- 🧪 **Sistema de supervivencia completo**:
  - Vida (HP)
  - Hambre
  - Sed
  - Estamina
  - Cordura / efectos visuales
- 👹 **Enemigos gestionados por un EnemyManager**, con comportamientos diferenciados (incluyendo guardianes / entidades especiales).
- 🔊 **Audio integrado**:
  - Música de menú y exploración
  - Sonidos de ataque, daño y muerte del jugador
  - Sonido al abrir puertas
- 💬 **Chat interno** con soporte de comandos (debug / utilidades).
- 🌐 **Infraestructura para multijugador** (cliente/servidor, input remoto, chat de red).
- 🎨 **Pixel art oscuro y minimalista**, con overlay de viñeta y efectos según estado del jugador.
- 🧱 **Mapa basado en tiles** con colisiones, puertas y spawns definidos por datos de mapa.

---

## 🧠 Historia y ambientación

Despiertas en un complejo subterráneo, sin memoria de quién eres ni cómo llegaste ahí. Solo sabes que:

- El lugar está **abandonado**, pero **preparado para contener algo**.
- Hay notas, objetos y estructuras que sugieren experimentos, vigilancia y encierro.
- A medida que exploras, se hace evidente que no estás solo: **alguien —o algo— te observa**.

No se trata únicamente de encontrar la salida:

- La **falta de recursos** empieza a afectarte.
- El entorno y la oscuridad juegan con tu mente.
- Cada puerta abierta puede acercarte a la verdad… o a la muerte.

> _"No soy el primero atrapado aquí… pero quizá sea el último en salir."_  

---

## 🎮 Cómo se juega

El loop principal del juego combina:

1. **Exploración** del mapa (pasillos, salas, zonas clave).
2. **Gestión de recursos** (comida, agua, pociones, llaves).
3. **Evitar/afrontar enemigos** usando combate y posicionamiento.
4. **Uso de llaves y objetos** para desbloquear nuevas áreas.
5. **Tomar decisiones** con consecuencias (qué usar, qué guardar, qué ignorar).

---

### Estadísticas de supervivencia

El jugador tiene un sistema de estadísticas gestionado por `PlayerStats`:

- ❤️ **Vida (Health)**  
  Si llega a 0 → muerte, animación de death + pantalla de muerte.

- 🍗 **Hambre (Hunger)**  
  Bajar demasiado afecta tu supervivencia y regeneración.

- 💧 **Sed (Thirst)**  
  Impacta negativamente en estamina y resistencia.

- ⚡ **Estamina (Stamina)**  
  Se consume al correr. Si abusas del sprint:
  - Entra en **agotamiento**.
  - Debes recuperar cierta cantidad antes de volver a correr.

- 🧠 **Cordura / efectos visuales (Sanity / Fx)**  
  Afecta la forma en que ves el mundo (vignette overlay, efectos visuales).

Estas estadísticas se actualizan cada frame según tus acciones: moverte, correr, recibir daño, usar ítems, etc.

---

### Inventario y objetos

El inventario del jugador (`PlayerInventory`) permite:

- **Almacenar ítems** como:
  - `potion_life`
  - `watter_bottle`
  - `meal`
  - `main_key`
  - `cyan_key`
- **Usar ítems** directamente desde la UI:
  - Pociones → curan vida.
  - Agua → recupera sed + algo de estamina.
  - Comida → afecta hambre, cordura y HP.
- **Soltar ítems al suelo**, generando pickups en el mundo.

El inventario cuenta con una **UI específica** renderizada sobre la pantalla de juego, con navegación y uso de ítems contextual.

---

### Puertas, llaves y progresión

El `DoorManager` se encarga de:

- Spawnear puertas desde los datos del mapa.
- Manejar puertas **abiertas / cerradas / bloqueadas**.
- Validar si el jugador tiene la llave adecuada en el inventario.
- Permitir interacción cercana:
  - Si tienes la llave correcta → la puerta se abre (sonido de puerta).
  - Si no → el juego puede lanzar feedback por chat/system message.

Esto permite diseñar:

- Rutas alternativas
- Backtracking controlado
- Accesos solo si exploraste y encontraste bien los ítems clave

---

### Enemigos e IA

Los enemigos se gestionan mediante:

- `EnemyManager` → spawnea enemigos a partir de los datos del mapa.
- Cada enemigo puede:
  - Patrullar zonas
  - Perseguir al jugador
  - Interactuar con ciertas puertas / triggers (dependiendo del tipo)
- Hay entidades especiales como **guardianes**, con interacción a través de inventario y chat.

Cuando el jugador ataca, se usan:

- Combos de ataques (`Attack_1`, `Attack_2`, `Attack_3`, `Attack_4`)
- Determinación de frames “hitbox activos”
- Detección de colisión con enemigos y aplicación de daño

---

### Chat interno y comandos

El juego incluye un **Chat interno** gestionado por:

- `ChatManager`
- `ChatRenderer`
- `ChatCommandBootstrap`
- `ChatCommandExecutor`

Permite:

- Enviar mensajes (en singleplayer o cliente multiplayer).
- Recibir mensajes de otros jugadores en modo cliente.
- Ejecutar **comandos internos** para debug / testing (ej: manipular inventario, TP, borrar pickups, toggles de debug, etc.).

> 🔤 **Abrir chat:** ver sección de Controles.  
> Los comandos disponibles están definidos en el código (`ChatCommandBootstrap`), e incluyen utilidades como:
> - Limpieza de pickups (`/wipeinv`, etc.)
> - Toggles de debug (noclip, FPS, colisiones)
> - Comandos de administración / pruebas

---

## 🎛 Controles

### 🖥 PC (teclado)

**Movimiento y acciones básicas:**

- `W / A / S / D` → Moverse
- `Z` → Saltar
- `X` → Correr (consume estamina; cuidado con el agotamiento)
- `C` → Atacar (combos de ataque)
- `E` → Abrir/Cerrar inventario (y usar objetos)
- `B` → Interactuar con puertas cercanas / entidades especiales
- `ESC` → Salir / menú

**Chat y comandos:**

- `T` → Abrir/Cerrar chat interno
  - Cuando el chat está abierto, puedes escribir mensajes y comandos.

**Otros:**

- `F11` (o tecla que configures) → Alternar fullscreen en el código (`ToggleFullscreen()`).

> 📱 **Android**: existe intención de soporte mediante joystick virtual y botones táctiles, pero aún pendiente de implementación final.

---

## 🧪 Modo desarrollador / debug

Durante el desarrollo, existen varias banderas y utilidades usadas para testear y ajustar el juego:

- `Player.DebugDrawHitboxes` → pintar hitboxes del jugador.
- `Player.DebugDrawFPS` → mostrar FPS en pantalla.
- `Player.DebugNoClip` → desactivar colisiones del jugador.
- `Game1.DebugShowCollisions` → visualizar tiles colisionables en el mapa.

Estas flags se suelen activar mediante **comandos en el chat** o internamente en el código, según lo que esté configurado en `ChatCommandBootstrap`.

---

## 🛠 Stack técnico

- **Lenguaje:** C# (.NET)
- **Framework:** [MonoGame 3.8](https://www.monogame.net/)
- **Motor de mapas:** TileMap + datos cargados externamente (Tiled u otro formato similar).
- **Audio:** `Song` + `SoundEffect` (MonoGame MediaPlayer / SoundEffect API)
- **Arquitectura principal:**
  - `Game1` → bucle principal, estados (menú / juego), render y update global
  - `Player` → movimiento, combate, animaciones, stats, interacción
  - `EnemyManager` / `Enemy` → IA y comportamiento de enemigos
  - `DoorManager` → puertas, llaves, colisiones lógicas
  - `PlayerInventory` / `ItemDatabase` / `ItemPickupManager` → inventario, ítems y pickups
  - `ChatManager` / `ChatRenderer` / `ChatCommandBootstrap` → chat in-game + comandos
  - `MultiplayerManager` / `GameNetMode` / `NetConfig` → infraestructura de cliente/servidor

---

## 💻 Requisitos

### Mínimos recomendados (PC)

- **Sistema operativo:** Windows 10 / 11
- **Framework:** .NET 6 o .NET 7
- **Dependencias:** MonoGame 3.8
- **RAM:** 4 GB
- **GPU:** Cualquier integrada moderna soportando OpenGL/DirectX usado por MonoGame
- **Almacenamiento:** ~200 MB libres (juego + assets + logs)

---

## 📦 Instalación y ejecución

### 1️⃣ Clonar el repositorio

```bash
git clone https://github.com/<TU_USUARIO>/<TU_REPO>.git
cd <TU_REPO>
