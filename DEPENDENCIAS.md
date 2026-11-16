# Inventario de dependencias de SGLN

El proyecto apunta a .NET Framework 4.8 y referencia cuatro ensamblados externos
almacenados directamente en `lib/`:

| Archivo | Uso |
|---|---|
| `MySql.Data.dll` | Conector ADO.NET para MySQL |
| `System.Buffers.dll` | Soporte de buffers |
| `System.Memory.dll` | Tipos de memoria y spans |
| `System.Numerics.Vectors.dll` | Tipos vectoriales |

El proyecto no declara actualmente un `packages.config` ni referencias
`PackageReference`; por eso las versiones deben obtenerse de los metadatos
reales de cada DLL y no inferirse del nombre del archivo.

El workflow de Windows ejecuta `Pruebas/Inventario-Dependencias.ps1`, que
registra para cada ensamblado:

- AssemblyVersion, FileVersion y ProductVersion.
- Empresa declarada en los metadatos.
- Tamaño y SHA-256 del archivo.

Estos datos permiten identificar exactamente los binarios que se compilan.
El inventario no sustituye el análisis de vulnerabilidades: después de conocer
las versiones efectivas hay que contrastarlas con los avisos de seguridad del
proveedor y planificar actualizaciones compatibles.

## Riesgo pendiente antes de producción

El inventario de CI identificó actualmente `MySql.Data.dll` **8.0.30.0**
(FileVersion 8.0.30, publicado el 26/07/2022). No debe considerarse una
dependencia actual para una instalación nueva de producción.

Oracle publicó Connector/NET 26.10.0 el 21/10/2026 y declara compatibilidad
con .NET Framework 4.8. La actualización es técnicamente viable para el marco
objetivo, pero el paquete actual incorpora dependencias adicionales
(BouncyCastle.Cryptography, Google.Protobuf, K4os.Compression.LZ4.Streams,
System.Configuration.ConfigurationManager y ZstdSharp.Port, entre otras).
No se debe reemplazar solo `MySql.Data.dll`: hay que incorporar el conjunto
completo de dependencias y sus redirecciones de ensamblado, compilar Release y
ejecutar la batería funcional contra `sgln_sv_pruebas` antes de publicar.

La familia MySQL 8.0 pasó a soporte sustentador el 21/04/2026; Oracle recomienda
migrar a una versión LTS vigente. Por ello, la actualización del conector y la
validación con la versión de servidor de producción quedan como requisito
pendiente de cierre de la puesta en producción.

Las referencias a `System.*` proporcionadas por .NET Framework no se incluyen
en esta lista porque forman parte de la plataforma de destino.
