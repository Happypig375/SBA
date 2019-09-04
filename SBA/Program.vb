Imports System.Console
Imports System.Collections.ObjectModel
Imports Unicode = System.Text.UnicodeEncoding

Module SunnysBigAdventure
    Structure Point
        Public Sub New(left As Integer, top As Integer)
            Me.Left = left
            Me.Top = top
        End Sub
        Public ReadOnly Property Left As Integer
        Public ReadOnly Property Top As Integer
        Public Overrides Function ToString() As String
            Return $"({Left}, {Top})"
        End Function
        Public ReadOnly Property ToUp As Point
            Get
                Return New Point(Left, Math.Max(Top - 1, 0))
            End Get
        End Property
        Public ReadOnly Property ToDown As Point
            Get
                Return New Point(Left, Math.Min(BufferHeight, Top + 1))
            End Get
        End Property
        Public ReadOnly Property ToLeft As Point
            Get
                Return New Point(Math.Max(Left - 1, 0), Top)
            End Get
        End Property
        Public ReadOnly Property ToRight As Point
            Get
                Return New Point(Math.Min(Left + 1, BufferWidth), Top)
            End Get
        End Property
    End Structure
    Structure Rectangle
        Public Sub New(top As Integer, left As Integer, width As Integer, height As Integer)
            Me.New(New Point(top, left), width, height)
        End Sub
        Public Sub New(topLeft As Point, bottomRight As Point)
            Me.New(topLeft, bottomRight.Left - topLeft.Left + 1, bottomRight.Top - topLeft.Top + 1)
        End Sub
        Public Sub New(topLeft As Point, width As Integer, height As Integer)
            Me.TopLeft = topLeft
            Me.Width = width
            Me.Height = height
        End Sub
        Public ReadOnly Property TopLeft As Point
        Public ReadOnly Property Width As Integer
        Public ReadOnly Property Height As Integer
        Public ReadOnly Property Left As Integer
            Get
                Return TopLeft.Left
            End Get
        End Property
        Public ReadOnly Property Right As Integer
            Get
                Return TopLeft.Left + Width - 1
            End Get
        End Property
        Public ReadOnly Property Top As Integer
            Get
                Return TopLeft.Top
            End Get
        End Property
        Public ReadOnly Property Bottom As Integer
            Get
                Return TopLeft.Top + Height - 1
            End Get
        End Property
        Function Contains(point As Point) As Boolean
            Return Left <= point.Left And point.Left <= Right And Top <= point.Top And point.Top <= Bottom
        End Function
        Public Overrides Function ToString() As String
            Return $"({Left}, {Top}) to ({Right}, {Bottom})"
        End Function
    End Structure
    Structure Sprite
        Public Sub New(display As Char, color As ConsoleColor) ' Consoles don't support surrogate pairs
            Me.Display = display
            Me.Color = color
        End Sub
        Public ReadOnly Property Display As Char
        Public ReadOnly Property Color As ConsoleColor
    End Structure
    Class Entity
        Public Sub New(sprite As Sprite)
            Me.Sprite = sprite
        End Sub
        Dim _position As Point?
        Dim _sprite As Sprite
        Sub Draw(newPosition As Point?)
            If _position.HasValue Then
                CursorPosition = _position.GetValueOrDefault()
                Write(" "c)
            End If
            If newPosition.HasValue Then
                CursorPosition = newPosition.GetValueOrDefault()
                ForegroundColor = _sprite.Color
                Write(_sprite.Display)
                _position = newPosition
            End If
        End Sub
        Public Property Sprite As Sprite
            Get
                Return _sprite
            End Get
            Set(value As Sprite)
                _sprite = value
                Draw(_position)
            End Set
        End Property
        Public Property Position As Point?
            Get
                Return _position
            End Get
            Set(value As Point?)
                If value Is Nothing Then
                    Draw(value)
                    Return
                End If
                For Each entity In Entities
                    If entity.Position.GetValueOrDefault().Equals(value) Then Return
                Next
                For Each rect In Solids
                    If rect.Contains(value) Then Return
                Next
                Draw(value)
            End Set
        End Property
    End Class
    Property CursorPosition As Point
        Get
            Return New Point(CursorLeft, CursorTop)
        End Get
        Set(value As Point)
            SetCursorPosition(value.Left, value.Top)
        End Set
    End Property
    Function ReadKey(timeout As TimeSpan) As ConsoleKey?
        If KeyAvailable Then Return Console.ReadKey(True).Key
        Dim beginWait = Date.Now
        While Not KeyAvailable And Date.Now.Subtract(beginWait) < timeout
            Threading.Thread.Sleep(100)
            If KeyAvailable Then Return Console.ReadKey(True).Key
        End While
        Return Nothing
    End Function
    Sub Redraw()
        ResetColor()
        Console.Clear()
        For Each rect In Solids
            CursorPosition = rect.TopLeft
            Write("┏"c)
            For i = 1 To rect.Width - 2
                Write("━"c)
            Next
            Write("┓"c)
            For y = 1 To rect.Height - 2
                SetCursorPosition(rect.Left, y)
                Write("┃"c)
                SetCursorPosition(rect.Right, y)
                Write("┃"c)
            Next
            SetCursorPosition(rect.Left, rect.Bottom)
            Write("┗"c)
            For i = 1 To rect.Width - 2
                Write("━"c)
            Next
            Write("┛"c)
        Next
        For Each entity In Entities
            If entity.Position.HasValue Then
                CursorPosition = entity.Position.GetValueOrDefault()
                ForegroundColor = entity.Sprite.Color
                Write(entity.Sprite.Display)
            End If
        Next
    End Sub
    Sub Clear()
        Entities.Clear()
        Text.Clear()
        Solids.Clear()
        Console.Clear()
    End Sub

    'Unicode: 
    '1.1☺☹☠❣❤✌☝✍♨✈⌛⌚☀☁☂❄☃☄♠♥♦♣♟☎⌨✉✏✒✂☢☣↗➡↘↙↖↕↔↩↪✡☸☯✝☦☪☮♈♉♊♋♌♍♎♏♐♑♒♓▶◀♀♂☑✔✖✳✴❇‼〰©®™Ⓜ
    '1.1㊗㊙▪▫☜♅♪♜☌♘☛♞☵☒♛♢✎‍♡☼☴♆☲☇♇☏☨☧☤☥♭☭☽☾❥☍☋☊☬♧☉#☞☶♁♤☷✐♮♖★♝*☰☫♫♙♃☚♬☩♄☓♯☟☈☻☱♕☳♔♩♚♗☡☐
    '3.0⁉♱♰☙
    '3.2⤴⤵♻〽◼◻◾◽☖♷⚁⚄⚆⚈♼☗♵⚉⚀⚇♹♲♸⚂♺♴⚅♳♽⚃♶
    '4.0☕☔⚡⚠⬆⬇⬅⏏⚏⚋⚎⚑⚊⚍⚐⚌
    '4.1☘⚓⚒⚔⚙⚖⚗⚰⚱♿⚛⚕♾⚜⚫⚪⚩⚭⚢⚥⚘⚤⚦⚨⚣⚬⚮⚚⚯⚧
    '5.0⚲
    '5.1⭐⬛⬜⛁⚶⚼⛃⚸⚴⚹⚳⚵⚻⚷⛀⚝⚺⛂
    '5.2⛷⛹⛰⛪⛩⛲⛺⛽⛵⛴⛅⛈⛱⛄⚽⚾⛳⛸⛑’⛏⛓⛔⭕❗⛟⛙⛞⛮⛶⛯⛜⛡⛿⛣⛊⛐⛾⛉⛚⛘⛠⛆⛝⛌⛕⛬⛍⛫⛖⚞⛨⚟⛻⛋⛒⛛⛭⛇⛼⚿⛗
    '6.0✋✊⏳⏰⏱⏲✨⛎⏩⏭⏯⏪⏮⏫⏬✅❌❎➕➖➗➰➿❓❔❕⛧⛢⛤ // Right-Handed interlaced pentagram: ⛥ Left-Handed interlaced pentagram: ⛦
    '7.0⏸⏹⏺
    '10.₿
    ReadOnly Sunny_ As New Sprite("☺", ConsoleColor.White)
    ReadOnly Sunny_Angry As New Sprite("☹", ConsoleColor.Red)
    ReadOnly Sunny As New Entity(Sunny_)

    ReadOnly Sun_ As New Sprite("☼", ConsoleColor.Yellow)
    ReadOnly Sun As New Entity(Sun_)

    ReadOnly Horsey_ As New Sprite("♘", ConsoleColor.Magenta)
    ReadOnly Horsey_Dead As New Sprite("♞", ConsoleColor.DarkMagenta)
    ReadOnly Horsey As New Entity(Horsey_)

    ReadOnly Entities As New List(Of Entity) From {Sun, Horsey, Sunny}
    ReadOnly Text As New Dictionary(Of Point, String)
    ReadOnly Solids As New ObservableCollection(Of Rectangle)

    Sub Main()
        OutputEncoding = New Unicode()
        If LargestWindowWidth < 48 Or LargestWindowHeight < 10 Then
            WriteLine("ERROR: Please decrease font size")
            Return
        End If
        WindowWidth = 48
        WindowHeight = 10
        AddHandler Solids.CollectionChanged, Sub(sender, e)

                                             End Sub
        Solids.Add(New Rectangle(0, 0, 2, 2))
        Sunny.Position = New Point(3, 3)
        Sun.Position = New Point(3, 6)
        While True
            Dim key = ReadKey(TimeSpan.FromSeconds(1))
            Select Case key
                Case ConsoleKey.LeftArrow
                    If Sunny.Position.HasValue Then Sunny.Position = Sunny.Position.GetValueOrDefault().ToLeft
                    Debug.WriteLine(ConsoleKey.LeftArrow)
                Case ConsoleKey.RightArrow
                    If Sunny.Position.HasValue Then Sunny.Position = Sunny.Position.GetValueOrDefault().ToRight
                    Debug.WriteLine(ConsoleKey.RightArrow)
                Case ConsoleKey.UpArrow
                    If Sunny.Position.HasValue Then Sunny.Position = Sunny.Position.GetValueOrDefault().ToUp
                    Debug.WriteLine(ConsoleKey.UpArrow)
                Case ConsoleKey.DownArrow
                    If Sunny.Position.HasValue Then Sunny.Position = Sunny.Position.GetValueOrDefault().ToDown
                    Debug.WriteLine(ConsoleKey.DownArrow)
                Case Else
                    Debug.WriteLine(key)
            End Select
        End While
    End Sub
End Module
