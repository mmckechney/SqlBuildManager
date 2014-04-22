Imports System.Runtime.InteropServices
Imports System.Windows.Forms

Public Class RTFScrollSync
    Private Declare Auto Function SendScrollPosMessage Lib "user32.dll" Alias "SendMessage" (ByVal hWnd As IntPtr, _
                 ByVal Msg As Integer, _
                 ByVal wParam As IntPtr, _
                 ByRef lParam As POINT) As Integer
    Private Const WM_USER = &H400
    Private Const EM_GETSCROLLPOS = WM_USER + 221
    Private Const EM_SETSCROLLPOS = WM_USER + 222
    Private Structure POINT
        Public x As Integer
        Public y As Integer
    End Structure
    Private aControls As New ArrayList
    Private sbScrollBarType As Windows.Forms.ScrollBars
    Public Property ScrollBarToSync() As Windows.Forms.ScrollBars
        Get
            Return sbScrollBarType
        End Get
        Set(ByVal Value As Windows.Forms.ScrollBars)
            sbScrollBarType = Value
        End Set
    End Property
    Public Sub AddControl(ByVal RTFControl As Object)
        Dim objControlSubClass As New clsWindowSubClass(RTFControl.Handle, _
                                                            RTFControl, Me)
        aControls.Add(objControlSubClass)
    End Sub
    Public Sub SyncScrollBars(ByVal Handle As IntPtr, _
                              ByVal SubClass As clsWindowSubClass, _
                              ByVal Window As Object, _
                              ByRef WindowsMessage As Message)
        Static blnIgnoreMessages As Boolean
        If blnIgnoreMessages = True Then Exit Sub
        blnIgnoreMessages = True
        Dim blnChangeVertPos As Boolean = False
        Dim blnChangeHorizPos As Boolean = False
        Dim lngVertPos, lngHorizPos As Long
        Dim stcScrollPoint As New POINT
        'Dim ptrScrollPoint As IntPtr
        SendScrollPosMessage(Handle, EM_GETSCROLLPOS, _
                                    New IntPtr(0), stcScrollPoint)
        lngVertPos = stcScrollPoint.y
        lngHorizPos = stcScrollPoint.x
        If (sbScrollBarType = RichTextBoxScrollBars.Both Or sbScrollBarType = RichTextBoxScrollBars.Vertical) Then
            blnChangeVertPos = True
        End If
        If (sbScrollBarType = RichTextBoxScrollBars.Both Or sbScrollBarType = RichTextBoxScrollBars.Horizontal) Then
            blnChangeHorizPos = True
        End If
        If blnChangeVertPos = True Or blnChangeHorizPos = True Then
            Dim objSubClass As clsWindowSubClass
            Dim objWindowMessage As New Windows.Forms.Message
            For Each objSubClass In aControls
                If objSubClass.Handle.ToInt32 <> Handle.ToInt32 Then
                    SendScrollPosMessage(objSubClass.Handle, EM_GETSCROLLPOS, New IntPtr(0), stcScrollPoint)

                    If blnChangeHorizPos = True Then
                        stcScrollPoint.x = lngHorizPos
                    End If
                    If blnChangeVertPos = True Then
                        stcScrollPoint.y = lngVertPos
                    End If
                    SendScrollPosMessage(objSubClass.Handle, EM_SETSCROLLPOS, _
                                               New IntPtr(0), stcScrollPoint)
                End If
            Next
        End If
        blnIgnoreMessages = False
    End Sub
End Class
Public Class clsWindowSubClass
    Inherits System.Windows.Forms.NativeWindow
    Private objWindow As Object
    Private objParent As RTFScrollSync
    Public Sub New(ByVal Handle As IntPtr, ByVal Window As Object, ByVal Parent As RTFScrollSync)
        objWindow = Window
        objParent = Parent
        MyBase.AssignHandle(Handle)
    End Sub
    Protected Overrides Sub WndProc(ByRef WindowMessage As System.Windows.Forms.Message)
        MyBase.WndProc(WindowMessage)
        objParent.SyncScrollBars(MyBase.Handle, Me, _
                                         objWindow, WindowMessage)
    End Sub
End Class

