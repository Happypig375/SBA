Imports System.Console
Imports System.Collections.ObjectModel
Imports Encoding = System.Text.Encoding
Imports SBA

Module SunnysBigAdventure
#Region "Structures"
    Structure Delta(Of T)
        Sub New(unchanged As T)
            Changed = False
            OldValue = unchanged
            NewValue = unchanged
        End Sub
        Sub New(oldValue As T, newValue As T)
            Changed = True
            Me.OldValue = oldValue
            Me.NewValue = newValue
        End Sub
        Public ReadOnly Property Changed As Boolean
        Public ReadOnly Property OldValue As T
        Public ReadOnly Property NewValue As T
    End Structure
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
    End Structure
    Structure Rectangle
        Public Sub New(left As Integer, top As Integer, width As Integer, height As Integer)
            Me.New(New Point(left, top), width, height)
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
                Return TopLeft.Left - 1
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
        Function PreciseCollidesWith(other As Rectangle) As Boolean
            Return Left <= other.Right AndAlso other.Left <= Right AndAlso Top <= other.Bottom AndAlso other.Top <= Bottom
        End Function
        Function SafeCollidesWith(other As Rectangle) As Boolean
            Return Left - 1 <= other.Right AndAlso other.Left - 1 <= Right AndAlso Top <= other.Bottom AndAlso other.Top <= Bottom
        End Function
        Public Overrides Function ToString() As String
            Return $"({Left}, {Top}) to ({Right}, {Bottom})"
        End Function
    End Structure
    Structure Sprite
        Public Sub New(display As Char, Optional color As ConsoleColor = ConsoleColor.White) ' Consoles don't support surrogate pairs
            Me.Display = display
            Me.Color = color
        End Sub
        Public ReadOnly Property Display As Char
        Public ReadOnly Property Color As ConsoleColor
    End Structure
#End Region
#Region "Helpers"
    ReadOnly NeedDoubleRectangleWidth As Boolean = Environment.OSVersion.Platform = PlatformID.Win32NT AndAlso
                                                   Environment.OSVersion.Version.Major = 6 AndAlso
                                                   Environment.OSVersion.Version.Minor = 1
    Property CursorPosition As Point
        Get
            Return New Point(CursorLeft, CursorTop)
        End Get
        Set(value As Point)
            SetCursorPosition(value.Left, value.Top)
        End Set
    End Property
    Sub IfHasValue(Of T As Structure)(nullable As T?, f As Action(Of T))
        If nullable.HasValue Then f(nullable.GetValueOrDefault())
    End Sub
    Function IfHasValue(Of T As Structure, TReturn As Structure)(nullable As T?, f As Func(Of T, TReturn)) As TReturn?
        Return If(nullable.HasValue, f(nullable.GetValueOrDefault()), New TReturn?())
    End Function
    Function IfHasValue(Of T As Structure, TReturn As Structure)(nullable As T?, f As Func(Of T, TReturn?)) As TReturn?
        Return If(nullable.HasValue, f(nullable.GetValueOrDefault()), New TReturn?())
    End Function
    Function IfHasValue(Of T As Structure, TReturn)(nullable As T?, f As Func(Of T, TReturn), defaultValue As TReturn) As TReturn
        Return If(nullable.HasValue, f(nullable.GetValueOrDefault()), defaultValue)
    End Function
    Function ReadKey(timeout As TimeSpan) As ConsoleKey?
        If KeyAvailable Then Return Console.ReadKey(True).Key
        Dim beginWait = Date.Now
        While Not KeyAvailable And Date.Now.Subtract(beginWait) < timeout
            Threading.Thread.Sleep(100)
            If KeyAvailable Then Return Console.ReadKey(True).Key
        End While
        Return Nothing
    End Function
    Sub WriteAt(position As Point?, sprite As Sprite)
        If position.HasValue Then
            CursorPosition = position.GetValueOrDefault()
            ForegroundColor = sprite.Color
            Write(sprite.Display)
        End If
    End Sub
#End Region
#Region "Entity Classes"
    MustInherit Class Entity
        Implements IDisposable
        Sub New(entities As ICollection(Of Entity))
            entities.Add(Me)
        End Sub
        Protected Overridable Function ForbidEntry(other As Entity, otherBounds As Rectangle) As Boolean
            Return TypeOf other IsNot TriggerZone AndAlso
                   Bounds IsNot Nothing AndAlso
                   Bounds.GetValueOrDefault().SafeCollidesWith(otherBounds)
        End Function
        Protected Function CanMoveTo(value As Rectangle?) As Boolean
            If value IsNot Nothing AndAlso CurrentRegion IsNot Nothing Then
                Dim rect = value.GetValueOrDefault()
                If rect.Left < 0 OrElse rect.Right >= WindowWidth OrElse rect.Top < 0 OrElse rect.Bottom >= WindowHeight Then Return False
                For Each entity In CurrentRegion.Entities
                    If Me IsNot entity AndAlso entity.ForbidEntry(Me, rect) Then Return False
                Next
            End If
            Return True
        End Function
        Protected MustOverride Sub RedrawAt(bounds As Delta(Of Rectangle?))
        Dim _bounds As Rectangle?
        Protected Property Bounds As Rectangle?
            Get
                Return _bounds
            End Get
            Set(value As Rectangle?)
                If Not CanMoveTo(value) Then Return
                RedrawAt(New Delta(Of Rectangle?)(_bounds, value))
                _bounds = value
            End Set
        End Property
        Public Property Position As Point?
            Get
                Return Bounds?.TopLeft
            End Get
            Set(value As Point?)
                Bounds = IfHasValue(value, AddressOf BoundsForNewPoint)
            End Set
        End Property
        Protected MustOverride Function BoundsForNewPoint(point As Point) As Rectangle?
        ''' <returns>Whether the point was different from original.</returns>
        Function Go(pointMap As Func(Of Point, Point)) As Boolean
            Return IfHasValue(Position, Function(point)
                                            Position = pointMap(point)
                                            Return Not point.Equals(Position)
                                        End Function, False)
        End Function
        Public Overridable Function GoUp() As Boolean
            Return Go(Function(point) New Point(point.Left, Math.Max(point.Top - 1, 0)))
        End Function
        Public Overridable Function GoDown() As Boolean
            Return Go(Function(point) New Point(point.Left, Math.Min(WindowHeight - 1, point.Top + 1)))
        End Function
        Public Overridable Function GoLeft() As Boolean
            Return Go(Function(point) New Point(Math.Max(point.Left - 1, 0), point.Top))
        End Function
        Public Overridable Function GoRight() As Boolean
            Return Go(Function(point) New Point(Math.Min(point.Left + 1, WindowWidth - 2), point.Top)) ' Sunny is too fat and spans 2 spaces
        End Function
        Public Overridable Sub Dispose() Implements IDisposable.Dispose
            Position = Nothing
        End Sub
    End Class
    Class RectangleEntity
        Inherits Entity
        Public Sub New(entities As ICollection(Of Entity), rect As Rectangle?)
            MyBase.New(entities)
            Rectangle = rect
        End Sub
        Protected Overrides Function BoundsForNewPoint(point As Point) As Rectangle?
            Return IfHasValue(Bounds, Function(rect) New Rectangle(point, rect.Width, rect.Height))
        End Function
        Public Property Rectangle As Rectangle?
            Get
                Return Bounds
            End Get
            Set(value As Rectangle?)
                Bounds = value
            End Set
        End Property
        Protected Overrides Sub RedrawAt(bounds As Delta(Of Rectangle?))
            If bounds.Changed Then
                Dim Draw =
                    Sub(Rectangle As Rectangle?, horizontal As Char, vertical As Char,
                        topLeft As Char, topRight As Char, bottomLeft As Char, bottomRight As Char)
                        IfHasValue(Rectangle,
                            Sub(rect)
                                ResetColor()
                                SetCursorPosition(rect.Left, rect.Bottom)
                                Write(bottomLeft)
                                For x = rect.Left + 1 To rect.Right - 1
                                    Write(horizontal)
                                Next
                                Write(bottomRight)
                                For y = rect.Top + 1 To rect.Bottom - 1
                                    SetCursorPosition(rect.Left, y)
                                    Write(vertical)
                                    SetCursorPosition(rect.Right, y)
                                    Write(vertical)
                                Next
                                CursorPosition = rect.TopLeft
                                Write(topLeft)
                                For x = rect.Left + 1 To rect.Right - 1
                                    Write(horizontal)
                                Next
                                Write(topRight)
                            End Sub)
                    End Sub
                Draw(bounds.OldValue, Empty, Empty, Empty, Empty, Empty, Empty)
                Draw(bounds.NewValue, "━"c, "┃"c, "┏"c, "┓"c, "┗"c, "┛"c)
            End If
        End Sub
    End Class
    Class TriggerZone
        Inherits RectangleEntity
        Public Sub New(entities As ICollection(Of Entity), rect As Rectangle?,
                       Optional keyPress As Func(Of ConsoleKey, Boolean) = Nothing,
                       Optional enter As Action = Nothing, Optional leave As Action = Nothing)
            MyBase.New(entities, rect)
            Me.Enter = enter
            Me.Leave = leave
            Me.KeyPress = keyPress
        End Sub
        Public Property Enter As Action
        Dim EnterLock As Boolean
        Public Property Leave As Action
        Dim LeaveLock As Boolean
        ''' <returns>Whether the key has been handled.</returns>
        Public Property KeyPress As Func(Of ConsoleKey, Boolean)
        Protected Overrides Function ForbidEntry(other As Entity, otherNewBounds As Rectangle) As Boolean
            If (TypeOf other Is PlayerEntity) Then
                Dim player = DirectCast(other, PlayerEntity)
                If Bounds?.PreciseCollidesWith(otherNewBounds) Then
                    player.Trigger = Me
                    If Not EnterLock Then
                        EnterLock = True
                        Enter?.Invoke()
                        EnterLock = False
                    End If
                ElseIf player.Trigger Is Me Then
                    If Not LeaveLock Then
                        LeaveLock = True
                        player.Trigger = Nothing
                        Leave?.Invoke()
                        LeaveLock = False
                    End If
                End If
            End If
            Return False
        End Function
        Public Overrides Sub Dispose()
            If ActiveEntity.Trigger Is Me Then
                ActiveEntity.Trigger = Nothing
                Leave?.Invoke()
            End If
            MyBase.Dispose()
        End Sub
        Protected Overrides Sub RedrawAt(bounds As Delta(Of Rectangle?)) ' Doesn't need to be drawn
        End Sub
    End Class
    Class TextEntity
        Inherits Entity
        Public Sub New(entities As ICollection(Of Entity), text As String, Optional position As Point? = Nothing)
            MyBase.New(entities)
            Me.Text = text
            Me.Position = position
        End Sub
        Protected Overrides Function BoundsForNewPoint(point As Point) As Rectangle?
            Return New Rectangle(point, Text.Length, 1)
        End Function
        Dim _text As String
        Public Property Text As String
            Get
                Return _text
            End Get
            Set(value As String)
                RedrawAt(New Delta(Of Rectangle?)(Bounds), New Delta(Of String)(_text, value))
                _text = value
            End Set
        End Property
        Protected Overrides Sub RedrawAt(bounds As Delta(Of Rectangle?))
            RedrawAt(bounds, New Delta(Of String)(_text))
        End Sub
        Protected Overloads Sub RedrawAt(bounds As Delta(Of Rectangle?), text As Delta(Of String))
            IfHasValue(bounds.OldValue, Sub(point)
                                            ResetColor()
                                            Debug.WriteLine("Pos: {0}", CursorPosition)
                                            CursorPosition = point.TopLeft
                                            For i = 1 To text.OldValue.Length
                                                Write(Empty)
                                            Next
                                        End Sub)
            IfHasValue(bounds.NewValue, Sub(point)
                                            ResetColor()
                                            CursorPosition = point.TopLeft
                                            Write(text.NewValue)
                                        End Sub)
        End Sub
    End Class
    Class SpriteEntity
        Inherits Entity
        Public Sub New(entities As ICollection(Of Entity), sprite As Sprite)
            MyBase.New(entities)
            _sprite = sprite
        End Sub
        Protected Overrides Function BoundsForNewPoint(point As Point) As Rectangle?
            Return New Rectangle(point, 1, 1)
        End Function
        Dim _sprite As Sprite
        Public Property Sprite As Sprite
            Get
                Return _sprite
            End Get
            Set(value As Sprite)
                RedrawAt(New Delta(Of Rectangle?)(Bounds), New Delta(Of Sprite)(_sprite, value))
                _sprite = value
            End Set
        End Property
        Protected Overrides Sub RedrawAt(bounds As Delta(Of Rectangle?))
            RedrawAt(bounds, New Delta(Of Sprite)(_sprite))
        End Sub
        Protected Overloads Sub RedrawAt(bounds As Delta(Of Rectangle?), sprite As Delta(Of Sprite))
            If bounds.Changed Then WriteAt(bounds.OldValue?.TopLeft, Empty_)
            WriteAt(bounds.NewValue?.TopLeft, sprite.NewValue)
        End Sub
    End Class
    Class GravityEntity
        Inherits SpriteEntity
        Public Sub New(entities As ICollection(Of Entity), sprite As Sprite)
            MyBase.New(entities, sprite)
            AddHandler Tick, AddressOf WhenTick
        End Sub
        Public Property VerticalVelocity As Integer
        Sub WhenTick()
            If VerticalVelocity > 0 Then
                MyBase.GoUp()
                VerticalVelocity -= 1
            ElseIf MyBase.GoDown() Then
                VerticalVelocity -= 1
            Else
                VerticalVelocity = 0
            End If
        End Sub
        Public Overrides Function GoUp() As Boolean
            If MyBase.GoDown() Then
                Return False ' Can't jump while falling
            Else
                MyBase.GoUp()
                VerticalVelocity = 2
                Return True
            End If
        End Function
        Public Overrides Sub Dispose()
            RemoveHandler Tick, AddressOf WhenTick
            MyBase.Dispose()
        End Sub
    End Class
    Class PlayerEntity
        Inherits GravityEntity
        Public Property Trigger As TriggerZone
        Public Sub New(entities As ICollection(Of Entity), sprite As Sprite)
            MyBase.New(entities, sprite)
        End Sub
        Public Sub HandleKey(key As ConsoleKey)
            If Trigger?.KeyPress Is Nothing OrElse Not Trigger.KeyPress(key) Then
                Select Case key
                    Case ConsoleKey.LeftArrow : GoLeft()
                    Case ConsoleKey.RightArrow : GoRight()
                    Case ConsoleKey.UpArrow : GoUp()
                    Case ConsoleKey.DownArrow : GoDown()
                End Select
            End If
        End Sub
        Public Overrides Function GoLeft() As Boolean
            Dim ret = MyBase.GoLeft()
            If Position?.Left = 0 AndAlso CurrentRegion.GoLeft() Then
                Position = New Point(WindowWidth - 3, Position.GetValueOrDefault().Top)
            End If
            Return ret
        End Function
        Public Overrides Function GoRight() As Boolean
            Dim ret = MyBase.GoRight()
            If Position?.Left = WindowWidth - 2 AndAlso CurrentRegion.GoRight() Then
                Position = New Point(1, Position.GetValueOrDefault().Top)
            End If
            Return ret
        End Function
    End Class
    Class GravityEntityFactory
        Private Class GravityEntityFactoryEntity
            Inherits GravityEntity
            Public Sub New(entities As ICollection(Of Entity), sprite As Sprite)
                MyBase.New(entities, sprite)
            End Sub
        End Class
        Public Sub New(entities As ICollection(Of Entity), sprite As Sprite)
            Me.Entities = entities
            Me.Sprite = sprite
        End Sub
        ReadOnly Sprite As Sprite
        ReadOnly Entities As ICollection(Of Entity)
        Public Property Template As Sprite
        Public Sub Create(position As Point?)
            Entities.Add(New GravityEntityFactoryEntity(Entities, Sprite) With {.Position = position})
        End Sub
        Public Sub RemoveAll()
            For Each item In Entities.OfType(Of GravityEntityFactoryEntity)
                Entities.Remove(item)
                item.Dispose()
            Next
        End Sub
    End Class
#End Region
#Region "Global Entities"
    'Unicode: 
    '1.1☺☹☠❣❤✌☝✍♨✈⌛⌚☀☁☂❄☃☄♠♥♦♣♟☎⌨✉✏✒✂☢☣↗➡↘↙↖↕↔↩↪✡☸☯✝☦☪☮♈♉♊♋♌♍♎♏♐♑♒♓▶◀♀♂☑✔✖✳✴❇‼〰©®™Ⓜ
    '1.1㊗㊙▪▫☜♅♪♜☌♘☛♞☵☒♛♢✎‍♡☼☴♆☲☇♇☏☨☧☤☥♭☭☽☾❥☍☋☊☬♧☉#☞☶♁♤☷✐♮♖★♝*☰☫♫♙♃☚♬☩♄☓♯☟☈☻☱♕☳♔♩♚♗☡☐
    '3.0⁉♱♰☙
    '3.2⤴⤵♻〽◼◻◾◽☖♷⚁⚄⚆⚈♼☗♵⚉⚀⚇♹♲♸⚂♺♴⚅♳♽⚃♶
    '4.0☕☔⚡⚠⬆⬇⬅⏏⚏⚋⚎⚑⚊⚍⚐⚌
    '4.1☘⚓⚒⚔⚙⚖⚗⚰⚱♿⚛⚕♾⚜⚫⚪⚩⚭⚢⚥⚘⚤⚦⚨⚣⚬⚮⚚⚯⚧
    '5.0    // ⚲
    '5.1⭐⬛⬜⚶⚼⚸⚴⚹⚳⚵⚻⚷⚝⚺  // ⛂⛁⛃⛀
    '5.2⛪⛲⛺⛽⛵⛅⛄⚽⚾⛳’⛔⭕❗  // ⛩⛴⛈⛱⛸⛑⛏⛓⛷⛹⛰⛟⛙⛞⛮⛶⛯⛜⛡⛿⛣⛊⛐⛾⛉⛚⛘⛠⛆⛝⛌⛕⛬⛍⛫⛖⚞⛨⚟⛻⛋⛒⛛⛭⛇⛼⚿⛗
    '6.0✋✊⏳⏰✨⛎⏩⏪⏫⏬✅❌❎➕➖➗➰➿❓❔❕ // ⏱⏲⏭⏯⏮⛧⛢⛤ // Right-Handed interlaced pentagram: ⛥ Left-Handed interlaced pentagram: ⛦
    '7.0    // ⏸⏹⏺
    '10.    // ₿
    Const Empty = " "c
    ReadOnly Empty_ As New Sprite(Empty)
    ReadOnly GlobalEntities As New List(Of Entity)
    Public ReadOnly Sunny_ As New Sprite("☺"c)
    Public ReadOnly Sunny_Angry As New Sprite("☹"c, ConsoleColor.Red)
    Public ReadOnly Sunny As New PlayerEntity(GlobalEntities, Sunny_)

    Public ReadOnly Sun_ As New Sprite("☼"c, ConsoleColor.Yellow)
    Public ReadOnly Sun As New SpriteEntity(GlobalEntities, Sun_)

    Public ReadOnly Horsey_ As New Sprite("♘"c, ConsoleColor.Magenta)
    Public ReadOnly Horsey_Dead As New Sprite("♞"c, ConsoleColor.DarkMagenta)
    Public ReadOnly Horsey As New SpriteEntity(GlobalEntities, Horsey_)

    Public ActiveEntity As PlayerEntity = Sunny
#End Region
#Region "Regions"
    MustInherit Class Region
        Implements IDisposable
        Sub New(Optional bedrock As Boolean = True)
            If bedrock Then Equals(New RectangleEntity(WriteEntities,
                                       New Rectangle(0, WindowHeight - 2, WindowWidth, 1)), Nothing)
        End Sub
        ''' <returns>Whether region was changed.</returns>
        Function Go(region As Func(Of Region)) As Boolean
            If region IsNot Nothing Then
                SetCurrentRegion = region
                Return True
            End If
            Return False
        End Function
        Public Function GoLeft() As Boolean
            Return Go(Left)
        End Function
        Public Function GoRight() As Boolean
            Return Go(Right)
        End Function
        Protected MustOverride ReadOnly Property Left As Func(Of Region)
        Protected MustOverride ReadOnly Property Right As Func(Of Region)
        Protected ReadOnly WriteEntities As New List(Of Entity)(GlobalEntities)
        Public ReadOnly Entities As New ReadOnlyCollection(Of Entity)(WriteEntities)
        Public Sub Dispose() Implements IDisposable.Dispose
            For Each entity In Entities.Except(GlobalEntities)
                entity.Dispose()
            Next
        End Sub
    End Class
    Dim _currentRegion As Region = New Region3_ConnectFour()
    Public ReadOnly Property CurrentRegion As Region
        Get
            Return _currentRegion
        End Get
    End Property
    Public WriteOnly Property SetCurrentRegion As Func(Of Region)
        Set(value As Func(Of Region))
            _currentRegion.Dispose()
            ' **Prevent collision detection between old and new region entities**
            _currentRegion = value()
        End Set
    End Property
    Class Region1_Title
        Inherits Region
        Protected ReadOnly SBA As New TextEntity(WriteEntities, "SBA: Sunny's Big Adventure", New Point(10, 0))
        Protected ReadOnly Arrows As New TextEntity(WriteEntities, "▶▶▶▶▶▶▶▶", New Point(20, 1))
        Protected ReadOnly Keybinds As New TextEntity(WriteEntities, "Arrow keys: Move", New Point(10, 9))
        'Protected ReadOnly Trigger As New TriggerZone(WriteEntities, New Rectangle(0, 1, WindowWidth, 8),
        '    Nothing, Sub() Sunny.Sprite = Sunny_, Function(key)
        '                                              Select Case key
        '                                                  Case ConsoleKey.Q
        '                                                      Sunny.Sprite = If(Sunny.Sprite.Equals(Sunny_), Sunny_Angry, Sunny_)
        '                                                      Return True
        '                                                  Case Else
        '                                                      Return False
        '                                              End Select
        '                                          End Function)
        Protected Overrides ReadOnly Property Left As Func(Of Region) = Nothing
        Protected Overrides ReadOnly Property Right As Func(Of Region) = Function() New Region2_NumberGuess()
    End Class
    Class Region2_NumberGuess
        Inherits Region
        Protected Passcode As Byte = CByte(New Random().Next(101))
        Protected ReadOnly Instruction As New TextEntity(WriteEntities, "You must input the correct")
        Protected ReadOnly Instruction2 As New TextEntity(WriteEntities, "passcode to continue! (0~100)")
        Protected ReadOnly Instruction3 As New TextEntity(WriteEntities, "Sunny: I must guess it...")
        Protected ReadOnly Input As New TextEntity(WriteEntities, "Input: ")
        Protected ReadOnly Barrier As New RectangleEntity(WriteEntities, New Rectangle(42, 0, 2, 8))
        Protected ReadOnly Trigger As New TriggerZone(WriteEntities, New Rectangle(30, 6, 6, 3),
                                                      Function(key)
                                                          Select Case key
                                                              Case ConsoleKey.D0 To ConsoleKey.D9
                                                                  Input.Text &= key.ToString()(1)
                                                              Case ConsoleKey.Enter
                                                                  Dim inputNumber As Byte
                                                                  If Byte.TryParse(String.Concat(Input.Text.SkipWhile(Function(c) Not Char.IsDigit(c))),
                                                                                      inputNumber) Then
                                                                      Select Case inputNumber
                                                                          Case Is < Passcode
                                                                              Instruction3.Text = "Sunny: The input is too small..."
                                                                          Case Is > Passcode
                                                                              Instruction3.Text = "Sunny: The input is too large..."
                                                                          Case Else
                                                                              Instruction3.Text = "Sunny: Yes! The passcode is correct!"
                                                                              Instruction.Dispose()
                                                                              Instruction2.Dispose()
                                                                              Input.Dispose()
                                                                              Barrier.Dispose()
                                                                              Trigger.Dispose()
                                                                      End Select
                                                                  Else
                                                                      Instruction3.Text = "Sunny: The input is too large..." ' inputNumber > 255
                                                                  End If
                                                                  Input.Text = "Input: "
                                                          End Select
                                                          Return True
                                                      End Function,
                                                      Sub()
                                                          Instruction.Position = New Point(10, 0)
                                                          Threading.Thread.Sleep(500)
                                                          Instruction2.Position = New Point(10, 1)
                                                          Threading.Thread.Sleep(500)
                                                          Instruction3.Position = New Point(0, 2)
                                                          Threading.Thread.Sleep(500)
                                                          Input.Position = New Point(0, 3)
                                                      End Sub)
        Protected Overrides ReadOnly Property Left As Func(Of Region) = Function() New Region1_Title()
        Protected Overrides ReadOnly Property Right As Func(Of Region) = Function() New Region3_ConnectFour()
    End Class
    Class Region3_ConnectFour
        Inherits Region
        Protected ReadOnly Whites As New GravityEntityFactory(WriteEntities, New Sprite("○"c))
        Protected ReadOnly Blacks As New GravityEntityFactory(WriteEntities, New Sprite("●"c))
        Protected ReadOnly GameField As New RectangleEntity(WriteEntities, New Rectangle(8, 1, 18, 8))
        Protected ReadOnly Trigger As New TriggerZone(WriteEntities,
                                                      IfHasValue(GameField.Rectangle, Function(rect) _
                                                          New Rectangle(rect.Left - 3, rect.Top - 1, rect.Width + 6, rect.Height + 1)),
                                                      Function(key)
                                                          Select Case key
                                                              Case ConsoleKey.Enter
                                                                  Whites.Create(IfHasValue(ActiveEntity.Position,
                                                                                           Function(pos) New Point(pos.Left, pos.Top + 2)))
                                                              Case ConsoleKey.LeftArrow
                                                                  ActiveEntity.GoLeft()
                                                                  ActiveEntity.GoLeft()
                                                              Case ConsoleKey.RightArrow
                                                                  ActiveEntity.GoRight()
                                                                  ActiveEntity.GoRight()
                                                          End Select
                                                          Return True
                                                      End Function,
    Sub()
        ActiveEntity.Position = IfHasValue(ActiveEntity.Position, Function(pos) New Point(pos.Left, 0))
    End Sub)
        Protected Overrides ReadOnly Property Left As Func(Of Region) = Function() New Region2_NumberGuess()
        Protected Overrides ReadOnly Property Right As Func(Of Region) = Nothing
    End Class
#End Region
    Const WindowWidth = 48
    Const WindowHeight = 10
    Public Event Tick()
    Sub Main()
        If LargestWindowWidth < 48 Or LargestWindowHeight < 10 Then
            WriteLine("ERROR: Please decrease font size")
            Return
        End If
        OutputEncoding = Encoding.Unicode
        Title = "SBA: Sunny's Big Adventure"
        Console.WindowWidth = WindowWidth
        Console.WindowHeight = WindowHeight
        CursorVisible = False
        Sunny.Position = New Point(2, 5)
        While True
            RaiseEvent Tick()
            Dim key = ReadKey(TimeSpan.FromSeconds(0.2))
            If key IsNot Nothing Then ActiveEntity.HandleKey(key.GetValueOrDefault())
        End While
    End Sub
End Module