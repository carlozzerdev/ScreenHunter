# ScreenHunter 0.1.2 — Guía de uso

Aplicación WinForms implementada para Windows 11 x64. El editor se ha comprobado con imágenes sintéticas. La validación de captura real, HDR y varias configuraciones de monitores sigue pendiente: consulta `estado_y_validacion.md` antes de dar esas funciones por verificadas.

La versión 0.1.1 corrige la ruta de captura: SDR usa una copia GDI RGB opaca y HDR espera un fotograma real de escritorio. Si Windows rechaza la operación, se muestra un error conservando el documento; no se abre un selector sobre un bitmap creado tras un error de API. Cierra la versión anterior y abre el ejecutable de la carpeta 0.1.2, conservando tus proyectos.

## Preparar y abrir la aplicación
Para probar el editor sin capturar, pulsa **Abrir proyecto** y abre `examples/sample.shproj` incluido en el ZIP. Es una imagen sintética con los nueve tipos de objeto y un grupo de ejemplo.

1. Descarga el ZIP correspondiente a tu equipo. La primera distribución es para Windows 11 x64.
2. Extrae todo el contenido en una carpeta propia; no ejecutes la aplicación desde dentro del ZIP.
3. Abre `ScreenHunter.exe`. Conserva las bibliotecas, carpetas de idiomas y archivos que lo acompañan.

El ZIP incluye .NET: no necesitas instalar Python, Visual Studio ni .NET. El uso normal está diseñado para usuario estándar.

## Capturar la pantalla
1. Abre ScreenHunter y prepara la ventana que quieras capturar.
2. Pulsa el botón **Capturar**, o utiliza el atajo global configurado. El valor inicial es **Ctrl+Alt+S**, configurable en Ajustes / Settings.
3. Se inicia el modo predeterminado elegido en Ajustes: **Región**, **Monitor** o **Ventana**. Puedes elegir otro modo para una captura puntual sin cambiar tus preferencias.
4. Para una región, arrastra desde una esquina hasta la opuesta y suelta el ratón. Para monitor o ventana, elige el destino.
5. La captura aparecerá en el editor. **Esc** cancelará la selección y conservará el trabajo anterior.

El atajo global necesita que ScreenHunter esté ejecutándose. Si otra aplicación utiliza la misma combinación, se mostrará un aviso: cambia el atajo en preferencias o utiliza el botón.

El botón dividido Capturar tiene una flecha desplegable para elegir un modo puntual. **Ventana** captura su rectángulo visible en pantalla: otras ventanas que la cubran pueden aparecer. Coloca el destino delante antes de capturar. No captura ventanas minimizadas ni el escritorio seguro. Un rechazo de permisos de Windows conserva el documento anterior y muestra un aviso.

## Elegir idioma, atajo y modo de captura
1. Abre **Ajustes / Settings** desde la ventana principal o el menú de la bandeja.
2. En **Idioma / Language**, elige **Español** o **English**. El valor inicial es español.
3. En **Atajo de captura / Capture shortcut**, introduce la combinación que prefieras. El valor inicial es **Ctrl+Alt+S**.
4. En **Modo de captura predeterminado / Default capture mode**, selecciona **Región / Region**, **Ventana / Window** o **Monitor / Monitor**. Inicialmente será región.
5. Pulsa **Aceptar / OK** para aplicar y guardar. **Cancelar / Cancel** descarta los cambios.

El idioma cambia sin reiniciar y no traduce los textos añadidos a la imagen. Botón y atajo utilizan el modo predeterminado. Las preferencias se conservan al volver a abrir ScreenHunter. Si el nuevo atajo está ocupado, se conserva la configuración anterior. Si no se pueden guardar las preferencias, se avisará y podrás aplicarlas solo durante la sesión. Los diálogos propios siguen el idioma elegido; algunos diálogos de Windows pueden seguir el idioma del sistema.

## Abrir una imagen existente
Utiliza **Abrir imagen / Open image** para elegir un PNG o JPEG. La imagen se abrirá como base de un nuevo documento y podrás utilizar las mismas herramientas de anotación. No se modificará automáticamente el archivo original. Si tienes trabajo pendiente, podrás guardarlo, descartarlo o cancelar la apertura.

## Capturar con retardo o incluir el cursor
En **Ajustes / Settings** elige **Retardo / Delay** (0, 3, 5 o 10 segundos) e **Incluir cursor / Include cursor**. Inicialmente no hay retardo y el cursor está desactivado.

Con retardo, elige primero región o destino, prepara el contenido durante la cuenta atrás y espera a que termine. La imagen se obtiene al finalizar la espera. **Esc** cancela. Si activas cursor, se incluirá el del instante capturado; la opción no añade cursor al abrir una imagen existente. Ambas preferencias se conservan entre sesiones.

## Capturar con HDR activado
La ruta HDR implementada recibe superficies FP16 y convierte a SDR sRGB para editar, copiar y guardar. No conserva el rango dinámico HDR original ni exporta HDR nativo. El mapeo tonal usa blanco SDR de referencia de 203 nits; su fidelidad con los distintos ajustes de brillo de Windows todavía requiere comprobación en equipo HDR. No se declara HDR validado.

## Añadir textos y bocadillos
1. Elige **Texto** o **Bocadillo**.
2. Marca la zona donde quieras colocarlo y escribe el contenido.
3. En sus propiedades, ajusta fuente, tamaño, color y fondo. En un bocadillo podrás ajustar también el borde.
4. Para cambiar el contenido posteriormente, selecciona el objeto y utiliza su acción de edición de texto.

Un texto simple puede tener fondo o no tenerlo. El cuerpo y el texto del bocadillo se mueven como un solo objeto, sin punta triangular.

## Añadir flechas, formas y trazos
- **Flecha**: arrastra desde el inicio hasta el destino que quieras señalar; ajusta extremos, color y grosor.
- **Rectángulo o elipse**: arrastra para definir la zona. Mantén **Shift** para crear un cuadrado o círculo.
- **Trazo libre**: mantén pulsado el botón del ratón mientras dibujas; al soltar, el trazo quedará como un objeto editable.

Para remarcar una zona sin taparla, utiliza un rectángulo o una elipse sin relleno y con borde de color. Para resaltarla con color, utiliza relleno semitransparente. Flechas y trazos permiten cambiar color y grosor.

## Línea, resaltador y marcador numerado
- **Línea / Line**: arrastra entre dos puntos. Selecciónala después para cambiar extremos, posición, grosor y color.
- **Resaltador / Highlighter**: arrastra sobre la zona que quieras destacar. Cada gesto crea un objeto; podrás cambiar color, grosor y transparencia sin alterar la imagen base. Se propone amarillo al 30 % de opacidad inicial.
- **Marcador numerado / Numbered marker**: haz clic para añadir un círculo con número. Podrás cambiar número, tamaño, color de texto, relleno y borde. La regla inicial propuesta asigna el máximo existente más uno; borrar un marcador no renumera los demás.

Los tres se pueden mover, duplicar y eliminar, y se conservan como objetos al guardar un proyecto editable.

## Seleccionar y modificar elementos
Para seleccionar varios, mantén **Shift** y pulsa sobre los objetos; repetir Shift+clic retira uno del conjunto. También podrás arrastrar un marco desde una zona vacía con la herramienta Seleccionar: incluirá los objetos completamente contenidos. Un clic vacío quita la selección.

La selección múltiple permitirá mover, redimensionar proporcionalmente, duplicar, eliminar y ordenar el conjunto. Los cambios de propiedades se aplicarán solo a los tipos compatibles; la edición de contenido de texto seguirá siendo individual. No crea un grupo permanente.

1. Activa **Seleccionar** y pulsa sobre el elemento.
2. Arrástralo para moverlo o utiliza los manejadores para cambiar su tamaño.
3. Ajusta las propiedades disponibles: color, grosor, borde, relleno, transparencia o tamaño de texto, según el tipo de objeto.
4. Si varios objetos se superponen, utiliza las acciones previstas **Traer delante** y **Enviar detrás** para ajustar su orden.

También estarán disponibles **Duplicar** y **Eliminar**. Un clic en una zona vacía quitará la selección. Cambiar el tamaño de una caja de texto ajustará sus líneas; el tamaño de la fuente se cambiará por separado.

## Rotar objetos
Selecciona uno o varios objetos y arrastra el manejador de giro. También podrás introducir grados en el panel; mantén **Shift** para ajustar el arrastre a pasos de 15° (gesto previsto).

Un objeto gira sobre su centro. Varios giran juntos alrededor del centro del conjunto y conservan su distribución. El campo **Girar selección / Rotate selection** aplica un incremento a todos. Texto y números giran con sus objetos. Podrás deshacer el giro o cancelarlo con Escape durante el gesto.

Seleccionar varios es temporal. Agrupar los mantiene unidos incluso después de deseleccionar y guardar el proyecto.

## Agrupar y desagrupar
1. Selecciona dos o más objetos y pulsa **Agrupar / Group** (atajo previsto **Ctrl+G**).
2. Después bastará pulsar sobre cualquier miembro para seleccionar el grupo. Podrás moverlo, redimensionarlo proporcionalmente, rotarlo o duplicarlo como una unidad.
3. Para editar por separado un texto o una forma, pulsa **Desagrupar / Ungroup** (atajo previsto **Ctrl+Shift+G**). Los objetos conservarán su aspecto y posición.

Los grupos se guardarán en el proyecto editable. Agrupar o desagrupar se podrá deshacer. El diseño inicial combina los grupos seleccionados en un único grupo plano, sin grupos dentro de otros.

Un grupo ocupa una posición conjunta delante/detrás. Si agrupas objetos que tenían otros objetos intercalados entre ellos, pueden cambiar esos solapes; podrás deshacer para recuperar el orden anterior.

## Corregir y encuadrar
- **Deshacer / Rehacer**: recuperar estados anteriores, incluido movimiento, tamaño, texto y estilo.
- **Zoom**: Ctrl+rueda para acercar o alejar; **Ajustar** adapta la imagen al espacio disponible y **100 %** muestra un píxel de documento por píxel del lienzo. No cambia la resolución de salida.
- **Recortar**: arrastra un rectángulo para limitar el área visible y exportada. Deshacer recupera el encuadre anterior; **Restaurar recorte** vuelve a la imagen completa conservando objetos.

## Copiar la imagen al portapapeles
1. Pulsa **Copiar imagen**.
2. Abre la aplicación de destino, por ejemplo Paint o un documento que admita imágenes.
3. Pega con **Ctrl+V**.

La copia incluirá la captura y las anotaciones, sin manejadores de selección ni controles del editor. Si el portapapeles está ocupado, conserva el trabajo y vuelve a intentarlo. Se propone Ctrl+C para copiar la imagen cuando no estés editando texto; dentro del texto mantendrá su comportamiento habitual.

## Guardar una imagen
1. Pulsa **Exportar imagen**.
2. Elige la carpeta y el nombre del archivo.
3. Selecciona **PNG** para conservar nitidez en textos y capturas, o **JPEG** para una salida con compresión y calidad ajustable.
4. Confirma el guardado. Si el archivo ya existe, se pedirá confirmación antes de sustituirlo.

La imagen exportada tendrá las anotaciones integradas en sus píxeles. Para JPEG se propone aplanar cualquier transparencia sobre blanco.

## Guardar para seguir editando
**Guardar proyecto** conservará la imagen base y los objetos separados; **Abrir proyecto** permitirá seguir modificándolos. Guardar un PNG/JPEG no conservará los objetos editables. Copiar o exportar no equivaldrá a guardar el proyecto.

Antes de sustituir un documento con cambios o salir, el aviso pregunta si quieres guardar: **Sí** guarda proyecto, **No** descarta los cambios y **Cancelar** conserva el documento abierto. La aplicación usa diálogos nativos, cuyos botones pueden seguir el idioma de Windows.

## Dejar preparada la captura y salir
Cerrar la ventana deja ScreenHunter en la bandeja del sistema para mantener activo el atajo. Para finalizar completamente, utiliza **Salir** desde su menú. El inicio automático con Windows no está previsto inicialmente.

## Configuración y problemas frecuentes
- **No funciona el atajo**: comprueba que la aplicación esté abierta y que la combinación no esté ocupada. Utiliza el botón Capturar para continuar.
- **No se guarda un archivo**: elige una carpeta con permisos de escritura y comprueba el espacio disponible. El documento debe permanecer abierto tras el error.
- **No se conservan preferencias**: la propuesta es guardarlas junto a la aplicación; si la carpeta no permite escritura, se avisará y podrás continuar con ajustes temporales.
- **Captura no disponible**: el contenido protegido y el escritorio seguro pueden impedir la captura.
- **PNG/JPEG ya no permite seleccionar objetos**: es una imagen aplanada; conserva el proyecto editable para seguir modificando los objetos.

Las capturas y anotaciones se procesarán localmente. No se prevén cuentas, nube ni subida automática de imágenes.

## Atajos del editor

| Atajo | Acción |
|---|---|
| Ctrl+O | Abrir imagen |
| Ctrl+S / Ctrl+Shift+S | Guardar proyecto / Guardar como |
| Ctrl+C | Copiar imagen, fuera de edición de texto |
| Ctrl+Z / Ctrl+Y | Deshacer / Rehacer |
| Ctrl+A | Seleccionar todos los objetos |
| Ctrl+D / Supr | Duplicar / Eliminar |
| Ctrl+G / Ctrl+Shift+G | Agrupar / Desagrupar |
| Flechas / Shift+flechas | Mover selección 1 / 10 píxeles |
| Shift al girar | Ajustar giro a pasos de 15° |
| Escape | Cancelar interacción; en reposo quitar selección |

Los atajos de objetos se usan con el foco en el lienzo. Dentro de un campo de texto, las teclas conservan su función de edición. Los estilos recientes se conservan por herramienta durante la sesión; no se guardan entre ejecuciones.

## Límites de esta versión
Hasta 64 megapíxeles, 32 768 píxeles por lado y 2000 objetos. Hasta 50 000 puntos por trazo y 500 000 por documento. Historial de hasta 80 operaciones, reducido antes si la geometría/texto acumulados superan el límite de memoria previsto. Proyectos `.shproj` con esquema 1, sin grupos anidados. Imagen insertada como objeto no incluida; Abrir imagen sustituye el documento tras confirmar cambios.


## Cambios 0.1.2
El bocadillo muestra cuerpo y texto sin punta triangular, también en proyectos anteriores. En Propiedades, Sombra activa o desactiva una sombra negra desplazada en cualquier objeto o selección múltiple. Se conserva en proyectos, duplicación, deshacer/rehacer y salidas. Está desactivada inicialmente.
