Imports System
Imports System.Data
Imports MySql.Data.MySqlClient

''' <summary>
''' Base común de los repositorios. Centraliza la apertura de conexiones,
''' la ejecución parametrizada de SQL, la obtención del último ID insertado
''' y el registro de auditoría.
''' </summary>
Public MustInherit Class RepositorioBase

    Protected Function CrearConexion() As MySqlConnection
        Dim conexion As New MySqlConnection(ConexionBD.CadenaConexion)
        conexion.Open()
        Return conexion
    End Function

    Protected Function Consultar(consulta As String, Optional parametros As Action(Of MySqlCommand) = Nothing) As DataTable
        Dim tabla As New DataTable()
        Using conexion As MySqlConnection = CrearConexion()
            Using comando As New MySqlCommand(consulta, conexion)
                If parametros IsNot Nothing Then parametros(comando)
                Using adaptador As New MySqlDataAdapter(comando)
                    adaptador.Fill(tabla)
                End Using
            End Using
        End Using
        Return tabla
    End Function

    Protected Function Ejecutar(consulta As String, Optional parametros As Action(Of MySqlCommand) = Nothing) As Integer
        Using conexion As MySqlConnection = CrearConexion()
            Using comando As New MySqlCommand(consulta, conexion)
                If parametros IsNot Nothing Then parametros(comando)
                Return comando.ExecuteNonQuery()
            End Using
        End Using
    End Function

    Protected Function Escalar(Of T)(consulta As String, Optional parametros As Action(Of MySqlCommand) = Nothing) As T
        Using conexion As MySqlConnection = CrearConexion()
            Using comando As New MySqlCommand(consulta, conexion)
                If parametros IsNot Nothing Then parametros(comando)
                Dim valor As Object = comando.ExecuteScalar()
                If valor Is Nothing OrElse Convert.IsDBNull(valor) Then Return Nothing
                Return DirectCast(Convert.ChangeType(valor, GetType(T)), T)
            End Using
        End Using
    End Function

    ''' <summary>
    ''' Inserta una fila y devuelve su ID. MySQL no tiene "OUTPUT INSERTED":
    ''' el INSERT y LAST_INSERT_ID() deben ejecutarse en la MISMA conexión.
    ''' </summary>
    Protected Function InsertarYObtenerId(consulta As String,
                                          Optional parametros As Action(Of MySqlCommand) = Nothing) As Integer
        Using conexion As MySqlConnection = CrearConexion()
            Using comando As New MySqlCommand(consulta, conexion)
                If parametros IsNot Nothing Then parametros(comando)
                comando.ExecuteNonQuery()
            End Using

            Using comandoId As New MySqlCommand("SELECT LAST_INSERT_ID()", conexion)
                Return Convert.ToInt32(comandoId.ExecuteScalar())
            End Using
        End Using
    End Function

    ''' <summary>
    ''' Convierte valores vacíos o Nothing en DBNull para las consultas.
    ''' </summary>
    Protected Shared Function ValorBD(valor As Object) As Object
        If valor Is Nothing Then Return DBNull.Value
        If TypeOf valor Is String AndAlso String.IsNullOrWhiteSpace(DirectCast(valor, String)) Then Return DBNull.Value
        If TypeOf valor Is Boolean Then Return If(DirectCast(valor, Boolean), 1, 0)
        Return valor
    End Function

    ''' <summary>
    ''' Agrega un parámetro con el tipo MySQL adecuado al valor. Se evita
    ''' AddWithValue porque con Nothing o DBNull infiere un tipo que puede
    ''' chocar con columnas numéricas o de fecha.
    ''' Por eso las consultas con filtros opcionales comparan con IS NULL.
    ''' </summary>
    Protected Shared Sub Parametro(comando As MySqlCommand, nombre As String, valor As Object)
        comando.Parameters.Add(New MySqlParameter(nombre, TipoMySQL(valor)) With {
            .Value = ValorBD(valor)
        })
    End Sub

    Private Shared Function TipoMySQL(valor As Object) As MySqlDbType
        If valor Is Nothing OrElse TypeOf valor Is DBNull Then Return MySqlDbType.VarString
        If TypeOf valor Is Boolean Then Return MySqlDbType.Byte
        If TypeOf valor Is Integer OrElse TypeOf valor Is Short Then Return MySqlDbType.Int32
        If TypeOf valor Is Long Then Return MySqlDbType.Int64
        If TypeOf valor Is Decimal Then Return MySqlDbType.Decimal
        If TypeOf valor Is Double OrElse TypeOf valor Is Single Then Return MySqlDbType.Double
        If TypeOf valor Is Date Then Return MySqlDbType.DateTime
        If TypeOf valor Is Byte() Then Return MySqlDbType.Blob
        Return MySqlDbType.VarString
    End Function

    Protected Sub Auditar(accion As String, tabla As String, idRegistro As Integer?, detalle As String)
        Auditar(Sesion.NombreUsuario, accion, tabla, idRegistro, detalle)
    End Sub

    Protected Sub Auditar(usuario As String, accion As String, tabla As String, idRegistro As Integer?, detalle As String)
        Try
            Ejecutar(
                "INSERT INTO Audit_Log (Usuario, Fecha_Hora, Accion, Tabla_Afectada, ID_Registro, Detalle) " &
                "VALUES (@usuario, NOW(), @accion, @tabla, @id, @detalle)",
                Sub(comando As MySqlCommand)
                    ' Se recorta a los anchos reales de las columnas: un detalle
                    ' demasiado largo nunca debe impedir que quede el registro.
                    Parametro(comando, "@usuario", Recortar(usuario, 50))
                    Parametro(comando, "@accion", Recortar(accion, 40))
                    Parametro(comando, "@tabla", Recortar(tabla, 80))
                    Parametro(comando, "@id", If(idRegistro.HasValue, CObj(idRegistro.Value), DBNull.Value))
                    Parametro(comando, "@detalle", Recortar(detalle, 1000))
                End Sub)
        Catch ex As Exception
            ' La auditoría nunca debe interrumpir la operación principal.
            RegistroErrores.Escribir("Auditoría", ex)
        End Try
    End Sub

    ''' <summary>
    ''' Ajusta un texto al ancho de la columna destino. Evita que un detalle
    ''' demasiado largo haga fallar el INSERT de la bitácora.
    ''' </summary>
    Private Shared Function Recortar(valor As String, maximaLongitud As Integer) As String
        If String.IsNullOrEmpty(valor) Then Return ""
        If valor.Length <= maximaLongitud Then Return valor
        Return valor.Substring(0, maximaLongitud)
    End Function

End Class
