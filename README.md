# Build Raspberry Pi .NET Core C# IoT Applications on Raspberry Pi OS or Ubuntu 20.04

![.net core loves single board computers](https://raw.githubusercontent.com/gloveboxes/Create-RaspberryPi-dotNET-Core-C-Sharp-IoT-Applications/master/resources/banner.png)

![twitter logo](https://raw.githubusercontent.com/gloveboxes/Create-RaspberryPi-dotNET-Core-C-Sharp-IoT-Applications/master/resources/twitter-logo.png) Follow me on [Twitter](https://twitter.com/dglover) @dglover

---

## #JulyOT

This is part of the #JulyOT [IoT Tech Community](http://aka.ms/julyot) series, a collection of blog posts, hands-on-labs, and videos designed to demonstrate and teach developers how to build projects with Azure Internet of Things (IoT) services. Please also follow [#JulyOT](https://twitter.com/hashtag/JulyOT) on Twitter.

---

## Operating Systems and ARM architectures supported

This tutorial has been tested with .NET Core applications running on **Raspberry Pi OS** and **Ubuntu 20.04** (including Ubuntu Mate 20.04) for both 32bit (ARM32) and 64bit (ARM64). The projects also include build tasks for Debug and Release configurations.

## Source Code

The source and the samples for this tutorial can be found [here](https://github.com/gloveboxes/Create-RaspberryPi-dotNET-Core-C-Sharp-IoT-Applications).

The examples are found in the samples director this repo and cover the following .NET Core scenarios.

1. Simple hello world: dotnet.core.iot.csharp
2. Azure IoT Hub: dotnet.core.iot.csharp.iothub
3. .NET Core Web API: dotnet.core.iot.csharp.webapi
4. .NET Core Web App: dotnet.core.iot.csharp.webapp
5. Azure IoT Hub or IoT Central with DPS (Device Provisioning Service): dotnet.core.iot.csharp.dps.iot
6. Azure IoT device twins: dotnet.core.iot.csharp.dps.iot.device-twins
7. Azure IoT Hub device twins, and Pi Sense HAT : dotnet.core.iot.csharp.dps.iot.device-twins.sense.hat
8. Simple Pi Sense HAT Sample: dotnet.core.iot.csharp.sense.hat
9: F# and .NET Core: dotnet.core.iot.fsharp

---

## Conventions used in the tutorial

The tutorial covers Raspberry Pi OS, Ubuntu 20.04 for both ARM32 and ARM64.

The tutorial refers to user and host as **user@host**.

For Raspberry Pi this maps to **pi@raspberrypi.local** or depending on your network set up **pi@raspberrypi**. The default password is **raspberry**.

For Ubuntu 20.04 this maps to **ubuntu@ubuntu.local** or **ubuntu@ubuntu**. The default password is **ubuntu**.

---

## Tips and Tricks for setting up Ubuntu 20.04 on a Raspberry Pi

Check out the following [Raspberry Pi Tips and Tricks](./TIPS_AND_TRICKS.md) to boot Ubuntu from USB3 SSD, how to overclock, enable WiFi, and support for the Raspberry Pi Sense HAT.

---

## Introduction

The .NET Core IoT Library connects your applications to hardware. In this tutorial you will learn how to:

1. Develop a C# .NET Core IoT application from your Linux, macOS or Windows 10 computer,
2. Streamline the develop, deploy, and debug process using Visual Studio Code,
3. Use the [.NET Core IoT](https://dotnet.microsoft.com/apps/iot?WT.mc_id=julyot-dnc-dglover) library to read the CPU temperature. The CPU temperature will be used to represent the room temperature in this lab scenario,
4. Stream temperature telemetry to [Azure IoT Hub](https://docs.microsoft.com/en-us/azure/iot-hub/about-iot-hub?WT.mc_id=julyot-dnc-dglover),
5. Set a virtual remote room thermostat.

---

## Raspberry Pi Hardware

.Net Core requires an AMR32v7 processor and above, so anything Raspberry Pi 2 or better and you are good to go. Note, Raspberry Pi Zero is an ARM32v6 processor, and is not supported.

The Raspberry Pi 3a Plus is a great device for .NET Core.

![](https://raw.githubusercontent.com/gloveboxes/Create-RaspberryPi-dotNET-Core-C-Sharp-IoT-Applications/master/resources/raspberrypi-3a-plus.jpg)

I'm super happy with my Raspberry Pi 4B 4GB and 8GB devices seen here dress with a heatsink case.

![](https://raw.githubusercontent.com/gloveboxes/Create-RaspberryPi-dotNET-Core-C-Sharp-IoT-Applications/master/resources/aluminium-heatsink-case-for-raspberry-pi-4-black_1_1.png)

I like to use Raspberry Pi OS Lite as it takes less resources than the full Raspberry Pi Desktop version and I do all my Raspberry Pi development from my computer.

If you've not set up a Raspberry Pi before then this is a great guide. "[Setting up a Headless Pi](https://learn.pimoroni.com/tutorial/sandyj/setting-up-a-headless-pi)". Be sure to use the WiFi network as your development computer.

If you are not comfortable setting up Raspberry Pi OS Lite (Headless), then follow the [Setting up your Raspberry Pi](https://projects.raspberrypi.org/en/projects/raspberry-pi-setting-up) guide to set up the full Raspberry Pi OS Desktop version.

### Optional Hardware

The Raspberry Pi Sense HAT has builtin sensors and a LED panel. If you have one then great, one of the labs uses this HAT.

![](https://raw.githubusercontent.com/gloveboxes/Create-RaspberryPi-dotNET-Core-C-Sharp-IoT-Applications/master/resources/pi-sense-hat.jpg)

---

## Why .NET Core

It used by millions of developers, it is mature, fast, supports multiple programming languages (C#, F#, and VB.NET), runs on multiple platforms (Linux, macOS, and Windows), and is supported across multiple processor architectures. It is used to build device, cloud, and IoT applications.

[.NET Core](https://docs.microsoft.com/en-au/dotnet/core?WT.mc_id=julyot-dnc-dglover) is an [open-source](https://github.com/dotnet/coreclr/blob/master/LICENSE.TXT), general-purpose development platform maintained by Microsoft and the .NET community on [GitHub](https://github.com/dotnet/core).

---

## Learning C#

![](https://raw.githubusercontent.com/gloveboxes/Create-RaspberryPi-dotNET-Core-C-Sharp-IoT-Applications/master/resources/c-sharp.png)

There are lots of great resources for learning C#. Check out the following:

1. [C# official documentation](https://docs.microsoft.com/en-us/dotnet/csharp/?WT.mc_id=julyot-dnc-dglover)
2. [C# 101 Series with Scott Hanselman and Kendra Havens](https://aka.ms/dotnet3-csharp)
3. [Full C# Tutorial Path for Beginners and Everyone Else](https://youtu.be/LUv20QxXjfw)

---

## The .NET Core IoT Libraries Open Source Project

The Microsoft .NET Core team along with the developer community are building support for [IoT](https://en.wikipedia.org/wiki/Internet_of_things) scenarios. The [.NET Core IoT Library](https://github.com/dotnet/iot) is supported on Linux, and Windows IoT Core, across ARM and Intel processor architectures. See the [.NET Core IoT Library Roadmap](https://github.com/dotnet/iot/blob/master/Documentation/roadmap.md) for more information.

### System.Device.Gpio

The [System.Device.Gpio](https://www.nuget.org/packages/System.Device.Gpio)  package supports general-purpose I/O ([GPIO](https://en.wikipedia.org/wiki/General-purpose_input/output)) pins, PWM, I2C, SPI and related interfaces for interacting with low-level hardware pins to control hardware sensors, displays and input devices on single-board-computers; [Raspberry Pi](https://www.raspberrypi.org/), [BeagleBoard](https://beagleboard.org/), [HummingBoard](https://www.solid-run.com/nxp-family/hummingboard/), [ODROID](https://www.hardkernel.com/), and other single-board-computers that are supported by Linux and Windows 10 IoT Core.

### Iot.Device.Bindings

The [.NET Core IoT Repository](https://github.com/dotnet/iot/tree/master/src) contains [IoT.Device.Bindings](https://www.nuget.org/packages/Iot.Device.Bindings), a growing set of community-maintained device bindings for IoT components that you can use with your .NET Core applications. If you can't find what you need then porting your own C/C++ driver libraries to .NET Core and C# is pretty straight forward too.

The drivers in the repository include sample code along with wiring diagrams. For example the [BMx280 - Digital Pressure Sensors BMP280/BME280](https://github.com/dotnet/iot/tree/master/src/devices/Bmxx80).

![](https://raw.githubusercontent.com/gloveboxes/Create-RaspberryPi-dotNET-Core-C-Sharp-IoT-Applications/master/resources/rpi-bmp280_i2c.png)

---

## Raspberry Pi .NET Core Developer Learning Path

* Lab 1: [Create your  first Raspberry Pi .NET Core C# IoT Application](labs/Lab_1_Build_dot_NET_Core_app/README.md)
* Lab 2: [Connect a room environment monitor to Azure IoT Central](labs/Lab_2_Azure_IoT_Central/README.md)
* Lab 3: [Remote control the room temperature](labs/Lab_3_IoT_Central_and_Device_Twins/README.md)
<!-- * Lab 4: [Create an ASP.NET Core Web to display the room temperature](labs/Lab_4_Create_ASP_dotNET_Core_WebApp/README.md)
* Lab 5: [Create an ASP.NET Core Web API to return the room temperature](labs/Lab_5_Create_ASP_dotNET_Core_WebApi/README.md) -->

---

Have fun and stay safe and be sure to follow us on [#JulyOT](https://twitter.com/hashtag/JulyOT?src=hash&ref_src=twsrc%5Etfw).