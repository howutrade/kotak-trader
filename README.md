# Kotak Trader
Trading Platform built with KotakNet

KotakTrader is a lightweight trading platform built with KotakNet. It's a winform and supports all the standard features of a trading platform. You can preview the demo here https://www.youtube.com/watch?v=VXgyNz0nZg4

# KotakNet DotNet Library

KotakNet is a highly reliable, scalable and efficient DotNet Library for Kotak API. KotakNet provides complete set of tools required by the developers to build algo trading systems, it greatly reduces burden of the developers and helps to deploy their trading system very quickly.

Components of KotakNet:

* KotakNet.Core.dll
* KotakNet.Bridge.dll
* KotakNet.Rtd.dll

KotakNet.Core.dll wraps all the methods supported by Kotak API. This is where the user session is maintained. It has all methods that are necessary to build a standard trdaing platform

DotNet developers can build console or Winform or WPF application on the top of KotakNet.Core, build and deploy customized execution algos (like Spread orders, P2M etc) or build personalized trading platforms for clients.

DotNet developers can get started with KotakTrader or build from the scratch.

KotakNet Documentation:
https://howutrade.github.io/kotaknet-doc/

## Quick Start (Build from Scratch)
```
Imports KotakNet.Core
Imports System.Threading

Module Demo
    Public Kotak As New Kotak

    ''' This is a basic example without any error handling
    Sub Main()
        '///Set Credentials
        '///set UserID property first before other properties as the settings are saved to file with 'UserID' as name
        Kotak.UserID = "XXXXXXXXX"
        Kotak.ConsumerKey = "xxxxxxxxxxxxxxxxxxxx"
        Kotak.ConsumerSecret = "xxxxxxxxxxxxxxxxxx"
        Kotak.AccessToken = "xxxxxxxxxx"
        Kotak.Password = "xxxx"

        '///Subscribe to Events
        AddHandler Kotak.AppUpdateEvent, AddressOf AppUpdate
        AddHandler Kotak.OrderUpdateEvent, AddressOf OrderUpdate
        AddHandler Kotak.QuotesReceivedEvent, AddressOf QuotesReceived

        '///Login to get Session Token
        Kotak.Login()

        '///Download Symbols
        Kotak.GetMasterContract()

        '/// Wait for the symbols to download
        Dim count As Integer = 0
        Do
            count = count + 1
            Thread.Sleep(1000)
            If (Kotak.SymbolStatus OrElse count > 300) Then Exit Do
        Loop

        If Not Kotak.SymbolStatus Then
            MsgBox("Symbol download fail", MsgBoxStyle.Exclamation, "KotakNet")
            Exit Sub
        End If

        '///Subscribe for Quotes
        Kotak.SubscribeQuotes("NSE", "ITC")
        Kotak.SubscribeQuotes("MCX", "CRUDEOIL22FEB18FUT")

        '///Place an order
        Dim OrderId As String = Kotak.PlaceRegularOrder("NSE", "ITC", "BUY", "MARKET", 1, "NRML", 0, 0)
        Console.WriteLine("OrderID: " & OrderId)

        Console.WriteLine("Press Any Key to exit")
        Console.ReadKey()
    End Sub

    Private Sub OrderUpdate(sender As Object, e As OrderUpdateEventArgs)
        Console.WriteLine("OrderUpdate - OrderId: " & e.OrderId & " Status: " & e.Status)
    End Sub

    Private Sub AppUpdate(sender As Object, e As AppUpdateEventArgs)
        Console.WriteLine("AppUpdate - Message: " & e.EventMessage)
    End Sub

    Private Sub QuotesReceived(sender As Object, e As QuotesReceivedEventArgs)
        Console.WriteLine("QuotesReceived - Symbol: " & e.TrdSym & " Ltp: " & e.LTP & " LTT: " & e.LTT.ToString("HH:mm:ss"))
    End Sub
End Module
```
*set UserID property first before other properties as the settings are saved to file with 'UserID' as name*

*In real case scenario, you need to add static variables (wherever applicable) to remove redundant calls*

# Indices Live Data

For Index data, pass Exch as "NSE_INDEX"

Index supported by Kotak API
| Index |
| --- |
| NIFTY 100 |
| NIFTY 50 |
| NIFTY BANK |
| NIFTY CPSE |
| NIFTY IT |
| NIFTY PSE |

Example:
SubscribeQuotes("NSE_INDEX", "NIFTY 50")

# Kotak API Subscription

For API subscription, check the this link
https://www.kotaksecurities.com/offers/trading-tools/trading-api/index.html

```diff
- (By default websocket is disabled for Kotak API, users has to enable websocket from their Kotak dashboard or email to ks.apihelp@kotak.com for enabling websocket.)
```
# KotakNet Bridge

Excel, AmiBroker and Other Bridge users who just want to place orders from their TA software can use KotakTrader as a frontend to receive bridge requests.

Installing KotakTrader is simple, download the release zip file from the below link
https://github.com/howutrade/kotak-trader/releases

Extract the zip file to a folder and follow the ReadMe file
Uers can refer the Bridge code samples to get started

Bridge Docs: https://howutrade.github.io/kotaknet-doc/html/T_KotakNet_Bridge_Bridge.htm

## Excel VBA

### Early Binding (Recommended)
To call Bridge functions from VBA, you need to add Reference 'Bridge For KotakNet'
In VBA Editor, Menu -> Tools -> References
From the available references select 'Bridge For KotakNet'
Declare a global Bridge Object in a module

```Public Bridge As New Bridge```

```
Dim Resp As String

'Place Order
Resp = Bridge.PlaceRegularOrder("NSE", "ITC", "BUY", "MARKET", "NRML", 2, 0, 0)

'Cancel Regular Order
Resp = Bridge.CancelRegularOrder("13805896989")

'Get Order Status
ordstatus = Bridge.GetOrderStatus("13805896989")
```

### Late Binding
For late binding, just create Bridge object and call the required functions

```
'Create Bridge Object
Dim Bridge As Object
Set Bridge = CreateObject("KotakNet.Bridge")

Dim Resp As String

'Place Order
Resp = Bridge.PlaceRegularOrder("NSE", "ITC", "BUY", "MARKET", "NRML", 2, 0, 0)

'Cancel Regular Order
Resp = Bridge.CancelRegularOrder("13805896989")

'Get Order Status
ordstatus = Bridge.GetOrderStatus("13805896989")
```

## Amibroker AFL
To call Bridge functions, you just need to create a Bridge object and call the required functions

```
//Create Bridge object
bridge = CreateStaticObject("KotakNet.Bridge");

//Place Order
resp = bridge.PlaceRegularOrder("NSE", "ITC", "BUY", "MARKET", "NRML", 2, 0, 0);

//Cancel Regular Order
resp = bridge.CancelRegularOrder("13805896989");

//Get Order Status
ordstatus = bridge.GetOrderStatus("13805896989");
```

*In real case scenario, you need to add static variables (wherever applicable) to remove redundant calls*

# Common Parameters

| Parameter | Value |
| --- | --- |
| Exch | NSE,NFO,BSE,CDS,MCX |
| Trdsym | CASH: Symbol (Ex: ITC)  |
| ... | FUT: [SYMBOL][YYMMMDD][FUT] (Ex: NIFTY22FEB24FUT) |
|... | OPT: [SYMBOL][YYMMMDD][STRIKE][OPT] (Ex: NIFTY22FEB2417300CE) |
| Transactions | BUY,SELL (SHORT,COVER) |
| Order Type | LIMIT,MARKET,SL,SL-M |
| Product Type | MIS,NRML |
| Quantity | CASH: Number of shares to Buy or Sell |
| ... | FNO: Number of lots to Buy or Sell (Not LotSize) |
| Validity | DAY,IOC |
| Limit/Trigger Price | Number (KotakNet will automatically round the price to ticksize) |
| Stgycode | Alphanumeric, must be 3 characters |
| Tag | Alphanumeric, must be 3-24 characters |
