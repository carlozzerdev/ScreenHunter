# 00 — Creación de ScreenHunter

Estado: implementación autorizada por el usuario. Versión 0.1.0 en validación; consultar validation.md para pruebas y limitaciones, especialmente captura, HDR y multimonitor.

## Objetivo
ScreenHunter: aplicación portable para Windows 11 x64 que permita capturar, anotar mediante objetos editables, copiar al portapapeles y guardar imágenes con pocos pasos. ARM64 fuera de la primera versión.

## Requisitos confirmados por la petición
- **SH00-F01**: iniciar captura mediante botón y combinación global configurable mientras la aplicación está ejecutándose, aunque no tenga el foco.
- **SH00-F02**: seleccionar, mover y redimensionar las anotaciones sin alterar permanentemente la imagen base durante la edición.
- **SH00-F03**: insertar texto simple y texto con bocadillo; editar contenido, posición, tamaño y colores. El bocadillo incluye cuerpo y punta ajustable como un objeto compuesto.
- **SH00-F04**: insertar flechas, círculos/elipses, cuadrados/rectángulos y trazos libres. Cada trazo terminado constituye un objeto seleccionable.
- **SH00-F05**: cambiar borde, grosor, relleno, color y transparencia cuando sean aplicables. Rectángulos y elipses permiten destacar zonas con relleno, contorno o ambos. Texto permite color y fondo; flechas y trazos tienen color y grosor, sin relleno artificial.
- **SH00-F06**: copiar al portapapeles y guardar el resultado compuesto mediante acciones visibles y sencillas. Los controles y manejadores de selección no aparecen en la salida.
- **SH00-N01**: funcionar en Windows 11 x64; distribución portable en ZIP sin runtime externo ni administrador para el uso normal.
- **SH00-N02**: arquitectura limpia, presentación separada de funcionalidad y respeto a DRY.
- **SH00-N06**: presentación en WinForms, sin WPF, por decisión del usuario.
- **SH00-N07**: documentación en español con instrucciones de uso, incluida en el ZIP y ajustada a las funciones disponibles. Mientras no exista aplicación, identificarla como guía prevista.

## Alcance inicial confirmado
- **SH00-F07**: captura de región rectangular, monitor completo y ventana. Botón y atajo inician el modo configurado en Ajustes; región es el valor inicial. Se permite elegir otro modo para una captura puntual sin alterar el predeterminado. Escape cancela y conserva el documento previo.
- **SH00-F08**: selección individual y múltiple. Clic selecciona uno; Shift+clic añade o retira un objeto; arrastrar desde zona vacía con Seleccionar crea un marco que selecciona objetos completamente contenidos. Clic vacío deselecciona. Mover, redimensionar proporcionalmente, rotar, duplicar, eliminar y ordenar el conjunto como una sola operación de historial. Ordenar conserva el orden relativo interno; duplicar deja seleccionadas las copias. Los grupos se seleccionan como unidades según F25. Ver objects.md para propiedades comunes y selección por marco.
- **SH00-F09**: deshacer/rehacer inserciones, borrados, texto, estilo, movimientos y cambios de tamaño. Un arrastre equivale a una operación. Nueva edición tras deshacer invalida la rama de rehacer.
- **SH00-F10**: zoom y ajuste a ventana sin modificar los píxeles exportados; recorte del documento reversible que recorta visualmente objetos sin borrarlos.
- **SH00-F11**: exportación PNG y JPEG; PNG predeterminado, JPEG con calidad configurable y fondo blanco al aplanar transparencias. Confirmar antes de sobrescribir. Errores conservan el trabajo.
- **SH00-F12**: guardar y abrir proyecto editable versionado con imagen base y anotaciones. Exportar una imagen no sustituye guardar proyecto.
- **SH00-F13**: bandeja de sistema, menú capturar/abrir/salir y preferencias locales. Cerrar la ventana minimiza a bandeja; salir es explícito y advierte de cambios sin guardar. No iniciar con Windows automáticamente.
- **SH00-F14**: atajo inicial Ctrl+Alt+S, configurable en Ajustes. Informar si está ocupado, conservar acceso por botón y no apropiarse de atajos del sistema. No registrar repeticiones de tecla como nuevas capturas. Si falla un cambio de atajo, conservar el anterior.
- **SH00-F15**: nuevo documento o salida con cambios pendientes ofrece guardar proyecto, descartar o cancelar. Copiar/exportar no elimina la condición de proyecto sin guardar.
- **SH00-N03**: interfaz en español e inglés, elegible en Ajustes / Settings. Traducir menús, herramientas, propiedades, ayudas y mensajes propios. Los diálogos nativos pueden seguir el idioma de Windows. Aplicar el idioma al aceptar, sin reiniciar ni modificar los textos de las anotaciones. Idioma inicial español. Etiquetas comprensibles y acceso por teclado a acciones principales. Atajos locales: Ctrl+Z/Y, Supr, Ctrl+C para copiar imagen fuera de edición de texto, Ctrl+S para guardar proyecto.
- **SH00-N04**: monitores múltiples, coordenadas negativas y escalas 100/125/150/200 %. Selección regional puede atravesar monitores. Exportar al tamaño real en píxeles del área capturada.
- **SH00-N05**: procesamiento local, sin red ni guardado silencioso de capturas.
- **SH00-F16**: Ajustes / Settings accesible desde ventana y bandeja. Permite elegir idioma español/inglés, atajo global y modo de captura predeterminado región/ventana/monitor. Aceptar valida y aplica; Cancelar descarta cambios. Persistir junto al ejecutable y restaurar al abrir. Si no se puede escribir, avisar y ofrecer aplicar solo a la sesión. Un atajo inválido u ocupado no altera la configuración previa.
- **SH00-F17**: abrir imágenes PNG/JPEG existentes mediante una acción visible. Crear un documento con esa imagen como base y sin anotaciones; aplicar los mismos avisos de cambios pendientes que al sustituir una captura. Una carga fallida conserva el documento anterior. No sobrescribir el archivo de origen automáticamente.
- **SH00-F18**: captura retardada configurable en Ajustes, con valores iniciales de diseño 0, 3, 5 y 10 segundos; 0 es el predeterminado. Tras elegir región o destino se muestra cuenta atrás cancelable y al finalizar se obtiene la imagen, no antes. Durante la espera se pueden preparar menús o ventanas. Ocultar controles propios antes de obtener el fotograma. Escape cancela y conserva el documento previo; no aceptar capturas concurrentes. Si el destino desaparece, informar sin perder trabajo.
- **SH00-F19**: incluir o excluir el cursor mediante Ajustes; desactivado inicialmente. Si está activado, componerlo una sola vez, en posición y forma del instante capturado, respetando punto activo, DPI y límites de región. No añadirlo a imágenes importadas.
- **SH00-F20**: capturar escritorios y ventanas con HDR activado, incluidos monitores mixtos HDR/SDR, evitando sobreexposición y colores lavados por conversión incorrecta. Diseño inicial: convertir mediante mapeo tonal a imagen SDR sRGB para editor, proyecto, PNG/JPEG y portapapeles, con resultado coherente entre ellos. Esta salida compatible no preserva rango dinámico HDR; exportación HDR nativa y conservación de originales HDR quedan como ampliación por revisar, sin considerarlas aprobadas por esta decisión de diseño. Si el dispositivo no permite capturar correctamente, comunicar el fallo sin sustituir el documento.

- **SH00-F21**: crear líneas rectas como objetos seleccionables, con extremos, posición, color y grosor editables. Sin punta. Permitir duplicar, eliminar, ordenar y deshacer/rehacer; conservar en proyecto y composición de salida.
- **SH00-F22**: crear resaltados mediante trazos anchos semitransparentes, un objeto seleccionable por gesto. Cambiar color, grosor, opacidad, posición y tamaño; conservar trayectoria, historial, proyecto y apariencia exportada. Valores de diseño iniciales amarillo y 30 % de opacidad.
- **SH00-F23**: crear marcadores numerados circulares con número editable y ajustes de tamaño, color de texto, relleno y borde. Permitir mover, duplicar, eliminar y ordenar, con historial y persistencia. Regla de numeración inicial propuesta: máximo existente + 1, empezando en 1; borrar no renumera, números manuales repetidos permitidos y redo conserva el valor original. Ver objects.md para detalles revisables.

- **SH00-F24**: rotar cualquier objeto y una selección múltiple mediante manejador visible y valor numérico en grados. Giro individual alrededor de su centro; giro conjunto alrededor del centro de la caja envolvente común, conservando distribución relativa y orientación de cada objeto respecto al conjunto. Guardar el resultado en proyecto y respetarlo al exportar. Un gesto genera un paso de historial; Escape cancela. Detalles en objects.md.

- **SH00-F25**: agrupar dos o más objetos seleccionados y desagrupar mediante acciones visibles. El grupo se selecciona como una unidad tras deseleccionar y al reabrir proyecto; permite mover, redimensionar proporcionalmente, rotar, duplicar, eliminar y ordenar el conjunto. Agrupar/desagrupar son operaciones reversibles que mantienen objetos editables, sin rasterizar ni alterar la imagen base. Diseño inicial sin grupos anidados: agrupar grupos combina sus miembros; para editar un miembro individual, desagrupar. Ver objects.md para orden de superposición y persistencia.

## Escenarios de aceptación
El detalle de objetos se está definiendo en [objects.md](objects.md). Las ampliaciones marcadas allí como propuestas no se consideran confirmadas ni sustituyen F08 hasta respuesta explícita.

1. Con otra aplicación activa y región como modo predeterminado, pulsar el atajo y seleccionar región: el editor muestra los píxeles elegidos; Escape no altera el trabajo anterior.
2. Añadir texto con bocadillo, flecha y rectángulo; cambiar selección, tamaño y colores: cada elemento mantiene su identidad y el resto no cambia.
3. Dibujar un trazo, seleccionarlo, moverlo y cambiar grosor; deshacer y rehacer recuperan los estados completos.
4. Copiar y pegar en otra aplicación y exportar PNG: misma composición, sin herramientas superpuestas.
5. Descomprimir en otro Windows 11 sin SDK/runtime: iniciar, capturar, editar y guardar sin instalar nada.
6. Cambiar idioma, atajo y modo predeterminado en Ajustes; aceptar, capturar y reiniciar: los tres valores se conservan y los textos de anotaciones permanecen intactos.

## Fuera de alcance inicial
Vídeo, OCR, nube, cuentas, plugins, reconocimiento automático, filtros fotográficos, edición colaborativa, instalación y actualización automática. Capturas protegidas y escritorio UAC no se sortean. El soporte de captura HDR forma parte de la primera versión; su contrato de salida inicial se describe en F20.

## Objetivos medibles propuestos
Referencia a registrar antes de medir: Windows 11 x64, 16 GB RAM, SSD, CPU/GPU identificadas, imagen SDR 3840×2160 y 100 objetos. Objetivos: editor visible en ≤2 s tras finalizar selección; copiar/exportar PNG en ≤2 s; arrastre sin pausas perceptibles con objetivo de ≥30 actualizaciones/s. No son resultados ya obtenidos. Medir diez ejecuciones y registrar mediana y máximo.
