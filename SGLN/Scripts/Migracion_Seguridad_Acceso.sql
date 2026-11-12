/* SGLN - Protección contra fuerza bruta.
   Ejecutar una vez como administrador MySQL en la base sgln_sv existente.
   Conserva el historial por ID de usuario aunque la cuenta sea eliminada. */

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
