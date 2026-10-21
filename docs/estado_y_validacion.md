# Estado de ScreenHunter 0.1.1

Implementación WinForms en cuatro proyectos: Domain, Application, Infrastructure.Windows y Presentation.WinForms. Núcleo sin WinForms ni Win32. Sin WPF y sin paquetes NuGet externos. Se han creado las nueve herramientas, selección múltiple, grupos, rotación, historial, recorte, importación, proyectos, exportación, portapapeles, preferencias bilingües, bandeja y coordinación de captura.

## Corrección 0.1.1
Ante el reporte de captura negra, SDR pasa a GDI/BitBlt sobre RGB sin alfa indefinido y comprueba el resultado de la API. HDR mantiene DXGI/FP16, descarta actualizaciones solo de cursor y espera con plazo acotado una imagen del escritorio. No se consideran erróneas las imágenes negras legítimas por su color. La prueba de selector sobre imagen sintética conserva colores dentro de la selección y oscurece fuera sin volver negro el fondo. La captura real continúa recibiendo acceso denegado en este entorno, tanto GDI como DXGI: no se declara validada la corrección en el equipo del usuario.

## Evidencia local
- Compilación Release sin advertencias ni errores.
- Pruebas de núcleo: geometría, grupos, historial, orden, numeración, cancelación y validación del modelo.
- Pruebas sintéticas Windows: persistencia, PNG/JPEG, recorte, preferencias, recursos es/en, lienzo y creación por gesto; imágenes de interfaz generadas con DrawToBitmap de la propia ventana.
- Paquete autocontenido ejecutado con DOTNET_ROOT inexistente y búsqueda multinivel desactivada. Traza de host confirma hostfxr/hostpolicy/coreclr cargados desde la carpeta del paquete. Esto no sustituye una prueba en otro equipo limpio.

Resultados detallados y recuentos finales en `specs/00_especificacion_creacion/validation.md` del repositorio; informes JSON en artifacts/ durante el desarrollo. No se incluyen pantallas privadas en la evidencia.

## Pendiente y limitaciones
- La prueba DXGI de la zona interior de una ventana sintética recibió E_ACCESSDENIED al llamar a DuplicateOutput1. No se ha podido verificar captura real bajo estas restricciones; no se solicita elevar privilegios ni se sortea el escritorio seguro. Fuera del entorno de desarrollo debe probarse en sesión interactiva normal.
- Pantallas HDR reales, configuraciones de varios monitores, escalas DPI mixtas, cursor y retardo requieren aceptación de hardware. El algoritmo HDR tiene pruebas numéricas, pero su fidelidad visual todavía no está demostrada. Usa referencia blanca fija de 203 nits.
- Ventana captura el rectángulo visible de escritorio, no contenido aislado detrás de otras ventanas. El destino debe estar visible y sin cubrir. No se incluye captura de ventanas minimizadas.
- No se ha comprobado de extremo a extremo el pegado en otra aplicación después de cerrar ScreenHunter. La implementación publica bitmap/PNG con persistencia de portapapeles de Windows.
- Objetivos de rendimiento de la spec aún no medidos con diez repeticiones sobre el hardware de referencia. No se consideran aprobados.
- Sin grupos anidados ni entrada para editar miembros dentro de un grupo: desagrupar para ello. No hay imagen insertada como objeto, exportación HDR nativa ni recuperación automática tras cierre inesperado.

El paquete es una primera versión para evaluación, no una declaración de que toda la matriz de aceptación esté superada.
