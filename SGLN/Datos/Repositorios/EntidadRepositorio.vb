Imports System
Imports System.Data
Imports MySql.Data.MySqlClient

''' <summary>
''' Acceso a datos de entidades legales, representantes y perfil LDA/FT.
''' </summary>
Public Class EntidadRepositorio
    Inherits RepositorioBase

    Public Function Buscar(filtro As String) As DataTable
        Dim texto As String = If(filtro, "").Trim()
        Return Consultar(
            "SELECT e.ID, e.Razon_Social, e.NIT, e.NRC, " &
            "       IFNULL(p.Nivel_Riesgo, 'Sin perfil') AS Riesgo, e.Activo " &
            "FROM Entidades_Legales e " &
            "LEFT JOIN Perfil_LDAFT p ON p.ID_Entidad = e.ID " &
            "WHERE (@filtro IS NULL OR e.Razon_Social LIKE @like OR e.NIT LIKE @like OR e.NRC LIKE @like) " &
            "ORDER BY e.Razon_Social",
            Sub(comando As MySqlCommand)
                Parametro(comando, "@filtro", texto)
                Parametro(comando, "@like", "%" & texto & "%")
            End Sub)
    End Function

    Public Function Obtener(id As Integer) As DataTable
        Return Consultar(
            "SELECT e.*, p.ID AS ID_Perfil, p.Nivel_Riesgo, p.Origen_Fondos_Verificado, " &
            "       p.Detalle_Origen_Fondos, p.Fecha_Verificacion_Listas, p.Observaciones AS Observaciones_LDAFT " &
            "FROM Entidades_Legales e " &
            "LEFT JOIN Perfil_LDAFT p ON p.ID_Entidad = e.ID " &
            "WHERE e.ID = @id",
            Sub(comando As MySqlCommand)
                Parametro(comando, "@id", id)
            End Sub)
    End Function

    Public Function Insertar(entidad As EntidadLegal) As Integer
        Dim nuevoId As Integer = InsertarYObtenerId(
            "INSERT INTO Entidades_Legales (Razon_Social, NIT, NRC, Domicilio, Activo) " &
            "VALUES (@razon, @nit, @nrc, @domicilio, @activo)",
            Sub(comando As MySqlCommand)
                AgregarParametros(comando, entidad)
            End Sub)

        Auditar("CREAR", "Entidades_Legales", nuevoId, entidad.RazonSocial)
        Return nuevoId
    End Function

    Public Sub Actualizar(entidad As EntidadLegal)
        Dim filas As Integer = Ejecutar(
            "UPDATE Entidades_Legales SET Razon_Social = @razon, NIT = @nit, NRC = @nrc, " &
            "       Domicilio = @domicilio, Activo = @activo WHERE ID = @id",
            Sub(comando As MySqlCommand)
                AgregarParametros(comando, entidad)
                Parametro(comando, "@id", entidad.ID)
            End Sub)

        If filas > 0 Then Auditar("MODIFICAR", "Entidades_Legales", entidad.ID, entidad.RazonSocial)
    End Sub

    Public Sub Desactivar(id As Integer)
        Dim filas As Integer = Ejecutar(
            "UPDATE Entidades_Legales SET Activo = 0 WHERE ID = @id",
            Sub(comando As MySqlCommand)
                Parametro(comando, "@id", id)
            End Sub)

        If filas > 0 Then Auditar("DESACTIVAR", "Entidades_Legales", id, "Entidad desactivada")
    End Sub

    Public Sub GuardarPerfil(perfil As PerfilLdaft)
        ' Un solo perfil por entidad: el duplicado se actualiza.
        Ejecutar(
            "INSERT INTO Perfil_LDAFT (ID_Entidad, Nivel_Riesgo, Origen_Fondos_Verificado, " &
            "       Detalle_Origen_Fondos, Fecha_Verificacion_Listas, Observaciones) " &
            "VALUES (@idEntidad, @riesgo, @origen, @detalle, @fecha, @observaciones) " &
            "ON DUPLICATE KEY UPDATE Nivel_Riesgo = VALUES(Nivel_Riesgo), " &
            "       Origen_Fondos_Verificado = VALUES(Origen_Fondos_Verificado), " &
            "       Detalle_Origen_Fondos = VALUES(Detalle_Origen_Fondos), " &
            "       Fecha_Verificacion_Listas = VALUES(Fecha_Verificacion_Listas), " &
            "       Fecha_Actualizacion = NOW(), " &
            "       Observaciones = VALUES(Observaciones)",
            Sub(comando As MySqlCommand)
                Parametro(comando, "@idEntidad", perfil.IDEntidad)
                Parametro(comando, "@riesgo", perfil.NivelRiesgo)
                Parametro(comando, "@origen", perfil.OrigenFondosVerificado)
                Parametro(comando, "@detalle", perfil.DetalleOrigenFondos)
                Parametro(comando, "@fecha", If(perfil.FechaVerificacionListas.HasValue, CObj(perfil.FechaVerificacionListas.Value), DBNull.Value))
                Parametro(comando, "@observaciones", perfil.Observaciones)
            End Sub)

        Auditar("MODIFICAR", "Perfil_LDAFT", perfil.IDEntidad, "Perfil LDA/FT de entidad actualizado")
    End Sub

    Public Function Representantes(idEntidad As Integer) As DataTable
        Return Consultar(
            "SELECT r.ID, c.ID AS ID_Cliente, CONCAT(c.Nombres, ' ', c.Apellidos) AS Nombre, " &
            "       r.Cargo, r.Tipo_Representacion " &
            "FROM Representantes_Legales r " &
            "INNER JOIN Clientes c ON c.ID = r.ID_Cliente " &
            "WHERE r.ID_Entidad = @id ORDER BY c.Apellidos, c.Nombres",
            Sub(comando As MySqlCommand)
                Parametro(comando, "@id", idEntidad)
            End Sub)
    End Function

    Public Function ClientesCombo() As DataTable
        Return Consultar(
            "SELECT ID, CONCAT(Nombres, ' ', Apellidos) AS Nombre FROM Clientes " &
            "WHERE Activo = 1 ORDER BY Apellidos, Nombres")
    End Function

    Public Function AgregarRepresentante(idEntidad As Integer, idCliente As Integer, cargo As String, tipo As String) As Respuesta
        Try
            Ejecutar(
                "INSERT INTO Representantes_Legales (ID_Entidad, ID_Cliente, Cargo, Tipo_Representacion) " &
                "VALUES (@entidad, @cliente, @cargo, @tipo)",
                Sub(comando As MySqlCommand)
                    Parametro(comando, "@entidad", idEntidad)
                    Parametro(comando, "@cliente", idCliente)
                    Parametro(comando, "@cargo", cargo)
                    Parametro(comando, "@tipo", tipo)
                End Sub)

            Auditar("CREAR", "Representantes_Legales", Nothing, "Representante agregado a entidad " & idEntidad)
            Return Respuesta.Ok("Representante agregado.")
        Catch ex As MySqlException When ex.Number = 1062
            ' 1062 = entrada duplicada (equivale a 2601/2627 de SQL Server).
            Return Respuesta.Fallo("Ese cliente ya está registrado como representante de la entidad.")
        End Try
    End Function

    Public Sub QuitarRepresentante(id As Integer)
        Dim filas As Integer = Ejecutar(
            "DELETE FROM Representantes_Legales WHERE ID = @id",
            Sub(comando As MySqlCommand)
                Parametro(comando, "@id", id)
            End Sub)

        If filas > 0 Then Auditar("ELIMINAR", "Representantes_Legales", id, "Representante removido")
    End Sub

    Private Sub AgregarParametros(comando As MySqlCommand, entidad As EntidadLegal)
        Parametro(comando, "@razon", entidad.RazonSocial)
        Parametro(comando, "@nit", entidad.NIT)
        Parametro(comando, "@nrc", entidad.NRC)
        Parametro(comando, "@domicilio", entidad.Domicilio)
        Parametro(comando, "@activo", entidad.Activo)
    End Sub

End Class
