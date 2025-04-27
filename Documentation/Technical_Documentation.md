# 📓 Progreso ShadowSky - Sesión del 26 de abril 2025

## 1. Decisión del entorno de desarrollo
- Uso de **Visual Studio Code** como IDE principal.
- Configuración para trabajar con **MonoGame** y contenido manual (.csproj).

## 2. Creación de la estructura general del proyecto
- Carpeta `Content/Images/Player/` para sprites del jugador.
- Carpeta `Content/Images/World/` para fondos.
- Carga inicial de imágenes (`player.png` y `background4.png`).

## 3. Configuración del archivo `.csproj`
- Corrección para copiar imágenes al directorio de salida (`CopyToOutputDirectory`).
- Inclusión de `Icon.ico` y `app.manifest`.

## 4. Desarrollo de la clase `Game1`
- Declaración de texturas y posición.
- Métodos de ciclo de juego:
  - `Initialize()`
  - `LoadContent()`
  - `Update()`
  - `Draw()`

## 5. Implementación de la movilidad del jugador
- Movimiento básico usando teclado (`W`, `A`, `S`, `D`).

## 6. Ajuste dinámico del fondo y ventana
- Tamaño de ventana configurado al tamaño de la pantalla.
- Escalado automático del fondo (`background4.png`) para ocupar toda la ventana.

## 7. Ajuste de escala del personaje
- Reducción del sprite del jugador al **10%** del tamaño original.

## 8. Pruebas y validación
- Carga correcta de texturas.
- Movimiento funcional.
- Escalado dinámico del fondo.
- Ejecución exitosa desde VS Code.

## 9. Creación del mensaje de commit
- Mensaje generado describiendo todos los avances logrados.

---

# 🔥 Resumen rápido
- Cambio de ambiente a **Visual Studio Code**.
- Creación de estructura de carpetas e imágenes.
- Configuración del proyecto.
- Desarrollo del motor básico de juego.
- Implementación de movimiento del personaje.
- Ajuste dinámico del fondo y ventana.
- Pruebas exitosas.

