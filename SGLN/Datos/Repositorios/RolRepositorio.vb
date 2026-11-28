Imports System
Imports System.Collections.Generic
Imports System.Data
Imports MySql.Data.MySqlClient

''' <summary>
''' Acceso a datos de roles y su relación con el catálogo de permisos.
''' Los candados del Administrador también existen como triggers en la base:
''' aquí solo se adelantan para dar un mensaje claro al usuario.
''' Las operaciones de rol y su matriz de permisos se confirman en una sola
''' transacción para evitar estados parciales.
''' </summary>
Public Class RolRepositorio
    Inherits RepositorioBase

    Public Function Listar() As DataTable
        If Not Permisos.TieneAcceso("config.roles") Then Return New DataTable()
        Return Consultar(
            "SELECT r.ID, r.Nombre, IFNULL(r.Descripcion, '') AS Descripcion, r.Activo, r.Protegido, " &
            "       (SELECT COUNT(*) FROM Usuarios u WHERE u.ID_Rol = r.ID) AS Usuarios, " &
            "       (SELECT COUNT(*) FROM Rol_Permiso rp WHERE rp.ID_Rol = r.ID) AS Permisos " &
            "FROM Roles r ORDER BY r.Protegido DESC, r.Nombre")
    End Function

    Public Function Obtener(id As Integer) As DataTable
        If Not Permisos.TieneAccesoAlguno("config.roles", "config.usuarios") Then Return New DataTable()
        Return Consultar(
            "SELECT r.ID, r.Nombre, IFNULL(r.Descripcion, '') AS Descripcion, r.Activo, r.Protegido, " &
            "       (SELECT COUNT(*) FROM Usuarios u WHERE u.ID_Rol = r.ID) AS Usuarios, " &
            "       (SELECT COUNT(*) FROM Rol_Permiso rp WHERE rp.ID_Rol = r.ID) AS Permisos " &
            "FROM Roles r WHERE r.ID = @id",
            Sub(comando As MySqlCommand)
                Parametro(comando, "@id", id)
            End Sub)
    End Function

    ''' <summary>Roles activos para el combo de alta/edición de usuarios.</summary>
    Public Function RolesActivos() As DataTable
        If Not Permisos.TieneAccesoAlguno("config.roles", "config.usuarios") Then Return New DataTable()
        Return Consultar(
            "SELECT ID, Nombre FROM Roles WHERE Activo = 1 ORDER BY Protegido DESC, Nombre")
    End Function

    Public Function CatalogoPermisos() As DataTable
        If Not Permisos.TieneAcceso("config.roles") Then Return New DataTable()
        Return Consultar(
            "SELECT Codigo, Modulo, Descripcion FROM Permisos ORDER BY Modulo, Codigo")
    End Function

    Public Function PermisosDeRol(idRol As Integer) As DataTable
        If Not Permisos.TieneAcceso("config.roles") OrElse idRol <= 0 Then Return New DataTable()

        ' La pantalla administrativa también permite reactivar roles inactivos.
        ' Debe recuperar su matriz completa para que una edición no la vacíe
        ' accidentalmente. La carga de sesión conserva el filtro Activo = 1
        ' en CargarPermisosDeRol, que es una ruta distinta y deliberada.
        Return Consultar(
            "SELECT rp.Codigo FROM Roles r " &
            "LEFT JOIN Rol_Permiso rp ON rp.ID_Rol = r.ID " &
            "WHERE r.ID = @rol",
            Sub(comando As MySqlCommand)
                Parametro(comando, "@rol", idRol)
            End Sub)
    End Function

    ''' <summary>
    ''' Lectura interna usada durante la carga de la sesión. No puede llamar a
    ''' TieneAcceso: esa validación vuelve a cargar los permisos y provocaría
    ''' recursión. Permisos.Cargar controla el momento y el identificador del rol.
    ''' </summary>
    Friend Function CargarPermisosDeRol(idRol As Integer) As DataTable
        If idRol <= 0 Then Return New DataTable()
        Return Consultar(
            "SELECT rp.Codigo, r.Protegido FROM Roles r " &
            "LEFT JOIN Rol_Permiso rp ON rp.ID_Rol = r.ID " &
            "WHERE r.ID = @rol AND r.Activo = 1",
            Sub(comando As MySqlCommand)
                Parametro(comando, "@rol", idRol)
            End Sub)
    End Function

    Public Function Crear(nombre As String, descripcion As String,
                          codigos As List(Of String)) As Respuesta
        If Not EsAdministradorAutenticado() Then
            Return Respuesta.Fallo("Solo un administrador puede crear o modificar roles y sus permisos.")
        End If
        If String.IsNullOrWhiteSpace(nombre) OrElse nombre.Trim().Length > 50 Then
            Return Respuesta.Fallo("El nombre del rol debe contener entre 1 y 50 caracteres.")
        End If
        If String.IsNullOrWhiteSpace(descripcion) OrElse descripcion.Trim().Length > 200 Then
            Return Respuesta.Fallo("La descripción del rol debe contener entre 1 y 200 caracteres.")
        End If

        Dim nuevoId As Integer = 0

        Try
            Using conexion As MySqlConnection = CrearConexion()
                Using transaccion As MySqlTransaction = conexion.BeginTransaction()
                    Try
                        Using comando As New MySqlCommand(
                            "INSERT INTO Roles (Nombre, Descripcion, Activo, Protegido) " &
                            "VALUES (@nombre, @descripcion, 1, 0)", conexion, transaccion)
                            Parametro(comando, "@nombre", nombre.Trim())
                            Parametro(comando, "@descripcion", descripcion.Trim())
                            comando.ExecuteNonQuery()
                        End Using

                        Using comandoId As New MySqlCommand("SELECT LAST_INSERT_ID()", conexion, transaccion)
                            nuevoId = Convert.ToInt32(comandoId.ExecuteScalar())
                        End Using

                        SincronizarPermisos(conexion, transaccion, nuevoId, codigos)
                        AuditarEnTransaccion(conexion, transaccion, "CREAR", "Roles", nuevoId, "Rol " & nombre.Trim())
                        transaccion.Commit()
                    Catch
                        Revertir(transaccion)
                        Throw
                    End Try
                End Using
            End Using
 
            Return Respuesta.Ok("Rol creado correctamente.")
        Catch ex As MySqlException When ex.Number = 1062
            Return Respuesta.Fallo("Ya existe un rol con ese nombre.")
        Catch ex As MySqlException When ex.Number = 1644
            Return Respuesta.Fallo(ex.Message)
        Catch ex As MySqlException When ex.Number = 1452
            Return Respuesta.Fallo("Uno de los permisos ya no existe en el catálogo.")
        Catch ex As Exception
            RegistroErrores.Escribir("Roles - crear", ex)
            Return Respuesta.Fallo("No se pudo crear el rol. Verifique la conexión y los datos.")
        End Try
    End Function

    Public Function Actualizar(id As Integer, nombre As String, descripcion As String,
                               activo As Boolean, codigos As List(Of String)) As Respuesta
        If Not EsAdministradorAutenticado() Then
            Return Respuesta.Fallo("Solo un administrador puede crear o modificar roles y sus permisos.")
        End If
        If id <= 0 Then Return Respuesta.Fallo("Seleccione un rol válido.")
        If String.IsNullOrWhiteSpace(nombre) OrElse nombre.Trim().Length > 50 Then
            Return Respuesta.Fallo("El nombre del rol debe contener entre 1 y 50 caracteres.")
        End If
        If String.IsNullOrWhiteSpace(descripcion) OrElse descripcion.Trim().Length > 200 Then
            Return Respuesta.Fallo("La descripción del rol debe contener entre 1 y 200 caracteres.")
        End If

        Try
            Using conexion As MySqlConnection = CrearConexion()
                Using transaccion As MySqlTransaction = conexion.BeginTransaction()
                    Try
                        ' Bloquea la fila y comprueba su existencia dentro de la
                        ' misma transacción que actualizará los permisos.
                        Dim rolProtegido As Boolean = False
                        Dim nombreActual As String = Nothing
                        Dim descripcionActual As String = Nothing
                        Dim activoActual As Boolean = False
                        Dim existeRol As Boolean = False
                        Using comandoExiste As New MySqlCommand(
                            "SELECT Nombre, Descripcion, Activo, Protegido FROM Roles WHERE ID = @id FOR UPDATE",
                            conexion, transaccion)
                            Parametro(comandoExiste, "@id", id)
                            Using lector As MySqlDataReader = comandoExiste.ExecuteReader()
                                If lector.Read() Then
                                    existeRol = True
                                    nombreActual = Convert.ToString(lector("Nombre"))
                                    descripcionActual = Convert.ToString(lector("Descripcion"))
                                    activoActual = Convert.ToInt32(lector("Activo")) = 1
                                    rolProtegido = Convert.ToInt32(lector("Protegido")) = 1
                                End If
                            End Using
                        End Using
                        If Not existeRol Then
                            Revertir(transaccion)
                            Return Respuesta.Fallo("El rol ya no existe.")
                        End If

                        If rolProtegido AndAlso Not activo Then
                            Throw New InvalidOperationException("El rol Administrador protegido no se puede desactivar.")
                        End If
                        If rolProtegido AndAlso
                           Not String.Equals(nombreActual, nombre.Trim(), StringComparison.Ordinal) Then
                            Throw New InvalidOperationException("El nombre del rol Administrador protegido no se puede modificar.")
                        End If
                        If Not activo Then
                            Using comandoUsuarios As New MySqlCommand(
                                "SELECT COUNT(*) FROM Usuarios WHERE ID_Rol = @id", conexion, transaccion)
                                Parametro(comandoUsuarios, "@id", id)
                                If Convert.ToInt32(comandoUsuarios.ExecuteScalar()) > 0 Then
                                    Throw New InvalidOperationException("No se puede desactivar un rol con usuarios asignados.")
                                End If
                            End Using
                        End If

                        Using comando As New MySqlCommand(
                            "UPDATE Roles SET Nombre = @nombre, Descripcion = @descripcion, Activo = @activo " &
                            "WHERE ID = @id", conexion, transaccion)
                            Parametro(comando, "@nombre", nombre.Trim())
                            Parametro(comando, "@descripcion", descripcion)
                            Parametro(comando, "@activo", activo)
                            Parametro(comando, "@id", id)
                            comando.ExecuteNonQuery()
                        End Using

                        Dim permisosModificados As Boolean = SincronizarPermisos(conexion, transaccion, id, codigos)
                        Dim datosModificados As Boolean =
                            Not String.Equals(nombreActual, nombre.Trim(), StringComparison.Ordinal) OrElse
                            Not String.Equals(descripcionActual, descripcion.Trim(), StringComparison.Ordinal) OrElse
                            activoActual <> activo
                        If datosModificados OrElse permisosModificados Then
                            AuditarEnTransaccion(conexion, transaccion, "MODIFICAR", "Roles", id,
                                "Rol " & nombre.Trim())
                        End If
                        transaccion.Commit()
                    Catch
                        Revertir(transaccion)
                        Throw
                    End Try
                End Using
            End Using
 
            Return Respuesta.Ok("Rol actualizado.")
        Catch ex As MySqlException When ex.Number = 1062
            Return Respuesta.Fallo("Ya existe un rol con ese nombre.")
        Catch ex As MySqlException When ex.Number = 1644
            Return Respuesta.Fallo(ex.Message)
        Catch ex As MySqlException When ex.Number = 1452
            Return Respuesta.Fallo("Uno de los permisos ya no existe en el catálogo.")
        Catch ex As InvalidOperationException
            Return Respuesta.Fallo(ex.Message)
        Catch ex As Exception
            RegistroErrores.Escribir("Roles - actualizar", ex)
            Return Respuesta.Fallo("No se pudo actualizar el rol. Verifique la conexión y los datos.")
        End Try
    End Function

    Public Function Eliminar(id As Integer) As Respuesta
        If Not EsAdministradorAutenticado() Then
            Return Respuesta.Fallo("Solo un administrador puede eliminar roles.")
        End If

        If id <= 0 Then Return Respuesta.Fallo("Seleccione un rol válido.")
        Try
            Dim filas As Integer = EnTransaccion(
                Function(conexion As MySqlConnection, transaccion As MySqlTransaction) As Integer
                    Dim protegido As Object
                    Using comandoExiste As New MySqlCommand(
                        "SELECT Protegido FROM Roles WHERE ID = @id FOR UPDATE", conexion, transaccion)
                        Parametro(comandoExiste, "@id", id)
                        protegido = comandoExiste.ExecuteScalar()
                    End Using
                    If protegido Is Nothing OrElse protegido Is DBNull.Value Then Return 0
                    If Convert.ToInt32(protegido) = 1 Then Return -1

                    Using comandoUsuarios As New MySqlCommand(
                        "SELECT COUNT(*) FROM Usuarios WHERE ID_Rol = @id", conexion, transaccion)
                        Parametro(comandoUsuarios, "@id", id)
                        If Convert.ToInt32(comandoUsuarios.ExecuteScalar()) > 0 Then Return -2
                    End Using

                    Dim afectadas As Integer = Ejecutar(conexion, transaccion,
                        "DELETE FROM Roles WHERE ID = @id",
                        Sub(comando As MySqlCommand)
                            Parametro(comando, "@id", id)
                        End Sub)
                    If afectadas > 0 Then
                        AuditarEnTransaccion(conexion, transaccion, "ELIMINAR", "Roles", id, "Rol eliminado")
                    End If
                    Return afectadas
                End Function)

            If filas = 0 Then Return Respuesta.Fallo("El rol ya no existe.")
            If filas = -1 Then Return Respuesta.Fallo("El rol Administrador protegido no se puede eliminar.")
            If filas = -2 Then Return Respuesta.Fallo("No se puede eliminar un rol con usuarios asignados.")
            Return Respuesta.Ok("Rol eliminado.")
        Catch ex As MySqlException When ex.Number = 1644
            Return Respuesta.Fallo(ex.Message)
        Catch ex As MySqlException When ex.Number = 1451
            Return Respuesta.Fallo("No se puede borrar el rol porque tiene usuarios asignados. " &
                                   "Cámbieles el rol o desactívelos primero.")
        Catch ex As Exception
            RegistroErrores.Escribir("Roles - eliminar", ex)
            Return Respuesta.Fallo("No se pudo eliminar el rol. Verifique la conexión y sus asignaciones.")
        End Try
    End Function

    ''' <summary>
    ''' Sincroniza la matriz de permisos del rol en una transacción propia.
    ''' Se conserva este método público para operaciones puntuales y pruebas.
    ''' </summary>
    Public Function GuardarPermisos(idRol As Integer, codigos As List(Of String)) As Respuesta
        If Not EsAdministradorAutenticado() Then
            Return Respuesta.Fallo("Solo un administrador puede modificar los permisos de roles.")
        End If
        If idRol <= 0 Then Return Respuesta.Fallo("Seleccione un rol válido.")

        Try
            Using conexion As MySqlConnection = CrearConexion()
                Using transaccion As MySqlTransaction = conexion.BeginTransaction()
                    Try
                        Dim existeRol As Boolean
                        Using comandoExiste As New MySqlCommand(
                            "SELECT ID FROM Roles WHERE ID = @id FOR UPDATE", conexion, transaccion)
                            Parametro(comandoExiste, "@id", idRol)
                            existeRol = comandoExiste.ExecuteScalar() IsNot Nothing
                        End Using
                        If Not existeRol Then
                            Revertir(transaccion)
                            Return Respuesta.Fallo("El rol ya no existe.")
                        End If

                        Dim permisosModificados As Boolean = SincronizarPermisos(conexion, transaccion, idRol, codigos)
                        If permisosModificados Then
                            AuditarEnTransaccion(conexion, transaccion, "MODIFICAR PERMISOS", "Roles", idRol,
                                "Matriz de permisos actualizada")
                        End If
                        transaccion.Commit()
                    Catch
                        Revertir(transaccion)
                        Throw
                    End Try
                End Using
            End Using

            Return Respuesta.Ok("Permisos del rol guardados.")
        Catch ex As MySqlException When ex.Number = 1644
            Return Respuesta.Fallo(ex.Message)
        Catch ex As MySqlException When ex.Number = 1452
            Return Respuesta.Fallo("Uno de los permisos ya no existe en el catálogo.")
        Catch ex As Exception
            RegistroErrores.Escribir("Roles - guardar permisos", ex)
            Return Respuesta.Fallo("No se pudieron guardar los permisos. Verifique la conexión y los datos.")
        End Try
    End Function

    ''' <summary>
    ''' Sincroniza permisos usando la conexión y transacción del alta/edición.
    ''' Solo elimina los códigos retirados, respetando los triggers del rol
    ''' Administrador. Cualquier error revierte también el cambio del rol.
    ''' </summary>
    Private Function SincronizarPermisos(conexion As MySqlConnection,
                                          transaccion As MySqlTransaction,
                                          idRol As Integer,
                                          codigos As List(Of String)) As Boolean

        Dim nuevos As New HashSet(Of String)(StringComparer.OrdinalIgnoreCase)
        If codigos IsNot Nothing Then
            For Each codigo As String In codigos
                If Not String.IsNullOrWhiteSpace(codigo) Then nuevos.Add(codigo.Trim())
            Next
        End If

        ' Defensa en profundidad: cualquier ruta de escritura (incluida
        ' GuardarPermisos) debe conservar los permisos mínimos del rol protegido.
        Dim esProtegido As Boolean = False
        Using comandoRol As New MySqlCommand(
            "SELECT Protegido FROM Roles WHERE ID = @rol FOR UPDATE", conexion, transaccion)
            Parametro(comandoRol, "@rol", idRol)
            Dim valor As Object = comandoRol.ExecuteScalar()
            esProtegido = valor IsNot Nothing AndAlso Not Convert.IsDBNull(valor) AndAlso
                          Convert.ToInt32(valor) = 1
        End Using
        If esProtegido AndAlso
           (Not nuevos.Contains("config.usuarios") OrElse Not nuevos.Contains("config.roles")) Then
            Throw New InvalidOperationException(
                "El rol Administrador protegido debe conservar los permisos 'config.usuarios' y 'config.roles'.")
        End If

        Dim actuales As New List(Of String)()
        Using comando As New MySqlCommand(
            "SELECT Codigo FROM Rol_Permiso WHERE ID_Rol = @rol", conexion, transaccion)
            Parametro(comando, "@rol", idRol)
            Using lector As MySqlDataReader = comando.ExecuteReader()
                While lector.Read()
                    actuales.Add(lector.GetString(0))
                End While
            End Using
        End Using

        Dim conjuntoActuales As New HashSet(Of String)(actuales, StringComparer.OrdinalIgnoreCase)
        Dim huboCambios As Boolean = False

        For Each codigo As String In actuales
            If nuevos.Contains(codigo) Then Continue For

            Using comando As New MySqlCommand(
                "DELETE FROM Rol_Permiso WHERE ID_Rol = @rol AND Codigo = @codigo",
                conexion, transaccion)
                Parametro(comando, "@rol", idRol)
                Parametro(comando, "@codigo", codigo)
                If comando.ExecuteNonQuery() > 0 Then huboCambios = True
            End Using
        Next

        For Each codigo As String In nuevos
            ' Los permisos que ya estaban asignados se conservan; insertarlos
            ' otra vez provocaba una violación de clave única al editar un rol.
            If conjuntoActuales.Contains(codigo) Then Continue For

            Using comando As New MySqlCommand(
                "INSERT INTO Rol_Permiso (ID_Rol, Codigo) VALUES (@rol, @codigo)",
                conexion, transaccion)
                Parametro(comando, "@rol", idRol)
                Parametro(comando, "@codigo", codigo)
                If comando.ExecuteNonQuery() > 0 Then huboCambios = True
            End Using
        Next

        Return huboCambios
    End Function

    Private Function EsAdministradorAutenticado() As Boolean
        If Sesion.UsuarioActual Is Nothing Then Return False
        If Not Permisos.TieneAcceso("config.roles") Then Return False
        Return Sesion.UsuarioActual.RolProtegido AndAlso Permisos.AccesoTotal
    End Function

    Private Shared Sub Revertir(transaccion As MySqlTransaction)
        Try
            transaccion.Rollback()
        Catch
            ' Se conserva la excepción original si MySQL ya revirtió o cerró
            ' la transacción al producirse el error.
        End Try
    End Sub

End Class
