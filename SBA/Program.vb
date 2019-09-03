Imports System.Console
Imports Unicode = System.Text.UnicodeEncoding

Module SunnysBigAdventure
    Public Class BiDictionary(Of T1, T2)
        Implements IEnumerable(Of KeyValuePair(Of T1, T2))
        ReadOnly to1 As New Dictionary(Of T2, T1)()
        ReadOnly to2 As New Dictionary(Of T1, T2)()
        Public Sub Add(t1 As T1, t2 As T2)
            to1.Add(t2, t1)
            to2.Add(t1, t2)
        End Sub
        Public Sub Remove(t1 As T1)
            Dim t2 As T2
            If to2.Remove(t1, t2) Then
                to1.Remove(t2)
            Else
                Throw New KeyNotFoundException()
            End If
        End Sub
        Public Sub Remove(t2 As T2)
            Dim t1 As T1
            If to1.Remove(t2, t1) Then
                to2.Remove(t1)
            Else
                Throw New KeyNotFoundException()
            End If
        End Sub
        Public Sub Clear()
            to1.Clear()
            to2.Clear()
        End Sub
        Default Public Property [Get](t2 As T2)
            Get
                Return to1(t2)
            End Get
            Set(value)
                to1(t2) = value
            End Set
        End Property
        Default Public Property [Get](t1 As T1)
            Get
                Return to2(t1)
            End Get
            Set(value)
                to2(t1) = value
            End Set
        End Property
        Public Function GetEnumerator() As IEnumerator(Of KeyValuePair(Of T1, T2)) Implements IEnumerable(Of KeyValuePair(Of T1, T2)).GetEnumerator
            Return to2.GetEnumerator()
        End Function
        Private Function IEnumerable_GetEnumerator() As IEnumerator Implements IEnumerable.GetEnumerator
            Return to2.GetEnumerator()
        End Function
    End Class
    Structure Point
        Public Sub New(x As Integer, y As Integer)
            Me.X = x
            Me.Y = y
        End Sub
        Public ReadOnly Property X As Integer
        Public ReadOnly Property Y As Integer
        Public Overrides Function ToString() As String
            Return $"({X}, {Y})"
        End Function
    End Structure
    Structure Rectangle
        Public Sub New(top As Integer, left As Integer, width As Integer, height As Integer)
            Me.New(New Point(top, left), width, height)
        End Sub
        Public Sub New(topLeft As Point, bottomRight As Point)
            Me.New(topLeft, bottomRight.X - topLeft.X + 1, bottomRight.Y - topLeft.Y + 1)
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
                Return TopLeft.X
            End Get
        End Property
        Public ReadOnly Property Right As Integer
            Get
                Return TopLeft.X + Width - 1
            End Get
        End Property
        Public ReadOnly Property Top As Integer
            Get
                Return TopLeft.Y
            End Get
        End Property
        Public ReadOnly Property Bottom As Integer
            Get
                Return TopLeft.Y + Height - 1
            End Get
        End Property
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
        Public Property Sprite As Sprite
    End Class
    Property CursorPosition As Point
        Get
            Return New Point(CursorLeft, CursorTop)
        End Get
        Set(value As Point)
            SetCursorPosition(value.X, value.Y)
        End Set
    End Property
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
        For Each sprite In Entities
            CursorPosition = sprite.Key
            ForegroundColor = sprite.Value.Sprite.Color
            Write(sprite.Value.Sprite.Display)
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

    ReadOnly Entities As New BiDictionary(Of Point, Entity)
    ReadOnly Text As New Dictionary(Of Point, String)
    ReadOnly Solids As New List(Of Rectangle)

    Sub Main()
        OutputEncoding = New Unicode()
        WindowWidth = 40
        WindowHeight = 20
        While True
            Solids.Add(New Rectangle(0, 0, 2, 2))
            Entities.Add(New Point(3, 3), Sunny)
            Entities.Add(New Point(3, 6), Sun)
            Redraw()
            Dim beginWait = Date.Now
            While Not KeyAvailable And Date.Now.Subtract(beginWait).TotalSeconds < 5
                Threading.Thread.Sleep(250)
                If KeyAvailable Then
                    Select Case ReadKey(True).Key
                        Case ConsoleKey.LeftArrow

                        Case ConsoleKey.RightArrow
                        Case ConsoleKey.UpArrow
                        Case ConsoleKey.DownArrow
                    End Select
                End If
            End While
        End While
    End Sub
End Module
