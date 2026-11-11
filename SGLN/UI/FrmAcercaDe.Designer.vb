<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class FrmAcercaDe
    Inherits System.Windows.Forms.Form

    <System.Diagnostics.DebuggerNonUserCode()>
    Protected Overrides Sub Dispose(ByVal disposing As Boolean)
        Try
            If disposing AndAlso components IsNot Nothing Then
                components.Dispose()
            End If
        Finally
            MyBase.Dispose(disposing)
        End Try
    End Sub

    Private components As System.ComponentModel.IContainer

    <System.Diagnostics.DebuggerStepThrough()>
    Private Sub InitializeComponent()
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(FrmAcercaDe))
        Me.lblTitulo = New System.Windows.Forms.Label()
        Me.lblVersion = New System.Windows.Forms.Label()
        Me.txtAviso = New System.Windows.Forms.TextBox()
        Me.btnCerrar = New System.Windows.Forms.Button()
        Me.SuspendLayout()
        '
        'lblTitulo
        '
        Me.lblTitulo.Font = New System.Drawing.Font("Segoe UI", 14.0!, System.Drawing.FontStyle.Bold)
        Me.lblTitulo.Location = New System.Drawing.Point(12, 9)
        Me.lblTitulo.Name = "lblTitulo"
        Me.lblTitulo.Size = New System.Drawing.Size(560, 30)
        Me.lblTitulo.TabIndex = 0
        Me.lblTitulo.Text = "SGLN El Salvador"
        '
        'lblVersion
        '
        Me.lblVersion.ForeColor = System.Drawing.Color.DimGray
        Me.lblVersion.Location = New System.Drawing.Point(14, 39)
        Me.lblVersion.Name = "lblVersion"
        Me.lblVersion.Size = New System.Drawing.Size(560, 20)
        Me.lblVersion.TabIndex = 1
        Me.lblVersion.Text = "Sistema de Gestión Legal y Notarial · Versión 1.0"
        '
        'txtAviso
        '
        Me.txtAviso.BackColor = System.Drawing.Color.White
        Me.txtAviso.BorderStyle = System.Windows.Forms.BorderStyle.None
        Me.txtAviso.Location = New System.Drawing.Point(17, 62)
        Me.txtAviso.Multiline = True
        Me.txtAviso.Name = "txtAviso"
        Me.txtAviso.ReadOnly = True
        Me.txtAviso.ScrollBars = System.Windows.Forms.ScrollBars.Vertical
        Me.txtAviso.Size = New System.Drawing.Size(563, 204)
        Me.txtAviso.TabIndex = 2
        Me.txtAviso.TabStop = False
        Me.txtAviso.Text = resources.GetString("txtAviso.Text")
        '
        'btnCerrar
        '
        Me.btnCerrar.Location = New System.Drawing.Point(454, 272)
        Me.btnCerrar.Name = "btnCerrar"
        Me.btnCerrar.Size = New System.Drawing.Size(120, 32)
        Me.btnCerrar.TabIndex = 3
        Me.btnCerrar.Text = "Cerrar"
        Me.btnCerrar.UseVisualStyleBackColor = True
        '
        'FrmAcercaDe
        '
        Me.AcceptButton = Me.btnCerrar
        Me.AutoScaleDimensions = New System.Drawing.SizeF(7.0!, 15.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.ClientSize = New System.Drawing.Size(590, 315)
        Me.Controls.Add(Me.btnCerrar)
        Me.Controls.Add(Me.txtAviso)
        Me.Controls.Add(Me.lblVersion)
        Me.Controls.Add(Me.lblTitulo)
        Me.Font = New System.Drawing.Font("Segoe UI", 9.0!)
        Me.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog
        Me.MaximizeBox = False
        Me.MinimizeBox = False
        Me.Name = "FrmAcercaDe"
        Me.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent
        Me.Text = "Acerca de SGLN"
        Me.ResumeLayout(False)
        Me.PerformLayout()

    End Sub

    Friend WithEvents lblTitulo As System.Windows.Forms.Label
    Friend WithEvents lblVersion As System.Windows.Forms.Label
    Friend WithEvents txtAviso As System.Windows.Forms.TextBox
    Friend WithEvents btnCerrar As System.Windows.Forms.Button
End Class
