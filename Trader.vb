Imports System.IO
Imports System.Text
Imports System.Threading
Imports System.Net
Imports KotakNet.Core
Imports System.Runtime.Serialization.Formatters.Binary

Public Class Trader
    Private IsFormLoaded As Boolean = False

    Private Kotak As New Kotak(True)
    Private LockWatch As New Object
    Private TokensWatch As New List(Of String)

    Private Watch As New DataTable
    Private AutoCompleteNse As New AutoCompleteStringCollection()
    Private AutoCompleteBse As New AutoCompleteStringCollection()
    Private AutoCompleteIdx As New AutoCompleteStringCollection()
    Private AutoCompleteNfoFut As New AutoCompleteStringCollection()
    Private AutoCompleteNfoOpt As New AutoCompleteStringCollection()
    Private AutoCompleteCdsFut As New AutoCompleteStringCollection()
    Private AutoCompleteCdsOpt As New AutoCompleteStringCollection()
    Private AutoCompleteMcxFut As New AutoCompleteStringCollection()
    Private AutoCompleteMcxOpt As New AutoCompleteStringCollection()

    Delegate Sub SetLabelStatusCallback(ByVal Text As String, ByVal color As Color)
    Private Sub SetLabelStatus(ByVal Text As String, ByVal color As Color)
        Try
            If LabelStatus.InvokeRequired Then
                Dim d As New SetLabelStatusCallback(AddressOf SetLabelStatus)
                LabelStatus.Invoke(d, New Object() {Text, color})
            Else
                LabelStatus.Text = Text
                LabelStatus.ForeColor = color
            End If
        Catch ex As Exception
        End Try
    End Sub

    Delegate Sub SetControlTextCallback(ByVal Control As Control, ByVal Text As String)
    Private Sub SetControlText(ByVal Control As Control, ByVal Text As String)
        Try
            If Control.InvokeRequired Then
                Dim d As New SetControlTextCallback(AddressOf SetControlText)
                Control.Invoke(d, New Object() {Control, Text})
            Else
                Control.Text = Text
            End If
        Catch ex As Exception
        End Try
    End Sub

    Delegate Sub SetControlCallback(ByVal Control As Control, ByVal Status As Boolean)
    Private Sub SetControl(ByVal Control As Control, ByVal Status As Boolean)
        Try
            If Control.InvokeRequired Then
                Dim d As New SetControlCallback(AddressOf SetControl)
                Control.Invoke(d, New Object() {Control, Status})
            Else
                Control.Enabled = Status
            End If
        Catch Ex As Exception
        End Try
    End Sub

    Delegate Sub SetRichBoxTextCallback(ByVal EventName As String, ByVal Message As String)
    Private Sub SetRichBoxText(ByVal EventName As String, ByVal Message As String)
        Try
            If (String.IsNullOrEmpty(EventName) OrElse String.IsNullOrEmpty(Message)) Then Exit Sub
            If RichTextBox1.InvokeRequired Then
                Dim d As New SetRichBoxTextCallback(AddressOf SetRichBoxText)
                Me.RichTextBox1.Invoke(d, New Object() {EventName, Message})
            Else
                RichTextBox1.Text &= Environment.NewLine & Now.ToString("HH:mm:ss") & " " & EventName.ToUpper & " " & Message
            End If
        Catch Ex As Exception
        End Try
    End Sub

    Delegate Sub SetLabelClockCallback(ByVal Text As String)
    Private Sub SetLabelClock(ByVal Text As String)
        Try
            If StripMenu.InvokeRequired Then
                Dim d As New SetLabelClockCallback(AddressOf SetLabelClock)
                StripMenu.Invoke(d, New Object() {Text})
            Else
                LabelClock.Text = Text
            End If
        Catch Ex As Exception
        End Try
    End Sub

    Private Sub Trader_FormClosing(sender As Object, e As FormClosingEventArgs) Handles Me.FormClosing
        If MsgBox("Close Application?", MsgBoxStyle.OkCancel, "KotakTrader") = MsgBoxResult.Cancel Then
            e.Cancel = True
            Exit Sub
        End If

        Try
            If Kotak.LoginStatus Then Kotak.Logout()
        Catch ex As Exception
        End Try
    End Sub

    Private Sub Trader_KeyDown(sender As Object, e As KeyEventArgs) Handles MyBase.KeyDown
        Try
            If e.KeyCode = Keys.F1 Then
                If Kotak.SymbolStatus Then
                    Kotak.ShowOrderWindow("REGULAR", "NSE", "", "BUY")
                End If
            End If

            If e.KeyCode = Keys.F2 Then
                If Kotak.SymbolStatus Then
                    Kotak.ShowOrderWindow("REGULAR", "NSE", "", "SELL")
                End If
            End If
        Catch ex As Exception
            MsgBox(ex.Message, MsgBoxStyle.Exclamation, "KotakTrader")
        End Try
    End Sub

    Private Sub Trader_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        Me.Height = Screen.PrimaryScreen.WorkingArea.Height * 0.7
        Me.Width = Screen.PrimaryScreen.WorkingArea.Width * 0.7
        Me.CenterToScreen()

        DataGridView1.DoubleBuffered(True)
        DataGridView2.DoubleBuffered(True)

        Me.Panel3.DoubleBuffered(True)
        Me.Panel3.ResizeRedraw(True)
        Me.Panel4.DoubleBuffered(True)
        Me.Panel4.ResizeRedraw(True)
        Me.Panel6.DoubleBuffered(True)
        Me.Panel6.ResizeRedraw(True)

        ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12

        LabelClock.Text = Now.ToString("HH:mm:ss")

        AddHandler Kotak.AppUpdateEvent, AddressOf AppUpdate
        AddHandler Kotak.OrderUpdateEvent, AddressOf OrderUpdate
        AddHandler Kotak.QuotesReceivedEvent, AddressOf QuotesReceived

        Me.Text = "KotakTrader v1.0 - Kotak API"
        RichTextBox1.Text = "KotakTrader v1.0 - Trading Platform Built with KotakNet"

        Panel4.Enabled = False
        Panel7.Enabled = False
        ButtonLogout.Enabled = False
        ButtonWebsock.Enabled = False
        ButtonSubscribe.Enabled = False

        ComboBoxExch.SelectedIndex = 0
        ComboBoxSeg.SelectedIndex = 0
        ComboBoxReports.SelectedIndex = 0

        LoadWatchTable()

        Dim Thread1 As New Thread(AddressOf Me.UpdateClock)
        Thread1.IsBackground = True
        Thread1.Start()
    End Sub

    Private Sub DataGridView1_DataError(ByVal sender As Object, ByVal e As DataGridViewDataErrorEventArgs) Handles DataGridView1.DataError
        e.Cancel = True
    End Sub

    Private Sub UpdateClock()
        Do
            Thread.Sleep(1000)
            SetLabelClock(Now.ToString("HH:mm:ss"))
        Loop
    End Sub

    Private Sub ExitToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles ExitToolStripMenuItem.Click
        Me.Close()
    End Sub

    Private Sub ButtonSettings_Click(sender As Object, e As EventArgs) Handles ButtonSettings.Click
        Kotak.ShowSettings()
    End Sub

    Private Sub ToolStripMenuItem7_Click(sender As Object, e As EventArgs) Handles ToolStripMenuItem7.Click
        Dim About As String = "KotakTrader v1.0" & vbNewLine & _
                              "Trading Platform Built With KotakNet" & vbNewLine & vbNewLine & _
                              "Author: https://howutrade.in" & vbNewLine & _
                              "Repository: https://github.com/howutrade/kotak-trader"
        MsgBox(About, MsgBoxStyle.Information, "KotakTrader")
    End Sub

    Private Sub OrderUpdate(sender As Object, e As OrderUpdateEventArgs)
        SetRichBoxText("ORDER UPDATE", "OrderId: " & e.OrderId & " Symbol: " & e.TrdSym & " Trans: " & e.TransType & " Qty: " & e.Qty & " Status: " & e.Status)
    End Sub

    Private Sub OnSymbolDownload()
        Try
            If Me.InvokeRequired Then
                Me.Invoke(New MethodInvoker(AddressOf OnSymbolDownload))
                Exit Sub
            End If

            LabelUserId.Text = Kotak.ClientID

            'Enable Logout button
            ButtonLogout.Enabled = True
            ButtonWebsock.Enabled = True
            ButtonSubscribe.Enabled = True

            AutoCompleteNse.AddRange(Kotak.SymbolsNSE)
            AutoCompleteBse.AddRange(Kotak.SymbolsBSE)
            AutoCompleteIdx.AddRange(Kotak.SymbolsIDX)
            AutoCompleteNfoFut.AddRange(Kotak.SymbolsNFOFUT)
            AutoCompleteNfoOpt.AddRange(Kotak.SymbolsNFOOPT)
            AutoCompleteCdsFut.AddRange(Kotak.SymbolsCDSFUT)
            AutoCompleteCdsOpt.AddRange(Kotak.SymbolsCDSOPT)
            AutoCompleteMcxFut.AddRange(Kotak.SymbolsMCXFUT)
            AutoCompleteMcxOpt.AddRange(Kotak.SymbolsMCXOPT)

            ComboBoxSym.AutoCompleteCustomSource = AutoCompleteNse

            Dim list() As String = Kotak.GetMarketWatchSymbols
            For Each symexch In list
                Dim parts() As String = symexch.Split(New String() {"."}, StringSplitOptions.RemoveEmptyEntries)
                If parts.Length < 2 Then Continue For
                Dim trdsym As String = parts(0)
                Dim exch As String = parts(1)
                AddSymbolToMarketWatch(exch, trdsym)
            Next

            Panel4.Enabled = True
            Panel7.Enabled = True
        Catch Ex As Exception
        End Try
    End Sub

    Private Sub UpdateWindowTitle()
        Try
            If Me.InvokeRequired Then
                Me.Invoke(New MethodInvoker(AddressOf UpdateWindowTitle))
            Else
                Me.Text = "KotakTrader v1.0 - " & Kotak.ClientID & ", Kotak API"
            End If
        Catch Ex As Exception
        End Try
    End Sub

    Private Sub AppUpdate(sender As Object, e As AppUpdateEventArgs)
        SetRichBoxText("APP UPDATE", "Code: " & e.EventCode & " Message: " & e.EventMessage)

        'On Login
        If e.EventCode = 1 Then
            UpdateWindowTitle()
        End If

        If e.EventCode = 2 Then
            SetLabelStatus("Logged out", Color.Red)
        End If

        'Symbol Downloaded
        If e.EventCode = 4 Then
            OnSymbolDownload()
            SetLabelStatus("Connected", Color.Green)
        End If

        If e.EventCode = 16 Then
            SetLabelStatus("Connecting websocket...", Color.DarkOrange)
        End If

        If e.EventCode = 17 Then
            SetLabelStatus("Websocket connect failed", Color.Red)
        End If

        If e.EventCode = 5 Then
            SetLabelStatus("Network disconnected", Color.Red)
        End If

        If e.EventCode = 6 Then
            SetLabelStatus("Connected", Color.Green)
        End If

        If e.EventCode = 7 Then
            SetLabelStatus("Websocket disconnected", Color.Red)
        End If

        If e.EventCode = 8 Then
            SetLabelStatus("Connected", Color.Green)
        End If

        'Contract Error
        If (e.EventCode = 12 OrElse e.EventCode = 13 OrElse e.EventCode = 14) Then
            SetControl(ButtonLogin, True)
            SetLabelStatus("Symbol download failed", Color.Salmon)
        End If
    End Sub

    Private Sub QuotesReceived(sender As Object, e As QuotesReceivedEventArgs)
        SyncLock (LockWatch)
            Try
                If TokensWatch.Contains(e.InstToken) Then
                    Dim Idx As Integer = TokensWatch.IndexOf(e.InstToken)
                    Watch.Rows(Idx)(2) = e.LTP
                    Watch.Rows(Idx)(3) = e.LTQ
                    Watch.Rows(Idx)(4) = e.NetChg
                    Watch.Rows(Idx)(5) = e.PctChg
                    Watch.Rows(Idx)(6) = e.BestBid
                    Watch.Rows(Idx)(7) = e.BidQty
                    Watch.Rows(Idx)(8) = e.BestAsk
                    Watch.Rows(Idx)(9) = e.AskQty
                    Watch.Rows(Idx)(10) = e.Open
                    Watch.Rows(Idx)(11) = e.High
                    Watch.Rows(Idx)(12) = e.Low
                    Watch.Rows(Idx)(13) = e.Close
                    Watch.Rows(Idx)(14) = e.ATP
                    Watch.Rows(Idx)(15) = e.Volume
                    Watch.Rows(Idx)(16) = e.TotalBuyQty
                    Watch.Rows(Idx)(17) = e.TotalSellQty
                    Watch.Rows(Idx)(18) = e.LTT.ToString("dd-MMM-yyyy HH:mm:ss")
                End If
            Catch Ex As Exception
            End Try
        End SyncLock
    End Sub

    Private Sub LoadWatchTable()
        Try
            Dim Header As String = "EXCH,TRDSYM,LTP,LTQ,NETCHG,PCTCHG,BESTBID,BIDQTY,BESTASK,ASKQTY,OPEN,HIGH,LOW,CLOSE,ATP,VOLUME,TOTALBUYQTY,TOTALSELLQTY,LTT,TOKEN"

            Dim ColumnNames() As String = Header.Split(New String() {","}, StringSplitOptions.None)
            For Each ColumnName As String In ColumnNames
                Watch.Columns.Add(ColumnName, GetType(String))
            Next

            DataGridView1.DataSource = Watch

            For Each Col As DataGridViewColumn In DataGridView1.Columns
                Col.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter
            Next
            DataGridView1.Columns(1).DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleLeft

            DataGridView1.ReadOnly = True
            DataGridView1.MultiSelect = False
            DataGridView1.RowHeadersVisible = False

            DataGridView1.AllowUserToAddRows = False
            DataGridView1.AllowUserToDeleteRows = False
            DataGridView1.AllowUserToOrderColumns = True
            DataGridView1.AllowUserToResizeColumns = True
            DataGridView1.AllowUserToResizeRows = True

            'Prevent When Adding Columns
            LoadColumnOrders()
        Catch Ex As Exception
        End Try
    End Sub

    Private Sub DataGridView1_ColumnDisplayIndexChanged(ByVal sender As Object, ByVal e As DataGridViewColumnEventArgs) Handles DataGridView1.ColumnDisplayIndexChanged
        Try
            If Not Me.IsFormLoaded Then Exit Sub

            Dim ColOrder As New Dictionary(Of String, Integer)
            For Each Col As DataGridViewColumn In DataGridView1.Columns
                ColOrder.Add(Col.Name, Col.DisplayIndex)
            Next

            Using Fs As FileStream = New FileStream("MW_COLUMN_ORDER.bin", FileMode.Create)
                Dim Formatter As New BinaryFormatter()
                Formatter.Serialize(Fs, ColOrder)
            End Using
        Catch ex As Exception
            MsgBox(ex.Message, MsgBoxStyle.Exclamation, "KotakTrader")
        End Try
    End Sub

    Private Sub LoadColumnOrders()
        Try
            If Not File.Exists("MW_COLUMN_ORDER.bin") Then Exit Sub

            Using Fs As FileStream = New FileStream("MW_COLUMN_ORDER.bin", FileMode.Open)
                Dim Formatter As New BinaryFormatter()
                Dim ColOrder As Dictionary(Of String, Integer) = CType(Formatter.Deserialize(Fs), Dictionary(Of String, Integer))

                For Each Col As DataGridViewColumn In DataGridView1.Columns
                    If ColOrder.ContainsKey(Col.Name) Then
                        Col.DisplayIndex = ColOrder(Col.Name)
                    End If
                Next
            End Using
        Catch ex As Exception
            MsgBox(ex.Message, MsgBoxStyle.Exclamation, "KotakTrader")
        End Try
    End Sub

    Private Sub ButtonLogout_Click(sender As Object, e As EventArgs) Handles ButtonLogout.Click
        Try
            If Kotak.Logout Then
                ButtonLogout.Enabled = False
                ButtonWebsock.Enabled = False
                ButtonSubscribe.Enabled = False
                Panel4.Enabled = False
                Panel7.Enabled = False
            End If
        Catch ex As Exception
            MsgBox(ex.Message, MsgBoxStyle.Exclamation, "KotakTrader")
        End Try
    End Sub

    Private Sub ButtonLogin_Click(sender As Object, e As EventArgs) Handles ButtonLogin.Click
        Try
            If String.IsNullOrEmpty(Kotak.ConsumerKey) OrElse String.IsNullOrEmpty(Kotak.ConsumerSecret) OrElse String.IsNullOrEmpty(Kotak.AccessToken) Then
                MsgBox("Enter API Credentials", MsgBoxStyle.Exclamation, "KotakTrader")
                Kotak.ShowSettings()
                Exit Sub
            End If

            If String.IsNullOrEmpty(Kotak.UserID) OrElse String.IsNullOrEmpty(Kotak.Password) Then
                MsgBox("Enter Kotak Credentials", MsgBoxStyle.Exclamation, "KotakTrader")
                Kotak.ShowSettings()
                Exit Sub
            End If

            If Kotak.Login Then
                ButtonLogin.Enabled = False
                Kotak.GetMasterContract()
                SetLabelStatus("Downloading Symbols...", Color.DarkOrange)
            End If
        Catch ex As Exception
            MsgBox(ex.Message, MsgBoxStyle.Exclamation, "KotakTrader")
        End Try
    End Sub

    Private Sub DataGridView1_KeyDown(sender As Object, e As KeyEventArgs) Handles DataGridView1.KeyDown
        If e.KeyCode = Keys.Delete Then
            DelSymbolFromMarketWatch()
        End If
    End Sub

    Private Sub DelSymbolFromMarketWatch()
        SyncLock (LockWatch)
            Try
                If Not (DataGridView1.SelectedCells.Count > 0) Then Exit Sub

                If Not Kotak.SymbolStatus Then Exit Sub
                If Kotak.LogoutStatus Then Exit Sub

                Dim InstToken As String
                Dim Exch As String
                Dim TrdSym As String


                With DataGridView1
                    Dim RowIndex As Integer = .SelectedCells(0).RowIndex
                    InstToken = .Rows(RowIndex).Cells(19).Value.ToString().Trim 'Last Column
                    Exch = .Rows(RowIndex).Cells(0).Value.ToString().Trim
                    TrdSym = .Rows(RowIndex).Cells(1).Value.ToString().Trim
                End With


                If String.IsNullOrEmpty(InstToken) Then Exit Sub
                Dim SymbolExch As String = TrdSym & "." & Exch

                If Not TokensWatch.Contains(InstToken) Then
                    MsgBox("Symbol Not Exists", MsgBoxStyle.Exclamation, "KotakTrader")
                    Exit Sub
                End If

                Dim Idx As Integer = TokensWatch.IndexOf(InstToken)
                If Idx < 0 Then Exit Sub

                Watch.Rows(Idx).Delete()
                TokensWatch.RemoveAt(Idx)

                'Unsubscribe Quotes
                Kotak.UnSubscribeQuotes(Exch, TrdSym)
            Catch Ex As Exception
                MsgBox(Ex.Message, MsgBoxStyle.Exclamation, "KotakTrader")
            End Try
        End SyncLock
    End Sub

    Private Sub AddSymbolToMarketWatch(ByVal Exch As String, ByVal TrdSym As String)
        SyncLock (LockWatch)
            Try
                If (String.IsNullOrEmpty(Exch) OrElse String.IsNullOrEmpty(TrdSym)) Then
                    Exit Sub
                End If

                If Not Kotak.SymbolStatus Then Exit Sub
                If Kotak.LogoutStatus Then Exit Sub
                If Not Kotak.IsValidTrdsymbol(Exch, TrdSym) Then Exit Sub

                Dim InstToken As String = Kotak.GetInstToken(Exch, TrdSym).ToUpper

                If TokensWatch.Contains(InstToken) Then
                    Exit Sub
                End If

                Watch.Rows.Add(New Object() {Exch, TrdSym, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, Now.ToString("dd-MMM-yyyy HH:mm:ss"), InstToken})

                TokensWatch.Add(InstToken)

                'Subscribe Quotes
                Kotak.SubscribeQuotes(Exch, TrdSym)
            Catch Ex As Exception
                MsgBox(Ex.Message, MsgBoxStyle.Exclamation, "KotakTrader")
            End Try
        End SyncLock
    End Sub

    Private Sub AddSymbolToMarketWatch()
        SyncLock (LockWatch)
            Try
                If ComboBoxExch.SelectedIndex < 0 Then
                    MsgBox("Select Exchange", MsgBoxStyle.Exclamation, "KotakTrader")
                    ComboBoxExch.Focus()
                    Exit Sub
                End If

                If String.IsNullOrEmpty(ComboBoxSym.Text) Then
                    MsgBox("Enter Symbol", MsgBoxStyle.Exclamation, "KotakTrader")
                    ComboBoxSym.Focus()
                    Exit Sub
                End If

                Dim Exch As String = ComboBoxExch.SelectedItem
                Dim TrdSym As String = ComboBoxSym.Text

                If Not Kotak.IsValidTrdsymbol(Exch, TrdSym) Then
                    MsgBox("Enter valid Symbol", MsgBoxStyle.Exclamation, "KotakTrader")
                    ComboBoxSym.Text = ""
                    ComboBoxSym.Focus()
                    Exit Sub
                End If

                If Not Kotak.SymbolStatus Then Exit Sub
                If Kotak.LogoutStatus Then Exit Sub

                Dim InstToken As String = Kotak.GetInstToken(Exch, TrdSym).ToUpper

                If TokensWatch.Contains(InstToken) Then
                    MsgBox("Symbol Exists Already", MsgBoxStyle.Exclamation, "KotakTrader")
                    ComboBoxSym.Text = ""
                    ComboBoxSym.Focus()
                    Exit Sub
                End If

                Watch.Rows.Add(New Object() {Exch, TrdSym, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, Now.ToString("dd-MMM-yyyy HH:mm:ss"), InstToken})

                TokensWatch.Add(InstToken)

                'Subscribe Quotes
                Kotak.SubscribeQuotes(Exch, TrdSym)

                ComboBoxSym.Text = ""
            Catch Ex As Exception
                MsgBox(Ex.Message, MsgBoxStyle.Exclamation, "KotakTrader")
            End Try
        End SyncLock
    End Sub

    Private Sub ButtonAdd_Click(sender As Object, e As EventArgs) Handles ButtonAdd.Click
        AddSymbolToMarketWatch()
    End Sub

    Private Sub ComboBoxSym_KeyDown(sender As Object, e As KeyEventArgs) Handles ComboBoxSym.KeyDown
        If e.KeyCode = Keys.Enter Then
            AddSymbolToMarketWatch()
        End If
    End Sub

    Private Sub ButtonDel_Click(sender As Object, e As EventArgs) Handles ButtonDel.Click
        DelSymbolFromMarketWatch()
    End Sub

    Private Sub ButtonSubscribe_Click(sender As Object, e As EventArgs) Handles ButtonSubscribe.Click
        Try
            If MsgBox("Are you sure? This will Unsubscribe and Subscribe" & vbNewLine & "Quotes for all symbols added in Market Watch", MsgBoxStyle.OkCancel, "KotakTrader") = MsgBoxResult.Cancel Then
                Exit Sub
            End If
            Kotak.ForceSubscribe()
        Catch Ex As Exception
            MsgBox(Ex.Message, MsgBoxStyle.Exclamation, "KotakTrader")
        End Try
    End Sub

    Private Sub RichTextBox1_TextChanged(sender As Object, e As EventArgs) Handles RichTextBox1.TextChanged
        Try
            RichTextBox1.SelectionStart = RichTextBox1.Text.Length
            RichTextBox1.ScrollToCaret()
        Catch ex As Exception
        End Try
    End Sub

    Private Sub ButtonWebsock_Click(sender As Object, e As EventArgs) Handles ButtonWebsock.Click
        Try
            If MsgBox("Are you sure? This will Close and Reopen" & vbNewLine & "Websocket connection", MsgBoxStyle.OkCancel, "KotakTrader") = MsgBoxResult.Cancel Then
                Exit Sub
            End If
            Kotak.ResetWebsocket()
        Catch Ex As Exception
            MsgBox(Ex.Message, MsgBoxStyle.Exclamation, "KotakTrader")
        End Try
    End Sub

    Private Sub ComboBoxSym_KeyPress(sender As Object, e As KeyPressEventArgs) Handles ComboBoxSym.KeyPress
        If Char.IsLetter(e.KeyChar) Then
            e.KeyChar = Char.ToUpper(e.KeyChar)
        End If
    End Sub

    Private Sub ComboBoxExch_SelectedIndexChanged(sender As Object, e As EventArgs) Handles ComboBoxExch.SelectedIndexChanged
        If Not Me.IsFormLoaded Then Exit Sub
        If (ComboBoxExch.SelectedItem.ToString = "NSE" OrElse ComboBoxExch.SelectedItem.ToString = "BSE") Then
            ComboBoxSeg.Items.Clear()
            ComboBoxSeg.Items.Add("EQ")
        ElseIf ComboBoxExch.SelectedItem.ToString = "NSE_INDEX" Then
            ComboBoxSeg.Items.Clear()
            ComboBoxSeg.Items.Add("IDX")
        Else
            ComboBoxSeg.Items.Clear()
            ComboBoxSeg.Items.Add("FUT")
            ComboBoxSeg.Items.Add("OPT")
        End If

        ComboBoxSeg.SelectedIndex = 0
    End Sub

    Private Sub ComboBoxSeg_SelectedIndexChanged(sender As Object, e As EventArgs) Handles ComboBoxSeg.SelectedIndexChanged
        If Not Me.IsFormLoaded Then Exit Sub
        If ComboBoxExch.SelectedItem.ToString = "NSE" Then
            ComboBoxSym.AutoCompleteCustomSource = AutoCompleteNse
        ElseIf ComboBoxExch.SelectedItem.ToString = "BSE" Then
            ComboBoxSym.AutoCompleteCustomSource = AutoCompleteBse
        ElseIf ComboBoxExch.SelectedItem.ToString = "NSE_INDEX" Then
            ComboBoxSym.AutoCompleteCustomSource = AutoCompleteIdx
        ElseIf ComboBoxExch.SelectedItem.ToString = "NFO" Then
            If ComboBoxSeg.SelectedItem.ToString = "FUT" Then
                ComboBoxSym.AutoCompleteCustomSource = AutoCompleteNfoFut
            Else
                ComboBoxSym.AutoCompleteCustomSource = AutoCompleteNfoOpt
            End If
        ElseIf ComboBoxExch.SelectedItem.ToString = "CDS" Then
            If ComboBoxSeg.SelectedItem.ToString = "FUT" Then
                ComboBoxSym.AutoCompleteCustomSource = AutoCompleteCdsFut
            Else
                ComboBoxSym.AutoCompleteCustomSource = AutoCompleteCdsOpt
            End If
        ElseIf ComboBoxExch.SelectedItem.ToString = "MCX" Then
            If ComboBoxSeg.SelectedItem.ToString = "FUT" Then
                ComboBoxSym.AutoCompleteCustomSource = AutoCompleteMcxFut
            Else
                ComboBoxSym.AutoCompleteCustomSource = AutoCompleteMcxOpt
            End If
        End If
        ComboBoxSym.SelectedIndex = -1
        ComboBoxSym.Text = ""
    End Sub

    Private Sub ButtonRefresh_Click(sender As Object, e As EventArgs) Handles ButtonRefresh.Click
        If Kotak.LogoutStatus Then
            MsgBox("User loggedout", MsgBoxStyle.Exclamation, "KotakTrader")
            Exit Sub
        End If

        If Not Kotak.LoginStatus Then
            MsgBox("User not loggedin", MsgBoxStyle.Exclamation, "KotakTrader")
            Exit Sub
        End If

        If Not Kotak.SymbolStatus Then
            MsgBox("Symbol not downloaded", MsgBoxStyle.Exclamation, "KotakTrader")
            Exit Sub
        End If

        If TabControl1.SelectedIndex = 0 Then
            TabControl1.SelectedIndex = 1
        End If

        FetchReports()
    End Sub

    Private Sub FetchReports()
        Try

            Dim ReportType As String = ComboBoxReports.SelectedItem
            If String.IsNullOrEmpty(ReportType) Then
                MsgBox("Select the report to download", MsgBoxStyle.Exclamation, "KotakTrader")
                Exit Sub
            End If

            Dim Report As String = String.Empty
            If ReportType.Equals("OrderBook") Then
                Report = Kotak.GetOrderBook
            ElseIf ReportType.Equals("TradeBook") Then
                Report = Kotak.GetTradeBook
            ElseIf ReportType.Equals("DayPositions") Then
                Report = Kotak.GetDayPositions
            ElseIf ReportType.Equals("NetPositions") Then
                Report = Kotak.GetNetPositions
            ElseIf ReportType.Equals("Holdings") Then
                Report = Kotak.GetHoldings
            ElseIf ReportType.Equals("Funds") Then
                Report = Kotak.GetFunds
            ElseIf ReportType.Equals("BridgePositions") Then
                Report = Kotak.GetBridgePositions
            ElseIf ReportType.Equals("Strategies") Then
                Report = Kotak.GetStrategies
            ElseIf ReportType.Equals("BridgeRequests") Then
                Report = Kotak.GetBridgeRequests
            Else
                MsgBox("Select the report to download", MsgBoxStyle.Information, "KotakTrader")
                Exit Sub
            End If

            If String.IsNullOrEmpty(Report) Then Exit Sub

            Dim Records() As String = Report.Split(New String() {vbNewLine}, StringSplitOptions.RemoveEmptyEntries)
            If Not (Records.Length > 0) Then Exit Sub

            Dim cols() As String = Records(0).Split(New String() {","}, StringSplitOptions.None)
            If Not (cols.Length > 2) Then Exit Sub

            Dim watch As New DataTable
            For Each col As String In cols
                watch.Columns.Add(col, GetType(String))
            Next

            For i = 1 To Records.Length - 1
                watch.Rows.Add(Records(i).Split(New String() {","}, StringSplitOptions.None))
            Next

            DataGridView2.DataSource = Nothing
            DataGridView2.Columns.Clear()

            For Each Col As DataGridViewColumn In DataGridView2.Columns
                Col.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter
            Next

            DataGridView2.ReadOnly = True
            DataGridView2.MultiSelect = False
            DataGridView2.RowHeadersVisible = False

            DataGridView2.AllowUserToAddRows = False
            DataGridView2.AllowUserToDeleteRows = False
            DataGridView2.AllowUserToOrderColumns = True
            DataGridView2.AllowUserToResizeColumns = True
            DataGridView2.AllowUserToResizeRows = True

            DataGridView2.DataSource = watch
        Catch ex As Exception
            MsgBox(ex.Message, MsgBoxStyle.Exclamation, "KotakTrader")
        End Try
    End Sub

    Private Sub Trader_Shown(sender As Object, e As EventArgs) Handles Me.Shown
        Me.IsFormLoaded = True
    End Sub

    Private Sub TabControl1_SelectedIndexChanged(sender As Object, e As EventArgs) Handles TabControl1.SelectedIndexChanged
        If Not Me.IsFormLoaded Then Exit Sub
        If Not Kotak.SymbolStatus Then Exit Sub
        If TabControl1.SelectedIndex = 1 Then
            FetchReports()
        End If
    End Sub

    Private Sub ComboReports_SelectedIndexChanged(sender As Object, e As EventArgs) Handles ComboBoxReports.SelectedIndexChanged
        If Not Me.IsFormLoaded Then Exit Sub
        If Not Kotak.SymbolStatus Then Exit Sub
        If Not (TabControl1.SelectedIndex = 1) Then
            TabControl1.SelectedIndex = 1
        End If

        If ComboBoxReports.SelectedIndex = 0 Then
            DataGridView2.ContextMenuStrip = MenuOrderBook
        ElseIf ComboBoxReports.SelectedIndex = 3 Then
            DataGridView2.ContextMenuStrip = MenuPositions
        ElseIf ComboBoxReports.SelectedIndex = 4 Then
            DataGridView2.ContextMenuStrip = MenuHoldings
        Else
            DataGridView2.ContextMenuStrip = MenuOthers
        End If

        FetchReports()
    End Sub

    Private Sub MenuWatch_Opening(sender As Object, e As System.ComponentModel.CancelEventArgs) Handles MenuWatch.Opening
        If Not (DataGridView1.Rows.Count > 0) Then
            e.Cancel = True
            Exit Sub
        End If

        If Not (DataGridView1.SelectedCells.Count > 0) Then
            BuyToolStripMenuItem1.Enabled = False
            SellToolStripMenuItem1.Enabled = False
            ExportToolStripMenuItem.Enabled = False
            Exit Sub
        End If

        If Not (Kotak.SymbolStatus) Then
            BuyToolStripMenuItem1.Enabled = False
            SellToolStripMenuItem1.Enabled = False
            ExportToolStripMenuItem.Enabled = False
            Exit Sub
        End If

        Dim RowIndex As Integer = DataGridView1.SelectedCells(0).RowIndex
        Dim exch As String = DataGridView1.Rows(RowIndex).Cells(0).Value

        If exch.Equals("NSE_INDEX") Then
            BuyToolStripMenuItem1.Enabled = False
            SellToolStripMenuItem1.Enabled = False
        Else
            BuyToolStripMenuItem1.Enabled = True
            SellToolStripMenuItem1.Enabled = True
        End If
    End Sub

    Private Sub ExportToCsv(ByRef Dgv As DataGridView)
        Try
            If Dgv Is Nothing Then Exit Sub
            If Not (Dgv.Rows.Count > 0) Then Exit Sub

            SaveFileDialog1.OverwritePrompt = True
            SaveFileDialog1.Title = "Export"
            SaveFileDialog1.Filter = "Csv Files (*.csv)|*.csv"

            If SaveFileDialog1.ShowDialog() = DialogResult.Cancel Then Exit Sub
            If String.IsNullOrEmpty(SaveFileDialog1.FileName) Then Exit Sub
            Dim FileName As String = SaveFileDialog1.FileName

            Dim Csv As New StringBuilder
            For Each Col As DataGridViewColumn In Dgv.Columns
                Csv.Append(Col.HeaderText & ",")
            Next
            Csv.Remove(Csv.Length - 1, 1)
            Csv.AppendLine()

            For Each Row As DataGridViewRow In Dgv.Rows
                For Each Cell As DataGridViewCell In Row.Cells
                    Csv.Append(Cell.Value & ",")
                Next
                Csv.Remove(Csv.Length - 1, 1)
                Csv.AppendLine()
            Next

            Csv.Remove(Csv.Length - 2, 2)
            File.WriteAllText(FileName, Csv.ToString())
        Catch ex As Exception
            MsgBox(ex.Message, MsgBoxStyle.Exclamation, "KotakTrader")
        End Try
    End Sub

    Private Sub ExportToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles ExportToolStripMenuItem.Click
        ExportToCsv(DataGridView1)
    End Sub

    Private Sub BuyToolStripMenuItem1_Click(sender As Object, e As EventArgs) Handles BuyToolStripMenuItem1.Click
        Try
            Dim RowIndex As Integer = DataGridView1.SelectedCells(0).RowIndex
            Dim exch As String = DataGridView1.Rows(RowIndex).Cells(0).Value
            Dim trdsym As String = DataGridView1.Rows(RowIndex).Cells(1).Value
            Kotak.ShowOrderWindow("REGULAR", exch, trdsym, "BUY", "MIS", "LIMIT")
        Catch ex As Exception
            MsgBox(ex.Message, MsgBoxStyle.Exclamation, "KotakTrader")
        End Try
    End Sub

    Private Sub SellToolStripMenuItem1_Click(sender As Object, e As EventArgs) Handles SellToolStripMenuItem1.Click
        Try
            Dim RowIndex As Integer = DataGridView1.SelectedCells(0).RowIndex
            Dim exch As String = DataGridView1.Rows(RowIndex).Cells(0).Value
            Dim trdsym As String = DataGridView1.Rows(RowIndex).Cells(1).Value
            Kotak.ShowOrderWindow("REGULAR", exch, trdsym, "SELL", "MIS", "LIMIT")
        Catch ex As Exception
            MsgBox(ex.Message, MsgBoxStyle.Exclamation, "KotakTrader")
        End Try
    End Sub

    Private Sub ExportToolStripMenuItem1_Click(sender As Object, e As EventArgs) Handles ExportToolStripMenuItem1.Click
        ExportToCsv(DataGridView2)
    End Sub

    Private Sub ExportToolStripMenuItem2_Click(sender As Object, e As EventArgs) Handles ExportToolStripMenuItem2.Click
        ExportToCsv(DataGridView2)
    End Sub

    Private Sub MenuOrderBook_Opening(sender As Object, e As System.ComponentModel.CancelEventArgs) Handles MenuOrderBook.Opening
        If Not (DataGridView2.Rows.Count > 0) Then
            e.Cancel = True
            Exit Sub
        End If

        If Not (DataGridView2.SelectedCells.Count > 0) Then
            ModifyToolStripMenuItem.Enabled = False
            CancelToolStripMenuItem1.Enabled = False
            ExportToolStripMenuItem.Enabled = False
            Exit Sub
        End If

        If Not (Kotak.SymbolStatus) Then
            ModifyToolStripMenuItem.Enabled = False
            CancelToolStripMenuItem1.Enabled = False
            ExportToolStripMenuItem.Enabled = False
            Exit Sub
        End If

        '//Get Status
        Dim RowIndex As Integer = DataGridView2.SelectedCells(0).RowIndex
        Dim Status As String = DataGridView2.Rows(RowIndex).Cells(1).Value

        If (Status.Equals("COMPLETE") OrElse Status.Equals("CANCELLED") OrElse Status.Equals("REJECTED")) Then
            ModifyToolStripMenuItem.Enabled = False
            CancelToolStripMenuItem1.Enabled = False
        Else
            ModifyToolStripMenuItem.Enabled = True
            CancelToolStripMenuItem1.Enabled = True
        End If

        ExportToolStripMenuItem.Enabled = True
    End Sub

    Private Sub CancelToolStripMenuItem1_Click(sender As Object, e As EventArgs) Handles CancelToolStripMenuItem1.Click
        Try
            Dim RowIndex As Integer = DataGridView2.SelectedCells(0).RowIndex
            Dim OrderID As String = DataGridView2.Rows(RowIndex).Cells(0).Value
            Kotak.CancelRegularOrder(OrderID)
            FetchReports()
        Catch ex As Exception
            MsgBox(ex.Message, MsgBoxStyle.Exclamation, "KotakTrader")
        End Try
    End Sub

    Private Sub ModifyToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles ModifyToolStripMenuItem.Click
        Try
            Dim RowIndex As Integer = DataGridView2.SelectedCells(0).RowIndex
            Dim OrderID As String = DataGridView2.Rows(RowIndex).Cells(0).Value
            Kotak.ShowModifyWindow(OrderID, True)
            FetchReports()
        Catch ex As Exception
            MsgBox(ex.Message, MsgBoxStyle.Exclamation, "KotakTrader")
        End Try
    End Sub

    Private Sub ToolStripMenuItem6_Click(sender As Object, e As EventArgs) Handles ToolStripMenuItem6.Click
        Process.Start("https://github.com/howutrade/kotak-trader/issues")
    End Sub

    Private Sub APIReferenceToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles APIReferenceToolStripMenuItem.Click
        Process.Start("https://howutrade.github.io/kotaknet-doc/")
    End Sub

    Private Sub BuyToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles BuyToolStripMenuItem.Click
        Try
            If Not Kotak.SymbolStatus Then Exit Sub
            Kotak.ShowOrderWindow("REGULAR", "NSE", "", "BUY")
        Catch ex As Exception
            MsgBox(ex.Message, MsgBoxStyle.Exclamation, "KotakTrader")
        End Try
    End Sub

    Private Sub SellToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles SellToolStripMenuItem.Click
        Try
            If Not Kotak.SymbolStatus Then Exit Sub
            Kotak.ShowOrderWindow("REGULAR", "NSE", "", "SELL")
        Catch ex As Exception
            MsgBox(ex.Message, MsgBoxStyle.Exclamation, "KotakTrader")
        End Try
    End Sub

    Private Sub Panel6_Paint(sender As Object, e As System.Windows.Forms.PaintEventArgs) Handles Panel6.Paint
        Dim rect As Rectangle = Me.Panel6.ClientRectangle
        Dim pen As New Pen(Color.FromArgb(220, 220, 240), 2)
        e.Graphics.DrawRectangle(pen, rect)
    End Sub

    Private Sub Panel3_Paint(sender As Object, e As System.Windows.Forms.PaintEventArgs) Handles Panel3.Paint
        Dim rect As Rectangle = Me.Panel3.ClientRectangle
        Dim pen As New Pen(Color.FromArgb(220, 220, 240), 2)
        e.Graphics.DrawRectangle(pen, rect)
    End Sub

    Private Sub Panel4_Paint(sender As Object, e As System.Windows.Forms.PaintEventArgs) Handles Panel4.Paint
        Dim rect As Rectangle = Me.Panel4.ClientRectangle
        Dim pen As New Pen(Color.FromArgb(220, 220, 240), 2)
        e.Graphics.DrawRectangle(pen, rect)
    End Sub

    Private Sub ToolStripMenuItem3_Click(sender As Object, e As EventArgs) Handles ExportToolStripMenuItem3.Click
        ExportToCsv(DataGridView2)
    End Sub

    Private Sub ToolStripMenuItem4_Click(sender As Object, e As EventArgs) Handles ExportToolStripMenuItem4.Click
        ExportToCsv(DataGridView2)
    End Sub

    Private Sub MenuOthers_Opening(sender As Object, e As System.ComponentModel.CancelEventArgs) Handles MenuOthers.Opening
        If Not (DataGridView2.Rows.Count > 0) Then
            e.Cancel = True
            Exit Sub
        End If

        If Not (DataGridView2.SelectedCells.Count > 0) Then
            ExportToolStripMenuItem2.Enabled = False
            Exit Sub
        End If

        If Not (Kotak.SymbolStatus) Then
            ExportToolStripMenuItem2.Enabled = False
            Exit Sub
        End If

        ExportToolStripMenuItem2.Enabled = True
    End Sub

    Private Sub MenuPositions_Opening(sender As Object, e As System.ComponentModel.CancelEventArgs) Handles MenuPositions.Opening
        If Not (DataGridView2.Rows.Count > 0) Then
            e.Cancel = True
            Exit Sub
        End If

        If Not (DataGridView2.SelectedCells.Count > 0) Then
            ExitPosToolStripMenuItem.Enabled = False
            ExportToolStripMenuItem3.Enabled = False
            Exit Sub
        End If

        If Not (Kotak.SymbolStatus) Then
            ExitPosToolStripMenuItem.Enabled = False
            ExportToolStripMenuItem3.Enabled = False
            Exit Sub
        End If

        '//Get Status
        Dim RowIndex As Integer = DataGridView2.SelectedCells(0).RowIndex
        Dim NetQty As Integer
        Integer.TryParse(DataGridView2.Rows(RowIndex).Cells(4).Value, NetQty)

        If NetQty = 0 Then
            ExitPosToolStripMenuItem.Enabled = False
        Else
            ExitPosToolStripMenuItem.Enabled = True
        End If

        ExportToolStripMenuItem3.Enabled = True
    End Sub

    Private Sub MenuHoldings_Opening(sender As Object, e As System.ComponentModel.CancelEventArgs) Handles MenuHoldings.Opening
        If Not (DataGridView2.Rows.Count > 0) Then
            e.Cancel = True
            Exit Sub
        End If

        If Not (DataGridView2.SelectedCells.Count > 0) Then
            ExitHoldToolStripMenuItem.Enabled = False
            ExportToolStripMenuItem4.Enabled = False
            Exit Sub
        End If

        If Not (Kotak.SymbolStatus) Then
            ExitHoldToolStripMenuItem.Enabled = False
            ExportToolStripMenuItem4.Enabled = False
            Exit Sub
        End If

        '//Get Status
        Dim RowIndex As Integer = DataGridView2.SelectedCells(0).RowIndex
        Dim NetQty As Integer
        Integer.TryParse(DataGridView2.Rows(RowIndex).Cells(3).Value, NetQty)
        Dim StockBal As Integer
        Integer.TryParse(DataGridView2.Rows(RowIndex).Cells(2).Value, StockBal)

        'Dim IsOpen As Boolean = (StockBal > 0 AndAlso (NetQty = 0 OrElse (StockBal + NetQty) = 0))
        Dim IsOpen As Boolean = (StockBal > 0 AndAlso NetQty = 0)

        If Not IsOpen Then
            ExitHoldToolStripMenuItem.Enabled = False
        Else
            ExitHoldToolStripMenuItem.Enabled = True
        End If

        ExportToolStripMenuItem4.Enabled = True
    End Sub

    Private Sub ExitPosToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles ExitPosToolStripMenuItem.Click
        Try
            Dim RowIndex As Integer = DataGridView2.SelectedCells(0).RowIndex
            Dim Exch As String = DataGridView2.Rows(RowIndex).Cells(0).Value
            Dim ProdType As String = DataGridView2.Rows(RowIndex).Cells(2).Value
            Dim Token As String = DataGridView2.Rows(RowIndex).Cells(7).Value

            Dim NetQty As Integer
            Integer.TryParse(DataGridView2.Rows(RowIndex).Cells(4).Value, NetQty)

            If Not (NetQty = 0) Then
                Dim Trans As String = If(NetQty > 0, "SELL", "BUY")
                Dim TrdSym As String = Kotak.GetTrdsym(Exch, Token)
                Kotak.SubscribeQuotes(Exch, TrdSym) '//CHECK

                Dim OrdType As String = "LIMIT"
                Dim Qty As Integer = Math.Abs(NetQty)
                Dim LmtPrice As Double

                If Trans.Equals("BUY") Then
                    LmtPrice = Kotak.GetUpperCircuit(Exch, TrdSym)
                    If Not (LmtPrice > 0) Then
                        LmtPrice = Kotak.GetHigh(Exch, TrdSym)
                        If Not (LmtPrice > 0) Then
                            OrdType = "MARKET"
                        End If
                    End If
                Else
                    LmtPrice = Kotak.GetLowerCircuit(Exch, TrdSym)
                    If Not (LmtPrice > 0) Then
                        LmtPrice = Kotak.GetLow(Exch, TrdSym)
                        If Not (LmtPrice > 0) Then
                            OrdType = "MARKET"
                        End If
                    End If
                End If

                Kotak.PlaceRegularOrder(Exch, TrdSym, Trans, OrdType, Qty, ProdType, LmtPrice, 0, 0, "DAY", Kotak.GetRequestId, "EXT", Kotak.GetUniqueString)
                FetchReports()
            End If
        Catch ex As Exception
            MsgBox(ex.Message, MsgBoxStyle.Exclamation, "KotakTrader")
        End Try
    End Sub

    Private Sub ExitHoldToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles ExitHoldToolStripMenuItem.Click
        Try
            Dim RowIndex As Integer = DataGridView2.SelectedCells(0).RowIndex
            Dim Exch As String = DataGridView2.Rows(RowIndex).Cells(0).Value
            Dim Token As String = DataGridView2.Rows(RowIndex).Cells(8).Value

            Dim NetQty As Integer
            Integer.TryParse(DataGridView2.Rows(RowIndex).Cells(3).Value, NetQty)

            Dim StockBal As Integer
            Integer.TryParse(DataGridView2.Rows(RowIndex).Cells(2).Value, StockBal)

            'Dim IsOpen As Boolean = (StockBal > 0 AndAlso (NetQty = 0 OrElse (StockBal + NetQty) = 0))
            Dim IsOpen As Boolean = (StockBal > 0 AndAlso NetQty = 0)

            If IsOpen Then
                Dim Trans As String = "SELL"
                Dim TrdSym As String = Kotak.GetTrdsym(Exch, Token)
                Kotak.SubscribeQuotes(Exch, TrdSym) '//CHECK

                Dim OrdType As String = "LIMIT"
                Dim Qty As Integer = Math.Abs(StockBal)
                Dim LmtPrice As Double

                LmtPrice = Kotak.GetLowerCircuit(Exch, TrdSym)
                If Not (LmtPrice > 0) Then
                    LmtPrice = Kotak.GetLow(Exch, TrdSym)
                    If Not (LmtPrice > 0) Then
                        OrdType = "MARKET"
                    End If
                End If

                Kotak.PlaceRegularOrder(Exch, TrdSym, Trans, OrdType, Qty, "NRML", LmtPrice, 0, 0, "DAY", Kotak.GetRequestId, "EXT", Kotak.GetUniqueString)
                FetchReports()
            End If
        Catch ex As Exception
            MsgBox(ex.Message, MsgBoxStyle.Exclamation, "KotakTrader")
        End Try
    End Sub
End Class
