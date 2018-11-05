Imports AForge.Video
Imports AForge.Video.DirectShow
Imports System.Threading
Imports System.ComponentModel

Public Class Form2
    Dim CAMERA As VideoCaptureDevice
    Dim bmp As Bitmap
    Dim trd1 As Thread
    Dim verificarPadrao As Boolean = False
    Dim porcDif As String = "0"
    Dim porcQualidade As Double = 5.0
    Dim imgDif1 As Bitmap
    Dim imgDif2 As Bitmap
    Dim imgDifRes As Bitmap

    '' BOTAO INICIA CAPTURA DA CAMERA
    Private Sub Button1_Click(sender As Object, e As EventArgs) Handles Button1.Click
        Dim cameras As VideoCaptureDeviceForm = New VideoCaptureDeviceForm

        If cameras.ShowDialog() = DialogResult.OK Then
            CAMERA = cameras.VideoDevice
            AddHandler CAMERA.NewFrame, New NewFrameEventHandler(AddressOf Captured)
            CAMERA.Start()
        End If

        Timer2.Enabled = True

    End Sub

    '' BOTAO SALVA COMO FOTO EM DISCO
    Private Sub Button2_Click(sender As Object, e As EventArgs) Handles Button2.Click
        SaveFileDialog1.DefaultExt = ".jpg"
        If SaveFileDialog1.ShowDialog = DialogResult.OK Then
            PictureBox2.Image.Save(SaveFileDialog1.FileName, System.Drawing.Imaging.ImageFormat.Jpeg)
        End If
    End Sub

    '' BOTAO SALVAR PADRAO
    Private Sub Button3_Click(sender As Object, e As EventArgs) Handles Button3.Click
        Try
            PictureBox3.Image = PictureBox2.Image
            PictureBox3.BackgroundImageLayout = ImageLayout.Stretch
            verificarPadrao = True
        Catch : End Try

        imgDif2 = PictureBox3.Image

    End Sub

    '' BOTAO PARAR CAPTURA
    Private Sub Button4_Click(sender As Object, e As EventArgs) Handles Button4.Click
        Timer1.Stop()
        Timer2.Stop()
        CAMERA.Stop()
        trd1.Suspend()

    End Sub

    '' BOTAO ATUALIZA INTERVALO
    Private Sub Button5_Click(sender As Object, e As EventArgs) Handles Button5.Click
        Timer2.Interval = NumericUpDown1.Value
    End Sub

    '' TICK A CADA x MS para atualizar HISTOGRAMA E PRETO E BRANCO
    Private Sub Timer2_Tick(sender As Object, e As EventArgs) Handles Timer2.Tick
        PictureBox2.Image = clarearImg(imgPB_2(PictureBox1.Image), TrackBar1.Value)
        'PictureBox2.Image = clarearImg(PictureBox2.Image, TrackBar1.Value)
        criarHistograma(PictureBox2)

        Try
            If CheckBox1.Checked = True Then
                desenhaLinhas()
            End If
        Catch : End Try

    End Sub

    '' TICK ATUALIZAR OUTROS DADOS
    Private Sub Timer1_Tick(sender As Object, e As EventArgs) Handles Timer1.Tick

        If verificarPadrao Then
            Try
                trd1 = New Thread(AddressOf diferencaImgs)
                trd1.IsBackground = True
                trd1.Start()
            Catch ex As Exception : End Try
        End If

        imgDif1 = PictureBox2.Image
        'imgDif2 = PictureBox3.Image

        '' Atualiza dados da diferenca
        Try : PictureBox4.Image = imgDifRes : Catch : End Try
        Try : Label1.Text = "Diferença: " + porcDif + "%" : Catch : End Try
        If porcDif < porcQualidade Then
            Label1.ForeColor = Color.Green
        Else
            Label1.ForeColor = Color.Red
        End If
    End Sub

    '' CLAREIA E ESCURECE IMAGEM
    Private Function clarearImg(ByRef imagem As Bitmap, ByRef vlr As Integer)
        Try
            For H As Integer = 0 To imagem.Width - 1
                For V As Integer = 0 To imagem.Height - 1

                    Dim C As Color = imagem.GetPixel(H, V)
                    Dim R As Integer = C.R + vlr
                    Dim G As Integer = C.G + vlr
                    Dim B As Integer = C.B + vlr

                    If R < 0 Then R = 0
                    If G < 0 Then G = 0
                    If B < 0 Then B = 0

                    imagem.SetPixel(H, V, Color.FromArgb(C.A, R, G, B))
                Next
            Next
            Return imagem
        Catch ex As Exception
        End Try
    End Function

    '' DESENHA LINHAS GUIAS
    Private Sub desenhaLinhas()
        Dim undo_image As Image
        Dim xStart, yStart, xEnd, yEnd As Integer
        Dim PintarBitmap As Bitmap
        Dim Pintura As Graphics
        Dim Caneta As New Pen(Color.Red, 2)
        Dim MinhaCor As Color = Color.Blue
        Dim MeuBrush As New Drawing.SolidBrush(Color.Red)
        Dim MeuBrushLargura As Integer = 100
        Dim ContinuousFlag As Boolean

        PintarBitmap = New Bitmap(PictureBox2.Image)
        Pintura = Graphics.FromImage(PintarBitmap)

        Pintura.SmoothingMode = Drawing2D.SmoothingMode.HighQuality
        Pintura.DrawLine(Caneta, 40, 0, 40, 120)
        Pintura.DrawLine(Caneta, 120, 0, 120, 120)
        Pintura.DrawLine(Caneta, 0, 30, 160, 30)
        Pintura.DrawLine(Caneta, 0, 90, 160, 90)
        PictureBox2.Image = PintarBitmap

    End Sub

    '' ATUALIZA FOTO - CAPTURA DA CAMERA
    Private Sub Captured(sender As Object, eventArgs As NewFrameEventArgs)
        bmp = DirectCast(eventArgs.Frame.Clone(), Bitmap)
        Try
            PictureBox1.Image = DirectCast(eventArgs.Frame.Clone(), Bitmap)

        Catch : End Try

        ''PictureBox1.BackgroundImageLayout = ImageLayout.Stretch
    End Sub

    '' Utilizado para passar os valores encontrados na foto
    Private valoresHist(255) As Integer '' 255 pontos de luminosidade
    '' Cria o histograma à partir de uma foto em uma pictureBox
    Private Sub criarHistograma(picBox As PictureBox)
        '' Pegar os tamanhos e imagem informada:
        Dim bmp As New Bitmap(picBox.Width, picBox.Height)
        picBox.DrawToBitmap(bmp, picBox.ClientRectangle)

        '' Limpa histograma
        For x As Integer = 0 To 255
            valoresHist(x) = 0
        Next

        '' Construir o histograma:
        For y As Integer = 0 To bmp.Height - 1                      '' Altura
            For x As Integer = 0 To bmp.Width - 1                   '' largura
                Dim b As Single = bmp.GetPixel(x, y).GetBrightness  '' Pega o brilho no determinado pixel
                valoresHist(CInt(b * 255)) += 1                     '' Salva temporariamente os valores do histograma
            Next
        Next

        '' Atualiza a pictureBox5:
        PictureBox5.Invalidate()
    End Sub

    '' DESENHA HISTOGRAMA
    Private Sub PictureBox5_Paint(sender As Object, e As System.Windows.Forms.PaintEventArgs) Handles PictureBox5.Paint
        Try
            ''PictureBox1.Image = Nothing
            If valoresHist.Max <= 0 Then Exit Sub '' Se não tiver valores, sair da função

            '' Desenhar o hisograma na pictureBox
            Dim a As Integer = PictureBox5.ClientSize.Height    '' Altura
            For i As Integer = 0 To 255 Step 2                  '' Alterar o passo se não quiser vazado, para melhor visualização
                Dim BarraAlgura As Integer = CInt(a * valoresHist(i) / valoresHist.Max) '' Cria altura da barra 
                Dim l As Integer = (PictureBox5.ClientSize.Width * i) \ 256             '' Verifica a localização da barra
                Using pn As New Pen(Brushes.Black, 2)                                   '' Desenha a barra:
                    e.Graphics.DrawLine(pn, l, a - BarraAlgura, l, a)
                End Using
            Next
        Catch ex As Exception
        End Try
    End Sub

    '' DIFENRENCA ENTRE IMAGENS
    Private Sub diferencaImgs()
        Try
            Dim bm1 As Bitmap = imgDif1
            Dim bm2 As Bitmap = imgDif2

            'Carrega as imagens
            Dim wid As Integer = Math.Min(bm1.Width, bm2.Width)
            Dim hgt As Integer = Math.Min(bm1.Height, bm2.Height)
            Dim bm3 As New Bitmap(wid, hgt)

            porcDif = FormatNumber(Diferenca(bm1, bm2), 2)

            ' Criar a imagem com as diferenças
            Dim iguais As Boolean = True
            Dim r1, g1, b1, r2, g2, b2, r3, g3, b3 As Integer
            Dim color1, color2, color3 As Color
            For x As Integer = 0 To wid - 1
                For y As Integer = 0 To hgt - 1
                    color1 = bm1.GetPixel(x, y)
                    r1 = color1.R : g1 = color1.G : b1 = color1.B

                    color2 = bm2.GetPixel(x, y)
                    r2 = color2.R : g2 = color2.G : b2 = color2.B

                    r3 = 128 + (r1 - r2) \ 2
                    g3 = 128 + (g1 - g2) \ 2
                    b3 = 128 + (b1 - b2) \ 2

                    color3 = Color.FromArgb(255, r3, g3, b3)
                    bm3.SetPixel(x, y, color3)
                Next y
            Next x

            'Mostra o resultado
            imgDifRes = bm3

            ''bm1.Dispose()
            ''bm2.Dispose()
        Catch ex As Exception

        End Try
    End Sub

    '' AO FECHAR APP
    Private Sub Form1_FormClosing(sender As Object, e As FormClosingEventArgs) Handles MyBase.FormClosing
        CAMERA.Stop()
    End Sub

    Private Sub BackgroundWorker1_DoWork(sender As Object, e As DoWorkEventArgs) Handles BackgroundWorker1.DoWork
        Dim segundoPlano As BackgroundWorker

        ' ----- Chama a thread em segundo plano (background)
        segundoPlano = CType(sender, BackgroundWorker)
        BW_work1(segundoPlano)

        ' ----- Verifica cancelamento
        If (segundoPlano.CancellationPending = True) Then e.Cancel = True

    End Sub

    Private Sub BW_work1(ByVal processoAtivo As BackgroundWorker)
        While processoAtivo.CancellationPending = False
            ''Try : diferencaImgs() : Catch ex As Exception : End Try
            ''Threading.Thread.Sleep(100)
            'Informa a thread principal que temos alterações:
            ''processoAtivo.ReportProgress(100)

            Try
                Dim bm1 As Bitmap = imgDif1
                Dim bm2 As Bitmap = imgDif2

                'Carrega as imagens
                Dim wid As Integer = Math.Min(bm1.Width, bm2.Width)
                Dim hgt As Integer = Math.Min(bm1.Height, bm2.Height)
                Dim bm3 As New Bitmap(wid, hgt)

                porcDif = FormatNumber(Diferenca(bm1, bm2), 2)

                ' Criar a imagem com as diferenças
                Dim r1, g1, b1, r2, g2, b2, r3, g3, b3 As Integer
                Dim color1, color2, color3 As Color
                For x As Integer = 0 To wid - 1
                    For y As Integer = 0 To hgt - 1
                        color1 = bm1.GetPixel(x, y)
                        r1 = color1.R : g1 = color1.G : b1 = color1.B

                        color2 = bm2.GetPixel(x, y)
                        r2 = color2.R : g2 = color2.G : b2 = color2.B

                        r3 = 128 + (r1 - r2) \ 2
                        g3 = 128 + (g1 - g2) \ 2
                        b3 = 128 + (b1 - b2) \ 2

                        color3 = Color.FromArgb(255, r3, g3, b3)
                        bm3.SetPixel(x, y, color3)

                    Next y
                Next x

                'Mostra o resultado
                imgDifRes = bm3

                bm1.Dispose()
                bm2.Dispose()
            Catch ex As Exception : End Try
        End While

    End Sub

    Private Sub Button6_Click(sender As Object, e As EventArgs) Handles Button6.Click
        porcQualidade = CDbl(TextBox1.Text)
    End Sub

    Private Sub Label1_Click(sender As Object, e As EventArgs) Handles Label1.Click
        porcQualidade = porcDif
        TextBox1.Text = porcDif
    End Sub
End Class