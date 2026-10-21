# Modelo de datos propuesto

Diseño conceptual; no código ni formato final aprobado.

Catálogo detallado en [objects.md](objects.md). Línea, resaltador, marcador numerado, selección múltiple, rotación y grupos confirmados. Imagen insertada sigue sin aprobarse.

| Entidad | Datos | Invariantes |
|---|---|---|
| Documento | ID, versión de esquema, imagen base, dimensiones, área de recorte, objetos ordenados | Dimensiones positivas; imagen original inmutable; orden define superposición |
| Anotación | ID, tipo, geometría, estilo | ID único y estable; números finitos; tipo conocido |
| Transformación | Posición y orientación por objeto; geometría local | Giro persistido en grados normalizados; pivote individual derivado del centro local; pivote de selección temporal, no persistido |
| Texto | Caja, contenido, familia/tamaño de fuente, color, fondo opcional | Fuente positiva; contenido editable |
| Bocadillo | Texto, caja, punta, relleno y borde | Cuerpo y punta se transforman como una anotación |
| Flecha | Inicio, fin, punta, grosor y color | Extremos editables; grosor positivo |
| Rectángulo / elipse | Caja, relleno opcional, borde opcional | Ancho/alto positivos; evitar objeto totalmente invisible |
| Trazo | Puntos, color, grosor | Al menos dos puntos; selección y transformación del conjunto |
| Línea | Inicio, fin, color y grosor | Segmento sin punta; extremos distintos y números finitos |
| Resaltador | Puntos, color, grosor y opacidad | Un objeto por gesto; conservar transparencia al exportar y reabrir |
| Marcador numerado | Centro, diámetro, número, estilo de texto, relleno y borde | Diámetro positivo; entero positivo; ID independiente del número; no renumerar otros objetos al borrar |
| Estilo | Colores RGBA, grosor, relleno/borde habilitados | Canales válidos; propiedades aplicables por tipo |
| Sesión de editor | Selección, herramienta, zoom, desplazamiento, historial | No forma parte de la imagen exportada |
| Selección múltiple | Conjunto de IDs existentes, caja envolvente derivada | Sin IDs repetidos ni obsoletos; no persistir ni exportar; una transformación del conjunto se confirma de forma atómica |
| Grupo | ID propio y miembros ordenados | Persistido; al menos dos miembros; un único grupo por objeto; sin anidamiento; bloque contiguo en orden de dibujo; geometrías y transformaciones se conservan por miembro |
| Preferencias | Versión, idioma es/en, atajo y modo región/ventana/monitor | Validar al cargar; valores iniciales español, Ctrl+Alt+S y región; sin contenido del usuario |
| Opciones de captura | Retardo 0/3/5/10 s, incluir cursor, destino | Persistir retardo y cursor; no persistir automáticamente contenido o títulos del destino |
| Metadatos de color | Origen SDR/HDR, conversión aplicada, espacio de imagen de trabajo | Imagen de trabajo sRGB según contrato F20; no etiquetar SDR como HDR |

## Proyecto editable (confirmado)
Contenedor con manifiesto JSON versionado e imagen PNG base. Esquema detallado por concretar en el diseño técnico. No serializar controles WinForms ni tipos ejecutables. Validar versión desconocida, campos, límites de memoria/descompresión y rutas; rechazar entradas peligrosas sin extracción arbitraria.

El proyecto conserva orden y estilos; historial y selección no se persisten inicialmente. Guardar mediante archivo temporal y sustitución segura, preservando el archivo anterior si falla. Fuente ausente: aviso y sustitución explícita; no prometer idéntica composición entre equipos sin resolver distribución de fuentes.
