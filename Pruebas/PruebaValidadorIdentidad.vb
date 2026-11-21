' Pruebas puras del validador de identidad; no requieren conexión MySQL.
Imports System
Imports SGLN

Module PruebaValidadorIdentidad

    Sub Main()
        Verificar(ValidadorIdentidad.EsDigitoDuiValido("03096201-2"),
                  "DUI conocido con guion")
        Verificar(Not ValidadorIdentidad.EsDigitoDuiValido("03096201-3"),
                  "DUI con dígito verificador incorrecto")
        Verificar(ValidadorIdentidad.EsDigitoDuiValido("030962012"),
                  "DUI conocido sin guion")
        Verificar(Not ValidadorIdentidad.EsDigitoDuiValido("0-30962012"),
                  "DUI con guion en posición incorrecta")
        Verificar(ValidadorIdentidad.EsFormatoNitValido("06140108081001"),
                  "NIT compacto de 14 dígitos")
        Verificar(ValidadorIdentidad.EsFormatoNitValido("0614-010808-100-1"),
                  "NIT con separadores correctos")
        Verificar(Not ValidadorIdentidad.EsFormatoNitValido("06140-10808-100-1"),
                  "NIT con separadores incorrectos")
        Verificar(Not ValidadorIdentidad.EsFormatoNitValido("ABCD06140108081001"),
                  "NIT con letras")
        Verificar(ValidadorIdentidad.FormatearNit("06140108081001") = "0614-010808-100-1",
                  "Normalización de NIT")
        Verificar(ValidadorIdentidad.FormatearDui("030962012") = "03096201-2",
                  "Normalización de DUI")

        Console.WriteLine("Todas las pruebas puras de identidad finalizaron correctamente.")
    End Sub

    Private Sub Verificar(condicion As Boolean, nombre As String)
        If Not condicion Then
            Throw New InvalidOperationException("FALLA: " & nombre)
        End If
        Console.WriteLine("OK: " & nombre)
    End Sub

End Module
