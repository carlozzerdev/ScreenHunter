# Investigación local

ObjectStyle se clona y serializa con el documento; permite añadir Shadow sin nueva dependencia. SceneRenderer comparte la composición de lienzo y salida. La punta se dibuja por separado y se añade a Bounds, Hit y manejadores: hay que retirarla de los cuatro lugares. Alternativa descartada: sombra como objeto separado, porque complicaría selección e historial.
