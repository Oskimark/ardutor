# ⚡ Arduino Fast Upload Helper

Herramienta ligera para **re-subir firmware ya compilado** sin tener que esperar nuevamente el proceso de compilación en el IDE.

---

## 🚀 Problema que resuelve

Cuando trabajas con hardware “barebones” (sin placas completas tipo Arduino con USB integrado), es muy común tener:

* conexiones inestables
* falsos contactos
* cables sueltos
* soldaduras imperfectas

Esto provoca que:

1. Compilas tu código (⏳ puede tardar bastante)
2. Intentas subirlo
3. ❌ Falla el upload por un problema físico
4. Arreglas el hardware
5. 😩 Tenés que volver a esperar la compilación completa

👉 **Totalmente innecesario**, porque el firmware ya estaba compilado.
PERO EL IDE NO TIENE UNA OPCION SOLO PARA SUBIR SIN RE COMPILAR

---

## 💡 Solución

**Arduino Fast Upload Helper** detecta automáticamente los builds generados por el IDE y te permite:

* 🔄 Reintentar el upload en segundos
* ⛔ Evitar recompilaciones innecesarias
* ⚡ Iterar mucho más rápido durante debugging de hardware

---

## 🧠 Cómo funciona

1. Monitorea la carpeta temporal de builds de Arduino:

   ```
   %LOCALAPPDATA%\Temp\arduino\sketches
   ```

2. Detecta cuando se genera un nuevo firmware:

   * `.bin` (ESP8266 / ESP32)
   * `.hex` (AVR)

3. Lee la configuración desde:

   ```
   build.options.json
   ```

4. Permite re-subir el firmware usando:

   * `esptool` (ESP)
   * `avrdude` (AVR)

---

## 🖥️ Características

* 🧩 Aplicación residente (system tray)
* 🔔 Notificación al detectar nuevo build
* ⌨️ Hotkey global para upload rápido
* ⚙️ Configuración de puerto serial
* 📂 Acceso rápido al build detectado
* 🔌 Soporte para:

  * ESP8266 / ESP32
  * Arduino AVR (UNO, Nano, etc.)

### 🆕 Modo manual

Además del modo automático, la app permite:

* 📁 Seleccionar manualmente un build previo
* 📄 Cargar archivos:

  * `.bin`
  * `.hex`
* ⚙️ Configurar manualmente:

  * tipo de placa / FQBN
  * chip
  * frecuencia de cristal
  * puerto serial
  * velocidad (baud)

👉 Ideal para:

* firmware externos
* builds antiguos
* pruebas rápidas sin depender del IDE

---

## ⚡ Flujo de uso

1. Compilas normalmente en el IDE
2. La app detecta el build automáticamente
3. Arreglas tu hardware si algo falla
4. Presionas el hotkey o usas el menú
5. 🚀 Upload instantáneo (sin recompilar)

---

## 📦 Instalación

1. Descargar la carpeta `publish`
2. Ejecutar:

```
ArduinoHelper.exe
```

> ⚠️ Requiere tener instalado .NET Runtime correspondiente

---

## 🛠️ Compilación (para desarrolladores)

```bash
dotnet publish ArduinoHelper.csproj -c Release -r win-x64 --self-contained false
```

---

## 📁 Estructura

```
/Services        → lógica de upload
/UI              → system tray
/Watcher         → monitoreo de builds
```

---

## ⚠️ Notas

* El tool no reemplaza el Arduino IDE
* Solo reutiliza builds ya compilados
* Ideal para debugging de hardware, no para desarrollo de código

---

## 🎯 Casos de uso ideales

* Prototipos en breadboard
* Circuitos soldados manualmente
* Chips sin interfaz USB (barebones)
* Desarrollo con muchos reintentos de upload

---

## 🤝 Contribuciones

Ideas, mejoras y PRs son bienvenidos.

---

## 📜 Licencia

Libre para usar, modificar y mejorar.

---

## 💬 Motivación

Reducir minutos de espera innecesaria a segundos.

---

**De esto:**
⏳ Compilar → Fallar → Esperar → Reintentar

**A esto:**
⚡ Upload → Fallar → Arreglar → Upload inmediato

---

🔥 Si trabajás con hardware real, esto te va a ahorrar MUCHO tiempo.
