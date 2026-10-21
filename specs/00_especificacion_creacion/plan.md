# Plan técnico inicial

Estado: implementación autorizada por el usuario; plan aplicado en 0.1.0. Evidencia y validaciones pendientes en validation.md.

WinForms es una decisión explícita del usuario y sustituye la propuesta anterior de WPF. Se usará MVP: formularios para interacción, presentadores para coordinar y casos de uso para funcionalidad. El resto del plan sigue sujeto a las aclaraciones pendientes.

El editor tendrá un lienzo WinForms dibujado, con doble búfer y renderizador compartido entre pantalla y exportación; las anotaciones no serán controles independientes. Aislar dibujo y medición de texto de las reglas del dominio. Un editor de texto temporal puede superponerse al lienzo, sin convertirse en el modelo persistido. Validar fidelidad de texto, recursos gráficos y escalado DPI durante la prueba técnica.

## Estructura futura propuesta
- `src/ScreenHunter.Domain`: documento, anotaciones, geometría, estilos e invariantes. Sin WinForms/Win32.
- `src/ScreenHunter.Application`: casos de uso, historial, selección, transformación y puertos de integración. Depende solo del dominio.
- `src/ScreenHunter.Infrastructure.Windows`: captura, atajos, portapapeles, archivos y configuración; implementa puertos del núcleo.
- `src/ScreenHunter.Presentation.WinForms`: formularios, presentadores, herramientas, lienzo y composición gráfica; depende de aplicación/dominio. El arranque conecta infraestructura mediante un único punto de composición.
- `tests/`: pruebas de dominio/aplicación e integración Windows separadas.
- `artifacts/`: resultados reproducibles y ZIP; no fuentes.

Estructura creada tras autorización. Sin paquetes NuGet externos ni contenedor DI; composición explícita en Program.cs.

Motor elegido: DXGI Desktop Duplication mediante interfaz COM acotada en infraestructura; salida BGRA SDR o FP16 para HDR, con conversión tonal a sRGB. La prueba de captura sintética recibió E_ACCESSDENIED en DuplicateOutput1 bajo este entorno; no se declara validada en hardware. Ventana se captura por su rectángulo visible del escritorio, sin aislamiento de ventanas superpuestas. Esta limitación se refleja en guía y registro de validación.

Corrección 0.1.1: sustituida la ruta SDR por GDI/BitBlt RGB con comprobación de retorno para solucionar el reporte de captura negra. DXGI queda para HDR y espera un fotograma de escritorio válido (no solo cursor). La prueba GDI también recibe acceso denegado en este entorno; evidencia y límites en validation.md. Esta actualización prevalece sobre la elección inicial del párrafo anterior.

Límites iniciales: 64 megapíxeles, 32 768 píxeles por lado, 2000 objetos, 50 000 puntos por trazo y 500 000 por documento. Historial de hasta 80 interacciones, también limitado por volumen de geometría/texto. Windows mínimo por comprobación de arranque: build 22000, x64; validación local build 26200.

## Estrategia
1. Aclarar requisitos y aprobar la tecnología propuesta.
2. Hacer una prueba técnica acotada del motor de captura, DPI, atajo y publicación ZIP. Registrar límites; si falla, actualizar research y plan antes de extenderla.
3. Construir núcleo de documento, estilos, coordenadas e historial.
4. Integrar selector y edición de objetos. Un único modelo alimenta una proyección visual compartida para editor y rasterización. Controles de selección son otra capa, excluida de exportación.
5. Añadir copiar/exportar y proyecto editable versionado.
6. Completar bandeja, configuración, accesibilidad básica, errores y validación de distribución.

## Coordenadas y consistencia
- Captura: píxeles físicos del escritorio virtual y origen absoluto posiblemente negativo.
- Documento: píxeles de la imagen base, origen local (0,0); geometría en precisión decimal de coma flotante.
- Vista: coordenadas cliente del lienzo WinForms; transformaciones explícitas documento↔vista según zoom, desplazamiento y DPI por monitor.
- No aplicar escala DPI dos veces. Hit testing de los objetos en coordenadas documento; tolerancia de selección expresada en tamaño visual estable.
- El zoom no altera geometría ni exportación. Flechas mantienen extremos; trazos escalan puntos; texto usa caja y fuente ajustables por propiedades. El redimensionado de caja de texto cambia ajuste de líneas, no la fuente automáticamente.
- Captura SDR y HDR obligatorias. Evaluar superficies de captura de alta precisión, conversión tonal HDR→SDR y composición por monitor antes de convertir a la imagen de trabajo sRGB. No pasar HDR directamente a una imagen de 8 bits. El formato y algoritmo concretos requieren prueba técnica y referencia visual conocida.

## UX y errores
Grupos persistidos como pertenencia plana con ID y miembros ordenados; transformaciones aplicadas a geometrías de miembros mediante las operaciones existentes. No crear una segunda implementación de transformaciones para grupos. El coordinador resuelve selección de un miembro a su grupo. Agrupar/desagrupar guarda en historial pertenencia y orden global, no solo geometría. Validación del proyecto impide referencias compartidas, inexistentes y grupos anidados.

Rotación modelada como transformación del objeto, independiente de WinForms. Componer la misma transformación para dibujo, hit testing y exportación, e invertirla al editar geometría local. Giro múltiple actualiza posición y orientación de cada objeto atómicamente sin guardar grupos. Evitar rasterizar anotaciones en operaciones de giro.

Selección como conjunto de IDs en la sesión; puede incluir objetos sueltos o grupos persistentes, sin duplicar miembros. La aplicación calcula caja envolvente y aplica transformaciones del conjunto en una operación atómica con historial único. Panel contextual calcula propiedades comunes/mixtas y compatibilidad por tipo. Reutilizar las mismas transformaciones geométricas para selección individual, múltiple y grupos; no implementar motores de edición duplicados.

Recursos de presentación separados para español e inglés. Los formularios, mensajes propios, ayudas y bandeja se actualizan al aceptar el idioma; los textos del documento permanecen intactos. Verificar expansión de textos, foco y acceso por teclado en ambos idiomas.

Ajustes trabaja con una copia de preferencias: Cancelar no modifica estado. Validar el nuevo atajo antes de liberar el anterior y restaurar la configuración previa si falla. Guardar idioma, atajo y modo predeterminado junto al ejecutable; si falla escritura, ofrecer sesión temporal o cancelar. Botón y atajo utilizan el modo guardado; la elección puntual de otro modo no lo cambia.

Guardar también retardo e inclusión de cursor. Para retardo cero, congelar la imagen antes de mostrar el selector para evitar controles propios. Para retardo positivo, seleccionar primero el destino, esperar sin congelar y capturar al terminar: son recorridos distintos. Estado adicional esperando, cuenta atrás cancelable y recuperación si cambian monitores o desaparece una ventana. Capturar cursor y fotograma de forma coherente; evitar duplicarlo si el motor ya lo incluye.

Importación por adaptador de archivos: validar contenido, dimensiones y decodificación antes de sustituir el documento; PNG/JPEG como formatos iniciales. La carga aplica el mismo flujo de cambios pendientes. Probar archivos corruptos y dimensiones excesivas sin agotar memoria.

Herramientas visibles, panel de propiedades contextual, colores recientes propuestos como mejora posterior. Estilo inicial por herramienta y cambios aplicados solo a selección actual; definir valores por defecto durante revisión UX.

Estados: inactivo → capturando → editando → exportando/guardando → editando. Solo una captura activa. Liberar recursos y restaurar ventanas propias al cancelar o fallar. No capturar diálogos del editor involuntariamente. Portapapeles ocupado: error recuperable y reintento manual. Fallo de escritura: no perder historial ni marcar proyecto guardado.

## Publicación y seguridad de archivos
ZIP autocontenido win-x64 en Release; sin trimming inicial. Verificar en Windows 11 limpio sin SDK/runtime y carpeta con espacios. No requerir escritura junto al EXE para capturar/exportar. Configuración no escribible: aviso y sesión temporal, sin fallback silencioso. No abrir enlaces ni ejecutar contenido de proyectos; validar versiones, tamaños, imágenes y rutas internas antes de cargar.

## Riesgos abiertos
Cambios de topologia de monitores, escalas mixtas, fidelidad texto/exportacion, fuentes ausentes, consumo de memoria e historial. Concretar limites mediante mediciones antes de validar una entrega.
