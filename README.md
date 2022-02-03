# Kotak Trader
Trading Platform built with KotakNet

KotakTrader is a simple winform application built with KotakNet (DotNet Library for Kotak API). KotakTrader supports all basic functionalities of a trdaing paltform. You can check the demo here https://github.com/howutrade/kotak-trader/tree/main/demo

The main purpose of the KotakTrader is to show how to use KotakNet in your program and call its functions. DotNet Develpoers can get started with KotakTrader or develop from the scratch.

# KotakNet DotNet Library

KotakNet is a highly reliable, efficient and scalabe DotNet libary built for Kotak API. KotakNet can be used to build trade or execution algorithms or can be used to build personalised trdaing platform.

Components of KotakNet:

1. KotakNet.Core.dll
2. KotakNet.Bridge.dll
3. KotakNet.Rtd.dll

KotakNet.Core.dll wraps all the methods supported by Kotak API. This is where user session is maintained.
KotakNet.Bridge can be used to place orders from Excel, AmiBroker and any program that supports COM
KotakNet.Rtd streams the websocket quotes receivecd from kotak server to excel through RTD

Core and Bridge Documentation:
https://howutrade.github.io/kotak-trader/html/R_Project_Documentation.htm

# For Non-Developers and AFL/VBA Coders
Non-developers, AFL/VBA coders and those who just want to place orders from their TA program can use Bridge. They can use KotakTrader as a frontend to login and receive order requests from Bridge.

Installing KotakTrader is a very simple;

1. Just downoad the release zip file from the below link
https://github.com/howutrade/kotak-trader/releases

2. Extract the contents of the downloaded zip file to a folder and follow the readme file

Example AFL and Excel Bridde codes:
https://github.com/howutrade/kotak-trader/tree/main/Bridge%20Examples

Bridge Documention:
https://howutrade.github.io/kotak-trader/html/N_KotakNet_Bridge.htm

*In real time scenario, you need to use static variables to restrict multiple order firing
*Bridge also supports dictinary function that can be used to manipulate static variables
