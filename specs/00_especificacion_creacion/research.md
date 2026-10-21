# Investigación y elección tecnológica

Fecha: 2026-10-15. Fuentes consultadas en línea; entorno inspeccionado sin instalar ni modificar software.

## Entorno observado
- Carpeta inicialmente vacía, sin flujo de specs existente.
- SDK .NET 10.0.401 instalado.
- Visual Studio Community 2026 18.9.3 instalado y registrado como completo. No se ha verificado la carga de trabajo de escritorio .NET ni los componentes específicos del SDK de Windows.

## Comparación técnica (evaluación para este proyecto)
| Opción | Ventajas | Costes | Evaluación |
|---|---|---|---|
| C# + WinForms + .NET 10 | Controles de escritorio, integración Windows y despliegue autocontenido | Lienzo propio, selección, DPI y render compartido | Elegida por el usuario |
| C# + WinUI 3 | Controles modernos e integración Windows | Dependencias de Windows App SDK y más piezas para este editor portable | Posible, sin beneficio suficiente aquí |
| Python + PySide6 | Prototipado rápido y escena gráfica madura | Empaquetado e integración nativa adicionales; revisar distribución/licencias de Qt | Viable, menor ajuste al entorno y objetivo |
| C++ + Qt/Win32 | Control nativo y rendimiento | Más complejidad de memoria, compilación, UI y mantenimiento | Reservar para necesidad medida |

WinForms es una decisión explícita del usuario. La recomendación inicial de WPF queda sustituida. La comparación es una evaluación de diseño, no un benchmark.

## Propuesta
C# / WinForms / .NET 10 LTS; MVP (Model–View–Presenter), núcleo independiente de UI y adaptadores Windows. Sin WPF ni controles WPF alojados. Publicación autocontenida win-x64 en carpeta ZIP; no exigir EXE único, trimming ni Native AOT. No hace falta escribir C/C++ para consumir APIs Windows desde C#.

Comprobar antes de implementar: carga «Desarrollo de escritorio de .NET», SDK de Windows compatible y restauración de dependencias. No instalar nada en esta fase. Si una prueba demostrara necesario un módulo C++, requeriría carga «Desarrollo para el escritorio con C++», MSVC correspondiente y SDK Windows; CMake solo si lo necesita ese módulo. No se propone instalarlo ahora.

## Captura: decisión pendiente de prueba
Decisión de implementación posterior: DXGI Desktop Duplication con interoperabilidad COM propia, sin dependencias externas. Prueba real de ventana sintética: E_ACCESSDENIED al duplicar salida en este entorno. Se conserva como resultado fallido, no como soporte demostrado. El análisis previo de WGC de los párrafos siguientes queda como alternativa investigada.

Evaluar Windows.Graphics.Capture para ventana/monitor y recorte de región a partir de imagen congelada. Probar interoperabilidad C#, selección programática de elementos, exclusión de ventanas propias, sincronización con el compositor, multimonitor y cancelación. La UI del selector se muestra después de obtener la imagen para no capturarse a sí misma. Una región que cruza monitores necesita composición por coordenadas físicas; ventanas pueden incluir o excluir sombras según el contrato que se concrete.

Si la prueba falla, comparar DXGI Desktop Duplication como alternativa. Elegir un motor validado, sin mantener varios por prevención. HDR es requisito confirmado; contenido protegido no se sortea. La prueba debe incluir captura retardada real y cursor opcional.

## HDR confirmado
Microsoft recomienda considerar `DXGI_FORMAT_R16G16B16A16_FLOAT` en toda la ruta de captura HDR para evitar recorte y aspecto lavado, y procesamiento adicional según la salida. Fuente: [captura de pantalla y HDR](https://learn.microsoft.com/en-us/windows/apps/develop/media-authoring-processing/screen-capture). Para WinForms se propone capturar con precisión suficiente y convertir a SDR sRGB antes del lienzo y de PNG/JPEG/portapapeles. Es una decisión de diseño derivada, pendiente de validar; no implica exportación HDR nativa ni requiere usar WinUI para la presentación. No declarar soporte probado sin equipo HDR y comparación de referencias.

## Fuentes primarias
- [Spec Kit y flujo SDD](https://github.com/github/spec-kit) y [flujo de etapas](https://github.github.com/spec-kit/reference/agentic-sdd.html): referencia para separar especificación, plan, tareas y análisis; no se ha instalado Spec Kit.
- [.NET: política de soporte](https://dotnet.microsoft.com/en-us/platform/support/policy/dotnet-core): .NET 10 es LTS, con soporte hasta noviembre de 2028.
- [Versiones SDK / Visual Studio](https://learn.microsoft.com/en-us/dotnet/core/porting/versioning-sdk-msbuild-vs): comprobar compatibilidad de herramientas.
- [Publicación .NET](https://learn.microsoft.com/en-us/dotnet/core/deploying/): despliegue autocontenido por plataforma/arquitectura.
- [Archivo único](https://learn.microsoft.com/en-us/dotnet/core/deploying/single-file/): opción distinta de autocontenido, con posibles extracciones de bibliotecas nativas.
- [Windows Graphics Capture](https://learn.microsoft.com/en-us/windows/uwp/audio-video-camera/screen-capture): API de captura y restricciones.
- [RegisterHotKey](https://learn.microsoft.com/en-us/windows/win32/api/winuser/nf-winuser-registerhotkey): registro global y reserva de combinaciones con tecla Windows.
