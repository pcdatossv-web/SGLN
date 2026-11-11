/* ============================================================
   SGLN El Salvador - Esquema relacional v1.0
   Motor: SQL Server (LocalDB / Express / Standard)
   Ejecutado automáticamente por Datos\InicializadorBD.vb
   ============================================================ */

IF OBJECT_ID('Usuarios', 'U') IS NULL
BEGIN
    CREATE TABLE Usuarios (
        ID              INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_Usuarios PRIMARY KEY,
        Usuario         NVARCHAR(50)  NOT NULL,
        Nombre_Completo NVARCHAR(150) NOT NULL,
        Clave_Hash      NVARCHAR(200) NOT NULL,
        Clave_Salt      NVARCHAR(100) NOT NULL,
        Rol             NVARCHAR(30)  NOT NULL,
        Activo          BIT           NOT NULL CONSTRAINT DF_Usuarios_Activo DEFAULT (1),
        Fecha_Creacion  DATETIME2(0)  NOT NULL CONSTRAINT DF_Usuarios_Fecha DEFAULT (SYSDATETIME()),
        CONSTRAINT UQ_Usuarios_Usuario UNIQUE (Usuario)
    );
END;

IF OBJECT_ID('Configuracion', 'U') IS NULL
BEGIN
    CREATE TABLE Configuracion (
        Clave NVARCHAR(60)  NOT NULL CONSTRAINT PK_Configuracion PRIMARY KEY,
        Valor NVARCHAR(500) NULL
    );
END;

IF OBJECT_ID('Feriados', 'U') IS NULL
BEGIN
    CREATE TABLE Feriados (
        ID          INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_Feriados PRIMARY KEY,
        Fecha       DATE          NOT NULL,
        Descripcion NVARCHAR(120) NOT NULL,
        CONSTRAINT UQ_Feriados_Fecha UNIQUE (Fecha)
    );
END;

IF OBJECT_ID('Juzgados', 'U') IS NULL
BEGIN
    CREATE TABLE Juzgados (
        ID      INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_Juzgados PRIMARY KEY,
        Nombre  NVARCHAR(180) NOT NULL,
        Materia NVARCHAR(40)  NOT NULL,
        Distrito NVARCHAR(80) NULL,
        Activo  BIT           NOT NULL CONSTRAINT DF_Juzgados_Activo DEFAULT (1)
    );
END;

IF OBJECT_ID('Tipos_Actuacion', 'U') IS NULL
BEGIN
    CREATE TABLE Tipos_Actuacion (
        ID                INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_Tipos_Actuacion PRIMARY KEY,
        Nombre            NVARCHAR(120) NOT NULL,
        Plazo_Dias_Default INT          NOT NULL,
        Materia           NVARCHAR(40)  NULL,
        Activo            BIT           NOT NULL CONSTRAINT DF_Tipos_Actuacion_Activo DEFAULT (1)
    );
END;

IF OBJECT_ID('Clientes', 'U') IS NULL
BEGIN
    CREATE TABLE Clientes (
        ID               INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_Clientes PRIMARY KEY,
        Nombres          NVARCHAR(120) NOT NULL,
        Apellidos        NVARCHAR(120) NOT NULL,
        DUI              NVARCHAR(12)  NULL,
        NIT              NVARCHAR(20)  NULL,
        Nacionalidad     NVARCHAR(60)  NULL,
        Profesion        NVARCHAR(120) NULL,
        Fecha_Nacimiento DATE          NULL,
        Direccion        NVARCHAR(300) NULL,
        Telefono         NVARCHAR(40)  NULL,
        Email            NVARCHAR(120) NULL,
        Activo           BIT           NOT NULL CONSTRAINT DF_Clientes_Activo DEFAULT (1),
        Fecha_Registro   DATETIME2(0)  NOT NULL CONSTRAINT DF_Clientes_Fecha DEFAULT (SYSDATETIME())
    );
END;

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'UX_Clientes_DUI' AND object_id = OBJECT_ID('Clientes'))
    CREATE UNIQUE INDEX UX_Clientes_DUI ON Clientes(DUI) WHERE DUI IS NOT NULL;

IF OBJECT_ID('Entidades_Legales', 'U') IS NULL
BEGIN
    CREATE TABLE Entidades_Legales (
        ID           INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_Entidades_Legales PRIMARY KEY,
        Razon_Social NVARCHAR(200) NOT NULL,
        NIT          NVARCHAR(20)  NULL,
        NRC          NVARCHAR(30)  NULL,
        Domicilio    NVARCHAR(300) NULL,
        Activo       BIT           NOT NULL CONSTRAINT DF_Entidades_Activo DEFAULT (1),
        Fecha_Registro DATETIME2(0) NOT NULL CONSTRAINT DF_Entidades_Fecha DEFAULT (SYSDATETIME())
    );
END;

IF OBJECT_ID('Representantes_Legales', 'U') IS NULL
BEGIN
    CREATE TABLE Representantes_Legales (
        ID                INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_Representantes PRIMARY KEY,
        ID_Entidad        INT NOT NULL,
        ID_Cliente        INT NOT NULL,
        Cargo             NVARCHAR(120) NULL,
        Tipo_Representacion NVARCHAR(80) NULL,
        CONSTRAINT FK_Representantes_Entidad FOREIGN KEY (ID_Entidad) REFERENCES Entidades_Legales(ID),
        CONSTRAINT FK_Representantes_Cliente FOREIGN KEY (ID_Cliente) REFERENCES Clientes(ID),
        CONSTRAINT UQ_Representantes UNIQUE (ID_Entidad, ID_Cliente)
    );
END;

IF OBJECT_ID('Perfil_LDAFT', 'U') IS NULL
BEGIN
    CREATE TABLE Perfil_LDAFT (
        ID                       INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_Perfil_LDAFT PRIMARY KEY,
        ID_Cliente               INT NULL,
        ID_Entidad               INT NULL,
        Nivel_Riesgo             NVARCHAR(10)  NOT NULL,
        Origen_Fondos_Verificado BIT           NOT NULL CONSTRAINT DF_Perfil_Origen DEFAULT (0),
        Detalle_Origen_Fondos    NVARCHAR(500) NULL,
        Fecha_Verificacion_Listas DATE         NULL,
        Fecha_Actualizacion      DATETIME2(0)  NOT NULL CONSTRAINT DF_Perfil_Fecha DEFAULT (SYSDATETIME()),
        Observaciones            NVARCHAR(500) NULL,
        CONSTRAINT FK_Perfil_Cliente FOREIGN KEY (ID_Cliente) REFERENCES Clientes(ID),
        CONSTRAINT FK_Perfil_Entidad FOREIGN KEY (ID_Entidad) REFERENCES Entidades_Legales(ID),
        CONSTRAINT CK_Perfil_Riesgo CHECK (Nivel_Riesgo IN ('Bajo', 'Medio', 'Alto')),
        CONSTRAINT CK_Perfil_Dueno CHECK (ID_Cliente IS NOT NULL OR ID_Entidad IS NOT NULL)
    );
END;

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'UX_Perfil_Cliente' AND object_id = OBJECT_ID('Perfil_LDAFT'))
    CREATE UNIQUE INDEX UX_Perfil_Cliente ON Perfil_LDAFT(ID_Cliente) WHERE ID_Cliente IS NOT NULL;

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'UX_Perfil_Entidad' AND object_id = OBJECT_ID('Perfil_LDAFT'))
    CREATE UNIQUE INDEX UX_Perfil_Entidad ON Perfil_LDAFT(ID_Entidad) WHERE ID_Entidad IS NOT NULL;

IF OBJECT_ID('Casos', 'U') IS NULL
BEGIN
    CREATE TABLE Casos (
        ID                INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_Casos PRIMARY KEY,
        ID_Cliente        INT NOT NULL,
        Numero_Expediente NVARCHAR(80) NOT NULL,
        Materia           NVARCHAR(40) NOT NULL,
        ID_Juzgado        INT NULL,
        Descripcion       NVARCHAR(1000) NULL,
        Estado            NVARCHAR(20) NOT NULL CONSTRAINT DF_Casos_Estado DEFAULT ('Activo'),
        Fecha_Apertura    DATE NOT NULL,
        Responsable       NVARCHAR(120) NULL,
        CONSTRAINT FK_Casos_Cliente FOREIGN KEY (ID_Cliente) REFERENCES Clientes(ID),
        CONSTRAINT FK_Casos_Juzgado FOREIGN KEY (ID_Juzgado) REFERENCES Juzgados(ID),
        CONSTRAINT CK_Casos_Estado CHECK (Estado IN ('Activo', 'Suspendido', 'Cerrado', 'Archivado'))
    );
END;

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'UX_Casos_Expediente' AND object_id = OBJECT_ID('Casos'))
    CREATE UNIQUE INDEX UX_Casos_Expediente ON Casos(Numero_Expediente, ID_Juzgado);

IF OBJECT_ID('Actuaciones_Procesales', 'U') IS NULL
BEGIN
    CREATE TABLE Actuaciones_Procesales (
        ID                        INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_Actuaciones PRIMARY KEY,
        ID_Caso                   INT NOT NULL,
        ID_Tipo_Actuacion         INT NOT NULL,
        Fecha_Notificacion        DATE NOT NULL,
        Plazo_Legal_Dias          INT NOT NULL,
        Fecha_Vencimiento_Calculada DATE NULL,
        Estado                    NVARCHAR(20) NOT NULL CONSTRAINT DF_Actuaciones_Estado DEFAULT ('Pendiente'),
        Fecha_Confirmacion        DATETIME2(0) NULL,
        Usuario_Confirma          NVARCHAR(50) NULL,
        Observaciones             NVARCHAR(500) NULL,
        CONSTRAINT FK_Actuaciones_Caso FOREIGN KEY (ID_Caso) REFERENCES Casos(ID),
        CONSTRAINT FK_Actuaciones_Tipo FOREIGN KEY (ID_Tipo_Actuacion) REFERENCES Tipos_Actuacion(ID),
        CONSTRAINT CK_Actuaciones_Estado CHECK (Estado IN ('Pendiente', 'Cumplido', 'Vencido'))
    );
END;

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Actuaciones_Caso' AND object_id = OBJECT_ID('Actuaciones_Procesales'))
    CREATE INDEX IX_Actuaciones_Caso ON Actuaciones_Procesales(ID_Caso);

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Actuaciones_Vencimiento' AND object_id = OBJECT_ID('Actuaciones_Procesales'))
    CREATE INDEX IX_Actuaciones_Vencimiento ON Actuaciones_Procesales(Estado, Fecha_Vencimiento_Calculada);

IF OBJECT_ID('Libros_Protocolo', 'U') IS NULL
BEGIN
    CREATE TABLE Libros_Protocolo (
        ID                    INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_Libros PRIMARY KEY,
        Numero_Libro          INT NOT NULL,
        Anio_Vigencia         INT NOT NULL,
        Fecha_Autorizacion    DATE NOT NULL,
        Fecha_Vencimiento     DATE NOT NULL,
        Estado                NVARCHAR(20) NOT NULL CONSTRAINT DF_Libros_Estado DEFAULT ('Vigente'),
        Razon_Cierre          NVARCHAR(1000) NULL,
        Fecha_Cierre          DATE NULL,
        Fecha_Entrega_Notariado DATE NULL,
        Indice_Generado       BIT NOT NULL CONSTRAINT DF_Libros_Indice DEFAULT (0),
        CONSTRAINT UQ_Libros_Numero UNIQUE (Numero_Libro, Anio_Vigencia),
        CONSTRAINT CK_Libros_Estado CHECK (Estado IN ('Vigente', 'Cerrado', 'Entregado'))
    );
END;

IF OBJECT_ID('Instrumentos', 'U') IS NULL
BEGIN
    CREATE TABLE Instrumentos (
        ID                       INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_Instrumentos PRIMARY KEY,
        ID_Libro                 INT NOT NULL,
        Numero_Escritura         INT NOT NULL,
        Fecha_Hora_Otorgamiento  DATETIME2(0) NOT NULL,
        Folio_Inicio             INT NOT NULL,
        Folio_Fin                INT NOT NULL,
        Objeto_Acto              NVARCHAR(500) NOT NULL,
        Estado                   NVARCHAR(20) NOT NULL CONSTRAINT DF_Instrumentos_Estado DEFAULT ('Vigente'),
        Fecha_Creacion           DATETIME2(0) NOT NULL CONSTRAINT DF_Instrumentos_Fecha DEFAULT (SYSDATETIME()),
        CONSTRAINT FK_Instrumentos_Libro FOREIGN KEY (ID_Libro) REFERENCES Libros_Protocolo(ID),
        CONSTRAINT UQ_Instrumentos_Numero UNIQUE (ID_Libro, Numero_Escritura),
        CONSTRAINT CK_Instrumentos_Folios CHECK (Folio_Inicio >= 1 AND Folio_Fin >= Folio_Inicio),
        CONSTRAINT CK_Instrumentos_Estado CHECK (Estado IN ('Vigente', 'Anulado'))
    );
END;

IF OBJECT_ID('Comparecientes_Instrumento', 'U') IS NULL
BEGIN
    CREATE TABLE Comparecientes_Instrumento (
        ID             INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_Comparecientes PRIMARY KEY,
        ID_Instrumento INT NOT NULL,
        ID_Cliente     INT NOT NULL,
        Calidad        NVARCHAR(30) NOT NULL,
        CONSTRAINT FK_Comparecientes_Instrumento FOREIGN KEY (ID_Instrumento) REFERENCES Instrumentos(ID),
        CONSTRAINT FK_Comparecientes_Cliente FOREIGN KEY (ID_Cliente) REFERENCES Clientes(ID),
        CONSTRAINT CK_Comparecientes_Calidad CHECK (Calidad IN ('Otorgante', 'Favorecido', 'Testigo', 'Intérprete')),
        CONSTRAINT UQ_Comparecientes UNIQUE (ID_Instrumento, ID_Cliente, Calidad)
    );
END;

IF OBJECT_ID('Testimonios_Expedidos', 'U') IS NULL
BEGIN
    CREATE TABLE Testimonios_Expedidos (
        ID              INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_Testimonios PRIMARY KEY,
        ID_Instrumento  INT NOT NULL,
        Tipo_Testimonio NVARCHAR(20) NOT NULL,
        Fecha_Expedicion DATE NOT NULL,
        Destinatario    NVARCHAR(200) NOT NULL,
        Usuario         NVARCHAR(50) NULL,
        CONSTRAINT FK_Testimonios_Instrumento FOREIGN KEY (ID_Instrumento) REFERENCES Instrumentos(ID),
        CONSTRAINT CK_Testimonios_Tipo CHECK (Tipo_Testimonio IN ('Primer', 'Ulterior'))
    );
END;

IF OBJECT_ID('Documentos', 'U') IS NULL
BEGIN
    CREATE TABLE Documentos (
        ID                     INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_Documentos PRIMARY KEY,
        Tipo_Documento         NVARCHAR(60) NOT NULL,
        Ruta_Archivo           NVARCHAR(500) NOT NULL,
        Hash                   NVARCHAR(64) NOT NULL,
        Estado_Procesamiento_IA NVARCHAR(30) NOT NULL CONSTRAINT DF_Documentos_IA DEFAULT ('No procesado'),
        ID_Caso                INT NULL,
        ID_Instrumento         INT NULL,
        Fecha_Carga            DATETIME2(0) NOT NULL CONSTRAINT DF_Documentos_Fecha DEFAULT (SYSDATETIME()),
        Usuario                NVARCHAR(50) NULL,
        CONSTRAINT FK_Documentos_Caso FOREIGN KEY (ID_Caso) REFERENCES Casos(ID),
        CONSTRAINT FK_Documentos_Instrumento FOREIGN KEY (ID_Instrumento) REFERENCES Instrumentos(ID)
    );
END;

IF OBJECT_ID('Operaciones_LDAFT', 'U') IS NULL
BEGIN
    CREATE TABLE Operaciones_LDAFT (
        ID                        INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_Operaciones PRIMARY KEY,
        ID_Caso                   INT NULL,
        ID_Instrumento            INT NULL,
        Tipo_Operacion            NVARCHAR(20) NOT NULL,
        Monto                     DECIMAL(18,2) NOT NULL,
        Moneda                    NVARCHAR(5) NOT NULL CONSTRAINT DF_Operaciones_Moneda DEFAULT ('USD'),
        Fecha_Operacion           DATE NOT NULL,
        Descripcion               NVARCHAR(1000) NULL,
        Estado                    NVARCHAR(20) NOT NULL CONSTRAINT DF_Operaciones_Estado DEFAULT ('Registrada'),
        Fecha_Vencimiento_Analisis DATE NULL,
        Fecha_Vencimiento_Reporte  DATE NULL,
        Fecha_Reporte             DATE NULL,
        Reportado_UIF             BIT NOT NULL CONSTRAINT DF_Operaciones_Reportado DEFAULT (0),
        Observaciones             NVARCHAR(500) NULL,
        CONSTRAINT FK_Operaciones_Caso FOREIGN KEY (ID_Caso) REFERENCES Casos(ID),
        CONSTRAINT FK_Operaciones_Instrumento FOREIGN KEY (ID_Instrumento) REFERENCES Instrumentos(ID),
        CONSTRAINT CK_Operaciones_Tipo CHECK (Tipo_Operacion IN ('Regulada', 'Inusual', 'Sospechosa')),
        CONSTRAINT CK_Operaciones_Estado CHECK (Estado IN ('Registrada', 'En análisis', 'Reportada', 'Descartada'))
    );
END;

IF OBJECT_ID('Solicitudes_ARCO', 'U') IS NULL
BEGIN
    CREATE TABLE Solicitudes_ARCO (
        ID                       INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_ARCO PRIMARY KEY,
        ID_Cliente               INT NOT NULL,
        Tipo_Derecho             NVARCHAR(30) NOT NULL,
        Fecha_Solicitud          DATE NOT NULL,
        Fecha_Vencimiento        DATE NOT NULL,
        Fecha_Vencimiento_Prorroga DATE NULL,
        Prorroga_Aplicada        BIT NOT NULL CONSTRAINT DF_ARCO_Prorroga DEFAULT (0),
        Estado                   NVARCHAR(20) NOT NULL CONSTRAINT DF_ARCO_Estado DEFAULT ('Pendiente'),
        Fecha_Respuesta          DATE NULL,
        Detalle                  NVARCHAR(1000) NULL,
        CONSTRAINT FK_ARCO_Cliente FOREIGN KEY (ID_Cliente) REFERENCES Clientes(ID),
        CONSTRAINT CK_ARCO_Derecho CHECK (Tipo_Derecho IN ('Acceso', 'Rectificación', 'Cancelación', 'Oposición', 'Portabilidad', 'Olvido')),
        CONSTRAINT CK_ARCO_Estado CHECK (Estado IN ('Pendiente', 'Respondida', 'Vencida'))
    );
END;

IF OBJECT_ID('Plantillas_Documentos', 'U') IS NULL
BEGIN
    CREATE TABLE Plantillas_Documentos (
        ID             INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_Plantillas PRIMARY KEY,
        Nombre         NVARCHAR(150) NOT NULL,
        Tipo           NVARCHAR(60) NOT NULL,
        Ruta_Plantilla NVARCHAR(500) NULL,
        Activa         BIT NOT NULL CONSTRAINT DF_Plantillas_Activa DEFAULT (1)
    );
END;

IF OBJECT_ID('Audit_Log', 'U') IS NULL
BEGIN
    CREATE TABLE Audit_Log (
        ID             BIGINT IDENTITY(1,1) NOT NULL CONSTRAINT PK_Audit PRIMARY KEY,
        Usuario        NVARCHAR(50) NOT NULL,
        Fecha_Hora     DATETIME2(0) NOT NULL CONSTRAINT DF_Audit_Fecha DEFAULT (SYSDATETIME()),
        Accion         NVARCHAR(40) NOT NULL,
        Tabla_Afectada NVARCHAR(80) NOT NULL,
        ID_Registro    INT NULL,
        Detalle        NVARCHAR(1000) NULL
    );
END;

/* Migración idempotente: las acciones de auditoría pueden superar los 20
   caracteres (ej. 'AGREGAR COMPARECIENTE'), lo que abortaba el INSERT con
   error 8152 y dejaba la bitácora incompleta. */
IF EXISTS (SELECT 1 FROM sys.columns
           WHERE object_id = OBJECT_ID('Audit_Log') AND name = 'Accion' AND max_length < 80)
    ALTER TABLE Audit_Log ALTER COLUMN Accion NVARCHAR(40) NOT NULL;

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Audit_Fecha' AND object_id = OBJECT_ID('Audit_Log'))
    CREATE INDEX IX_Audit_Fecha ON Audit_Log(Fecha_Hora DESC);
