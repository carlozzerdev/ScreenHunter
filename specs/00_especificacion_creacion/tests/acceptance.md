# Plan de pruebas de aceptación

Estado: diseñado, no ejecutado. Contenido sintético; no capturas de información privada.

| ID | Requisitos | Escenario y resultado esperado | Nivel |
|---|---|---|---|
| A01 | F01,F07,F14 | Botón y atajo desde otra app abren captura; repetir tecla no duplica; Escape conserva documento | Windows/manual |
| A02 | F07,N04 | Región, ventana y monitor; coordenadas negativas y región entre monitores: píxeles y dimensiones correctos; overlay propio ausente | Windows/manual |
| A03 | F02–F05,F08 | Crear cada tipo, seleccionar con solapes, mover, redimensionar, cambiar estilo, duplicar, borrar y ordenar; solo cambia el objeto esperado | Núcleo + UI |
| A04 | F03 | Editar texto multilínea y punta de bocadillo; cambiar fuente/caja y volver a seleccionar sin perder contenido | Núcleo + UI |
| A05 | F04,F05 | Trazo libre como objeto; relleno/borde opcionales en formas, transparencia y grosor visibles; impedir forma invisible accidental | Núcleo + UI |
| A06 | F09 | Undo/redo de cada mutación, un arrastre por paso; nueva edición elimina redo; cancelar interacción no añade pasos | Núcleo |
| A07 | F10,N04 | Zoom 25/100/200 %, recortar y deshacer: tamaño de exportación estable, geometría e hit testing correctos | Núcleo + UI |
| A08 | F06,F11 | PNG/JPEG y pegado en Paint: composición correcta sin selección; copiar persiste tras salir; JPEG aplana en blanco | Integración + visual |
| A09 | F11,F12,F15 | Acceso denegado, disco lleno simulado, sobrescritura cancelada: trabajo intacto; cerrar/nuevo ofrece guardar/descartar/cancelar | Integración + UI |
| A10 | F12 | Guardar/abrir proyecto preserva imagen, objetos, orden y estilos; corrupto, versión desconocida o contenedor malicioso se rechazan sin pérdida | Persistencia |
| A11 | F13,F14 | Bandeja captura/abre/sale; atajo ocupado informa; al salir queda liberado; carpeta sin escritura permite sesión temporal con aviso | Windows/manual |
| A12 | N01 | ZIP en Windows 11 limpio sin .NET/SDK, usuario estándar, ruta con espacios: ejecutar, capturar, editar, copiar y guardar | Equipo limpio |
| A13 | N02 | Revisar dependencias: núcleo sin WinForms/Windows; misma regla geométrica/estilo en vista y exportación; sin duplicación sustancial | Revisión |
| A14 | N03 | Acciones principales por teclado, foco visible, herramientas etiquetadas; Ctrl+C dentro de texto conserva edición convencional | UI/manual |
| A15 | N04 | Escalas 100/125/150/200 %, mezcla de DPI, monitor izquierdo y desconexión durante captura: sin desplazamientos ni bloqueo | Windows/manual |
| A16 | N05 | Sin tráfico de red iniciado por app, sin capturas temporales persistentes ni contenido en logs | Inspección + Windows |
| A17 | Objetivos de rendimiento | Registrar hardware, diez mediciones y mediana/máximo para captura, PNG y arrastre según spec.md | Medición |
| A19 | N06 | Presentación WinForms; revisar proyectos y dependencias para comprobar ausencia de WPF; formularios sin reglas de negocio | Revisión |
| A20 | N07 | Seguir la guía desde descomprimir hasta copiar y guardar; comprobar que cada instrucción coincide con la versión y que la guía está en el ZIP | Manual |
| A21 | N03,F16 | Cambiar español/inglés al aceptar sin reiniciar: formularios, herramientas, ayudas, mensajes y bandeja traducidos, sin truncamientos; anotaciones intactas; idioma persiste al reiniciar | UI + integración |
| A22 | F07,F14,F16 | Cambiar atajo y cada modo predeterminado; botón y nuevo atajo usan ese modo; el anterior queda liberado; reinicio conserva valores; selección puntual no cambia preferencias | Windows/manual |
| A23 | F16 | Cancelar Ajustes no cambia nada; atajo ocupado conserva valores previos; fallo de escritura avisa y permite sesión temporal; preferencias corruptas se recuperan con aviso y valores iniciales | Integración |

| ID | Requisitos | Escenario y resultado esperado | Nivel |
|---|---|---|---|
| A24 | F17 | Abrir PNG/JPEG, anotar y exportar sin cambiar el original; cancelar aviso de cambios o cargar archivo corrupto conserva documento; rechazar tamaño excesivo de forma recuperable | Integración + UI |
| A25 | F18 | Retardo 0/3/5/10 s: con retardo capturar el contenido posterior a la espera, no el de selección; permitir preparar un menú; Escape conserva documento; destino desaparecido informa sin bloqueo | Windows/manual |
| A26 | F19 | Cursor activado/desactivado en región, ventana y monitor; forma, posición, punto activo y DPI correctos, sin duplicados; no añadir cursor a imagen importada | Windows/manual |
| A27 | F20 | Equipo HDR real con HDR activado/desactivado y monitores mixtos: patrón con luces y gradientes, sin recorte por truncamiento ni aspecto lavado; registrar comparación y parámetros; editor, proyecto reabierto, PNG/JPEG y pegado coherentes según conversión SDR | Equipo HDR + visual |
| A28 | F18,F19 | Reiniciar conserva retardo y cursor; Cancelar Ajustes no cambia valores; cambiar idioma conserva ambas opciones y traduce sus etiquetas | Integración + UI |
| A29 | F21 | Crear línea, editar ambos extremos, color y grosor, mover, duplicar y ordenar; undo/redo, proyecto reabierto y exportación conservan geometría y apariencia | Núcleo + UI + persistencia |
| A30 | F22 | Dibujar resaltador sobre texto de referencia: contenido visible; editar color/grosor/opacidad y tamaño; solapes coherentes entre vista y salida; undo/redo y proyecto conservan trayectoria y estilo | Núcleo + UI + visual |
| A31 | F23 | Crear 1,2,3; borrar 2 y crear nuevo obtiene 4; duplicar obtiene 5. Editar número manualmente, permitir repetidos; undo/redo conserva valores, reabrir deriva siguiente de máximo; círculo y número siguen encajados al redimensionar | Núcleo + UI + persistencia |
| A32 | F08 | Shift+clic añade/retira; marco selecciona solo objetos completos; Shift+marco añade; mover desde objeto seleccionado conserva conjunto; clic vacío y Escape quitan selección en reposo | Núcleo + UI |
| A33 | F08,F09 | Mover/redimensionar proporcionalmente selección mixta, duplicar, eliminar y ordenar: proporciones y orden interno conservados, número de marcador duplicado sigue F23; cada operación se deshace/rehace en un paso; cancelar no muta | Núcleo + UI |
| A34 | F08,F12 | Propiedades mixtas muestran Varios/Mixed, aplicar afecta solo compatibles y no cambia contenido; proyecto/exportación no incluyen selección ni marco; editar texto conserva gestos convencionales | UI + persistencia + visual |
| A35 | F24 | Girar cada tipo por manejador y grados, Shift ajusta 15°; texto/puntas giran juntos; selección por clic/marco y redimensionado funcionan tras giro; undo/redo y Escape recuperan estado | Núcleo + UI |
| A36 | F08,F12,F24 | Girar selección mixta 90° y 360°: centro y distancias relativas correctos; orientación/posición individuales persisten y exportan igual; un paso de historial, sin grupo permanente ni deformación acumulada | Núcleo + persistencia + visual |
| A37 | F25 | Agrupar texto/flecha/marcador; deseleccionar y volver a pulsar selecciona grupo; mover/escalar/girar y desagrupar conserva aspecto y objetos; undo/redo recupera pertenencia, geometría y orden exactos | Núcleo + UI |
| A38 | F12,F23,F25 | Guardar/abrir conserva grupo; duplicar crea IDs y números nuevos; borrar/undo recupera miembros; exportación sin marco; rechazar proyecto con pertenencia doble, referencia inexistente o anidamiento | Persistencia + UI |
| A39 | F25 | Agrupar objetos intercalados con otros crea bloque en posición del miembro más adelantado, conserva orden interno/ajeno; undo restaura solapes originales; combinar grupos aplana y undo recupera grupos previos | Núcleo + visual |
| A40 | F01,F07,F20 | El selector mantiene los colores de referencia dentro de la región y atenúa el resto; copia RGB/PNG no introduce transparencia negra. DXGI no usa un fotograma solo de cursor como imagen inicial; timeout/fallo conserva el documento. Repetir captura SDR/HDR real en sesión permitida | Regresión sintética + Windows |

Los IDs F/N corresponden a SH00-F/SH00-N. A18 queda reservado sin caso activo para conservar los identificadores posteriores. Los extras Q09 están confirmados. Para imágenes, usar patrón de píxeles conocido y comparaciones tolerantes a antialiasing de texto; no pruebas que repliquen simplemente la implementación. A27 no puede aprobarse solo con monitor SDR o inspección de código.

Registro por ejecución: fecha, build/commit, entorno, pasos, esperado, observado, estado (aprobado/fallido/no ejecutado), evidencia sintética y defectos. No existe cobertura demostrada todavía.
