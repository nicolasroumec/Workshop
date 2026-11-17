# Changelog

Todos los cambios importantes de Workshop se anotan en este archivo.

El formato sigue [Keep a Changelog](https://keepachangelog.com/es-ES/1.1.0/) y el proyecto usa [versionado semántico](https://semver.org/lang/es/).

## [Sin publicar]

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

[Sin publicar]: https://github.com/nicolasroumec/Workshop/compare/v0.2.0...HEAD
[0.2.0]: https://github.com/nicolasroumec/Workshop/compare/v0.1.0...v0.2.0
[0.1.0]: https://github.com/nicolasroumec/Workshop/releases/tag/v0.1.0
