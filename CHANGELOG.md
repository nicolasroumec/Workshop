# Changelog

Todos los cambios importantes de Workshop se anotan en este archivo.

El formato sigue [Keep a Changelog](https://keepachangelog.com/es-ES/1.1.0/) y el proyecto usa [versionado semántico](https://semver.org/lang/es/).

## [Sin publicar]

## [0.6.0] - 2026-11-19

### Agregado

- **Auditoría CSS** revisa también las variables CSS: las que se usan con `var(--x)` sin declararse en ningún lado (separadas en sin respaldo y con respaldo) y las que se declaran y nadie usa, con archivo y número de línea. Busca en todos los `.css`, incluidos los `.razor.css`, y en los estilos inline de `.razor`, `.cs`, `.cshtml`, `.html`, `.js` y `.ts`. Las variables de Bootstrap (`--bs-*`) cuentan como declaradas.

### Cambiado

- La herramienta **CSS muerto** pasa a llamarse **Auditoría CSS**.

## [0.5.0] - 2026-11-18

### Agregado

- Opción **Instalar en esta PC** en el menú: compila Workshop, lo instala en `%LOCALAPPDATA%\Programs\Workshop` y lo agrega al PATH para abrirlo con `workshop`. Solo aparece al correr desde el repositorio y también sirve para actualizar.

## [0.4.0] - 2026-11-18

### Agregado

- Herramienta **Conversor Clarion**: convierte fechas, horas y fecha con hora entre el formato Clarion y el legible, en ambos sentidos.

## [0.3.0] - 2026-11-18

### Agregado

- Herramienta **Crear tag**: muestra los commits desde el último tag, propone la versión siguiente (patch, minor o major), pide un mensaje y publica el tag en GitHub. Frena si hay cambios sin commitear, si el commit actual no es el último de la rama principal o si no hay commits nuevos.

### Cambiado

- La versión sale del último tag de git con MinVer, ya no se edita en el `.csproj`.

## [0.2.0] - 2026-11-17

### Agregado

- Banner "Workshop" y panel del proyecto en el encabezado; la pestaña de la terminal muestra el proyecto actual.
- Rama de git y commits sin subir en el panel del proyecto.
- Menú agrupado en "Herramientas" y "Proyecto", con íconos y una descripción por herramienta.
- Rutas de archivo clickeables (`Ctrl + click`) en los resultados de ambas herramientas.
- **CSS muerto** muestra los resultados como árbol carpeta → archivo → clase y una barra de progreso con los archivos leídos.

### Cambiado

- Los colores de la consola salen de una clase `Theme` central.
- `ITool` pide `Icon` y `Description` además de `Name`.

### Corregido

- La consola escribe en UTF-8, así se ven los emojis y los bordes redondeados.

## [0.1.0] - 2026-11-17

### Agregado

- Menú interactivo de consola con proyectos guardados: agregar, cambiar y recordar el último usado.
- Herramientas enchufables: cualquier clase que implemente `ITool` aparece sola en el menú.
- Cancelación con `Ctrl + C` dentro de una herramienta, sin cerrar la app.
- Herramienta **CSS muerto**: clases definidas en los CSS globales que no se usan en el código, con detección de clases armadas por interpolación.
- Herramienta **Código duplicado**: bloques repetidos detectados con jscpd, tabla con los más grandes y reporte HTML.
- La versión se muestra en el encabezado del menú.
- Proyecto de tests con xUnit v3 sobre Microsoft.Testing.Platform.
- README en español e inglés.

[Sin publicar]: https://github.com/nicolasroumec/Workshop/compare/v0.6.0...HEAD
[0.6.0]: https://github.com/nicolasroumec/Workshop/compare/v0.5.0...v0.6.0
[0.5.0]: https://github.com/nicolasroumec/Workshop/compare/v0.4.0...v0.5.0
[0.4.0]: https://github.com/nicolasroumec/Workshop/compare/v0.3.0...v0.4.0
[0.3.0]: https://github.com/nicolasroumec/Workshop/compare/v0.2.0...v0.3.0
[0.2.0]: https://github.com/nicolasroumec/Workshop/compare/v0.1.0...v0.2.0
[0.1.0]: https://github.com/nicolasroumec/Workshop/releases/tag/v0.1.0
