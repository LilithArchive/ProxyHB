Imports System.ComponentModel
Imports System.Net
Imports Newtonsoft.Json.Linq
Imports Titanium.Web.Proxy
Imports Titanium.Web.Proxy.EventArguments
Imports Titanium.Web.Proxy.Models

Public Class Form1
    Dim WithEvents ProxyHB As New ProxyServer()
    Dim CaptchaToken As String = ""
    Private Sub Form1_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        Try
            Dim explicitEndPoint = New ExplicitProxyEndPoint(IPAddress.Any, 666, True)
            ProxyHB.AddEndPoint(explicitEndPoint)
            ProxyHB.Start()
            ProxyHB.SetAsSystemHttpProxy(explicitEndPoint)
            ProxyHB.SetAsSystemHttpsProxy(explicitEndPoint)
            LogText("[Proxy started on port " & explicitEndPoint.Port & "]")
        Catch ex As Exception
            LogText("[Error]" & vbNewLine & ex.Message)
        End Try
    End Sub

    Private Sub Form1_Closing(sender As Object, e As CancelEventArgs) Handles Me.Closing
        ProxyHB.DisableAllSystemProxies()
    End Sub

    Private Function ProxyHB_BeforeRequest(sender As Object, e As SessionEventArgs) As Task Handles ProxyHB.BeforeRequest
        Dim ReqHost As String = e.HttpClient.Request.Host
        Dim ReqURL As String = e.HttpClient.Request.Url
        If ReqHost.Contains("habbo") Then
            If ReqURL.Contains("api/public/captcha?token=") Then
                CaptchaToken = ReqURL.Remove(0, ReqURL.IndexOf("?token=") + 7)
                LogText("[Token detected]")
            End If
            If ReqURL.Contains("api/public/authentication/login") Then
                If e.HttpClient.Request.HeaderText.Contains("app:/HabboTablet.swf") Then
                    Dim ReqBodyJSON = JObject.Parse(e.GetRequestBodyAsString.Result)
                    ReqBodyJSON.Add("captchaToken", CaptchaToken)
                    e.SetRequestBodyString(ReqBodyJSON.ToString)
                    If CaptchaToken = "" Then
                        LogText("[Error: Token requested, you need a new one]")
                    Else
                        LogText("[Token injected]")
                    End If
                    CaptchaToken = ""
                End If
            End If
            End If
    End Function

    Public Sub LogText(ByVal NewText As String)
        If InvokeRequired Then
            Me.Invoke(New Action(Of String)(AddressOf LogText), New Object() {NewText})
            Return
        End If

        TextBox1.AppendText(NewText & vbNewLine)
    End Sub

End Class
