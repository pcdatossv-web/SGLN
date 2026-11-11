-- MySQL dump 10.13  Distrib 8.0.39, for Win64 (x86_64)
--
-- Host: localhost    Database: sgln_sv_pruebas
-- ------------------------------------------------------
-- Server version	8.0.39

/*!40101 SET @OLD_CHARACTER_SET_CLIENT=@@CHARACTER_SET_CLIENT */;
/*!40101 SET @OLD_CHARACTER_SET_RESULTS=@@CHARACTER_SET_RESULTS */;
/*!40101 SET @OLD_COLLATION_CONNECTION=@@COLLATION_CONNECTION */;
/*!50503 SET NAMES utf8mb4 */;
/*!40103 SET @OLD_TIME_ZONE=@@TIME_ZONE */;
/*!40103 SET TIME_ZONE='+00:00' */;
/*!40014 SET @OLD_UNIQUE_CHECKS=@@UNIQUE_CHECKS, UNIQUE_CHECKS=0 */;
/*!40014 SET @OLD_FOREIGN_KEY_CHECKS=@@FOREIGN_KEY_CHECKS, FOREIGN_KEY_CHECKS=0 */;
/*!40101 SET @OLD_SQL_MODE=@@SQL_MODE, SQL_MODE='NO_AUTO_VALUE_ON_ZERO' */;
/*!40111 SET @OLD_SQL_NOTES=@@SQL_NOTES, SQL_NOTES=0 */;

--
-- Current Database: `sgln_sv_pruebas`
--

CREATE DATABASE /*!32312 IF NOT EXISTS*/ `sgln_sv_pruebas` /*!40100 DEFAULT CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci */ /*!80016 DEFAULT ENCRYPTION='N' */;

USE `sgln_sv_pruebas`;

--
-- Table structure for table `actuaciones_procesales`
--

DROP TABLE IF EXISTS `actuaciones_procesales`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `actuaciones_procesales` (
  `ID` int NOT NULL AUTO_INCREMENT,
  `ID_Caso` int NOT NULL,
  `ID_Tipo_Actuacion` int NOT NULL,
  `Fecha_Notificacion` date NOT NULL,
  `Plazo_Legal_Dias` int NOT NULL,
  `Fecha_Vencimiento_Calculada` date DEFAULT NULL,
  `Estado` varchar(20) NOT NULL DEFAULT 'Pendiente',
  `Fecha_Confirmacion` datetime DEFAULT NULL,
  `Usuario_Confirma` varchar(50) DEFAULT NULL,
  `Observaciones` varchar(500) DEFAULT NULL,
  PRIMARY KEY (`ID`),
  KEY `IX_Actuaciones_Caso` (`ID_Caso`),
  KEY `IX_Actuaciones_Vencimiento` (`Estado`,`Fecha_Vencimiento_Calculada`),
  KEY `FK_Actuaciones_Tipo` (`ID_Tipo_Actuacion`),
  CONSTRAINT `FK_Actuaciones_Caso` FOREIGN KEY (`ID_Caso`) REFERENCES `casos` (`ID`),
  CONSTRAINT `FK_Actuaciones_Tipo` FOREIGN KEY (`ID_Tipo_Actuacion`) REFERENCES `tipos_actuacion` (`ID`),
  CONSTRAINT `CK_Actuaciones_Estado` CHECK ((`Estado` in (_utf8mb4'Pendiente',_utf8mb4'Cumplido',_utf8mb4'Vencido')))
) ENGINE=InnoDB AUTO_INCREMENT=3 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `actuaciones_procesales`
--

LOCK TABLES `actuaciones_procesales` WRITE;
/*!40000 ALTER TABLE `actuaciones_procesales` DISABLE KEYS */;
INSERT INTO `actuaciones_procesales` VALUES (1,1,1,'2026-11-06',15,'2026-11-27','Cumplido','2026-11-11 12:13:56','admin','Notificación del traslado de la demanda'),(2,1,1,'2026-01-05',3,'2026-01-08','Vencido','2026-11-11 12:13:56','admin',NULL);
/*!40000 ALTER TABLE `actuaciones_procesales` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `audit_log`
--

DROP TABLE IF EXISTS `audit_log`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `audit_log` (
  `ID` bigint NOT NULL AUTO_INCREMENT,
  `Usuario` varchar(50) NOT NULL,
  `Fecha_Hora` datetime NOT NULL DEFAULT CURRENT_TIMESTAMP,
  `Accion` varchar(40) NOT NULL,
  `Tabla_Afectada` varchar(80) NOT NULL,
  `ID_Registro` int DEFAULT NULL,
  `Detalle` varchar(1000) DEFAULT NULL,
  PRIMARY KEY (`ID`),
  KEY `IX_Audit_Fecha` (`Fecha_Hora`)
) ENGINE=InnoDB AUTO_INCREMENT=49 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `audit_log`
--

LOCK TABLES `audit_log` WRITE;
/*!40000 ALTER TABLE `audit_log` DISABLE KEYS */;
INSERT INTO `audit_log` VALUES (1,'admin','2026-11-11 12:13:43','INGRESO','Usuarios',1,'Inicio de sesión'),(2,'sistema','2026-11-11 12:13:46','MODIFICAR','Usuarios',1,'Cambio de clave'),(3,'admin','2026-11-11 12:13:47','INGRESO','Usuarios',1,'Inicio de sesión'),(4,'sistema','2026-11-11 12:13:50','MODIFICAR','Usuarios',1,'Cambio de clave'),(5,'admin','2026-11-11 12:13:51','INGRESO','Usuarios',1,'Inicio de sesión'),(6,'admin','2026-11-11 12:13:51','CREAR','Roles',6,'Rol Rol de prueba'),(7,'admin','2026-11-11 12:13:51','MODIFICAR','Roles',6,'Rol Rol de prueba editado'),(8,'admin','2026-11-11 12:13:51','MODIFICAR','Roles',6,'Rol Rol de prueba editado'),(9,'admin','2026-11-11 12:13:52','CREAR','Usuarios',2,'pruebauser con rol 6'),(10,'pruebauser','2026-11-11 12:13:52','INGRESO','Usuarios',2,'Inicio de sesión'),(11,'admin','2026-11-11 12:13:54','CREAR','Usuarios',4,'admin2 con rol 1'),(12,'admin2','2026-11-11 12:13:55','INGRESO','Usuarios',4,'Inicio de sesión'),(13,'admin','2026-11-11 12:13:55','MODIFICAR','Usuarios',4,'Rol 2, activo = True'),(14,'admin','2026-11-11 12:13:55','MODIFICAR','Usuarios',4,'Rol 1, activo = True'),(15,'admin2','2026-11-11 12:13:56','INGRESO','Usuarios',4,'Inicio de sesión'),(16,'admin','2026-11-11 12:13:56','ELIMINAR','Roles',6,'Rol eliminado'),(17,'admin','2026-11-11 12:13:56','CREAR','Clientes',1,'Ana María Pérez López'),(18,'admin','2026-11-11 12:13:56','MODIFICAR','Perfil_LDAFT',1,'Perfil LDA/FT actualizado'),(19,'admin','2026-11-11 12:13:56','CREAR','Entidades_Legales',1,'Inversiones Cuscatlán S.A. de C.V.'),(20,'admin','2026-11-11 12:13:56','MODIFICAR','Perfil_LDAFT',1,'Perfil LDA/FT de entidad actualizado'),(21,'admin','2026-11-11 12:13:56','CREAR','Representantes_Legales',NULL,'Representante agregado a entidad 1'),(22,'admin','2026-11-11 12:13:56','CREAR','Casos',1,'123-2026'),(23,'admin','2026-11-11 12:13:56','CONFIRMAR PLAZO','Actuaciones_Procesales',1,'Vencimiento 27/11/2026'),(24,'admin','2026-11-11 12:13:56','CONFIRMAR PLAZO','Actuaciones_Procesales',2,'Vencimiento 08/01/2026'),(25,'admin','2026-11-11 12:13:56','ACTUALIZAR','Actuaciones_Procesales',NULL,'1 plazos marcados como vencidos'),(26,'admin','2026-11-11 12:13:56','CUMPLIR PLAZO','Actuaciones_Procesales',1,'Actuación marcada como cumplida'),(27,'admin','2026-11-11 12:13:56','CREAR','Libros_Protocolo',1,'Libro 1 del año 2026'),(28,'admin','2026-11-11 12:13:56','CREAR','Instrumentos',1,'Escritura 1'),(29,'admin','2026-11-11 12:13:56','CREAR','Instrumentos',2,'Escritura 2'),(30,'admin','2026-11-11 12:13:56','AGREGAR COMPARECIENTE','Comparecientes_Instrumento',1,'Cliente 1 como Otorgante'),(31,'admin','2026-11-11 12:13:56','AGREGAR COMPARECIENTE','Comparecientes_Instrumento',2,'Cliente 1 como Otorgante'),(32,'admin','2026-11-11 12:13:56','CERRAR','Libros_Protocolo',1,'Libro completo, 2 instrumentos'),(33,'admin','2026-11-11 12:13:56','EXPEDIR TESTIMONIO','Testimonios_Expedidos',1,'Primer testimonio para Banco Cuscatlán, S.A.'),(34,'admin','2026-11-11 12:13:56','ANULAR','Instrumentos',2,'Instrumento anulado'),(35,'admin','2026-11-11 12:13:56','ENTREGAR','Libros_Protocolo',1,'Entrega al Notariado el 11/11/2026'),(36,'admin','2026-11-11 12:13:56','GENERAR ÍNDICE','Libros_Protocolo',1,'Índice del libro marcado como generado'),(37,'admin','2026-11-11 12:13:56','IMPORTAR DOCUMENTO','Documentos',1,'Contrato | SHA-256 b1aa08a9db609cf8ffab43800dde115819a8746a9d93224f5d22917dcab3bc36'),(38,'admin','2026-11-11 12:13:56','MODIFICAR','Documentos',1,'Metadatos del documento actualizados'),(39,'admin','2026-11-11 12:13:56','CREAR','Operaciones_LDAFT',1,'Regulada por 12,500.00 USD'),(40,'admin','2026-11-11 12:13:56','CREAR','Operaciones_LDAFT',2,'Sospechosa por 800.00 USD'),(41,'admin','2026-11-11 12:13:56','REPORTAR UIF','Operaciones_LDAFT',1,'Reporte registrado el 11/11/2026'),(42,'admin','2026-11-11 12:13:56','DESCARTAR','Operaciones_LDAFT',2,'Operación descartada'),(43,'admin','2026-11-11 12:13:56','CREAR','Solicitudes_ARCO',1,'Acceso del cliente 1'),(44,'admin','2026-11-11 12:13:56','CREAR','Solicitudes_ARCO',2,'Rectificación del cliente 1'),(45,'admin','2026-11-11 12:13:56','RESPONDER ARCO','Solicitudes_ARCO',1,'Respuesta el 11/11/2026'),(46,'admin','2026-11-11 12:13:56','MODIFICAR','Configuracion',NULL,'Prueba_Clav = Valor 1'),(47,'admin','2026-11-11 12:13:56','CREAR','Feriados',NULL,'31/12/2026 - Feriado de prueba'),(48,'admin','2026-11-11 12:13:56','ELIMINAR','Feriados',37,'Feriado eliminado');
/*!40000 ALTER TABLE `audit_log` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `casos`
--

DROP TABLE IF EXISTS `casos`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `casos` (
  `ID` int NOT NULL AUTO_INCREMENT,
  `ID_Cliente` int NOT NULL,
  `Numero_Expediente` varchar(80) NOT NULL,
  `Materia` varchar(40) NOT NULL,
  `ID_Juzgado` int DEFAULT NULL,
  `Descripcion` varchar(1000) DEFAULT NULL,
  `Estado` varchar(20) NOT NULL DEFAULT 'Activo',
  `Fecha_Apertura` date NOT NULL,
  `Responsable` varchar(120) DEFAULT NULL,
  PRIMARY KEY (`ID`),
  UNIQUE KEY `UX_Casos_Expediente` (`Numero_Expediente`,`ID_Juzgado`),
  KEY `FK_Casos_Cliente` (`ID_Cliente`),
  KEY `FK_Casos_Juzgado` (`ID_Juzgado`),
  CONSTRAINT `FK_Casos_Cliente` FOREIGN KEY (`ID_Cliente`) REFERENCES `clientes` (`ID`),
  CONSTRAINT `FK_Casos_Juzgado` FOREIGN KEY (`ID_Juzgado`) REFERENCES `juzgados` (`ID`),
  CONSTRAINT `CK_Casos_Estado` CHECK ((`Estado` in (_utf8mb4'Activo',_utf8mb4'Suspendido',_utf8mb4'Cerrado',_utf8mb4'Archivado')))
) ENGINE=InnoDB AUTO_INCREMENT=3 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `casos`
--

LOCK TABLES `casos` WRITE;
/*!40000 ALTER TABLE `casos` DISABLE KEYS */;
INSERT INTO `casos` VALUES (1,1,'123-2026','Civil',6,'Juicio ordinario mercantil, reclamación de créditos','Activo','2026-11-11','Abg. Ana María Pérez');
/*!40000 ALTER TABLE `casos` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `clientes`
--

DROP TABLE IF EXISTS `clientes`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `clientes` (
  `ID` int NOT NULL AUTO_INCREMENT,
  `Nombres` varchar(120) NOT NULL,
  `Apellidos` varchar(120) NOT NULL,
  `DUI` varchar(12) DEFAULT NULL,
  `NIT` varchar(20) DEFAULT NULL,
  `Nacionalidad` varchar(60) DEFAULT NULL,
  `Profesion` varchar(120) DEFAULT NULL,
  `Fecha_Nacimiento` date DEFAULT NULL,
  `Direccion` varchar(300) DEFAULT NULL,
  `Telefono` varchar(40) DEFAULT NULL,
  `Email` varchar(120) DEFAULT NULL,
  `Activo` tinyint(1) NOT NULL DEFAULT '1',
  `Fecha_Registro` datetime NOT NULL DEFAULT CURRENT_TIMESTAMP,
  PRIMARY KEY (`ID`),
  UNIQUE KEY `UX_Clientes_DUI` (`DUI`)
) ENGINE=InnoDB AUTO_INCREMENT=3 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `clientes`
--

LOCK TABLES `clientes` WRITE;
/*!40000 ALTER TABLE `clientes` DISABLE KEYS */;
INSERT INTO `clientes` VALUES (1,'Ana María','Pérez López','00000000-0','0614-010808-100-1','Salvadoreña','Contadora pública','1985-03-14','Col. San Benito, San Salvador','2222-0000','ana.perez@ejemplo.com',1,'2026-11-11 12:13:56');
/*!40000 ALTER TABLE `clientes` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `comparecientes_instrumento`
--

DROP TABLE IF EXISTS `comparecientes_instrumento`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `comparecientes_instrumento` (
  `ID` int NOT NULL AUTO_INCREMENT,
  `ID_Instrumento` int NOT NULL,
  `ID_Cliente` int NOT NULL,
  `Calidad` varchar(30) NOT NULL,
  PRIMARY KEY (`ID`),
  UNIQUE KEY `UQ_Comparecientes` (`ID_Instrumento`,`ID_Cliente`,`Calidad`),
  KEY `FK_Comparecientes_Cliente` (`ID_Cliente`),
  CONSTRAINT `FK_Comparecientes_Cliente` FOREIGN KEY (`ID_Cliente`) REFERENCES `clientes` (`ID`),
  CONSTRAINT `FK_Comparecientes_Instrumento` FOREIGN KEY (`ID_Instrumento`) REFERENCES `instrumentos` (`ID`),
  CONSTRAINT `CK_Comparecientes_Calidad` CHECK ((`Calidad` in (_utf8mb4'Otorgante',_utf8mb4'Favorecido',_utf8mb4'Testigo',_utf8mb4'Intérprete')))
) ENGINE=InnoDB AUTO_INCREMENT=4 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `comparecientes_instrumento`
--

LOCK TABLES `comparecientes_instrumento` WRITE;
/*!40000 ALTER TABLE `comparecientes_instrumento` DISABLE KEYS */;
INSERT INTO `comparecientes_instrumento` VALUES (1,1,1,'Otorgante'),(3,2,1,'Otorgante');
/*!40000 ALTER TABLE `comparecientes_instrumento` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `configuracion`
--

DROP TABLE IF EXISTS `configuracion`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `configuracion` (
  `Clave` varchar(60) NOT NULL,
  `Valor` varchar(500) DEFAULT NULL,
  PRIMARY KEY (`Clave`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `configuracion`
--

LOCK TABLES `configuracion` WRITE;
/*!40000 ALTER TABLE `configuracion` DISABLE KEYS */;
INSERT INTO `configuracion` VALUES ('ARCO_Dias_Prorroga','5'),('ARCO_Dias_Respuesta','10'),('Carpeta_Documentos','Documentos'),('Carpeta_Respaldos','Respaldos'),('LDA_Analisis_Dias','5'),('LDA_Reporte_Dias','15'),('Nombre_Bufete','Bufete Legal y Notarial'),('Protocolo_Dias_Entrega','15'),('Prueba_Clav','Valor 1'),('Umbral_Efectivo','10000'),('Version_Esquema','2.0');
/*!40000 ALTER TABLE `configuracion` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `documentos`
--

DROP TABLE IF EXISTS `documentos`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `documentos` (
  `ID` int NOT NULL AUTO_INCREMENT,
  `Tipo_Documento` varchar(60) NOT NULL,
  `Ruta_Archivo` varchar(500) NOT NULL,
  `Hash` varchar(64) NOT NULL,
  `Estado_Procesamiento_IA` varchar(30) NOT NULL DEFAULT 'No procesado',
  `ID_Caso` int DEFAULT NULL,
  `ID_Instrumento` int DEFAULT NULL,
  `Fecha_Carga` datetime NOT NULL DEFAULT CURRENT_TIMESTAMP,
  `Usuario` varchar(50) DEFAULT NULL,
  PRIMARY KEY (`ID`),
  KEY `FK_Documentos_Caso` (`ID_Caso`),
  KEY `FK_Documentos_Instrumento` (`ID_Instrumento`),
  CONSTRAINT `FK_Documentos_Caso` FOREIGN KEY (`ID_Caso`) REFERENCES `casos` (`ID`),
  CONSTRAINT `FK_Documentos_Instrumento` FOREIGN KEY (`ID_Instrumento`) REFERENCES `instrumentos` (`ID`)
) ENGINE=InnoDB AUTO_INCREMENT=2 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `documentos`
--

LOCK TABLES `documentos` WRITE;
/*!40000 ALTER TABLE `documentos` DISABLE KEYS */;
INSERT INTO `documentos` VALUES (1,'Escritura pública','Documentos\\20261111_121356_1800b993.pdf','b1aa08a9db609cf8ffab43800dde115819a8746a9d93224f5d22917dcab3bc36','No procesado',NULL,1,'2026-11-11 12:13:56','admin');
/*!40000 ALTER TABLE `documentos` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `entidades_legales`
--

DROP TABLE IF EXISTS `entidades_legales`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `entidades_legales` (
  `ID` int NOT NULL AUTO_INCREMENT,
  `Razon_Social` varchar(200) NOT NULL,
  `NIT` varchar(20) DEFAULT NULL,
  `NRC` varchar(30) DEFAULT NULL,
  `Domicilio` varchar(300) DEFAULT NULL,
  `Activo` tinyint(1) NOT NULL DEFAULT '1',
  `Fecha_Registro` datetime NOT NULL DEFAULT CURRENT_TIMESTAMP,
  PRIMARY KEY (`ID`)
) ENGINE=InnoDB AUTO_INCREMENT=2 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `entidades_legales`
--

LOCK TABLES `entidades_legales` WRITE;
/*!40000 ALTER TABLE `entidades_legales` DISABLE KEYS */;
INSERT INTO `entidades_legales` VALUES (1,'Inversiones Cuscatlán S.A. de C.V.','0614-010808-100-2','123456-7','Torre Cuscatlán, San Salvador',1,'2026-11-11 12:13:56');
/*!40000 ALTER TABLE `entidades_legales` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `feriados`
--

DROP TABLE IF EXISTS `feriados`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `feriados` (
  `ID` int NOT NULL AUTO_INCREMENT,
  `Fecha` date NOT NULL,
  `Descripcion` varchar(120) NOT NULL,
  PRIMARY KEY (`ID`),
  UNIQUE KEY `UQ_Feriados_Fecha` (`Fecha`)
) ENGINE=InnoDB AUTO_INCREMENT=38 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `feriados`
--

LOCK TABLES `feriados` WRITE;
/*!40000 ALTER TABLE `feriados` DISABLE KEYS */;
INSERT INTO `feriados` VALUES (1,'2025-01-01','Año Nuevo'),(2,'2025-05-01','Día del Trabajo'),(3,'2025-08-06','Día del Divino Salvador del Mundo'),(4,'2025-09-15','Día de la Independencia'),(5,'2025-11-02','Día de los Difuntos'),(6,'2025-12-25','Navidad'),(7,'2025-04-17','Jueves Santo'),(8,'2025-04-18','Viernes Santo'),(9,'2025-04-19','Sábado Santo'),(10,'2026-01-01','Año Nuevo'),(11,'2026-05-01','Día del Trabajo'),(12,'2026-08-06','Día del Divino Salvador del Mundo'),(13,'2026-09-15','Día de la Independencia'),(14,'2026-11-02','Día de los Difuntos'),(15,'2026-12-25','Navidad'),(16,'2026-04-02','Jueves Santo'),(17,'2026-04-03','Viernes Santo'),(18,'2026-04-04','Sábado Santo'),(19,'2027-01-01','Año Nuevo'),(20,'2027-05-01','Día del Trabajo'),(21,'2027-08-06','Día del Divino Salvador del Mundo'),(22,'2027-09-15','Día de la Independencia'),(23,'2027-11-02','Día de los Difuntos'),(24,'2027-12-25','Navidad'),(25,'2027-03-25','Jueves Santo'),(26,'2027-03-26','Viernes Santo'),(27,'2027-03-27','Sábado Santo'),(28,'2028-01-01','Año Nuevo'),(29,'2028-05-01','Día del Trabajo'),(30,'2028-08-06','Día del Divino Salvador del Mundo'),(31,'2028-09-15','Día de la Independencia'),(32,'2028-11-02','Día de los Difuntos'),(33,'2028-12-25','Navidad'),(34,'2028-04-13','Jueves Santo'),(35,'2028-04-14','Viernes Santo'),(36,'2028-04-15','Sábado Santo');
/*!40000 ALTER TABLE `feriados` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `instrumentos`
--

DROP TABLE IF EXISTS `instrumentos`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `instrumentos` (
  `ID` int NOT NULL AUTO_INCREMENT,
  `ID_Libro` int NOT NULL,
  `Numero_Escritura` int NOT NULL,
  `Fecha_Hora_Otorgamiento` datetime NOT NULL,
  `Folio_Inicio` int NOT NULL,
  `Folio_Fin` int NOT NULL,
  `Objeto_Acto` varchar(500) NOT NULL,
  `Estado` varchar(20) NOT NULL DEFAULT 'Vigente',
  `Fecha_Creacion` datetime NOT NULL DEFAULT CURRENT_TIMESTAMP,
  PRIMARY KEY (`ID`),
  UNIQUE KEY `UQ_Instrumentos_Numero` (`ID_Libro`,`Numero_Escritura`),
  CONSTRAINT `FK_Instrumentos_Libro` FOREIGN KEY (`ID_Libro`) REFERENCES `libros_protocolo` (`ID`),
  CONSTRAINT `CK_Instrumentos_Estado` CHECK ((`Estado` in (_utf8mb4'Vigente',_utf8mb4'Anulado'))),
  CONSTRAINT `CK_Instrumentos_Folios` CHECK (((`Folio_Inicio` >= 1) and (`Folio_Fin` >= `Folio_Inicio`)))
) ENGINE=InnoDB AUTO_INCREMENT=4 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `instrumentos`
--

LOCK TABLES `instrumentos` WRITE;
/*!40000 ALTER TABLE `instrumentos` DISABLE KEYS */;
INSERT INTO `instrumentos` VALUES (1,1,1,'2026-02-10 10:30:00',1,15,'Compraventa de vehículo','Vigente','2026-11-11 12:13:56'),(2,1,2,'2026-03-15 09:00:00',16,30,'Poder general de administración','Anulado','2026-11-11 12:13:56');
/*!40000 ALTER TABLE `instrumentos` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `juzgados`
--

DROP TABLE IF EXISTS `juzgados`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `juzgados` (
  `ID` int NOT NULL AUTO_INCREMENT,
  `Nombre` varchar(180) NOT NULL,
  `Materia` varchar(40) NOT NULL,
  `Distrito` varchar(80) DEFAULT NULL,
  `Activo` tinyint(1) NOT NULL DEFAULT '1',
  PRIMARY KEY (`ID`)
) ENGINE=InnoDB AUTO_INCREMENT=7 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `juzgados`
--

LOCK TABLES `juzgados` WRITE;
/*!40000 ALTER TABLE `juzgados` DISABLE KEYS */;
INSERT INTO `juzgados` VALUES (1,'Juzgado Primero de lo Civil de San Salvador','Civil','San Salvador',1),(2,'Juzgado Segundo de lo Civil de San Salvador','Civil','San Salvador',1),(3,'Juzgado Primero de lo Penal de San Salvador','Penal','San Salvador',1),(4,'Juzgado de lo Laboral de San Salvador','Laboral','San Salvador',1),(5,'Juzgado de Familia de San Salvador','Familia','San Salvador',1),(6,'Cámara Primera de lo Civil de la Primera Sección de Occidente','Civil','Santa Ana',1);
/*!40000 ALTER TABLE `juzgados` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `libros_protocolo`
--

DROP TABLE IF EXISTS `libros_protocolo`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `libros_protocolo` (
  `ID` int NOT NULL AUTO_INCREMENT,
  `Numero_Libro` int NOT NULL,
  `Anio_Vigencia` int NOT NULL,
  `Fecha_Autorizacion` date NOT NULL,
  `Fecha_Vencimiento` date NOT NULL,
  `Estado` varchar(20) NOT NULL DEFAULT 'Vigente',
  `Razon_Cierre` varchar(1000) DEFAULT NULL,
  `Fecha_Cierre` date DEFAULT NULL,
  `Fecha_Entrega_Notariado` date DEFAULT NULL,
  `Indice_Generado` tinyint(1) NOT NULL DEFAULT '0',
  PRIMARY KEY (`ID`),
  UNIQUE KEY `UQ_Libros_Numero` (`Numero_Libro`,`Anio_Vigencia`),
  CONSTRAINT `CK_Libros_Estado` CHECK ((`Estado` in (_utf8mb4'Vigente',_utf8mb4'Cerrado',_utf8mb4'Entregado')))
) ENGINE=InnoDB AUTO_INCREMENT=3 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `libros_protocolo`
--

LOCK TABLES `libros_protocolo` WRITE;
/*!40000 ALTER TABLE `libros_protocolo` DISABLE KEYS */;
INSERT INTO `libros_protocolo` VALUES (1,1,2026,'2026-01-05','2027-01-05','Entregado','Libro completo, 2 instrumentos','2026-11-11','2026-11-11',1);
/*!40000 ALTER TABLE `libros_protocolo` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `operaciones_ldaft`
--

DROP TABLE IF EXISTS `operaciones_ldaft`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `operaciones_ldaft` (
  `ID` int NOT NULL AUTO_INCREMENT,
  `ID_Caso` int DEFAULT NULL,
  `ID_Instrumento` int DEFAULT NULL,
  `Tipo_Operacion` varchar(20) NOT NULL,
  `Monto` decimal(18,2) NOT NULL,
  `Moneda` varchar(5) NOT NULL DEFAULT 'USD',
  `Fecha_Operacion` date NOT NULL,
  `Descripcion` varchar(1000) DEFAULT NULL,
  `Estado` varchar(20) NOT NULL DEFAULT 'Registrada',
  `Fecha_Vencimiento_Analisis` date DEFAULT NULL,
  `Fecha_Vencimiento_Reporte` date DEFAULT NULL,
  `Fecha_Reporte` date DEFAULT NULL,
  `Reportado_UIF` tinyint(1) NOT NULL DEFAULT '0',
  `Observaciones` varchar(500) DEFAULT NULL,
  PRIMARY KEY (`ID`),
  KEY `FK_Operaciones_Caso` (`ID_Caso`),
  KEY `FK_Operaciones_Instrumento` (`ID_Instrumento`),
  CONSTRAINT `FK_Operaciones_Caso` FOREIGN KEY (`ID_Caso`) REFERENCES `casos` (`ID`),
  CONSTRAINT `FK_Operaciones_Instrumento` FOREIGN KEY (`ID_Instrumento`) REFERENCES `instrumentos` (`ID`),
  CONSTRAINT `CK_Operaciones_Estado` CHECK ((`Estado` in (_utf8mb4'Registrada',_utf8mb4'En análisis',_utf8mb4'Reportada',_utf8mb4'Descartada'))),
  CONSTRAINT `CK_Operaciones_Tipo` CHECK ((`Tipo_Operacion` in (_utf8mb4'Regulada',_utf8mb4'Inusual',_utf8mb4'Sospechosa')))
) ENGINE=InnoDB AUTO_INCREMENT=3 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `operaciones_ldaft`
--

LOCK TABLES `operaciones_ldaft` WRITE;
/*!40000 ALTER TABLE `operaciones_ldaft` DISABLE KEYS */;
INSERT INTO `operaciones_ldaft` VALUES (1,NULL,1,'Regulada',12500.00,'USD','2026-11-11','Depósito en efectivo del cliente Ana María Pérez','Reportada','2026-11-18','2026-12-02','2026-11-11',1,NULL),(2,NULL,NULL,'Sospechosa',800.00,'USD','2026-11-11','Estructura de operación extraña','Descartada','2026-11-18','2026-12-02',NULL,0,NULL);
/*!40000 ALTER TABLE `operaciones_ldaft` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `perfil_ldaft`
--

DROP TABLE IF EXISTS `perfil_ldaft`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `perfil_ldaft` (
  `ID` int NOT NULL AUTO_INCREMENT,
  `ID_Cliente` int DEFAULT NULL,
  `ID_Entidad` int DEFAULT NULL,
  `Nivel_Riesgo` varchar(10) NOT NULL,
  `Origen_Fondos_Verificado` tinyint(1) NOT NULL DEFAULT '0',
  `Detalle_Origen_Fondos` varchar(500) DEFAULT NULL,
  `Fecha_Verificacion_Listas` date DEFAULT NULL,
  `Fecha_Actualizacion` datetime NOT NULL DEFAULT CURRENT_TIMESTAMP,
  `Observaciones` varchar(500) DEFAULT NULL,
  PRIMARY KEY (`ID`),
  UNIQUE KEY `UX_Perfil_Cliente` (`ID_Cliente`),
  UNIQUE KEY `UX_Perfil_Entidad` (`ID_Entidad`),
  CONSTRAINT `FK_Perfil_Cliente` FOREIGN KEY (`ID_Cliente`) REFERENCES `clientes` (`ID`),
  CONSTRAINT `FK_Perfil_Entidad` FOREIGN KEY (`ID_Entidad`) REFERENCES `entidades_legales` (`ID`),
  CONSTRAINT `CK_Perfil_Dueno` CHECK (((`ID_Cliente` is not null) or (`ID_Entidad` is not null))),
  CONSTRAINT `CK_Perfil_Riesgo` CHECK ((`Nivel_Riesgo` in (_utf8mb4'Bajo',_utf8mb4'Medio',_utf8mb4'Alto')))
) ENGINE=InnoDB AUTO_INCREMENT=3 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `perfil_ldaft`
--

LOCK TABLES `perfil_ldaft` WRITE;
/*!40000 ALTER TABLE `perfil_ldaft` DISABLE KEYS */;
INSERT INTO `perfil_ldaft` VALUES (1,1,NULL,'Medio',1,'Salario y honorarios profesionales','2026-11-11','2026-11-11 12:13:56',NULL),(2,NULL,1,'Bajo',0,NULL,NULL,'2026-11-11 12:13:56',NULL);
/*!40000 ALTER TABLE `perfil_ldaft` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `permisos`
--

DROP TABLE IF EXISTS `permisos`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `permisos` (
  `Codigo` varchar(60) NOT NULL,
  `Modulo` varchar(40) NOT NULL,
  `Descripcion` varchar(150) NOT NULL,
  PRIMARY KEY (`Codigo`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `permisos`
--

LOCK TABLES `permisos` WRITE;
/*!40000 ALTER TABLE `permisos` DISABLE KEYS */;
INSERT INTO `permisos` VALUES ('auditoria.exportar','Auditoría','Exportar la bitácora a CSV'),('auditoria.ver','Auditoría','Consultar la bitácora'),('casos.confirmar_plazo','Casos','Confirmar plazos procesales'),('casos.cumplir_plazo','Casos','Marcar plazos como cumplidos'),('casos.gestionar','Casos','Crear y editar casos'),('casos.ver','Casos','Consultar casos y plazos'),('clientes.desactivar','Clientes','Desactivar clientes'),('clientes.gestionar','Clientes','Crear y editar clientes'),('clientes.ver','Clientes','Consultar clientes y su perfil LDA/FT'),('config.feriados','Configuración','Administrar feriados'),('config.respaldos','Configuración','Crear respaldos de la base de datos'),('config.roles','Configuración','Administrar roles y permisos'),('config.seguridad','Configuración','Cambiar claves y parámetros de seguridad'),('config.usuarios','Configuración','Administrar usuarios'),('config.ver','Configuración','Acceder a configuración'),('cumplimiento.arco','Cumplimiento','Gestionar solicitudes ARCO'),('cumplimiento.operaciones','Cumplimiento','Registrar y reportar operaciones LDA/FT'),('cumplimiento.reportar_uif','Cumplimiento','Registrar reportes a la UIF'),('cumplimiento.ver','Cumplimiento','Consultar LDA/FT y ARCO'),('documentos.importar','Documentos','Importar documentos'),('documentos.metadatos','Documentos','Editar metadatos de documentos'),('documentos.ver','Documentos','Consultar documentos'),('documentos.verificar','Documentos','Verificar integridad de documentos'),('entidades.gestionar','Entidades','Crear y editar entidades y representantes'),('entidades.ver','Entidades','Consultar entidades legales'),('protocolo.cerrar_libro','Protocolo','Cerrar libros de protocolo'),('protocolo.entregar','Protocolo','Registrar entrega al Notariado'),('protocolo.gestionar','Protocolo','Crear libros, escrituras y comparecientes'),('protocolo.testimonios','Protocolo','Expedir y anular testimonios'),('protocolo.ver','Protocolo','Consultar el protocolo notarial');
/*!40000 ALTER TABLE `permisos` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `plantillas_documentos`
--

DROP TABLE IF EXISTS `plantillas_documentos`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `plantillas_documentos` (
  `ID` int NOT NULL AUTO_INCREMENT,
  `Nombre` varchar(150) NOT NULL,
  `Tipo` varchar(60) NOT NULL,
  `Ruta_Plantilla` varchar(500) DEFAULT NULL,
  `Activa` tinyint(1) NOT NULL DEFAULT '1',
  PRIMARY KEY (`ID`)
) ENGINE=InnoDB AUTO_INCREMENT=4 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `plantillas_documentos`
--

LOCK TABLES `plantillas_documentos` WRITE;
/*!40000 ALTER TABLE `plantillas_documentos` DISABLE KEYS */;
INSERT INTO `plantillas_documentos` VALUES (1,'Compraventa de Vehículo','Escritura',NULL,1),(2,'Poder General','Escritura',NULL,1),(3,'Constitución de Sociedad','Escritura',NULL,1);
/*!40000 ALTER TABLE `plantillas_documentos` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `representantes_legales`
--

DROP TABLE IF EXISTS `representantes_legales`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `representantes_legales` (
  `ID` int NOT NULL AUTO_INCREMENT,
  `ID_Entidad` int NOT NULL,
  `ID_Cliente` int NOT NULL,
  `Cargo` varchar(120) DEFAULT NULL,
  `Tipo_Representacion` varchar(80) DEFAULT NULL,
  PRIMARY KEY (`ID`),
  UNIQUE KEY `UQ_Representantes` (`ID_Entidad`,`ID_Cliente`),
  KEY `FK_Representantes_Cliente` (`ID_Cliente`),
  CONSTRAINT `FK_Representantes_Cliente` FOREIGN KEY (`ID_Cliente`) REFERENCES `clientes` (`ID`),
  CONSTRAINT `FK_Representantes_Entidad` FOREIGN KEY (`ID_Entidad`) REFERENCES `entidades_legales` (`ID`)
) ENGINE=InnoDB AUTO_INCREMENT=3 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `representantes_legales`
--

LOCK TABLES `representantes_legales` WRITE;
/*!40000 ALTER TABLE `representantes_legales` DISABLE KEYS */;
INSERT INTO `representantes_legales` VALUES (1,1,1,'Presidenta de la junta directiva','Judicial y extrajudicial');
/*!40000 ALTER TABLE `representantes_legales` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `rol_permiso`
--

DROP TABLE IF EXISTS `rol_permiso`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `rol_permiso` (
  `ID_Rol` int NOT NULL,
  `Codigo` varchar(60) NOT NULL,
  PRIMARY KEY (`ID_Rol`,`Codigo`),
  KEY `IX_Rol_Permiso_Codigo` (`Codigo`),
  CONSTRAINT `FK_RolPermiso_Permiso` FOREIGN KEY (`Codigo`) REFERENCES `permisos` (`Codigo`) ON DELETE CASCADE,
  CONSTRAINT `FK_RolPermiso_Rol` FOREIGN KEY (`ID_Rol`) REFERENCES `roles` (`ID`) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `rol_permiso`
--

LOCK TABLES `rol_permiso` WRITE;
/*!40000 ALTER TABLE `rol_permiso` DISABLE KEYS */;
INSERT INTO `rol_permiso` VALUES (1,'auditoria.exportar'),(5,'auditoria.exportar'),(1,'auditoria.ver'),(2,'auditoria.ver'),(4,'auditoria.ver'),(5,'auditoria.ver'),(1,'casos.confirmar_plazo'),(2,'casos.confirmar_plazo'),(1,'casos.cumplir_plazo'),(2,'casos.cumplir_plazo'),(3,'casos.cumplir_plazo'),(1,'casos.gestionar'),(2,'casos.gestionar'),(1,'casos.ver'),(2,'casos.ver'),(3,'casos.ver'),(4,'casos.ver'),(5,'casos.ver'),(1,'clientes.desactivar'),(1,'clientes.gestionar'),(2,'clientes.gestionar'),(3,'clientes.gestionar'),(1,'clientes.ver'),(2,'clientes.ver'),(3,'clientes.ver'),(4,'clientes.ver'),(5,'clientes.ver'),(1,'config.feriados'),(1,'config.respaldos'),(1,'config.roles'),(1,'config.seguridad'),(1,'config.usuarios'),(1,'config.ver'),(1,'cumplimiento.arco'),(3,'cumplimiento.arco'),(5,'cumplimiento.arco'),(1,'cumplimiento.operaciones'),(5,'cumplimiento.operaciones'),(1,'cumplimiento.reportar_uif'),(5,'cumplimiento.reportar_uif'),(1,'cumplimiento.ver'),(2,'cumplimiento.ver'),(3,'cumplimiento.ver'),(5,'cumplimiento.ver'),(1,'documentos.importar'),(2,'documentos.importar'),(3,'documentos.importar'),(4,'documentos.importar'),(1,'documentos.metadatos'),(2,'documentos.metadatos'),(4,'documentos.metadatos'),(1,'documentos.ver'),(2,'documentos.ver'),(3,'documentos.ver'),(4,'documentos.ver'),(5,'documentos.ver'),(1,'documentos.verificar'),(2,'documentos.verificar'),(3,'documentos.verificar'),(4,'documentos.verificar'),(1,'entidades.gestionar'),(2,'entidades.gestionar'),(1,'entidades.ver'),(2,'entidades.ver'),(3,'entidades.ver'),(5,'entidades.ver'),(1,'protocolo.cerrar_libro'),(2,'protocolo.cerrar_libro'),(4,'protocolo.cerrar_libro'),(1,'protocolo.entregar'),(4,'protocolo.entregar'),(1,'protocolo.gestionar'),(2,'protocolo.gestionar'),(4,'protocolo.gestionar'),(1,'protocolo.testimonios'),(2,'protocolo.testimonios'),(4,'protocolo.testimonios'),(1,'protocolo.ver'),(2,'protocolo.ver'),(3,'protocolo.ver'),(4,'protocolo.ver');
/*!40000 ALTER TABLE `rol_permiso` ENABLE KEYS */;
UNLOCK TABLES;
/*!50003 SET @saved_cs_client      = @@character_set_client */ ;
/*!50003 SET @saved_cs_results     = @@character_set_results */ ;
/*!50003 SET @saved_col_connection = @@collation_connection */ ;
/*!50003 SET character_set_client  = utf8mb4 */ ;
/*!50003 SET character_set_results = utf8mb4 */ ;
/*!50003 SET collation_connection  = utf8mb4_0900_ai_ci */ ;
/*!50003 SET @saved_sql_mode       = @@sql_mode */ ;
/*!50003 SET sql_mode              = 'ONLY_FULL_GROUP_BY,STRICT_TRANS_TABLES,NO_ZERO_IN_DATE,NO_ZERO_DATE,ERROR_FOR_DIVISION_BY_ZERO,NO_ENGINE_SUBSTITUTION' */ ;
DELIMITER ;;
/*!50003 CREATE*/ /*!50017 DEFINER=`root`@`localhost`*/ /*!50003 TRIGGER `trg_rol_permiso_admin_bd` BEFORE DELETE ON `rol_permiso` FOR EACH ROW BEGIN
    DECLARE v_protegido TINYINT;

    SELECT Protegido INTO v_protegido FROM Roles WHERE ID = OLD.ID_Rol LIMIT 1;

    IF v_protegido = 1 AND OLD.Codigo IN ('config.usuarios', 'config.roles') THEN
        SIGNAL SQLSTATE '45000'
            SET MESSAGE_TEXT = 'El Administrador no puede perder los permisos de Usuarios ni de Roles.';
    END IF;
END */;;
DELIMITER ;
/*!50003 SET sql_mode              = @saved_sql_mode */ ;
/*!50003 SET character_set_client  = @saved_cs_client */ ;
/*!50003 SET character_set_results = @saved_cs_results */ ;
/*!50003 SET collation_connection  = @saved_col_connection */ ;

--
-- Table structure for table `roles`
--

DROP TABLE IF EXISTS `roles`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `roles` (
  `ID` int NOT NULL AUTO_INCREMENT,
  `Nombre` varchar(50) NOT NULL,
  `Descripcion` varchar(200) DEFAULT NULL,
  `Activo` tinyint(1) NOT NULL DEFAULT '1',
  `Protegido` tinyint(1) NOT NULL DEFAULT '0',
  PRIMARY KEY (`ID`),
  UNIQUE KEY `UQ_Roles_Nombre` (`Nombre`)
) ENGINE=InnoDB AUTO_INCREMENT=7 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `roles`
--

LOCK TABLES `roles` WRITE;
/*!40000 ALTER TABLE `roles` DISABLE KEYS */;
INSERT INTO `roles` VALUES (1,'Administrador','Acceso total. Rol protegido del sistema.',1,1),(2,'Abogado','Gestión legal y notarial; sin configuración del sistema.',1,0),(3,'Auxiliar','Apoyo administrativo: consulta y carga de documentos.',1,0),(4,'Notario','Protocolo notarial y documentos.',1,0),(5,'Oficial de Cumplimiento','LDA/FT, derechos ARCO y auditoría.',1,0);
/*!40000 ALTER TABLE `roles` ENABLE KEYS */;
UNLOCK TABLES;
/*!50003 SET @saved_cs_client      = @@character_set_client */ ;
/*!50003 SET @saved_cs_results     = @@character_set_results */ ;
/*!50003 SET @saved_col_connection = @@collation_connection */ ;
/*!50003 SET character_set_client  = utf8mb4 */ ;
/*!50003 SET character_set_results = utf8mb4 */ ;
/*!50003 SET collation_connection  = utf8mb4_0900_ai_ci */ ;
/*!50003 SET @saved_sql_mode       = @@sql_mode */ ;
/*!50003 SET sql_mode              = 'ONLY_FULL_GROUP_BY,STRICT_TRANS_TABLES,NO_ZERO_IN_DATE,NO_ZERO_DATE,ERROR_FOR_DIVISION_BY_ZERO,NO_ENGINE_SUBSTITUTION' */ ;
DELIMITER ;;
/*!50003 CREATE*/ /*!50017 DEFINER=`root`@`localhost`*/ /*!50003 TRIGGER `trg_roles_protegido_bu` BEFORE UPDATE ON `roles` FOR EACH ROW BEGIN
    IF OLD.Protegido = 1 AND (NEW.Protegido = 0 OR NEW.Activo = 0 OR NEW.Nombre <> OLD.Nombre) THEN
        SIGNAL SQLSTATE '45000'
            SET MESSAGE_TEXT = 'El rol Administrador es protegido: no se puede desactivar, renombrar ni desproteger.';
    END IF;
END */;;
DELIMITER ;
/*!50003 SET sql_mode              = @saved_sql_mode */ ;
/*!50003 SET character_set_client  = @saved_cs_client */ ;
/*!50003 SET character_set_results = @saved_cs_results */ ;
/*!50003 SET collation_connection  = @saved_col_connection */ ;
/*!50003 SET @saved_cs_client      = @@character_set_client */ ;
/*!50003 SET @saved_cs_results     = @@character_set_results */ ;
/*!50003 SET @saved_col_connection = @@collation_connection */ ;
/*!50003 SET character_set_client  = utf8mb4 */ ;
/*!50003 SET character_set_results = utf8mb4 */ ;
/*!50003 SET collation_connection  = utf8mb4_0900_ai_ci */ ;
/*!50003 SET @saved_sql_mode       = @@sql_mode */ ;
/*!50003 SET sql_mode              = 'ONLY_FULL_GROUP_BY,STRICT_TRANS_TABLES,NO_ZERO_IN_DATE,NO_ZERO_DATE,ERROR_FOR_DIVISION_BY_ZERO,NO_ENGINE_SUBSTITUTION' */ ;
DELIMITER ;;
/*!50003 CREATE*/ /*!50017 DEFINER=`root`@`localhost`*/ /*!50003 TRIGGER `trg_roles_protegido_bd` BEFORE DELETE ON `roles` FOR EACH ROW BEGIN
    IF OLD.Protegido = 1 THEN
        SIGNAL SQLSTATE '45000'
            SET MESSAGE_TEXT = 'El rol Administrador es protegido y no se puede eliminar.';
    END IF;
END */;;
DELIMITER ;
/*!50003 SET sql_mode              = @saved_sql_mode */ ;
/*!50003 SET character_set_client  = @saved_cs_client */ ;
/*!50003 SET character_set_results = @saved_cs_results */ ;
/*!50003 SET collation_connection  = @saved_col_connection */ ;

--
-- Table structure for table `solicitudes_arco`
--

DROP TABLE IF EXISTS `solicitudes_arco`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `solicitudes_arco` (
  `ID` int NOT NULL AUTO_INCREMENT,
  `ID_Cliente` int NOT NULL,
  `Tipo_Derecho` varchar(30) NOT NULL,
  `Fecha_Solicitud` date NOT NULL,
  `Fecha_Vencimiento` date NOT NULL,
  `Fecha_Vencimiento_Prorroga` date DEFAULT NULL,
  `Prorroga_Aplicada` tinyint(1) NOT NULL DEFAULT '0',
  `Estado` varchar(20) NOT NULL DEFAULT 'Pendiente',
  `Fecha_Respuesta` date DEFAULT NULL,
  `Detalle` varchar(1000) DEFAULT NULL,
  PRIMARY KEY (`ID`),
  KEY `FK_ARCO_Cliente` (`ID_Cliente`),
  CONSTRAINT `FK_ARCO_Cliente` FOREIGN KEY (`ID_Cliente`) REFERENCES `clientes` (`ID`),
  CONSTRAINT `CK_ARCO_Derecho` CHECK ((`Tipo_Derecho` in (_utf8mb4'Acceso',_utf8mb4'Rectificación',_utf8mb4'Cancelación',_utf8mb4'Oposición',_utf8mb4'Portabilidad',_utf8mb4'Olvido'))),
  CONSTRAINT `CK_ARCO_Estado` CHECK ((`Estado` in (_utf8mb4'Pendiente',_utf8mb4'Respondida',_utf8mb4'Vencida')))
) ENGINE=InnoDB AUTO_INCREMENT=3 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `solicitudes_arco`
--

LOCK TABLES `solicitudes_arco` WRITE;
/*!40000 ALTER TABLE `solicitudes_arco` DISABLE KEYS */;
INSERT INTO `solicitudes_arco` VALUES (1,1,'Acceso','2026-11-11','2026-11-25',NULL,0,'Respondida','2026-11-11','Solicitud de copia de datos personales'),(2,1,'Rectificación','2026-11-11','2026-11-25','2026-12-02',1,'Pendiente',NULL,'Solicitud con prórroga');
/*!40000 ALTER TABLE `solicitudes_arco` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `testimonios_expedidos`
--

DROP TABLE IF EXISTS `testimonios_expedidos`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `testimonios_expedidos` (
  `ID` int NOT NULL AUTO_INCREMENT,
  `ID_Instrumento` int NOT NULL,
  `Tipo_Testimonio` varchar(20) NOT NULL,
  `Fecha_Expedicion` date NOT NULL,
  `Destinatario` varchar(200) NOT NULL,
  `Usuario` varchar(50) DEFAULT NULL,
  PRIMARY KEY (`ID`),
  KEY `FK_Testimonios_Instrumento` (`ID_Instrumento`),
  CONSTRAINT `FK_Testimonios_Instrumento` FOREIGN KEY (`ID_Instrumento`) REFERENCES `instrumentos` (`ID`),
  CONSTRAINT `CK_Testimonios_Tipo` CHECK ((`Tipo_Testimonio` in (_utf8mb4'Primer',_utf8mb4'Ulterior')))
) ENGINE=InnoDB AUTO_INCREMENT=2 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `testimonios_expedidos`
--

LOCK TABLES `testimonios_expedidos` WRITE;
/*!40000 ALTER TABLE `testimonios_expedidos` DISABLE KEYS */;
INSERT INTO `testimonios_expedidos` VALUES (1,1,'Primer','2026-11-11','Banco Cuscatlán, S.A.','admin');
/*!40000 ALTER TABLE `testimonios_expedidos` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `tipos_actuacion`
--

DROP TABLE IF EXISTS `tipos_actuacion`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `tipos_actuacion` (
  `ID` int NOT NULL AUTO_INCREMENT,
  `Nombre` varchar(120) NOT NULL,
  `Plazo_Dias_Default` int NOT NULL,
  `Materia` varchar(40) DEFAULT NULL,
  `Activo` tinyint(1) NOT NULL DEFAULT '1',
  PRIMARY KEY (`ID`)
) ENGINE=InnoDB AUTO_INCREMENT=7 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `tipos_actuacion`
--

LOCK TABLES `tipos_actuacion` WRITE;
/*!40000 ALTER TABLE `tipos_actuacion` DISABLE KEYS */;
INSERT INTO `tipos_actuacion` VALUES (1,'Contestación de demanda',15,'Civil',1),(2,'Apelación de sentencia',10,'Civil',1),(3,'Recurso de revocatoria',3,'Civil',1),(4,'Recurso de nulidad',5,'Civil',1),(5,'Incidente',5,'Civil',1),(6,'Otro (verificar plazo legal)',5,'General',1);
/*!40000 ALTER TABLE `tipos_actuacion` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `usuarios`
--

DROP TABLE IF EXISTS `usuarios`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `usuarios` (
  `ID` int NOT NULL AUTO_INCREMENT,
  `Usuario` varchar(50) NOT NULL,
  `Nombre_Completo` varchar(150) NOT NULL,
  `Clave_Hash` varchar(200) NOT NULL,
  `Clave_Salt` varchar(100) NOT NULL,
  `ID_Rol` int DEFAULT NULL,
  `Activo` tinyint(1) NOT NULL DEFAULT '1',
  `Fecha_Creacion` datetime NOT NULL DEFAULT CURRENT_TIMESTAMP,
  PRIMARY KEY (`ID`),
  UNIQUE KEY `UQ_Usuarios_Usuario` (`Usuario`),
  KEY `IX_Usuarios_Rol` (`ID_Rol`),
  CONSTRAINT `FK_Usuarios_Rol` FOREIGN KEY (`ID_Rol`) REFERENCES `roles` (`ID`)
) ENGINE=InnoDB AUTO_INCREMENT=5 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `usuarios`
--

LOCK TABLES `usuarios` WRITE;
/*!40000 ALTER TABLE `usuarios` DISABLE KEYS */;
INSERT INTO `usuarios` VALUES (1,'admin','Administrador del sistema','8aDsHTc+N+Y+ZYcX1vJMcGdYVsqgQBHZkmSvQ6Ec+5I=','E/IGnEJwDOg1FrEvhKALbw==',1,1,'2026-11-11 12:13:41');
/*!40000 ALTER TABLE `usuarios` ENABLE KEYS */;
UNLOCK TABLES;
/*!50003 SET @saved_cs_client      = @@character_set_client */ ;
/*!50003 SET @saved_cs_results     = @@character_set_results */ ;
/*!50003 SET @saved_col_connection = @@collation_connection */ ;
/*!50003 SET character_set_client  = utf8mb4 */ ;
/*!50003 SET character_set_results = utf8mb4 */ ;
/*!50003 SET collation_connection  = utf8mb4_0900_ai_ci */ ;
/*!50003 SET @saved_sql_mode       = @@sql_mode */ ;
/*!50003 SET sql_mode              = 'ONLY_FULL_GROUP_BY,STRICT_TRANS_TABLES,NO_ZERO_IN_DATE,NO_ZERO_DATE,ERROR_FOR_DIVISION_BY_ZERO,NO_ENGINE_SUBSTITUTION' */ ;
DELIMITER ;;
/*!50003 CREATE*/ /*!50017 DEFINER=`root`@`localhost`*/ /*!50003 TRIGGER `trg_usuarios_admin_bu` BEFORE UPDATE ON `usuarios` FOR EACH ROW BEGIN
    DECLARE v_rol_protegido INT;
    DECLARE v_otros INT;

    SELECT ID INTO v_rol_protegido FROM Roles WHERE Protegido = 1 LIMIT 1;

    IF OLD.ID_Rol = v_rol_protegido
       AND (NEW.ID_Rol <> OLD.ID_Rol OR NEW.Activo = 0) THEN
        SELECT COUNT(*) INTO v_otros
        FROM Usuarios
        WHERE ID_Rol = v_rol_protegido AND Activo = 1 AND ID <> OLD.ID;

        IF v_otros = 0 THEN
            SIGNAL SQLSTATE '45000'
                SET MESSAGE_TEXT = 'Debe existir al menos un usuario administrador activo.';
        END IF;
    END IF;
END */;;
DELIMITER ;
/*!50003 SET sql_mode              = @saved_sql_mode */ ;
/*!50003 SET character_set_client  = @saved_cs_client */ ;
/*!50003 SET character_set_results = @saved_cs_results */ ;
/*!50003 SET collation_connection  = @saved_col_connection */ ;
/*!50003 SET @saved_cs_client      = @@character_set_client */ ;
/*!50003 SET @saved_cs_results     = @@character_set_results */ ;
/*!50003 SET @saved_col_connection = @@collation_connection */ ;
/*!50003 SET character_set_client  = utf8mb4 */ ;
/*!50003 SET character_set_results = utf8mb4 */ ;
/*!50003 SET collation_connection  = utf8mb4_0900_ai_ci */ ;
/*!50003 SET @saved_sql_mode       = @@sql_mode */ ;
/*!50003 SET sql_mode              = 'ONLY_FULL_GROUP_BY,STRICT_TRANS_TABLES,NO_ZERO_IN_DATE,NO_ZERO_DATE,ERROR_FOR_DIVISION_BY_ZERO,NO_ENGINE_SUBSTITUTION' */ ;
DELIMITER ;;
/*!50003 CREATE*/ /*!50017 DEFINER=`root`@`localhost`*/ /*!50003 TRIGGER `trg_usuarios_admin_bd` BEFORE DELETE ON `usuarios` FOR EACH ROW BEGIN
    DECLARE v_rol_protegido INT;
    DECLARE v_otros INT;

    SELECT ID INTO v_rol_protegido FROM Roles WHERE Protegido = 1 LIMIT 1;

    IF OLD.ID_Rol = v_rol_protegido THEN
        SELECT COUNT(*) INTO v_otros
        FROM Usuarios
        WHERE ID_Rol = v_rol_protegido AND Activo = 1 AND ID <> OLD.ID;

        IF v_otros = 0 THEN
            SIGNAL SQLSTATE '45000'
                SET MESSAGE_TEXT = 'No se puede eliminar el último usuario administrador activo.';
        END IF;
    END IF;
END */;;
DELIMITER ;
/*!50003 SET sql_mode              = @saved_sql_mode */ ;
/*!50003 SET character_set_client  = @saved_cs_client */ ;
/*!50003 SET character_set_results = @saved_cs_results */ ;
/*!50003 SET collation_connection  = @saved_col_connection */ ;

--
-- Dumping routines for database 'sgln_sv_pruebas'
--
/*!40103 SET TIME_ZONE=@OLD_TIME_ZONE */;

/*!40101 SET SQL_MODE=@OLD_SQL_MODE */;
/*!40014 SET FOREIGN_KEY_CHECKS=@OLD_FOREIGN_KEY_CHECKS */;
/*!40014 SET UNIQUE_CHECKS=@OLD_UNIQUE_CHECKS */;
/*!40101 SET CHARACTER_SET_CLIENT=@OLD_CHARACTER_SET_CLIENT */;
/*!40101 SET CHARACTER_SET_RESULTS=@OLD_CHARACTER_SET_RESULTS */;
/*!40101 SET COLLATION_CONNECTION=@OLD_COLLATION_CONNECTION */;
/*!40111 SET SQL_NOTES=@OLD_SQL_NOTES */;

-- Dump completed on 2026-11-11 12:13:57
