Imports System
Imports MySql.Data.MySqlClient

''' <summary>
''' Punto único de acceso a la cadena de conexión y a la base de datos MySQL.
''' La cadena vigente la resuelve ConfiguracionConexion (registro de Windows,
''' luego App.config y por último valores por defecto); ninguna capa de la
''' aplicación vuelve a conocer el texto de la cadena.
''' </summary>
Public Module ConexionBD

    Public ReadOnly Property CadenaConexion As String
        Get
            Return ConfiguracionConexion.CadenaActual()
        End Get
    End Property

    ''' <summary>
    ''' Constructor de la cadena vigente. Se usa para leer el servidor y la base
    ''' sin volver a interpretar el texto de la conexión a mano.
    ''' </summary>
    Public Function ConstructorConexion() As MySqlConnectionStringBuilder
        Return New MySqlConnectionStringBuilder(CadenaConexion)
    End Function

    Public Function NombreBaseDatos() As String
        Return ConstructorConexion().Database
    End Function

    Public Function ProbarConexion() As Boolean
        Try
            Using conexion As New MySqlConnection(CadenaConexion)
                conexion.Open()
                Return True
            End Using
        Catch ex As Exception
            RegistroErrores.Escribir("Probar conexión", ex)
            Return False
        End Try
    End Function

End Module
