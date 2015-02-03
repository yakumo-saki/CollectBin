Imports System.Windows.Forms
Imports System.IO

Class MainWindow

    Private Shared ReadOnly logger As log4net.ILog = _
        log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType)

    Private Sub Window_loaded() Handles Me.Loaded

        FromDir.Text = My.Settings.FromDir
        ToDir.Text = My.Settings.ToDir

        DebugBin.IsChecked = My.Settings.DebugSelected
        ReleaseBin.IsChecked = Not My.Settings.DebugSelected

        ExcludeExt.Text = My.Settings.ExcludeSuffix

    End Sub

    Private Sub Window_closing() Handles Me.Closing

        My.Settings.FromDir = FromDir.Text
        My.Settings.ToDir = ToDir.Text
        My.Settings.DebugSelected = DebugBin.IsChecked
        My.Settings.ExcludeSuffix = ExcludeExt.Text
        My.Settings.Save()

    End Sub

    Private Sub GoButton_Click(sender As Object, e As RoutedEventArgs) Handles GoButton.Click

        Log.Document.Blocks.Clear()
        Log.AppendText("START " & Now.ToString("yyyy/MM/dd HH:mm:ss.fff") & vbCr)
        logger.Info("処理開始")

        Dim f = FromDir.Text
        Dim t = ToDir.Text

        Dim fd = New System.IO.DirectoryInfo(f)

        ' be sure to directory exists
        If System.IO.Directory.Exists(t) = False Then
            MessageBox.Show("宛先ディレクトリが存在しません")
            logger.Error("宛先ディレクトリが存在しません")
            Exit Sub
        End If

        ' ターゲットディレクトリ内全部削除
        Dim td = New System.IO.DirectoryInfo(t)
        Dim tdFiles = td.EnumerateFiles("*", SearchOption.AllDirectories)
        If tdFiles.Count > 0 Then

            logger.Info("ディレクトリをクリーンアップ。ファイル削除＝" & tdFiles.Count)
            Log.AppendText("CLEAN UP " & t & vbNewLine)

            For Each file In tdFiles
                Try
                    file.Delete()
                Catch ex As Exception
                    Log.AppendText("FAILED ファイルを削除できません " & file.FullName & vbCr & ex.ToString & vbCr)
                    logger.Error("ファイル削除に失敗 " & file.FullName, ex)
                    Console.Beep(1970, 300)
                End Try
            Next

        End If

        Dim tdDirs = td.EnumerateDirectories("*", SearchOption.AllDirectories)
        For Each d In tdDirs

            Try
                d.Delete(True)
            Catch ex As FileNotFoundException
                ' サブディレクトリが消されてしまった可能性があるので無視
            Catch ex As Exception
                Log.AppendText("FAILED ファイルを削除できません " & d.FullName & vbCr & ex.ToString & vbCr)
                logger.Error("ファイル削除に失敗 " & d.FullName, ex)
                Console.Beep(1970, 300)
            End Try

        Next

        ' 
        Dim targetDirName As String = IIf(DebugBin.IsChecked, "Debug", "Release")

        ' 除外する末尾配列を取得
        Dim extSuffixes() As String = ExcludeExt.Text.Split(";")

        ' find bin directory and copy all files
        For Each Directory In fd.EnumerateDirectories()

            Dim binDir = Directory.GetDirectories("bin")
            If binDir.Count = 0 Then Continue For

            Dim tgtDir = binDir(0).GetDirectories(targetDirName)
            If tgtDir.Count = 0 Then Continue For

            ' コピー元のディレクトリ
            Dim targetBinDir = tgtDir(0)

            ' ディレクトリを作成する
            Dim subDirs As New List(Of DirectoryInfo)
            subDirs.AddRange(targetBinDir.GetDirectories("*", SearchOption.AllDirectories))
            subDirs.Add(targetBinDir)
            For Each d In subDirs

                Dim relativeDir = d.FullName.Replace(targetBinDir.FullName, "").TrimStart("\")

                Dim dest = IO.Path.Combine(td.FullName, relativeDir)

                If relativeDir.Length > 0 Then
                    logger.Info("CREATE DIR " & relativeDir)
                    Path.CreateDirectoryIfNone(dest)
                End If


                ' ファイルをコピーする
                Dim copyFiles = d.GetFiles

                For Each file In copyFiles

                    If IsExcludeFilename(file.Name, extSuffixes) Then
                        Continue For
                    End If

                    Dim destPath = My.Computer.FileSystem.CombinePath(dest, file.Name)

                    ' 表示用相対パス
                    Dim relativePath = IIf(relativeDir.Length = 0, "", relativeDir & "\") & file.Name

                    Try
                        file.CopyTo(destPath, True)
                        Log.AppendText("COPY SUCCESS " & relativePath & vbCr)
                        logger.Info("COPY SUCCESS " & relativePath)
                    Catch ex As Exception
                        Log.AppendText("COPY FAIL    " & relativePath & vbCr)
                        logger.Info("COPY FAIL " & relativePath, ex)
                    End Try

                    Log.ScrollToEnd()

                Next

            Next

        Next

        Log.AppendText("DONE " & Now.ToString("yyyy/MM/dd HH:mm:ss.fff") & vbNewLine)
        Log.ScrollToEnd()
        logger.Info("処理完了")

        Console.Beep(2000, 150)
        Console.Beep(1000, 120)

    End Sub
    Private Sub CleanButton_Click(sender As Object, e As RoutedEventArgs) Handles CleanButton.Click

        Log.Document.Blocks.Clear()
        Log.AppendText("START " & Now.ToString("yyyy/MM/dd HH:mm:ss.fff") & vbCr)
        logger.Info("処理開始")

        Dim f = FromDir.Text
        Dim fd = New System.IO.DirectoryInfo(f)

        Dim targetDirName As String = IIf(DebugBin.IsChecked, "Debug", "Release")

        ' find bin directory and copy all files
        For Each Directory In fd.EnumerateDirectories()

            Dim binDir = Directory.GetDirectories("bin")
            If binDir.Count = 0 Then Continue For

            Dim tgtDir = binDir(0).GetDirectories(targetDirName)
            If tgtDir.Count = 0 Then Continue For

            Dim targetBinDir = tgtDir(0)

            Try
                targetBinDir.Delete(True)
                Log.AppendText("DELETE SUCCESS " & targetBinDir.FullName & vbCr)
                logger.Info("DELETE SUCCESS " & targetBinDir.FullName)
            Catch ex As Exception
                Log.AppendText("DELETE FAIL    " & targetBinDir.FullName & vbCr)
                logger.Info("DELETE FAIL    " & targetBinDir.FullName)
            End Try

            Log.ScrollToEnd()

        Next

        Log.AppendText("DONE " & Now.ToString("yyyy/MM/dd HH:mm:ss.fff") & vbNewLine)
        Log.ScrollToEnd()
        logger.Info("処理完了")

        Console.Beep(2000, 50)
        Console.Beep(2000, 100)
        Console.Beep(1000, 60)
        Console.Beep(1500, 120)

    End Sub

    Private Function IsExcludeFilename(filename As String, extSuffix() As String)

        For Each ext In extSuffix

            Dim myext = ext.Trim

            If myext.Length = 0 Then Continue For

            If filename.ToUpper.EndsWith(myext.ToUpper) Then
                Return True
            End If
        Next

        Return False

    End Function

    Private Sub DirChoiseButton_Click(sender As Object, e As RoutedEventArgs) Handles FromDirButton.Click, ToDirButton.Click

        Dim dialog = New FolderBrowserDialog
        dialog.ShowDialog()

        If dialog.SelectedPath = "" Then Exit Sub

        Dim btn = CType(sender, Controls.Button)

        If btn.Name.Contains("From") Then
            FromDir.Text = dialog.SelectedPath
            My.Settings.FromDir = FromDir.Text
            My.Settings.Save()
        Else
            ToDir.Text = dialog.SelectedPath
            My.Settings.ToDir = ToDir.Text
            My.Settings.Save()
        End If

    End Sub


End Class
