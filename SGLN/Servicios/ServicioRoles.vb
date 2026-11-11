Imports System
Imports System.Collections.Generic
Imports System.Data
Imports System.Text.RegularExpressions

''' <summary>
''' Reglas de negocio de roles y permisos. Se pueden crear roles nuevos, pero
''' el rol protegido (Administrador) no se puede eliminar, desactivar ni
''' renombrar, y siempre conserva los permisos de Usuarios y Roles.
''' </summary>
Public Class ServicioRoles

    Private ReadOnly _repositorio As New RolRepositorio()

    ' Sin estos dos permisos el administrador no podría volver a entrar a
    ' mantener la seguridad del sistema.
    Private Shared ReadOnly PermisosObligatorios As String() = {"config.usuarios", "config.roles"}

    Public Function Listar() As DataTable
        Return _repositorio.Listar()
    End Function

    Public Function Obtener(id As Integer) As DataTable
        Return _repositorio.Obtener(id)
    End Function

    Public Function CatalogoPermisos() As DataTable
        Return _repositorio.CatalogoPermisos()
    End Function

    Public Function PermisosDeRol(idRol As Integer) As List(Of String)
        Dim lista As New List(Of String)()
        For Each fila As DataRow In _repositorio.PermisosDeRol(idRol).Rows
            lista.Add(Convert.ToString(fila("Codigo")))
        Next
        Return lista
    End Function

    Public Function Crear(nombre As String, descripcion As String, codigos As List(Of String)) As Respuesta
        Dim errorNombre As String = ValidarNombre(nombre, 0)
        If errorNombre <> "" Then Return Respuesta.Fallo(errorNombre)

        If String.IsNullOrWhiteSpace(descripcion) Then
            Return Respuesta.Fallo("Indique una descripción breve del rol.")
        End If
        If descripcion.Trim().Length > 200 Then
            Return Respuesta.Fallo("La descripción no puede superar 200 caracteres.")
        End If

        Return _repositorio.Crear(nombre, descripcion.Trim(), NormalizarCodigos(codigos))
    End Function

    Public Function Actualizar(id As Integer, nombre As String, descripcion As String,
                               activo As Boolean, codigos As List(Of String)) As Respuesta

        Dim actual As DataTable = _repositorio.Obtener(id)
        If actual.Rows.Count = 0 Then Return Respuesta.Fallo("El rol ya no existe.")

        Dim fila As DataRow = actual.Rows(0)
        Dim protegido As Boolean = Convert.ToInt32(fila("Protegido")) = 1
        Dim nombreActual As String = Convert.ToString(fila("Nombre"))
        Dim permisos As List(Of String) = NormalizarCodigos(codigos)

        If protegido Then
            ' El rol administrador solo puede cambiar su descripción y sus
            ' permisos; nunca el nombre, el estado ni los permisos mínimos.
            nombre = nombreActual
            activo = True

            For Each obligatorio As String In PermisosObligatorios
                If Not permisos.Contains(obligatorio) Then
                    Return Respuesta.Fallo(
                        "El rol " & nombreActual & " no puede perder los permisos 'config.usuarios' ni 'config.roles'.")
                End If
            Next
        Else
            Dim errorNombre As String = ValidarNombre(nombre, id)
            If errorNombre <> "" Then Return Respuesta.Fallo(errorNombre)

            If String.IsNullOrWhiteSpace(descripcion) Then
                Return Respuesta.Fallo("Indique una descripción breve del rol.")
            End If

            If Not activo AndAlso Convert.ToInt32(fila("Usuarios")) > 0 Then
                Return Respuesta.Fallo(
                    "No puede desactivar un rol con usuarios asignados. " &
                    "Cámbieles el rol primero.")
            End If
        End If

        Return _repositorio.Actualizar(id, nombre, descripcion.Trim(), activo, permisos)
    End Function

    Public Function Eliminar(id As Integer) As Respuesta
        Dim actual As DataTable = _repositorio.Obtener(id)
        If actual.Rows.Count = 0 Then Return Respuesta.Fallo("El rol ya no existe.")

        If Convert.ToInt32(actual.Rows(0)("Protegido")) = 1 Then
            Return Respuesta.Fallo("El rol " & Convert.ToString(actual.Rows(0)("Nombre")) &
                                   " es el rol administrador del sistema y no se puede eliminar.")
        End If

        If Convert.ToInt32(actual.Rows(0)("Usuarios")) > 0 Then
            Return Respuesta.Fallo("El rol tiene usuarios asignados. Cámbieles el rol antes de eliminarlo.")
        End If

        Return _repositorio.Eliminar(id)
    End Function

    Private Function ValidarNombre(nombre As String, idExcluir As Integer) As String
        If String.IsNullOrWhiteSpace(nombre) Then Return "Indique el nombre del rol."
        If nombre.Trim().Length > 50 Then Return "El nombre del rol no puede superar 50 caracteres."

        Dim repetido As Integer = 0
        For Each fila As DataRow In _repositorio.Listar().Rows
            If Convert.ToInt32(fila("ID")) = idExcluir Then Continue For
            If String.Equals(Convert.ToString(fila("Nombre")).Trim(), nombre.Trim(), StringComparison.OrdinalIgnoreCase) Then
                repetido += 1
            End If
        Next

        If repetido > 0 Then Return "Ya existe un rol con ese nombre."
        Return ""
    End Function

    ''' <summary>
    ''' Descarta códigos que no existan en el catálogo para no dejar basura en
    ''' Rol_Permiso (la clave foránea lo impediría de todos modos).
    ''' </summary>
    Private Function NormalizarCodigos(codigos As List(Of String)) As List(Of String)
        Dim validos As New HashSet(Of String)(StringComparer.OrdinalIgnoreCase)
        For Each fila As DataRow In _repositorio.CatalogoPermisos().Rows
            validos.Add(Convert.ToString(fila("Codigo")))
        Next

        Dim resultado As New List(Of String)()
        If codigos Is Nothing Then Return resultado

        For Each codigo As String In codigos
            If validos.Contains(codigo) AndAlso Not resultado.Contains(codigo) Then
                resultado.Add(codigo)
            End If
        Next
        Return resultado
    End Function

End Class
