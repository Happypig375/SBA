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
    Implements IComparable(Of Point)
    Public Sub New(left As Integer, top As Integer)
        Me.Left = left
        Me.Top = top
    End Sub
    Public ReadOnly Property Left As Integer
    Public ReadOnly Property Top As Integer
    Public Overrides Function ToString() As String
        Return $"({Left}, {Top})"
    End Function
    Public Function CompareTo(other As Point) As Integer Implements IComparable(Of Point).CompareTo
        If Left < other.Left Then Return -1
        If Left > other.Left Then Return 1
        If Top < other.Top Then Return -1
        If Top > other.Top Then Return 1
        Return 0
    End Function
    Public Shared Operator =(p1 As Point, p2 As Point) As Boolean
        Return p1.Left = p2.Left AndAlso p1.Top = p2.Top
    End Operator
    Public Shared Operator <>(p1 As Point, p2 As Point) As Boolean
        Return p1.Left <> p2.Left OrElse p1.Top <> p2.Top
    End Operator
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
        ResetColor()
    End If
End Sub
ReadOnly Random As New Random()
<Runtime.CompilerServices.Extension>
Function RandomItem(Of T)(list As ICollection(Of T)) As T
    Return list.ElementAt(Random.Next(list.Count))
End Function
Const FileName = "SBA.config"
Const FieldSeparator = ChrW(&H1F) ' U+001F Unit Separator
Dim UserName As String
Sub SaveRegion(region As Region)
    Dim config = IO.File.ReadAllLines(FileName)
    For i = 0 To config.Length - 1
        Dim parts = config(i).Split(FieldSeparator)
        If parts(0) = UserName Then _
            config(i) = String.Join(FieldSeparator, parts(0), parts(1), region.GetType().FullName)
    Next
    IO.File.WriteAllLines(FileName, config)
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
            Dim newPosition = Position
            For Each entity In CurrentRegion.Entities
                If Me IsNot entity AndAlso entity.ForbidEntry(Me, rect) Then Return False
                If newPosition <> Position Then Return False ' Position was set in ForbidEntry, already moved elsewhere
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
                                        Return If(point <> Position, True)
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
    Public Sub New(entities As ICollection(Of Entity), rect As Rectangle?, Optional color As ConsoleColor = ConsoleColor.White)
        MyBase.New(entities)
        Me.Color = color
        Rectangle = rect
    End Sub
    Protected Overrides Function ForbidEntry(other As Entity, otherBounds As Rectangle) As Boolean
        Return IfHasValue(Bounds, Function(rect) _
            TypeOf other Is Entity AndAlso (
            New Rectangle(rect.TopLeft, New Point(rect.Right, rect.Top)).SafeCollidesWith(otherBounds) OrElse
            New Rectangle(rect.TopLeft, New Point(rect.Left, rect.Bottom)).SafeCollidesWith(otherBounds) OrElse
            New Rectangle(New Point(rect.Left, rect.Bottom), New Point(rect.Right, rect.Bottom)).SafeCollidesWith(otherBounds) OrElse
            New Rectangle(New Point(rect.Right, rect.Top), New Point(rect.Right, rect.Bottom)).SafeCollidesWith(otherBounds)), False)
    End Function
    Protected Overrides Function BoundsForNewPoint(point As Point) As Rectangle?
        Return IfHasValue(Bounds, Function(rect) New Rectangle(point, rect.Width, rect.Height))
    End Function
    Public Property Color As ConsoleColor
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
                            Dim DrawHorizontal =
                                Sub(y As Integer)
                                    For x = 2 To rect.Width - 1 Step If(NeedDoubleRectangleWidth, 2, 1)
                                        SetCursorPosition(rect.Left + x, y)
                                        Write(horizontal)
                                    Next
                                End Sub
                            ForegroundColor = Color
                            DrawHorizontal(rect.Bottom)
                            SetCursorPosition(rect.Left, rect.Bottom)
                            Write(bottomLeft)
                            SetCursorPosition(rect.Right, rect.Bottom)
                            Write(bottomRight)
                            For y = rect.Bottom - 1 To rect.Top + 1 Step -1
                                SetCursorPosition(rect.Left, y)
                                Write(vertical)
                                SetCursorPosition(rect.Right, y)
                                Write(vertical)
                            Next
                            DrawHorizontal(rect.Top)
                            CursorPosition = rect.TopLeft
                            Write(topLeft)
                            SetCursorPosition(rect.Right, rect.Top)
                            Write(topRight)
                            ResetColor()
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
    Public Sub New(entities As ICollection(Of Entity), text As String,
                   Optional position As Point? = Nothing, Optional color As ConsoleColor = ConsoleColor.White)
        MyBase.New(entities)
        Me.Color = color
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
    Public Property Color As ConsoleColor
    Protected Overrides Sub RedrawAt(bounds As Delta(Of Rectangle?))
        RedrawAt(bounds, New Delta(Of String)(_text))
    End Sub
    Protected Overloads Sub RedrawAt(bounds As Delta(Of Rectangle?), text As Delta(Of String))
        IfHasValue(bounds.OldValue, Sub(point)
                                        ForegroundColor = Color
                                        CursorPosition = point.TopLeft
                                        For i = 1 To text.OldValue.Length
                                            Write(Empty)
                                        Next
                                    End Sub)
        IfHasValue(bounds.NewValue, Sub(point)
                                        ForegroundColor = Color
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
MustInherit Class TickEntity
    Inherits SpriteEntity
    Protected MustOverride Sub OnTick()
    Public Sub New(entities As ICollection(Of Entity), sprite As Sprite)
        MyBase.New(entities, sprite)
        AddHandler Tick, AddressOf OnTick
    End Sub
    Public Overrides Sub Dispose()
        RemoveHandler Tick, AddressOf OnTick
        MyBase.Dispose()
    End Sub
End Class
Class IteratingSpriteEntity
    Inherits TickEntity
    ReadOnly sprites As IEnumerator(Of Sprite)
    Public Sub New(entities As ICollection(Of Entity), sprites As IEnumerable(Of Sprite), position As Point)
        MyBase.New(entities, sprites.First())
        Me.Position = position
        Me.sprites = sprites.Skip(1).GetEnumerator()
    End Sub
    Protected Overrides Sub OnTick()
        If sprites.MoveNext() Then Sprite = sprites.Current Else Dispose()
    End Sub
    Public Overrides Sub Dispose()
        sprites.Dispose()
        MyBase.Dispose()
    End Sub
End Class
Class GravityEntity
    Inherits TickEntity
    Public Sub New(entities As ICollection(Of Entity), sprite As Sprite)
        MyBase.New(entities, sprite)
    End Sub
    Protected Overrides Sub OnTick()
        If VerticalVelocity > 0 Then
            MyBase.GoUp()
            VerticalVelocity -= 1
        ElseIf MyBase.GoDown() Then
            VerticalVelocity -= 1
        Else
            VerticalVelocity = 0
            RaiseEvent GroundHit()
            GroundHitEvent = Nothing
        End If
    End Sub
    Public Property VerticalVelocity As Integer
    Public Event GroundHit()
    Public Overrides Function GoUp() As Boolean
        If MyBase.GoDown() Then
            Return False ' Can't jump while falling
        Else
            MyBase.GoUp()
            VerticalVelocity = 2
            Return True
        End If
    End Function
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
    Class GravityEntityFactoryEntity
        Inherits GravityEntity
        Friend Owner As GravityEntityFactory
        Public Sub New(entities As ICollection(Of Entity), sprite As Sprite,
                       owner As GravityEntityFactory, position As Point?, onHitGround As GroundHitEventHandler)
            MyBase.New(entities, sprite)
            Me.Owner = owner
            Me.Position = position
            If onHitGround IsNot Nothing Then AddHandler GroundHit, onHitGround
        End Sub
    End Class
    Public Sub New(entities As ICollection(Of Entity), sprite As Sprite)
        Me.Entities = entities
        Me.Sprite = sprite
    End Sub
    ReadOnly Sprite As Sprite
    ReadOnly Entities As ICollection(Of Entity)
    Public Sub Add(position As Point?, Optional onHitGround As GravityEntity.GroundHitEventHandler = Nothing)
        Entities.Add(New GravityEntityFactoryEntity(Entities, Sprite, Me, position, onHitGround) With {.VerticalVelocity = 1})
    End Sub
    Public Sub Clear()
        For Each item In Entities.OfType(Of GravityEntityFactoryEntity).Where(Function(e) e.Owner Is Me)
            Entities.Remove(item)
            item.Dispose()
        Next
    End Sub
    Public Function ItemAt(position As Point) As GravityEntity
        For Each item In Entities.OfType(Of GravityEntityFactoryEntity).Where(Function(e) e.Owner Is Me)
            If item.Position = position Then Return item
        Next
        Return Nothing
    End Function
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
    ReadOnly GlobalEntities As New HashSet(Of Entity)

    Public ReadOnly Sunny_ As New Sprite("☺"c, ConsoleColor.Green)
    Public ReadOnly Sunny_Angry As New Sprite("☹"c, ConsoleColor.Red)
    Public ReadOnly Sunny As New PlayerEntity(GlobalEntities, Sunny_)

    Public ReadOnly Sun_ As New Sprite("☼"c, ConsoleColor.Yellow)
    Public ReadOnly Sun As New SpriteEntity(GlobalEntities, Sun_)

    Public ReadOnly Horsey_ As New Sprite("♘"c, ConsoleColor.Magenta)
    Public ReadOnly Horsey_Dead As New Sprite("♞"c, ConsoleColor.DarkMagenta)
    Public ReadOnly Horsey As New SpriteEntity(GlobalEntities, Horsey_)

    Public ReadOnly Firework_1 As New Sprite("✳"c, ConsoleColor.Yellow)
    Public ReadOnly Firework_2 As New Sprite("✴"c, ConsoleColor.Red)
    Public ReadOnly Firework_3 As New Sprite("❇"c, ConsoleColor.DarkRed)

    Public ActiveEntity As PlayerEntity = Sunny
#End Region
#Region "Regions"
Dim _currentRegion As Region
MustInherit Class Region
    Implements IDisposable
    Sub New(Optional bedrock As Boolean = True)
        If bedrock Then Equals(New RectangleEntity(WriteEntities,
                                   New Rectangle(0, WindowHeight - 2, WindowWidth, 1)), Nothing)
    End Sub
    ''' <returns>Whether region was changed.</returns>
    Public Function GoLeft() As Boolean
        If Left IsNot Nothing Then
            SetCurrentRegion = Left
            Return True
        End If
        Return False
    End Function
    ''' <returns>Whether region was changed.</returns>
    Public Function GoRight() As Boolean
        If Right IsNot Nothing Then
            SetCurrentRegion = Right
            SaveRegion(CurrentRegion)
            Return True
        End If
        Return False
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
    Protected ReadOnly SBA As New TextEntity(WriteEntities, "SBA: Sunny's Big Adventure", New Point(10, 0), ConsoleColor.Yellow)
    Protected ReadOnly Arrow1 As New TextEntity(WriteEntities, "▶", New Point(17, 1), ConsoleColor.DarkRed)
    Protected ReadOnly Arrow2 As New TextEntity(WriteEntities, "▶", New Point(18, 1), ConsoleColor.Red)
    Protected ReadOnly Arrow3 As New TextEntity(WriteEntities, "▶", New Point(19, 1), ConsoleColor.Yellow)
    Protected ReadOnly Arrow4 As New TextEntity(WriteEntities, "▶", New Point(20, 1), ConsoleColor.Green)
    Protected ReadOnly Arrow5 As New TextEntity(WriteEntities, "▶", New Point(21, 1), ConsoleColor.Cyan)
    Protected ReadOnly Arrow6 As New TextEntity(WriteEntities, "▶", New Point(22, 1), ConsoleColor.Blue)
    Protected ReadOnly Arrow7 As New TextEntity(WriteEntities, "▶", New Point(23, 1), ConsoleColor.Magenta)
    Protected ReadOnly Keybinds As New TextEntity(WriteEntities, "Arrow keys: Move", New Point(10, 9))
    Protected ReadOnly Trigger As New TriggerZone(WriteEntities, New Rectangle(30, 1, WindowWidth - 30, 8))
    Protected Overrides ReadOnly Property Left As Func(Of Region) = Nothing
    Protected Overrides ReadOnly Property Right As Func(Of Region) = Function() New Region2_NumberGuess()
End Class
Class Region2_NumberGuess
    Inherits Region
    Protected Passcode As Byte = CByte(Random.Next(101))
    Protected ReadOnly Instruction As New TextEntity(WriteEntities, "You must input the correct")
    Protected ReadOnly Instruction2 As New TextEntity(WriteEntities, "passcode to continue! (0~100)")
    Protected ReadOnly Instruction3 As New TextEntity(WriteEntities, "Sunny: I must guess it...")
    Protected ReadOnly Instruction4 As New TextEntity(WriteEntities, "0 to 9: Input passcode")
    Protected ReadOnly Input As New TextEntity(WriteEntities, "Input: ")
    Protected ReadOnly Barrier As New RectangleEntity(WriteEntities, New Rectangle(42, 0, 2, 8), ConsoleColor.Cyan)
    Protected ReadOnly Trigger As New TriggerZone(WriteEntities, New Rectangle(30, 6, 6, 3),
        Function(key)
            ActiveEntity.Sprite = Sunny_
            Select Case key
                Case ConsoleKey.D0 To ConsoleKey.D9
                    Input.Text &= key.ToString()(1)
                Case ConsoleKey.Backspace
                    If Input.Text <> "Input: " Then _
                        Input.Text = Input.Text.Substring(0, Input.Text.Length - 1)
                Case ConsoleKey.Enter
                    Dim inputNumber As Byte
                    If Byte.TryParse(String.Concat(Input.Text.SkipWhile(Function(c) Not Char.IsDigit(c))),
                                        inputNumber) Then
                        Select Case inputNumber
                            Case Is < Passcode
                                Instruction3.Text = "Sunny: The input is too small..."
                                ActiveEntity.Sprite = Sunny_Angry
                            Case Is > Passcode
                                Instruction3.Text = "Sunny: The input is too large..."
                                ActiveEntity.Sprite = Sunny_Angry
                            Case Else
                                Instruction3.Color = ConsoleColor.Green
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
            Threading.Thread.Sleep(500)
            Instruction4.Position = New Point(10, 9)
        End Sub)
    Protected Overrides ReadOnly Property Left As Func(Of Region) = Function() New Region1_Title()
    Protected Overrides ReadOnly Property Right As Func(Of Region) = Function() New Region3_BullsAndCows()
End Class
Class Region3_BullsAndCows
    Inherits Region
    Protected Passcode As Short = CShort(Random.Next(10000))
    Protected ReadOnly Instruction As New TextEntity(WriteEntities, "You must input the correct passcode")
    Protected ReadOnly Instruction2 As New TextEntity(WriteEntities, "to continue! (4 digits: 0000~9999)")
    Protected ReadOnly Instruction3 As New TextEntity(WriteEntities, "Sunny: Oh no! This only allows 12 tries!")
    Protected ReadOnly Instruction4 As New TextEntity(WriteEntities, "+: Correct Number & Position, -: Correct Number")
    Protected ReadOnly Input As New TextEntity(WriteEntities, "Input: ")
    Protected ReadOnly Barrier As New RectangleEntity(WriteEntities, New Rectangle(42, 0, 2, 8), ConsoleColor.Cyan)
    Protected ReadOnly Trigger As New TriggerZone(WriteEntities, New Rectangle(30, 6, 6, 3),
		Function(key)
			ActiveEntity.Sprite = Sunny_
			Select Case key
				Case ConsoleKey.D0 To ConsoleKey.D9
					If Input.Text.Length < "Input: 9999".Length Then Input.Text &= key.ToString()(1)
				Case ConsoleKey.Backspace
					If Input.Text <> "Input: " Then _
						Input.Text = Input.Text.Substring(0, Input.Text.Length - 1)
				Case ConsoleKey.Enter
					If Input.Text.Length = "Input: 9999".Length Then
						Dim inputNumber = Short.Parse(String.Concat(
									Input.Text.SkipWhile(Function(c) Not Char.IsDigit(c))))
						Dim t = Matches(inputNumber, Passcode)
						Dim p = NextPosition()
						If t.CorrectNumPos = 4 Then
							Input.Color = ConsoleColor.Green
							Input.Text = "Gate open!"
							Barrier.Dispose()
							Trigger.Dispose()
						ElseIf p.HasValue Then
							Input.Text = "Input: "
							Dim e As New TextEntity(WriteEntities, String.Concat(inputNumber.ToString().PadLeft(4, "0"c),
								" ", New String("+"c, t.CorrectNumPos), New String("-"c, t.CorrectNum)), p)
							ActiveEntity.Sprite = Sunny_Angry
						Else
							Input.Color = ConsoleColor.Red
							Input.Text = "Gate locked."
							ActiveEntity.Sprite = Sunny_Angry
							Trigger.Dispose()
						End If
					Else
						Input.Text = "Input: "
					End If
			End Select
			Return True
		End Function,
    Sub()
        ActiveEntity.Sprite = Sunny_
        Instruction.Position = New Point(5, 0)
        Threading.Thread.Sleep(500)
        Instruction2.Position = New Point(5, 1)
        Threading.Thread.Sleep(500)
        Instruction3.Position = New Point(0, 2)
        Threading.Thread.Sleep(500)
        Input.Position = New Point(0, 3)
        Threading.Thread.Sleep(500)
        Instruction4.Position = New Point(0, 9)
    End Sub)
    Public Shared Function Matches(guess As Short, actual As Short) As (CorrectNumPos As Integer, CorrectNum As Integer)
        Dim g = guess.ToString().PadLeft(4, "0"c)
        Dim a = actual.ToString().PadLeft(4, "0"c)
        Dim correctNumPosCount = g.Zip(a, Function(gc, ac) gc = ac).Count(Function(b) b)
        Dim correctNumCount = g.
            GroupBy(Function(gc) gc).
            OrderBy(Function(gc) gc.Key).
            GroupJoin(a, Function(gc) gc.Key, Function(ac) ac, Function(gc, ac) Math.Min(ac.Count, gc.Count)).
            Sum() - correctNumPosCount
        Return (correctNumPosCount, correctNumCount)
    End Function
    Protected NextPositionStore As New Point(12, 2)
    Function NextPosition() As Point?
        Dim p As New Point(NextPositionStore.Left, NextPositionStore.Top + 1)
        If p.Top = 7 Then _
            If p.Left = 12 + 2 * 10 Then NextPosition = Nothing _
            Else NextPosition = New Point(p.Left + 10, 3) Else NextPosition = p
        If NextPosition.HasValue Then NextPositionStore = NextPosition.GetValueOrDefault()
    End Function
    Protected Overrides ReadOnly Property Left As Func(Of Region) = Function() New Region2_NumberGuess()
    Protected Overrides ReadOnly Property Right As Func(Of Region) = Function() New Region4_ConnectFour()
End Class
Class Region4_ConnectFour
    Inherits Region
    Enum Player
        None
        Player
        CPU
    End Enum
    Protected ReadOnly GameArea As New Rectangle(8, 1, 17, 8)
    Protected ReadOnly Instructions As New TextEntity(WriteEntities, "Press 1~7 to add a piece. Connect 4 to win")
    Protected ReadOnly Ruler As New TextEntity(WriteEntities, "11223344556677")
    Protected ReadOnly Whites As New GravityEntityFactory(WriteEntities, New Sprite("○"c, ConsoleColor.Green))
    Protected ReadOnly Blacks As New GravityEntityFactory(WriteEntities, New Sprite("●"c, ConsoleColor.Red))
    Protected ReadOnly GameField As New RectangleEntity(WriteEntities, GameArea, ConsoleColor.Blue)
    Protected WaitingForCPU As Boolean = False
    Protected ReadOnly Trigger As New TriggerZone(WriteEntities,
        New Rectangle(GameArea.Left - 3, GameArea.Top - 1, GameArea.Width + 6, GameArea.Height + 1),
        Function(key)
            ActiveEntity.Sprite = Sunny_
            If Not WaitingForCPU Then
                Select Case key
                    Case ConsoleKey.D1 To ConsoleKey.D7
                        Dim i = key - ConsoleKey.D0
                        If Not Entities.Any(Function(e) If(e.Position = New Point(GameArea.Left + i * 2, GameArea.Top + 1), False)) Then
                            Whites.Add(New Point(GameArea.Left + i * 2, GameArea.Top + 1), AddressOf OnCPUTick)
                            WaitingForCPU = True
                        End If
                End Select
            End If
            Return True
        End Function,
        Sub()
            Instructions.Position = New Point(3, 0)
            Ruler.Position = New Point(10, 9)
            AddHandler Tick, AddressOf OnTick
        End Sub, Sub() RemoveHandler Tick, AddressOf OnTick)
    Protected Overrides ReadOnly Property Left As Func(Of Region) = Function() New Region3_BullsAndCows()
    Protected Overrides ReadOnly Property Right As Func(Of Region) = Function() New Region5_Win()
    Protected Sub OnTick()
        Select Case WhoWin(Nothing)
            Case Player.Player
                Instructions.Color = ConsoleColor.Green
                Instructions.Text = "You win!"
                Instructions.Position = New Point(13, 0)
                ActiveEntity.Position = New Point(GameArea.Right + 4, GameArea.Bottom - 3)
                Trigger.Dispose()
            Case Player.CPU
                Instructions.Color = ConsoleColor.Red
                Instructions.Text = "CPU wins!"
                Instructions.Position = New Point(13, 0)
                ActiveEntity.Sprite = Sunny_Angry
                Trigger.Dispose()
        End Select
    End Sub
    Protected Function WhoWin(simulateCoordinatesOpt As (blackX As Integer?, whiteX As Integer?)) As Player
        Dim Matches =
            Function(side As GravityEntityFactory, point As Point, simulatePosition As Point?)
                Dim occupier As GravityEntityFactory.GravityEntityFactoryEntity = Nothing
                For Each item In Entities.OfType(Of GravityEntityFactory.GravityEntityFactoryEntity)
                    If item.Position = New Point(GameArea.Left + point.Left * 2, GameArea.Top - 1 + point.Top) Then
                        occupier = item
                        Exit For
                    End If
                Next
                Return If(occupier IsNot Nothing,
                    occupier.Owner Is side AndAlso occupier.VerticalVelocity = 0,
                    point = simulatePosition)
            End Function
        Dim black = IfHasValue(simulateCoordinatesOpt.blackX,
            Function(blackX)
                For y = 1 To 7
                    If Matches(Whites, New Point(blackX, y), Nothing) OrElse
                       Matches(Blacks, New Point(blackX, y), Nothing) Then
                        Return New Point(blackX, If(y = 1, 1, y - 1))
                    End If
                Next
                Return New Point(blackX, 7)
            End Function)
        Dim white = IfHasValue(simulateCoordinatesOpt.whiteX,
                    Function(whiteX)
                        For y = 1 To 7
                            If Matches(Whites, New Point(whiteX, y), Nothing) OrElse
                               Matches(Blacks, New Point(whiteX, y), Nothing) OrElse
                               black = New Point(whiteX, y) Then _
                               Return New Point(whiteX, If(y = 1, 1, y - 1))
                        Next
                        Return New Point(whiteX, 7)
                    End Function)
        Dim connected = Function(p1 As Point, p2 As Point, p3 As Point, p4 As Point)
                            If Matches(Whites, p1, white) AndAlso
                               Matches(Whites, p2, white) AndAlso
                               Matches(Whites, p3, white) AndAlso
                               Matches(Whites, p4, white) Then Return Player.Player
                            If Matches(Blacks, p1, black) AndAlso
                               Matches(Blacks, p2, black) AndAlso
                               Matches(Blacks, p3, black) AndAlso
                               Matches(Blacks, p4, black) Then Return Player.CPU
                            Return Player.None
                        End Function
        ' -
        For y = 1 To 7
            For x = 1 To 4
                WhoWin = connected(New Point(x, y), New Point(x + 1, y), New Point(x + 2, y), New Point(x + 3, y))
                If WhoWin <> Player.None Then Return WhoWin
            Next
        Next
        ' |
        For y = 1 To 4
            For x = 1 To 7
                WhoWin = connected(New Point(x, y), New Point(x, y + 1), New Point(x, y + 2), New Point(x, y + 3))
                If WhoWin <> Player.None Then Return WhoWin
            Next
        Next
        ' \
        For x = 1 To 4
            For y = 1 To 4
                WhoWin = connected(New Point(x, y), New Point(x + 1, y + 1), New Point(x + 2, y + 2), New Point(x + 3, y + 3))
                If WhoWin <> Player.None Then Return WhoWin
            Next
        Next
        ' /
        For x = 1 To 4
            For y = 4 To 7
                WhoWin = connected(New Point(x, y), New Point(x + 1, y - 1), New Point(x + 2, y - 2), New Point(x + 3, y - 3))
                If WhoWin <> Player.None Then Return WhoWin
            Next
        Next
        Return Player.None
    End Function
    Sub OnCPUTick()
        Dim cpu = CPUTurn()
        If cpu IsNot Nothing Then
            Blacks.Add(New Point(GameArea.Left + cpu.GetValueOrDefault() * 2, GameArea.Top + 1))
        Else
            Instructions.Text = "Stalemate. Press Enter to restart."
            Instructions.Position = New Point(3, 0)
        End If
        RemoveHandler Tick, AddressOf OnCPUTick
        WaitingForCPU = False
    End Sub
    Function CPUTurn() As Integer?
        Dim choices = Enumerable.Range(1, 7).Where(Function(x) _
               If(Not (Whites.ItemAt(New Point(GameArea.Left + x * 2, GameArea.Top + 1))?.VerticalVelocity = 0 OrElse
                       Blacks.ItemAt(New Point(GameArea.Left + x * 2, GameArea.Top + 1))?.VerticalVelocity = 0), True)).ToHashSet()
        Dim avoidChoices = New HashSet(Of Integer)()
        ' 1. Black win
        For Each x In choices
            If WhoWin((x, Nothing)) = Player.CPU Then Return x
        Next
        ' 2. Block white win
        For Each x In choices
            If WhoWin((Nothing, x)) = Player.Player Then Return x
        Next
        ' 3. Avoid white win
        For Each x In choices
            For Each x2 In choices
                If WhoWin((x, x2)) = Player.Player Then avoidChoices.Add(x)
            Next
        Next
        ' 4. Random placement
        choices.ExceptWith(avoidChoices)
        Return If(choices.Count > 0, choices.RandomItem(),
               If(avoidChoices.Count > 0, avoidChoices.RandomItem(), New Integer?()))
    End Function
End Class
Class Region5_Win
    Inherits Region
    Protected ReadOnly Win As New TextEntity(WriteEntities, "You win!! Press Enter to start again.", New Point(6, 0))
    Protected ReadOnly Fireworks As Sprite() = {Firework_1, Firework_2, Firework_3}
    Protected ReadOnly Trigger As New TriggerZone(WriteEntities, New Rectangle(0, 0, WindowWidth, WindowHeight),
        Function(key)
            If key = ConsoleKey.Enter Then
                SetCurrentRegion = Function() New Region1_Title()
                SaveRegion(CurrentRegion)
            End If
            Return False
        End Function,
        Sub() If Not TickEvent.GetInvocationList().Where(Function(x) x.Target Is Me).Any() Then AddHandler Tick, AddressOf OnTick,
        Sub() RemoveHandler Tick, AddressOf OnTick)
    Protected Sub OnTick()
        WriteEntities.Add(New IteratingSpriteEntity(WriteEntities, Fireworks, New Point(Random.Next(WindowWidth), Random.Next(1, 7))))
    End Sub
    Protected Overrides ReadOnly Property Left As Func(Of Region) = Function() New Region4_ConnectFour()
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

        If Not IO.File.Exists(FileName) Then IO.File.Create(FileName).Dispose()
Login:  WriteLine("Login")
        WriteLine("-----")
        While _currentRegion Is Nothing
            Write("User name (Empty to register): ")
            UserName = ReadLine()
            If UserName = "" Then
                Clear()
                WriteLine("Register")
                WriteLine("--------")
                While UserName = ""
                    Write("User name (Empty to login): ")
                    UserName = ReadLine()
                    If UserName = "" Then
                        Clear()
                        GoTo Login
                    Else
                        If IO.File.ReadLines(FileName).Any(Function(line) line.Split(FieldSeparator)(0) = UserName) Then
                            WriteLine("User already exists.")
                            UserName = ""
                        End If
                    End If
                End While
                Write("Password: ")
                IO.File.AppendAllLines(FileName, {String.Join(FieldSeparator, UserName, ReadLine(), GetType(Region1_Title).FullName)})
                WriteLine("Registration successful. Press Enter to start!")
                ReadLine()
                Clear()
                _currentRegion = New Region1_Title
            Else
                Dim config = IO.File.ReadAllLines(FileName)
                Dim line = config.Select(Function(l) l.Split(FieldSeparator)).FirstOrDefault(Function(l) l(0) = UserName)
                If line Is Nothing Then
                    WriteLine("User not found.")
                Else
                    Write("Password: ")
                    If ReadLine() = line(1) Then
                        Clear()
                        _currentRegion = CType(Type.GetType(line(2)).GetConstructor({}).Invoke({}), Region)
                    Else
                        WriteLine("Incorrect password.")
                    End If
                End If
            End If
        End While

        CursorVisible = False
        Sunny.Position = New Point(2, 5)
        While True
            RaiseEvent Tick()
            Dim key = ReadKey(TimeSpan.FromSeconds(0.2))
            If key IsNot Nothing Then ActiveEntity.HandleKey(key.GetValueOrDefault())
        End While
    End Sub
End Module