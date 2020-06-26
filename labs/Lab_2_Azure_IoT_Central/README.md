# Lab 2: Connect a room environment monitor to Azure IoT Central

<!-- ![](resources/azure-sphere-iot-central-banner.png) -->

---

|Author|[Dave Glover](https://developer.microsoft.com/en-us/advocates/dave-glover?WT.mc_id=github-blog-dglover), Microsoft Cloud Developer Advocate, [@dglover](https://twitter.com/dglover) |
|:----|:---|
|Date| March 2020|

---

## Raspberry Pi .NET learning Path

Each module assumes you have completed the previous module.

[Home](../../README.md)

* Lab 0: [Lab Set Up](../Lab_0_Introduction_and_Lab_Set_Up/README.md)
* Lab 1: [Introduction to Azure Sphere development](../Lab_1_Visual_Studio_and_Azure_Sphere/README.md)
* Lab 2: [Connect a room environment monitor to Azure IoT](../Lab_2_Send_Telemetry_to_Azure_IoT_Central/README.md)
* Lab 3: [Set the room virtual thermostat with Azure IoT Device Twins](../Lab_3_Control_Device_with_Device_Twins/README.md)
* Lab 4: [Remote reboot your Azure Sphere with Azure IoT Direct Methods](../Lab_4_Control_Device_with_Direct_Methods/README.md)
* Lab 5: [Integrate FreeRTOS Real-time room sensors with Azure IoT](../Lab_5_FreeRTOS_and_Inter-Core_Messaging/README.md)
* Lab 6: [Integrate Azure RTOS Real-time room sensors with Azure IoT](../Lab_6_AzureRTOS_and_Inter-Core_Messaging/README.md)
* Lab 7: [Connect and control your room environment monitor with Azure IoT](../Lab_7_Azure_IoT_Central_RTOS_Integration/README.md)
<!-- * Lab 8: [Over-the-air (OTA) Deployment](/zdocs/Lab_8_Over-the-air-deployment/README.md) -->

---

## What you will learn

You will learn how to build a High-Level [Azure Sphere](https://azure.microsoft.com/services/azure-sphere/?WT.mc_id=github-blog-dglover) application that connects and sends telemetry to [Azure IoT Central](https://azure.microsoft.com/services/iot-central/?WT.mc_id=github-blog-dglover).

---

## Prerequisites

The lab assumes you understand the content from [Lab 1: Introduction to Azure Sphere development](../Lab_1_Visual_Studio_and_Azure_Sphere/README.md).



---

## Supported browsers for Azure IoT Central

We recommend that you use the most up-to-date browser that's compatible with your operating system. The following browsers are supported:

* Microsoft Edge (latest version). This means the **Chromium** version of the Microsoft Edge Browser
* Safari (latest version, Mac only)
* Chrome (latest version)
* Firefox (latest version)

[Supported browsers for Azure IoT Central](https://docs.microsoft.com/en-us/azure/iot-central/core/iot-central-supported-browsers)

## Tutorial Overview

1. How to create an Azure IoT Central Application
2. Enabling trust between your Azure Sphere Tenant and your Azure IoT Central Application for automatic device enrollment.
3. Explicitly allowing connections to Azure IoT Central Application network endpoints.
4. Configuring the Azure Sphere Application to connect to Azure IoT Central.

---

## Key Concepts

### Azure IoT

Your Azure Sphere device can securely connect and communicate with cloud services. Azure Sphere includes built-in library support for both Azure IoT Hub and Azure IoT Central. This lab focuses on Azure IoT Central.

This project leverages the [Azure IoT Hub Device Provisioning Service (PDS)](https://docs.microsoft.com/en-us/azure-sphere/app-development/use-azure-iot?WT.mc_id=github-blog-dglover), which is included with Azure IoT Central. The Device Provisioning Service (DPS) enables zero-touch, just-in-time, large scale device provisioning.

Take a moment to read [Your IoT journey: simplified and secure with Azure IoT Central and Azure Sphere](https://techcommunity.microsoft.com/t5/internet-of-things/your-iot-journey-simplified-and-secure-with-azure-iot-central/ba-p/1404247).

---

## Azure IoT Central

[Azure IoT Central](https://azure.microsoft.com/en-in/services/iot-central/?WT.mc_id=github-blog-dglover) provides an easy way to connect, monitor, and manage your Internet of Things (IoT) assets at scale.

![Azure IoT Central](resources/azure-iot-central.jpg)

---

## Step 1: Create a new IoT Central Application

1. So the lab instructions are still visible, right mouse click, and open this link "[Azure IoT Central](https://azure.microsoft.com/en-au/services/iot-central/?WT.mc_id=pycon-blog-dglover)" in a new window.

2. Click **Build a solution**.

3. Next, you will need to sign with your Microsoft Personal, or Work, or School account. If you do not have a Microsoft account, then you can create one for free using the **Create one!** link.

    ![iot central](resources/iot-central-login.png)

4. Expand the sidebar menu by clicking on the **Burger menu** icon.

    ![](resources/iot-central-burger-menu.png)

5. Click **+ New application** to create a new Azure IoT Central application. 

6. Select **Custom app**

    ![](resources/iot-central-custom-app.png)

### Create a new application

1. Specify the **Application name**, the **URL**, select the **Free** pricing plan, and complete the registration form. 

    ![](resources/iot-central-new-application.png)

2. Then click **Create**.

### Create a new device template

A device template is a blueprint that defines the characteristics and behaviors of a type of device that connects to an Azure IoT Central application.

For more information on device templates, review the [Define a new IoT device type in your Azure IoT Central application](https://docs.microsoft.com/en-us/azure/iot-central/core/howto-set-up-template?WT.mc_id=github-blog-dglover) article. 

1. Click **Device templates**, then **+ New**.
    ![](resources/iot-central-template-new.png)

2. Click the **IoT device** template type.

    ![](resources/iot-central-new-iot-device-template.png)

3. Create an **IoT Device** Template.

    1. Select **IoT device**,
    2. Click **Next:Customise**,
    3. Name your template **Azure Sphere**,
    4. Click **Next: Review**,
    5. Click **Create**.

### Import a Capability Model

1. Click **Import capability model**
2. Navigate to the folder you cloned the Azure Sphere Developer Learning Path into.
3. Navigate to the **iot_central** folder.
4. Select **Azure_Sphere_Developer_Learning_Path.json** and open

### Create a device visualization view

1. Click **Views**.
    ![](resources/iot-central-create-a-view.png)
2. Select **Visualizing the device**.
    ![](resources/iot-central-add-tile-status.png)
3. Select **Humidity**, **Pressure**, and **Temperature** telemetry items.
    ![](resources/iot-central-add-tile-environment.png)
4. Click **Add Tile**.
7. Click **Save** to save the view.

### Create a properties form

1. Click **Views**.
2. Click the **Editing device and cloud data** option.
    ![](resources/iot-central-view-properties-create.png)
3. Expand the **Properties** section.
4. Select **all properties**.
    ![](resources/iot-central-add-tile-form.png)
5. Click **Add Section**.
6. Click **Save** to save the form.

### Publish the device template

1. Click **Publish** to publish the template. Publishing the template makes it available for devices.
    ![](resources/iot-central-template-publish.png)
    <br/>

2. Next, confirm and click **Publish**

>See [Define a new IoT device type in your Azure IoT Central application](https://docs.microsoft.com/en-us/azure/iot-central/core/howto-set-up-template?WT.mc_id=github-blog-dglover) for information on creating your own device templates.

---

## Step 2: Link your Azure Sphere Tenant to IoT Central

You need to set up a trust relationship between your Azure Sphere tenant and your IoT Central application.

Devices claimed by your Azure Sphere tenant will be automatically enrolled when it first connects to your IoT Central application.

### Download the tenant authentication CA certificate

1. Open an **Azure Sphere Developer Command Prompt**
2. Be sure to make a note of the current directory, or change to the Azure Sphere Learning path directory. You will need the name of this directory in the next step. 
3. Download the Certificate Authority (CA) certificate for your Azure Sphere tenant:

    ```bash
    azsphere tenant download-CA-certificate --output CAcertificate.cer
    ```

    The output file must have the .cer extension.

### Upload the tenant CA certificate to Azure IoT Central and generate a verification code

1. In Azure IoT Central, go to Administration > Device Connection > Manage primary certificate.

2. Click the folder icon next to the Primary box and navigate to the directory where you downloaded the certificate. If you don't see the .cer file in the list, make sure that the view filter is set to All files (*). Select the certificate and then click the gear icon next to the Primary box.

3. The Primary Certificate dialog box appears. The Subject and Thumbprint fields contain information about the current Azure Sphere tenant and primary root certificate.

4. Click the Refresh icon to the right of the Verification Code box to generate a verification code. Copy the verification code to the clipboard.

    ![](resources/iot-central-certificate-verify.png)

### Verify the tenant CA certificate

1. Return to the Azure Sphere Developer Command Prompt.

2. Download a validation certificate that proves that you own the tenant CA certificate. Replace code in the command with the verification code from the previous step.

    ```bash
    azsphere tenant download-validation-certificate --output ValidationCertification.cer --verificationcode <code>
    ```
3. The Azure Sphere Security Service signs the validation certificate with the verification code to prove that you own the CA.

### Use the validation certificate to verify the tenant identity

1. Return to Azure IoT Central and click Verify.
2. When prompted, navigate to the validation certificate that you downloaded in the previous step and select it. When the verification process is complete, the Primary Certificate dialog box displays the Verified message. Click Close to dismiss the box.

    ![](resources/iot-central-certificate-verified.png)

After you complete these steps, any device that is claimed into your Azure Sphere tenant will automatically be enrolled in your Azure IoT Central application when it first connects.

---

## Step 3: Explicitly allow connections to Azure IoT Central Endpoints

Remember, applications on Azure Sphere are locked down by default, including hardware and network endpoints. You must explicitly allow connections to the network endpoints of your Azure IoT Central application otherwise your Azure Sphere application will not be able to connect.

Follow these steps:

1. Open the **Azure Sphere Developer Command Prompt**.
2. Navigate to the Samples -> AzureIoT -> Tools directory. You cloned the Azure Sphere samples repository in the first lab.
    * On Windows, navigate to the Samples\AzureIoT\Tools\win-x64 directory.
    * On Linux, navigate to the Samples\AzureIoT\Tools\linux-x64 directory. On Linux, you may need to explicitly set execution permissions for the ShowIoTCentralConfig tool. From a terminal, run `chmod +x ShowIoTCentralConfig` to add execution permissions for the tool.

3. When you run the **ShowIoTCentralConfig** tool, you will be prompted for input data. The following table outlines what information you will be prompted for and where to obtain the required data.

    | Input data | From |
    |------------|----------|
    | **Are you using a legacy (2018) IoT Central application (Y/N)** | Respond **N** |
    | **IoT&nbsp;Central&nbsp;App&nbsp;URL** |  This can be found in your browser address bar. For Example https://myiotcentralapp.azureiotcentral.com/ |
    | **API token** | This can be generated from your IoT Central application. In the Azure IoT Central application select **Administration**, select **API Tokens**, select **Generate Token**, provide a name for the token (for example, "AzureSphereSample"), select **Administrator** as the role, and click **Generate**. Copy the token to the clipboard. The token starts with **SharedAccessSignature**. |
    | **ID Scope** | In the Azure IoT Central application, select **Administration** > **Device Connection** and then copy the **ID Scope** |
4. Run the **ShowIoTCentralConfig** tool.
    Now follow the prompts that the tool provides, and copy the information from the output into the app_manifest.json file in Visual Studio.

    > **Note**: Your organization might require consent for the ShowIoTCentralConfig tool to access your Azure IoT Central data in the same way that the Azure API requires such consent. In some organizations, [enterprise application permissions](https://docs.microsoft.com/azure-sphere/install/admin-consent) must be granted by an IT administrator.
5. Review the output from the **ShowIoTCentralConfig** tool. It will look similar to the following text.

    </br>

    Find and modify the CmdArgs, AllowedConnections and DeviceAuthentication lines in your app_manifest.json so each includes the content from the below:

    ```json
    "CmdArgs": [ "0ne000BDC00" ],
    "Capabilities": {
        "AllowedConnections": [ "global.azure-devices-provisioning.net", "iotc-9999bc-3305-99ba-885e-6573fc4cf701.azure-devices.net", "iotc-789999fa-8306-4994-b70a-399c46501044.azure-devices.net", "iotc-7a099966-a8c1-4f33-b803-bf29998713787.azure-devices.net", "iotc-97299997-05ab-4988-8142-e299995acdb7.azure-devices.net", "iotc-d099995-7fec-460c-b717-e99999bf4551.azure-devices.net", "iotc-789999dd-3bf5-49d7-9e12-f6999991df8c.azure-devices.net", "iotc-29999917-7344-49e4-9344-5e0cc9999d9b.azure-devices.net", "iotc-99999e59-df2a-41d8-bacd-ebb9999143ab.azure-devices.net", "iotc-c0a9999b-d256-4aaf-aa06-e90e999902b3.azure-devices.net", "iotc-f9199991-ceb1-4f38-9f1c-13199992570e.azure-devices.net" ],
        "DeviceAuthentication": "--- YOUR AZURE SPHERE TENANT ID---",
    }
    ```

6. **Copy** the output from the ShowIoTCentralConfig tool to **_Notepad_** as you will need this information soon.

---

## Step 4: Get the Azure Sphere Tenant ID

We need the ID of the Azure Sphere Tenant that is now trusted by Azure IoT Central.

1. From the **Azure Sphere Developer Command Prompt**, run **```azsphere tenant show-selected```**.
    * The output of this command will look similar to the following.
        ```text
        Default Azure Sphere tenant ID is 'yourSphereTenant' (99999999-e021-43ce-9999-fa9999499994).
        ```
    * The **Tenant ID** is the numeric value inside the parentheses.
2. **Copy the Tenant ID to _Notepad_** as you will need it soon.

<!-- ---

## Azure IoT Central Device Templates

A device template is a blueprint that defines the characteristics and behaviors of a type of device that connects to an Azure IoT Central application.

For more information on device templates, review the [Define a new IoT device type in your Azure IoT Central application](https://docs.microsoft.com/en-us/azure/iot-central/core/howto-set-up-template?WT.mc_id=github-blog-dglover) article. 

1. From Azure IoT Central, navigate to **Device templates**, and select the **Azure Sphere** template.
2. Click on **Interfaces** to list the interface capabilities.
3. Explore the IoT Central device template interfaces, properties, and views.

![](resources/iot-central-device-template-display.png) -->

---

## Open Lab 2

### Step 1: Start Visual Studio Code

![](resources/vs-code-start.png)

### Step 2: Open the lab project

1. Click **Open folder**.
2. Open the Azure-Sphere lab folder.
4. Open the **Lab_2_Send_Telemetry_to_Azure_IoT_Central** folder.
5. Click **Select Folder** or the **OK** button to open the project.

### Step 3: Set your developer board configuration

These labs support developer boards from AVNET and Seeed Studio. You need to set the configuration that matches your developer board.

The default developer board configuration is for the AVENT Azure Sphere Starter Kit. If you have this board, there is no additional configuration required.

1. Open CMakeList.txt
	![](resources/vs-code-open-cmake.png)
2. Add a # at the beginning of the set AVNET line to disable it.
3. Uncomment the **set** command that corresponds to your Azure Sphere developer board.

	```text
	set(AVNET TRUE "AVNET Azure Sphere Starter Kit")                
	# set(SEEED_STUDIO_RDB TRUE "Seeed Studio Azure Sphere MT3620 Development Kit (aka Reference Design Board or rdb)")
	# set(SEEED_STUDIO_MINI TRUE "Seeed Studio Azure Sphere MT3620 Mini Dev Board")
	```	

4. Save the file. This will auto-generate the CMake cache.

### Step 4: Configure the Azure Sphere Application

1. Open the **app_manifest.json** file

    ![](resources/vs-code-open-app-manifest.png)

2. Update the Azure IoT Central Application connection properties.
    * Update **CmdArgs** with your Azure IoT Central **ID Scope**. 
    * Update **DeviceAuthentication** with your **Azure Sphere Tenant ID**. Remember, this was the numeric value output from the ```azsphere tenant show-selected``` command that you copied to Notepad.

3. Update the network endpoints **AllowedConnections** with your Azure IoT Central Application endpoint URLs you copied to Notepad.

4. **Review** your updated manifest_app.json file. It should look similar to the following.

    ```json
    {
        "SchemaVersion": 1,
        "Name": "AzureSphereIoTCentral",
        "ComponentId": "25025d2c-66da-4448-bae1-ac26fcdd3627",
        "EntryPoint": "/bin/app",
        "CmdArgs": [ "0ne0099999D" ],
        "Capabilities": {
            "Gpio": [
                "$BUTTON_A",
                "$BUTTON_B",
                "$LED2",
                "$NETWORK_CONNECTED_LED",
                "$LED_RED",
                "$LED_GREEN",
                "$LED_BLUE"
            ],
            "I2cMaster": [ "$I2cMaster2" ],
            "PowerControls": [ "ForceReboot" ],
            "AllowedConnections": [ 
                "global.azure-devices-provisioning.net",
                "iotc-9999bc-3305-99ba-885e-6573fc4cf701.azure-devices.net", 
                "iotc-789999fa-8306-4994-b70a-399c46501044.azure-devices.net", 
                "iotc-7a099966-a8c1-4f33-b803-bf29998713787.azure-devices.net",
                "iotc-97299997-05ab-4988-8142-e299995acdb7.azure-devices.net", 
                "iotc-d099995-7fec-460c-b717-e99999bf4551.azure-devices.net", 
                "iotc-789999dd-3bf5-49d7-9e12-f6999991df8c.azure-devices.net", 
                "iotc-29999917-7344-49e4-9344-5e0cc9999d9b.azure-devices.net", 
                "iotc-99999e59-df2a-41d8-bacd-ebb9999143ab.azure-devices.net", 
                "iotc-c0a9999b-d256-4aaf-aa06-e90e999902b3.azure-devices.net", 
                "iotc-f9199991-ceb1-4f38-9f1c-13199992570e.azure-devices.net" 
            ],
            "DeviceAuthentication": "9d7e79eb-9999-43ce-9999-fa8888888894"
        },
        "ApplicationType": "Default"
    }
    ```

4. **IMPORTANT**. Copy the contents of your **app_manifest.json** file to **notepad** as you will need this configuration information for the next labs.

---

## Understanding the Azure Sphere Application

### Sending telemetry to Azure IoT Central

Open **main.c**, and scroll down to the **MeasureSensorHandler** function.

>Pro Tip. Use **Go to Symbol in Editor** in Visual Studio Code. Use the keyboard shortcut <kbd>Ctrl+Shift+O</kbd> and start typing *measure*. You will often see a function name listed twice in the dropdown. The first is the [function prototype declaration](https://en.wikipedia.org/wiki/Function_prototype#:~:text=In%20computer%20programming%2C%20a%20function,but%20omits%20the%20function%20body.), and the second is the implementation of the function.

![](resources/vs-code-function-navigate-measure-sensor-telemetry.png)

In the **MeasureSensorHandler** function there is a call to **SendMsgLedOn(msgBuffer);**.

```c
/// <summary>
/// Read sensor and send to Azure IoT
/// </summary>
static void MeasureSensorHandler(EventLoopTimer* eventLoopTimer)
{
	static int msgId = 0;
	static LP_ENVIRONMENT environment;
	static const char* MsgTemplate = "{ \"Temperature\": \"%3.2f\", \"Humidity\": \"%3.1f\", \"Pressure\":\"%3.1f\", \"Light\":%d, \"MsgId\":%d }";

	if (ConsumeEventLoopTimerEvent(eventLoopTimer) != 0)
	{
		lp_terminate(ExitCode_ConsumeEventLoopTimeEvent);
		return;
	}

	if (lp_readTelemetry(&environment))
	{
		if (snprintf(msgBuffer, JSON_MESSAGE_BYTES, MsgTemplate, environment.temperature, environment.humidity, environment.pressure, environment.light, msgId++) > 0)
		{
			SendMsgLedOn(msgBuffer);
		}
	}
}
```

Function **SendMsgLedOn** will turn on the send message LED, then **SendMsg(message)** is called to send a JSON formatted message to Azure IoT Central.

```c
/// <summary>
/// Turn on LED2, send message to Azure IoT and set a one shot timer to turn LED2 off
/// </summary>
static void SendMsgLedOn(char* message)
{
	lp_gpioOn(&sendMsgLed);
	Log_Debug("%s\n", message);
	lp_sendMsg(message);
	lp_setOneShotTimer(&sendMsgLedOffOneShotTimer, &led2BlinkPeriod);
}
```

<!-- ---

### Sending Events to Azure IoT Central

Azure IoT Central can be configured to treat some telemetry as events. You would typically use these for alerts or notifications. For example, an emergency button was pressed.

If you review the Azure Sphere Device template Interface in IoT Central you will see that ButtonA and ButtonB are both configured as **Events**.

![](resources/iot-central-template-interface-events.png)

Review the handler function **ButtonPressCheckHandler**. The following code sends the Button Pressed events to Azure IoT Central.

```c
// Send ButtonA Pressed Event
if (snprintf(msgBuffer, JSON_MESSAGE_BYTES, cstrJsonEvent, cstrEvtButtonA) > 0) {
	SendMsgLed2On(msgBuffer);
}
```

The Azure Sphere will send the following JSON formatted event messages to IoT Central depending on which button is pressed.

* { "ButtonA": "occurred" }
* { "ButtonB": "occurred" } -->

---

## Deploying the Application to Azure Sphere

### Step 1: Start the app build deploy process

1. Ensure main.c is open.
2. Select **CMake: [Debug]: Ready** from the Visual Studio Code Status Bar.

	![](resources/vs-code-start-application.png)

3. From Visual Studio Code, press <kbd>F5</kbd> to build, deploy, start, and attached the remote debugger to the application now running the Azure Sphere device.


### Step 2: View debugger output

1. Open the Visual Studio Code **Output** tab to view the output from **Log_Debug** statements in the code.

	> Pro Tip. You can open the output window by using the Visual Studio Code <kbd>Ctrl+K Ctrl+H</kbd> shortcut or click the **Output** tab.
2. You will see the device negotiating security, and then it will start sending telemetry to Azure IoT Central.

#### Notes

1. You may see a couple of *ERROR: failure to create IoTHub Handle* messages displayed. These messages occur while the connection to IoT Central is being negotiated.

---

## Expected Device Behaviour

### Avnet Azure Sphere MT3620 Starter Kit

![](resources/avnet-azure-sphere.jpg)

1. The blue LED will start to blink.
2. LED3 will turn yellow when connected to Azure. 
3. Press **Button A** on the device to change the blink rate.

### Seeed Studio Azure Sphere MT3620 Development Kit

![](resources/seeed-studio-azure-sphere-rdb.jpg)

1. The green LED will start to blink.
2. The network LED will turn red when connected to Azure.
3. Press **Button A** on the device to change the blink rate.

### Seeed Studio MT3620 Mini Dev Board

![](resources/seeed-studio-azure-sphere-mini.png)

1. The green LED closest to the USB connector will start to blink.

---

## Step 3: View the device telemetry on the Azure IoT Central Dashboard

1. Switch back to Azure IoT Central in your web browser.
2. You need to wait a minute or two before your Azure Sphere device is automatically enrolled.
3. The newly enrolled device will have a numeric name that matches your Azure Sphere Device ID.
4. To display your Azure Sphere Device ID, start the **Azure Sphere Developer Command Prompt** and run the following command.

    ```bash
    azsphere device show-attached
    ```

---

## Step 4: Migrate your device to the Azure Sphere Template

You need to **Migrate** the newly enrolled device to the **Azure Sphere** template. The template maps the JSON formatted telemetry to the dashboard.

1. Select the newly enrolled device from the **All devices** template.
2. Click **Migrate**

    ![](resources/iot-central-migrate-device.png)

3. Select the Azure Sphere Template, and then click migrate.

    ![](resources/iot-central-migrate-select-template.png)

---

## Step 5: Display the Azure Sphere device telemetry

1. Click **Devices** on the sidebar.
2. Select the **Azure Sphere** template.
3. Click on the migrated device.
4. Select the **View** tab to view the device telemetry.
5. Rename your device. Click the **Rename** button and give your device a friendly name.

> Azure IoT Central does not update immediately. It may take a minute or two for the temperature, humidity, and pressure telemetry to be displayed.

![](resources/iot-central-display-measurements.png)

---

## In Review

We learned the following:

1. How to create an Azure IoT Central Application
2. Enabling trust between your Azure Sphere Tenant and your Azure IoT Central Application for automatic device enrollment.
3. Explicitly allowing connections to Azure IoT Central Application network endpoints.
4. Configuring the Azure Sphere Application to connect to Azure IoT Central.

---

## Close Visual Studio

Now close **Close Visual Studio**.

---

## Finished 完了 fertig finito समाप्त terminado

Congratulations you have finished Lab 2.

![](resources/finished.jpg)

---

**[NEXT](../Lab_3_Control_Device_with_Device_Twins/README.md)**

---
