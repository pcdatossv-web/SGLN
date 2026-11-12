/* ============================================================
   SGLN El Salvador - Esquema relacional v2.0 (MySQL 8.0+)
   Motor: MySQL / MariaDB (cliente-servidor)

   Instalación (una sola vez, con un usuario administrador):
     mysql -u root -p < Instalacion_Servidor_MySQL.sql
     mysql -u root -p sgln_sv < Esquema_MySQL.sql

   El usuario de la aplicación (sgln_app) solo tiene SELECT, INSERT,
   UPDATE y DELETE: no puede alterar el esquema ni borrar tablas.
   ============================================================ */

SET NAMES utf8mb4;

/* ------------------------------------------------------------------
   1. Seguridad: roles y permisos
   ------------------------------------------------------------------ */

CREATE TABLE IF NOT EXISTS Roles (
    ID          INT NOT NULL AUTO_INCREMENT,
    Nombre      VARCHAR(50) NOT NULL,
    Descripcion VARCHAR(200) NULL,
    Activo      TINYINT(1) NOT NULL DEFAULT 1,
    -- Un rol protegido no se puede borrar, desactivar ni renombrar.
    Protegido   TINYINT(1) NOT NULL DEFAULT 0,
    PRIMARY KEY (ID),
    UNIQUE KEY UQ_Roles_Nombre (Nombre)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;

CREATE TABLE IF NOT EXISTS Permisos (
    Codigo      VARCHAR(60) NOT NULL,
    Modulo      VARCHAR(40) NOT NULL,
    Descripcion VARCHAR(150) NOT NULL,
    PRIMARY KEY (Codigo)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;

CREATE TABLE IF NOT EXISTS Rol_Permiso (
    ID_Rol INT NOT NULL,
    Codigo VARCHAR(60) NOT NULL,
    PRIMARY KEY (ID_Rol, Codigo),
    KEY IX_Rol_Permiso_Codigo (Codigo),
    CONSTRAINT FK_RolPermiso_Rol FOREIGN KEY (ID_Rol) REFERENCES Roles(ID) ON DELETE CASCADE,
    CONSTRAINT FK_RolPermiso_Permiso FOREIGN KEY (Codigo) REFERENCES Permisos(Codigo) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;

/* ------------------------------------------------------------------
   2. Usuarios y configuración
   ------------------------------------------------------------------ */

CREATE TABLE IF NOT EXISTS Usuarios (
    ID              INT NOT NULL AUTO_INCREMENT,
    Usuario         VARCHAR(50) NOT NULL,
    Nombre_Completo VARCHAR(150) NOT NULL,
    Clave_Hash      VARCHAR(200) NOT NULL,
    Clave_Salt      VARCHAR(100) NOT NULL,
    ID_Rol          INT NULL,
    Activo          TINYINT(1) NOT NULL DEFAULT 1,
    Fecha_Creacion  DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    PRIMARY KEY (ID),
    UNIQUE KEY UQ_Usuarios_Usuario (Usuario),
    KEY IX_Usuarios_Rol (ID_Rol),
    CONSTRAINT FK_Usuarios_Rol FOREIGN KEY (ID_Rol) REFERENCES Roles(ID)
 ) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;

CREATE TABLE IF NOT EXISTS Intentos_Acceso (
    Usuario_ID       INT NOT NULL,
    Usuario          VARCHAR(50) NOT NULL,
    Intentos         INT NOT NULL DEFAULT 0,
    Inicio_Ventana   DATETIME NULL,
    Ultimo_Intento   DATETIME NULL,
    Bloqueado_Hasta  DATETIME NULL,
    PRIMARY KEY (Usuario_ID),
    KEY IX_Intentos_Acceso_Usuario (Usuario),
    KEY IX_Intentos_Acceso_Bloqueo (Bloqueado_Hasta)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;

CREATE TABLE IF NOT EXISTS Configuracion (
    Clave VARCHAR(60) NOT NULL,
    Valor VARCHAR(500) NULL,
    PRIMARY KEY (Clave)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;

CREATE TABLE IF NOT EXISTS Feriados (
    ID          INT NOT NULL AUTO_INCREMENT,
    Fecha       DATE NOT NULL,
    Descripcion VARCHAR(120) NOT NULL,
    PRIMARY KEY (ID),
    UNIQUE KEY UQ_Feriados_Fecha (Fecha)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;

/* ------------------------------------------------------------------
   3. Catálogos judiciales
   ------------------------------------------------------------------ */

CREATE TABLE IF NOT EXISTS Juzgados (
    ID       INT NOT NULL AUTO_INCREMENT,
    Nombre   VARCHAR(180) NOT NULL,
    Materia  VARCHAR(40) NOT NULL,
    Distrito VARCHAR(80) NULL,
    Activo   TINYINT(1) NOT NULL DEFAULT 1,
    PRIMARY KEY (ID)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;

CREATE TABLE IF NOT EXISTS Tipos_Actuacion (
    ID                 INT NOT NULL AUTO_INCREMENT,
    Nombre             VARCHAR(120) NOT NULL,
    Plazo_Dias_Default INT NOT NULL,
    Materia            VARCHAR(40) NULL,
    Activo             TINYINT(1) NOT NULL DEFAULT 1,
    PRIMARY KEY (ID)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;

/* ------------------------------------------------------------------
   4. Clientes y entidades
   ------------------------------------------------------------------ */

CREATE TABLE IF NOT EXISTS Clientes (
    ID               INT NOT NULL AUTO_INCREMENT,
    Nombres          VARCHAR(120) NOT NULL,
    Apellidos        VARCHAR(120) NOT NULL,
    DUI              VARCHAR(12) NULL,
    NIT              VARCHAR(20) NULL,
    Nacionalidad     VARCHAR(60) NULL,
    Profesion        VARCHAR(120) NULL,
    Fecha_Nacimiento DATE NULL,
    Direccion        VARCHAR(300) NULL,
    Telefono         VARCHAR(40) NULL,
    Email            VARCHAR(120) NULL,
    Activo           TINYINT(1) NOT NULL DEFAULT 1,
    Fecha_Registro   DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    PRIMARY KEY (ID),
    -- En MySQL un índice único permite varios NULL, igual que el índice
    -- filtrado de SQL Server (DUI ... WHERE DUI IS NOT NULL).
    UNIQUE KEY UX_Clientes_DUI (DUI)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;

CREATE TABLE IF NOT EXISTS Entidades_Legales (
    ID             INT NOT NULL AUTO_INCREMENT,
    Razon_Social   VARCHAR(200) NOT NULL,
    NIT            VARCHAR(20) NULL,
    NRC            VARCHAR(30) NULL,
    Domicilio      VARCHAR(300) NULL,
    Activo         TINYINT(1) NOT NULL DEFAULT 1,
    Fecha_Registro DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    PRIMARY KEY (ID)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;

CREATE TABLE IF NOT EXISTS Representantes_Legales (
    ID                   INT NOT NULL AUTO_INCREMENT,
    ID_Entidad           INT NOT NULL,
    ID_Cliente           INT NOT NULL,
    Cargo                VARCHAR(120) NULL,
    Tipo_Representacion  VARCHAR(80) NULL,
    PRIMARY KEY (ID),
    UNIQUE KEY UQ_Representantes (ID_Entidad, ID_Cliente),
    CONSTRAINT FK_Representantes_Entidad FOREIGN KEY (ID_Entidad) REFERENCES Entidades_Legales(ID),
    CONSTRAINT FK_Representantes_Cliente FOREIGN KEY (ID_Cliente) REFERENCES Clientes(ID)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;

CREATE TABLE IF NOT EXISTS Perfil_LDAFT (
    ID                        INT NOT NULL AUTO_INCREMENT,
    ID_Cliente                INT NULL,
    ID_Entidad                INT NULL,
    Nivel_Riesgo              VARCHAR(10) NOT NULL,
    Origen_Fondos_Verificado  TINYINT(1) NOT NULL DEFAULT 0,
    Detalle_Origen_Fondos     VARCHAR(500) NULL,
    Fecha_Verificacion_Listas DATE NULL,
    Fecha_Actualizacion       DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    Observaciones             VARCHAR(500) NULL,
    PRIMARY KEY (ID),
    UNIQUE KEY UX_Perfil_Cliente (ID_Cliente),
    UNIQUE KEY UX_Perfil_Entidad (ID_Entidad),
    CONSTRAINT FK_Perfil_Cliente FOREIGN KEY (ID_Cliente) REFERENCES Clientes(ID),
    CONSTRAINT FK_Perfil_Entidad FOREIGN KEY (ID_Entidad) REFERENCES Entidades_Legales(ID),
    CONSTRAINT CK_Perfil_Riesgo CHECK (Nivel_Riesgo IN ('Bajo', 'Medio', 'Alto')),
    CONSTRAINT CK_Perfil_Dueno CHECK (ID_Cliente IS NOT NULL OR ID_Entidad IS NOT NULL),
    CONSTRAINT CK_Perfil_Dueno_Exclusivo CHECK (
        (ID_Cliente IS NOT NULL AND ID_Entidad IS NULL) OR
        (ID_Cliente IS NULL AND ID_Entidad IS NOT NULL)
    )
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;

/* ------------------------------------------------------------------
   5. Casos y actuaciones
   ------------------------------------------------------------------ */

CREATE TABLE IF NOT EXISTS Casos (
    ID                INT NOT NULL AUTO_INCREMENT,
    ID_Cliente        INT NOT NULL,
    Numero_Expediente VARCHAR(80) NOT NULL,
    Materia           VARCHAR(40) NOT NULL,
    ID_Juzgado        INT NULL,
    Descripcion       VARCHAR(1000) NULL,
    Estado            VARCHAR(20) NOT NULL DEFAULT 'Activo',
    Fecha_Apertura    DATE NOT NULL,
    Responsable       VARCHAR(120) NULL,
    PRIMARY KEY (ID),
    UNIQUE KEY UX_Casos_Expediente (Numero_Expediente, ID_Juzgado),
    CONSTRAINT FK_Casos_Cliente FOREIGN KEY (ID_Cliente) REFERENCES Clientes(ID),
    CONSTRAINT FK_Casos_Juzgado FOREIGN KEY (ID_Juzgado) REFERENCES Juzgados(ID),
    CONSTRAINT CK_Casos_Estado CHECK (Estado IN ('Activo', 'Suspendido', 'Cerrado', 'Archivado'))
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;

CREATE TABLE IF NOT EXISTS Actuaciones_Procesales (
    ID                          INT NOT NULL AUTO_INCREMENT,
    ID_Caso                     INT NOT NULL,
    ID_Tipo_Actuacion           INT NOT NULL,
    Fecha_Notificacion          DATE NOT NULL,
    Plazo_Legal_Dias            INT NOT NULL,
    Fecha_Vencimiento_Calculada DATE NULL,
    Estado                      VARCHAR(20) NOT NULL DEFAULT 'Pendiente',
    Fecha_Confirmacion          DATETIME NULL,
    Usuario_Confirma            VARCHAR(50) NULL,
    Observaciones               VARCHAR(500) NULL,
    PRIMARY KEY (ID),
    KEY IX_Actuaciones_Caso (ID_Caso),
    KEY IX_Actuaciones_Vencimiento (Estado, Fecha_Vencimiento_Calculada),
    CONSTRAINT FK_Actuaciones_Caso FOREIGN KEY (ID_Caso) REFERENCES Casos(ID),
    CONSTRAINT FK_Actuaciones_Tipo FOREIGN KEY (ID_Tipo_Actuacion) REFERENCES Tipos_Actuacion(ID),
    CONSTRAINT CK_Actuaciones_Estado CHECK (Estado IN ('Pendiente', 'Cumplido', 'Vencido'))
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;

/* ------------------------------------------------------------------
   6. Protocolo notarial
   ------------------------------------------------------------------ */

CREATE TABLE IF NOT EXISTS Libros_Protocolo (
    ID                      INT NOT NULL AUTO_INCREMENT,
    Numero_Libro            INT NOT NULL,
    Anio_Vigencia           INT NOT NULL,
    Fecha_Autorizacion      DATE NOT NULL,
    Fecha_Vencimiento       DATE NOT NULL,
    Estado                  VARCHAR(20) NOT NULL DEFAULT 'Vigente',
    Razon_Cierre            VARCHAR(1000) NULL,
    Fecha_Cierre            DATE NULL,
    Fecha_Entrega_Notariado DATE NULL,
    Indice_Generado         TINYINT(1) NOT NULL DEFAULT 0,
    PRIMARY KEY (ID),
    UNIQUE KEY UQ_Libros_Numero (Numero_Libro, Anio_Vigencia),
    CONSTRAINT CK_Libros_Estado CHECK (Estado IN ('Vigente', 'Cerrado', 'Entregado'))
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;

CREATE TABLE IF NOT EXISTS Instrumentos (
    ID                      INT NOT NULL AUTO_INCREMENT,
    ID_Libro                INT NOT NULL,
    Numero_Escritura        INT NOT NULL,
    Fecha_Hora_Otorgamiento DATETIME NOT NULL,
    Folio_Inicio            INT NOT NULL,
    Folio_Fin               INT NOT NULL,
    Objeto_Acto             VARCHAR(500) NOT NULL,
    Estado                  VARCHAR(20) NOT NULL DEFAULT 'Vigente',
    Fecha_Creacion          DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    PRIMARY KEY (ID),
    UNIQUE KEY UQ_Instrumentos_Numero (ID_Libro, Numero_Escritura),
    CONSTRAINT FK_Instrumentos_Libro FOREIGN KEY (ID_Libro) REFERENCES Libros_Protocolo(ID),
    CONSTRAINT CK_Instrumentos_Folios CHECK (Folio_Inicio >= 1 AND Folio_Fin >= Folio_Inicio),
    CONSTRAINT CK_Instrumentos_Estado CHECK (Estado IN ('Vigente', 'Anulado'))
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;

CREATE TABLE IF NOT EXISTS Comparecientes_Instrumento (
    ID             INT NOT NULL AUTO_INCREMENT,
    ID_Instrumento INT NOT NULL,
    ID_Cliente     INT NOT NULL,
    Calidad        VARCHAR(30) NOT NULL,
    PRIMARY KEY (ID),
    UNIQUE KEY UQ_Comparecientes (ID_Instrumento, ID_Cliente, Calidad),
    CONSTRAINT FK_Comparecientes_Instrumento FOREIGN KEY (ID_Instrumento) REFERENCES Instrumentos(ID),
    CONSTRAINT FK_Comparecientes_Cliente FOREIGN KEY (ID_Cliente) REFERENCES Clientes(ID),
    CONSTRAINT CK_Comparecientes_Calidad CHECK (Calidad IN ('Otorgante', 'Favorecido', 'Testigo', 'Intérprete'))
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;

CREATE TABLE IF NOT EXISTS Testimonios_Expedidos (
    ID              INT NOT NULL AUTO_INCREMENT,
    ID_Instrumento  INT NOT NULL,
    Tipo_Testimonio VARCHAR(20) NOT NULL,
    Fecha_Expedicion DATE NOT NULL,
    Destinatario    VARCHAR(200) NOT NULL,
    Usuario         VARCHAR(50) NULL,
    PRIMARY KEY (ID),
    CONSTRAINT FK_Testimonios_Instrumento FOREIGN KEY (ID_Instrumento) REFERENCES Instrumentos(ID),
    CONSTRAINT CK_Testimonios_Tipo CHECK (Tipo_Testimonio IN ('Primer', 'Ulterior'))
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;

/* ------------------------------------------------------------------
   7. Documentos
   ------------------------------------------------------------------ */

CREATE TABLE IF NOT EXISTS Documentos (
    ID                      INT NOT NULL AUTO_INCREMENT,
    Tipo_Documento          VARCHAR(60) NOT NULL,
    Ruta_Archivo            VARCHAR(500) NOT NULL,
    Hash                    VARCHAR(64) NOT NULL,
    Estado_Procesamiento_IA VARCHAR(30) NOT NULL DEFAULT 'No procesado',
    ID_Caso                 INT NULL,
    ID_Instrumento          INT NULL,
    Fecha_Carga             DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    Usuario                 VARCHAR(50) NULL,
    PRIMARY KEY (ID),
    CONSTRAINT FK_Documentos_Caso FOREIGN KEY (ID_Caso) REFERENCES Casos(ID),
    CONSTRAINT FK_Documentos_Instrumento FOREIGN KEY (ID_Instrumento) REFERENCES Instrumentos(ID)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;

/* ------------------------------------------------------------------
   8. Cumplimiento LDA/FT y ARCO
   ------------------------------------------------------------------ */

CREATE TABLE IF NOT EXISTS Operaciones_LDAFT (
    ID                         INT NOT NULL AUTO_INCREMENT,
    ID_Caso                    INT NULL,
    ID_Instrumento             INT NULL,
    Tipo_Operacion             VARCHAR(20) NOT NULL,
    Monto                      DECIMAL(18,2) NOT NULL,
    Moneda                     VARCHAR(5) NOT NULL DEFAULT 'USD',
    Fecha_Operacion            DATE NOT NULL,
    Descripcion                VARCHAR(1000) NULL,
    Estado                     VARCHAR(20) NOT NULL DEFAULT 'Registrada',
    Fecha_Vencimiento_Analisis DATE NULL,
    Fecha_Vencimiento_Reporte  DATE NULL,
    Fecha_Reporte              DATE NULL,
    Reportado_UIF              TINYINT(1) NOT NULL DEFAULT 0,
    Observaciones              VARCHAR(500) NULL,
    PRIMARY KEY (ID),
    CONSTRAINT FK_Operaciones_Caso FOREIGN KEY (ID_Caso) REFERENCES Casos(ID),
    CONSTRAINT FK_Operaciones_Instrumento FOREIGN KEY (ID_Instrumento) REFERENCES Instrumentos(ID),
    CONSTRAINT CK_Operaciones_Tipo CHECK (Tipo_Operacion IN ('Regulada', 'Inusual', 'Sospechosa')),
    CONSTRAINT CK_Operaciones_Estado CHECK (Estado IN ('Registrada', 'En análisis', 'Reportada', 'Descartada'))
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;

CREATE TABLE IF NOT EXISTS Solicitudes_ARCO (
    ID                         INT NOT NULL AUTO_INCREMENT,
    ID_Cliente                 INT NOT NULL,
    Tipo_Derecho               VARCHAR(30) NOT NULL,
    Fecha_Solicitud            DATE NOT NULL,
    Fecha_Vencimiento          DATE NOT NULL,
    Fecha_Vencimiento_Prorroga DATE NULL,
    Prorroga_Aplicada          TINYINT(1) NOT NULL DEFAULT 0,
    Estado                     VARCHAR(20) NOT NULL DEFAULT 'Pendiente',
    Fecha_Respuesta            DATE NULL,
    Detalle                    VARCHAR(1000) NULL,
    PRIMARY KEY (ID),
    CONSTRAINT FK_ARCO_Cliente FOREIGN KEY (ID_Cliente) REFERENCES Clientes(ID),
    CONSTRAINT CK_ARCO_Derecho CHECK (Tipo_Derecho IN ('Acceso', 'Rectificación', 'Cancelación', 'Oposición', 'Portabilidad', 'Olvido')),
    CONSTRAINT CK_ARCO_Estado CHECK (Estado IN ('Pendiente', 'Respondida', 'Vencida'))
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;

/* ------------------------------------------------------------------
   9. Plantillas y auditoría
   ------------------------------------------------------------------ */

CREATE TABLE IF NOT EXISTS Plantillas_Documentos (
    ID             INT NOT NULL AUTO_INCREMENT,
    Nombre         VARCHAR(150) NOT NULL,
    Tipo           VARCHAR(60) NOT NULL,
    Ruta_Plantilla VARCHAR(500) NULL,
    Activa         TINYINT(1) NOT NULL DEFAULT 1,
    PRIMARY KEY (ID)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;

CREATE TABLE IF NOT EXISTS Audit_Log (
    ID             BIGINT NOT NULL AUTO_INCREMENT,
    Usuario        VARCHAR(50) NOT NULL,
    Fecha_Hora     DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    Accion         VARCHAR(40) NOT NULL,
    Tabla_Afectada VARCHAR(80) NOT NULL,
    ID_Registro    INT NULL,
    Detalle        VARCHAR(1000) NULL,
    PRIMARY KEY (ID),
    KEY IX_Audit_Fecha (Fecha_Hora)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;

/* ==================================================================
   10. Datos de seguridad iniciales (roles y permisos)
   ================================================================== */

INSERT INTO Roles (Nombre, Descripcion, Activo, Protegido) VALUES
    ('Administrador', 'Acceso total. Rol protegido del sistema.', 1, 1),
    ('Abogado', 'Gestión legal y notarial; sin configuración del sistema.', 1, 0),
    ('Auxiliar', 'Apoyo administrativo: consulta y carga de documentos.', 1, 0),
    ('Notario', 'Protocolo notarial y documentos.', 1, 0),
    ('Oficial de Cumplimiento', 'LDA/FT, derechos ARCO y auditoría.', 1, 0)
ON DUPLICATE KEY UPDATE Descripcion = VALUES(Descripcion), Activo = 1;

INSERT INTO Permisos (Codigo, Modulo, Descripcion) VALUES
    ('clientes.ver',              'Clientes',      'Consultar clientes y su perfil LDA/FT'),
    ('clientes.gestionar',        'Clientes',      'Crear y editar clientes'),
    ('clientes.desactivar',       'Clientes',      'Desactivar clientes'),
    ('entidades.ver',             'Entidades',     'Consultar entidades legales'),
    ('entidades.gestionar',       'Entidades',     'Crear y editar entidades y representantes'),
    ('casos.ver',                 'Casos',         'Consultar casos y plazos'),
    ('casos.gestionar',           'Casos',         'Crear y editar casos'),
    ('casos.confirmar_plazo',     'Casos',         'Confirmar plazos procesales'),
    ('casos.cumplir_plazo',       'Casos',         'Marcar plazos como cumplidos'),
    ('protocolo.ver',             'Protocolo',     'Consultar el protocolo notarial'),
    ('protocolo.gestionar',       'Protocolo',     'Crear libros, escrituras y comparecientes'),
    ('protocolo.cerrar_libro',    'Protocolo',     'Cerrar libros de protocolo'),
    ('protocolo.testimonios',     'Protocolo',     'Expedir y anular testimonios'),
    ('protocolo.entregar',        'Protocolo',     'Registrar entrega al Notariado'),
    ('documentos.ver',            'Documentos',    'Consultar documentos'),
    ('documentos.importar',       'Documentos',    'Importar documentos'),
    ('documentos.verificar',      'Documentos',    'Verificar integridad de documentos'),
    ('documentos.metadatos',      'Documentos',    'Editar metadatos de documentos'),
    ('cumplimiento.ver',          'Cumplimiento',  'Consultar LDA/FT y ARCO'),
    ('cumplimiento.operaciones',  'Cumplimiento',  'Registrar y reportar operaciones LDA/FT'),
    ('cumplimiento.reportar_uif', 'Cumplimiento',  'Registrar reportes a la UIF'),
    ('cumplimiento.arco',         'Cumplimiento',  'Gestionar solicitudes ARCO'),
    ('auditoria.ver',             'Auditoría',     'Consultar la bitácora'),
    ('auditoria.exportar',        'Auditoría',     'Exportar la bitácora a CSV'),
    ('config.ver',                'Configuración', 'Acceder a configuración'),
    ('config.editar',             'Configuración', 'Modificar parámetros de configuración'),
    ('config.feriados',           'Configuración', 'Administrar feriados'),
    ('config.usuarios',           'Configuración', 'Administrar usuarios'),
    ('config.roles',              'Configuración', 'Administrar roles y permisos'),
    ('config.seguridad',          'Configuración', 'Cambiar claves y parámetros de seguridad'),
    ('config.respaldos',          'Configuración', 'Crear respaldos de la base de datos')
ON DUPLICATE KEY UPDATE Modulo = VALUES(Modulo), Descripcion = VALUES(Descripcion);

/* Permisos por rol. Se usa INSERT IGNORE para poder reejecutar el script. */

/* Administrador: todos los permisos del catálogo. */
INSERT IGNORE INTO Rol_Permiso (ID_Rol, Codigo)
SELECT r.ID, p.Codigo FROM Roles r CROSS JOIN Permisos p WHERE r.Protegido = 1;

/* Abogado */
INSERT IGNORE INTO Rol_Permiso (ID_Rol, Codigo)
SELECT r.ID, p.Codigo FROM Roles r CROSS JOIN Permisos p
WHERE r.Nombre = 'Abogado' AND p.Codigo IN (
    'clientes.ver','clientes.gestionar','entidades.ver','entidades.gestionar',
    'casos.ver','casos.gestionar','casos.confirmar_plazo','casos.cumplir_plazo',
    'protocolo.ver','protocolo.gestionar','protocolo.cerrar_libro','protocolo.testimonios',
    'documentos.ver','documentos.importar','documentos.verificar','documentos.metadatos',
    'cumplimiento.ver','auditoria.ver');

/* Auxiliar */
INSERT IGNORE INTO Rol_Permiso (ID_Rol, Codigo)
SELECT r.ID, p.Codigo FROM Roles r CROSS JOIN Permisos p
WHERE r.Nombre = 'Auxiliar' AND p.Codigo IN (
    'clientes.ver','clientes.gestionar','entidades.ver',
    'casos.ver','casos.cumplir_plazo','protocolo.ver',
    'documentos.ver','documentos.importar','documentos.verificar',
    'cumplimiento.ver','cumplimiento.arco');

/* Notario */
INSERT IGNORE INTO Rol_Permiso (ID_Rol, Codigo)
SELECT r.ID, p.Codigo FROM Roles r CROSS JOIN Permisos p
WHERE r.Nombre = 'Notario' AND p.Codigo IN (
    'clientes.ver','casos.ver',
    'protocolo.ver','protocolo.gestionar','protocolo.cerrar_libro',
    'protocolo.testimonios','protocolo.entregar',
    'documentos.ver','documentos.importar','documentos.verificar','documentos.metadatos',
    'auditoria.ver');

/* Oficial de Cumplimiento */
INSERT IGNORE INTO Rol_Permiso (ID_Rol, Codigo)
SELECT r.ID, p.Codigo FROM Roles r CROSS JOIN Permisos p
WHERE r.Nombre = 'Oficial de Cumplimiento' AND p.Codigo IN (
    'clientes.ver','entidades.ver','casos.ver','documentos.ver',
    'cumplimiento.ver','cumplimiento.operaciones','cumplimiento.reportar_uif','cumplimiento.arco',
    'auditoria.ver','auditoria.exportar');

/* Los candados (triggers) se instalan aparte, con el archivo
   Triggers_MySQL.sql, porque el conector ADO no interpreta DELIMITER. */
