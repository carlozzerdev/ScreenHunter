# Catálogo de objetos del editor

Estado: nueve tipos, selección múltiple, rotación y agrupación confirmados. Los detalles nuevos de interacción son decisiones de diseño revisables. Imagen insertada no aprobada. Este documento desarrolla F02–F05/F08/F21–F25.

## Objetos base confirmados

| ID | Objeto / nombre en inglés | Creación propuesta | Propiedades específicas propuestas |
|---|---|---|---|
| O01 | Texto / Text | Clic para caja inicial o arrastre para definir caja; escribir | Contenido multilínea, fuente, tamaño, negrita, cursiva, subrayado, alineación izquierda/centro/derecha, color de texto, fondo opcional, borde opcional, margen interior |
| O02 | Bocadillo / Callout | Dibujar cuerpo y escribir; punta con manejador propio | Propiedades de texto; cuerpo rectangular redondeado o elíptico; relleno y borde opcionales; punta triangular ajustable, que sigue unida al cuerpo |
| O03 | Flecha / Arrow | Arrastrar del origen al destino | Extremos, color, grosor, tamaño de punta; punta final o en ambos extremos; línea continua o discontinua |
| O04 | Rectángulo / Rectangle | Arrastre entre esquinas opuestas | Ancho, alto, borde y relleno independientes, grosor, color, opacidad, radio de esquinas; Shift al crear mantiene forma cuadrada |
| O05 | Elipse / Ellipse | Arrastre para caja envolvente | Ancho, alto, borde y relleno independientes, grosor, color, opacidad; Shift al crear mantiene forma circular |
| O06 | Trazo libre / Freehand | Arrastrar con botón pulsado; soltar termina objeto | Puntos de trayectoria, color y grosor; redimensionar transforma el conjunto; sin edición individual de nodos inicialmente |

Texto y bocadillo son tipos distintos: quitar fondo a un texto no crea una punta. Círculo y cuadrado son proporciones de elipse/rectángulo, no implementaciones duplicadas. Remarcar es una combinación de estilo: sin relleno con borde visible, o relleno semitransparente con borde opcional. No requiere un tipo geométrico nuevo.

## Propiedades y acciones comunes propuestas
- Identidad estable, posición, orden de superposición y tamaño o geometría apropiada al tipo.
- Seleccionar, mover, redimensionar, duplicar, eliminar, traer delante y enviar detrás.
- Color de borde/trazo y relleno por separado cuando proceda; habilitar/deshabilitar cada uno, no solo simularlo con un color igual al fondo. Evitar confirmar objetos totalmente invisibles.
- Opacidad del objeto; colores de relleno también pueden tener transparencia. El panel muestra el resultado efectivo sin aplicar dos veces el mismo ajuste. Valores de diseño iniciales: objeto 100 %, resaltados 30 %.
- Panel contextual: solo propiedades aplicables. Cambiar el grosor no cambia la fuente; redimensionar caja de texto ajusta líneas sin escalar la fuente.
- Estilo reciente por herramienta durante la sesión. Cambiar propiedades con un objeto seleccionado afecta solo a ese objeto; sin selección, configura el estilo del siguiente objeto de esa herramienta. Persistencia de estos estilos entre sesiones pendiente de decidir.
- Toda modificación confirmada participa en deshacer/rehacer. El proyecto conserva tipo, geometría, contenido y estilo; la exportación conserva su apariencia.

## Gestos de edición propuestos
1. Al terminar la creación, seleccionar el objeto nuevo y volver a la herramienta Seleccionar. Se propone un modo de herramienta fijada para repetir inserciones, pendiente de revisar.
2. Clic selecciona el objeto visible superior bajo el puntero. Los objetos sin relleno se seleccionan por contorno; el interior vacío no bloquea el acceso a objetos inferiores. Los textos se seleccionan por caja, aunque no tengan fondo.
3. Arrastre sobre el objeto lo mueve; manejadores de esquina/lado cambian su caja. Flechas y líneas tienen manejadores de extremos; bocadillos tienen además el de punta.
4. Doble clic en texto o bocadillo edita contenido. Escape cancela la interacción actual y recupera el estado previo. En reposo, Escape quita selección.
5. Supr elimina salvo durante edición de texto, donde funciona como tecla de texto. Flechas de teclado desplazan la selección 1 píxel del documento; Shift+flecha, 10 píxeles. No depender del zoom para esas distancias.
6. Shift al crear rectángulos/elipses fija proporciones; en flechas y líneas restringe dirección a incrementos de 45°. Durante redimensionado mantiene proporción inicial. No afecta a escritura de texto.
7. Un arrastre o una edición de texto confirmada genera un paso de historial, no uno por movimiento o carácter. Cancelar no genera pasos.

Las dimensiones y grosores se expresan en píxeles del documento; tamaño tipográfico visible en puntos con conversión explícita. Los valores exactos por defecto, mínimos y máximos se fijarán durante la revisión de interfaz.

## Objetos adicionales confirmados

| ID | Objeto | Uso y comportamiento propuesto |
|---|---|---|
| P01 | Línea / Line | Segmento sin punta, color/grosor y extremos editables. Comparte geometría con flecha, con identidad de herramienta propia |
| P02 | Resaltador / Highlighter | Trazo ancho semitransparente para subrayar visualmente; un objeto por gesto, opacidad ajustable. Debe poder cambiarse y borrarse como cualquier anotación |
| P03 | Marcador numerado / Numbered marker | Círculo con número para instrucciones por pasos. Número inicial automático, editable; relleno, borde, texto y tamaño. Propuesta: borrar no renumera los restantes, duplicar asigna el siguiente número libre |

P01–P03 conservan sus identificadores históricos aunque ya estén confirmados. La confirmación incluye los tipos de objeto; los valores y reglas adicionales de diseño se concretan a continuación y son revisables.

### Diseño inicial de línea, resaltador y marcador
- Línea: segmento recto sin puntas, color y grosor; arrastrar para crear y editar extremos. Sin relleno. Comparte reglas geométricas con flecha, evitando duplicación.
- Resaltador: trazo libre ancho de color semitransparente, un objeto por gesto; valores iniciales propuestos amarillo y opacidad 30 %. Composición normal por transparencia, no borrado de píxeles. Mover/redimensionar conserva trayectoria relativa; ajustar color, grosor y opacidad. Las zonas de cruce pueden verse más intensas; validar y documentar el resultado de trazos solapados.
- Marcador: clic para crear círculo con número centrado; diámetro, color de texto, relleno y borde ajustables. Redimensionar mantiene círculo y adapta el tamaño del número para encajarlo. Número entero positivo editable, sin renumeración automática de otros objetos.
- Numeración propuesta: nuevo marcador y duplicado reciben uno más que el máximo actual, empezando en 1; si no hay marcadores, empezar en 1. Permitir números repetidos introducidos manualmente. Eliminar no cambia los demás. Deshacer/rehacer restaura números exactos, sin recalcular; al abrir proyecto derivar el siguiente valor de los marcadores guardados. Esta regla sustituye la sugerencia anterior de «siguiente número libre» para evitar reutilizar huecos mientras existan números superiores.

## Objeto opcional no confirmado

| ID | Objeto | Uso y comportamiento propuesto |
|---|---|---|
| P04 | Imagen insertada / Image object | PNG/JPEG colocado como objeto sobre la base, con tamaño, posición y opacidad; conservar proporciones por defecto. Distinto de Abrir imagen, que crea un documento nuevo |

## Selección múltiple confirmada
- Clic sin modificador selecciona un objeto. Shift+clic alterna su pertenencia sin cambiar los demás. Clic en zona vacía deselecciona; arrastrar desde zona vacía crea marco, seleccionando solo los objetos completamente contenidos, independientemente de la dirección del arrastre. Shift+marco añade a la selección actual.
- Solo disponible con Seleccionar, fuera de edición de texto. Marco, selección y manejadores no se guardan en el proyecto ni aparecen al exportar.
- Arrastrar cualquier objeto ya seleccionado mueve todo el conjunto; no reduce la selección al pulsarlo. Flechas de teclado y Shift+flecha desplazan todo el conjunto 1/10 píxeles de documento.
- Caja envolvente común con manejadores de esquina para redimensionado proporcional desde la esquina opuesta; preservar distribución relativa. Escalar geometría, texto (tamaño de fuente), grosores y marcadores por el mismo factor; esto difiere del redimensionado individual de caja de texto, que solo reajusta líneas. Sin deformación no proporcional del conjunto inicialmente.
- Borrar, duplicar y cambiar orden afectan a todos los seleccionados en un paso de historial. Traer delante/enviar detrás conserva el orden relativo entre ellos. Duplicar mantiene posiciones relativas y selecciona las copias; desplazamiento inicial de diseño: 10 píxeles por eje.
- Panel de propiedades muestra valores comunes o «Varios / Mixed». Cambiar una propiedad la aplica solo a objetos compatibles; indicar cuántos recibirán el cambio. No ofrecer edición conjunta del contenido de texto ni de números de marcador. Establecer grosor común asigna el mismo valor a los objetos compatibles, no altera los demás atributos.
- Escape cancela el marco o transformación activa recuperando el estado previo; en reposo quita toda la selección. Un gesto confirmado equivale a una operación de historial.
- Selección múltiple es estado temporal; agrupar crea una unidad persistente. Ambas funciones, junto con rotación, están confirmadas.

## Agrupación confirmada
- Seleccionar al menos dos unidades (objetos o grupos) y pulsar **Agrupar / Group**. Diseño de atajos: Ctrl+G; **Desagrupar / Ungroup**, Ctrl+Shift+G. Disponibles también en barra o menú contextual, fuera de edición de texto.
- El grupo tiene ID propio y miembros con identidad, geometría, contenido y estilos conservados. No es una imagen aplanada. Tras agrupar queda seleccionado; tras desagrupar quedan seleccionados sus miembros.
- Clic sobre la parte visible de cualquier miembro selecciona el grupo. El espacio vacío entre miembros no intercepta clics. Shift+clic alterna el grupo completo; marco requiere contener su extensión visual completa. No seleccionar simultáneamente un grupo y uno de sus miembros.
- Mover, escalar proporcionalmente, rotar, duplicar, borrar y ordenar afectan a todos sus miembros en un paso de historial. Mismas reglas de selección múltiple para escala, propiedades compatibles y pivote central; giro de grupo se introduce como incremento, no como orientación absoluta de todos los miembros.
- Cambiar color/grosor desde el grupo afecta solo a miembros compatibles y muestra Varios/Mixed cuando corresponda. No ofrecer edición conjunta de textos o números. Para editar contenido o geometría particular de un miembro, desagrupar primero; no hay modo de entrar al grupo inicialmente.
- Diseño inicial sin anidamiento. Agrupar grupos y objetos genera un grupo plano nuevo con todos los miembros; undo recupera exactamente los grupos anteriores. Desagrupar no cambia posiciones, tamaños, ángulos ni estilos.
- El grupo ocupa un bloque contiguo en el orden de dibujo. Al agrupar objetos antes intercalados con otros, conservar orden relativo de miembros y colocar el bloque en la posición del miembro seleccionado más adelantado. Esto puede cambiar solapes con objetos ajenos intercalados: agrupar es reversible y la guía debe explicarlo. Los ajenos mantienen su orden relativo. Desagrupar conserva el orden visual actual del bloque.
- Guardar miembros y pertenencia en el proyecto; no guardar selección temporal. Al cargar, rechazar referencias inexistentes, miembros duplicados o compartidos entre grupos y anidamiento. Exportar solo el aspecto de los miembros, sin marco del grupo.
- Duplicar crea IDs nuevos para grupo y miembros; conserva distribución y aplica regla F23 para marcadores: asignar números nuevos en orden de dibujo, incrementando el máximo del documento. Undo/redo restaura los valores asignados sin recalcular.
- Supr sobre grupo elimina grupo y miembros; undo recupera todo. Deshacer agrupar/desagrupar restaura además orden anterior y pertenencia, incluso cuando agrupar cambió solapes.

## Rotación confirmada
- Disponible en los nueve tipos y en selección múltiple. Manejador exterior separado de los de tamaño; el panel permite valor en grados y restablecer orientación individual a 0°.
- Diseño inicial: ángulos positivos en sentido horario; Shift durante arrastre ajusta a pasos de 15°. Valores equivalentes se normalizan a [0°, 360°).
- Individual: centro de la caja local del objeto como pivote. Girar también texto, punta de bocadillo y número del marcador, sin reglas especiales que los mantengan horizontales.
- Múltiple: centro de la caja envolvente de la selección al iniciar el gesto. Girar posiciones y orientaciones por el mismo incremento; no crear un grupo permanente. El campo se denomina «Girar selección» y expresa un incremento por operación, inicialmente 0°, no un ángulo absoluto común.
- Persistir transformación por objeto, nunca la selección o su pivote temporal. Selección por clic usa geometría transformada; marco requiere contener toda la extensión visual rotada. Los manejadores siguen el objeto; redimensionado individual opera sobre sus ejes locales. Para selección múltiple, caja envolvente común en ejes del documento y escala uniforme.
- No mover la imagen base ni cambiar sus dimensiones. Partes fuera del lienzo se recortan al exportar pero se conservan en proyecto. Giro no altera grosor, fuente ni tamaño propio; solo orientación y, en conjunto, posición.
- Un gesto o entrada numérica confirmada es un paso de historial. Escape restaura exactamente el estado anterior; no acumular deformaciones al girar repetidamente mediante rasterización.

## Operaciones que no son objetos
Capturar, importar una imagen como base, recortar el documento, zoom, deshacer, copiar imagen y exportar son operaciones del documento o de la sesión. No aparecen como anotaciones seleccionables.

## Escenarios para revisar este catálogo
- Crear cada objeto base, volver a seleccionarlo, cambiar todas sus propiedades aplicables, guardar proyecto y abrir: identidad y apariencia conservadas.
- Remarcar el mismo contenido con un rectángulo sin relleno y con una elipse semitransparente, sin modificar la captura original.
- Seleccionar por contorno una forma sin relleno y acceder a un texto situado en su interior sin que la forma lo intercepte.
- Editar texto sin que Supr o flechas ejecuten comandos de objetos; cancelar una modificación recupera el estado anterior.
- Exportar con varios objetos superpuestos: orden, colores y grosor coinciden con la vista, sin manejadores.

Estos escenarios son diseño de aceptación, no resultados de pruebas. Una vez confirmadas las decisiones, integrar requisitos y casos identificados en spec.md y tests/acceptance.md.
