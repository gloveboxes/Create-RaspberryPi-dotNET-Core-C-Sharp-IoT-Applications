# Raspberry Pi Ubuntu 20.04 Tips and Tricks

![](./resources/ubuntu_raspberrypi_tips_tricks.png)

---

## Rename the Ubuntu Hostname

```bash
sudo hostnamectl set-hostname new-name
```

Edit the /etc/hosts file and replace any references to the old hostname with the new host name.

---

## Enable WiFi from Ubuntu 20.04

To use WiFi 5G you must set the wireless central regulatory domain.

1. Install the Linux wireless central regulatory domain agent plus some network tools on to the Raspberry Pi

    ```bash
    sudo apt install crda net-tools wireless-tools
    ```

2. End the /etc/default/crda Registration Domain file and add your twi character country code after the **=** sign. For Australia the country code is **AU**, select the right code for your country.

    ```text
    REGDOMAIN=AU
    ```
3. Edit the /etc/netplan/50-cloud-init.yaml file

    ```bash
    sudo nano /etc/netplan/50-cloud-init.yaml
    ```
4. Configure the wifi settings. The following is an example of what you will likely need to add. You will need to add the **wifis** section onwards. Note, the indentation is important. Use 4 spaces per level.

    ```text
    # This file is generated from information provided by the datasource.  Changes
    # to it will not persist across an instance reboot.  To disable cloud-init's
    # network configuration capabilities, write a file
    # /etc/cloud/cloud.cfg.d/99-disable-network-config.cfg with the following:
    # network: {config: disabled}
    network:
        ethernets:
            eth0:
                dhcp4: true
                dhcp6: true
                optional: true
        version: 2
        wifis:
            wlan0:
                dhcp4: true
                dhcp6: true
                optional: true
                access-points:
                    "<Your wifi access point name>":
                        password: "<Your wifi access point password>"
    ```

---

## Boot Ubuntu 20.04 from USB3 SSD

For now boot from USB3 SSD is not directly supported by Ubuntu 20.04. But you can do a kernel pivot, which means boot from SD Card as normal and then switch the root drive to the SSD drive and continue to bring up and run the OS from the SSD. With a decent SSD you will get excellent IO performance on a Raspberry Pi.

1. Create your Ubuntu 20.04 SD Card as usual. The easiest way is to use the Raspberry Pi Imager.
2. Start the Raspberry Pi from the SD Card. Not you will either need to start with a HDMI screen, keyboard/mouse attached, or start the Raspberry Pi attached to your network by Ethernet.
3. If you started the Raspberry Pi connected via Ethernet then you will need to SSH into the Raspberry Pi running Ubuntu 20.04.

    ```bash
    ssh ubuntu@ubuntu
    ```

    The default password is ubuntu.
4. Connect the USB3 SSD drive to the Raspberry Pi. **WARNING**. The following process will delete all data from the USB3 SSD Drive.
5. Run the following command on the Raspberry Pi.

    ```bash
    # Partition drive
    sudo sfdisk --delete /dev/sda
    sleep 5
    echo 'type=83' | sudo sfdisk /dev/sda
    sleep 5

    # Format drive
    sudo mkfs.ext4 /dev/sda1 -L usb3-writable

    # Copy system to alternate boot drive
    sudo mkdir /media/usbdrive
    sudo mount /dev/sda1 /media/usbdrive
    sudo rsync -avx / /media/usbdrive

    # Update to cmdline.txt to boot from alternative drive
    sudo sed -i 's/writable/usb3-writable/g' /boot/firmware/cmdline.txt

    sudo reboot
    ```

    The system will reboot, login again and check that you are now running from the USB3 SSD drive. The easiest way is way is to use the disk free command.
    ```bash
    df
    ```
    You will see that root is mounted from /dev/sda1

    ```text
    ubuntu@ubuntu:~$ df
    Filesystem     1K-blocks    Used Available Use% Mounted on
    udev             1891920       0   1891920   0% /dev
    tmpfs             388440    4068    384372   2% /run
    /dev/sda1      122819416 4437140 112100280   4% /
    tmpfs            1942184       0   1942184   0% /dev/shm
    tmpfs               5120       0      5120   0% /run/lock
    tmpfs            1942184       0   1942184   0% /sys/fs/cgroup
    /dev/loop0         49664   49664         0 100% /snap/core18/1708
    /dev/loop1         62720   62720         0 100% /snap/lxd/14808
    /dev/loop2         26624   26624         0 100% /snap/snapd/8147
    /dev/loop4         49664   49664         0 100% /snap/core18/1883
    /dev/loop3         65152   65152         0 100% /snap/lxd/16104
    /dev/mmcblk0p1    258095   99840    158256  39% /boot/firmware
    /dev/loop6         26624   26624         0 100% /snap/snapd/8543
    tmpfs             388436       0    388436   0% /run/user/1000
    ```

---

## How to overclock your Raspberry Pi 4 from Ubuntu 20.04

A great reference for this process is [How to overclock Raspberry Pi 4](https://magpi.raspberrypi.org/articles/how-to-overclock-raspberry-pi-4) MagPi article.

**WARNING**: If you overclock the Raspberry Pi 4 you will need some sort of cooling otherwise the CPU will heat up quickly and the CPU frequency will be throttled back to reduce the temperature which somewhat defeats the purpose of overclocking.

1. Edit the usercfg.txt file

    ```bash
    sudo nano /boot/firmware/usercfg.txt
    ```

2. Add the following overclocking options to the end of the file. I found over clocking to 2000 about right, for my Pi, any higher and the system became unstable.

    ```text
    over_voltage=6
    arm_freq=2000
    gpu_freq=700
    ```

3. Save the changes. <kbd>ctrl+x</kbd>, follow prompts to save and overwrite existing file.
4. Reboot the Raspberry Pi

    ```bash
    sudo reboot
    ```

### Monitor CPU Frequency

```bash
sudo watch -n 1  cat /sys/devices/system/cpu/cpu*/cpufreq/cpuinfo_cur_freq
```

---

## Controlling onboard LEDs

1. Edit the usercfg.txt file

    ```bash
    sudo nano /boot/firmware/usercfg.txt
    ```

### Turn off Power LED

Add the following:

```text
dtparam=pwr_led_trigger=none
dtparam=pwr_led_activelow=off
```

### Turn off Activity LED

Add the following:

```text
dtparam=act_led_trigger=none
dtparam=act_led_activelow=off
```

---

## Autostart services with rc.local and systemd

```bash
cat <<EOF | sudo tee /etc/rc.local
#!/bin/sh -e
#
# rc.local
#
# This script is executed at the end of each multiuser runlevel.
# Make sure that the script will "exit 0" on success or any other
# value on error.
#
# In order to enable or disable this script just change the execution
# bits.
#
# By default this script does nothing.

exit 0
EOF
```

```bash
sudo chmod +x /etc/rc.local
```

---

## Raspberry Pi Sense Hat on Ubuntu 20.04

1. Edit the usercfg.txt file

    ```bash
    sudo nano /boot/firmware/usercfg.txt
    ```

2. Add the following to the end of the file. The first line will enable the Raspberry Pi to boot with the HAT attached. The second line enables I2C support.

    ```text
    hdmi_force_hotplug=1
    ```

3. Save the changes. <kbd>ctrl+x</kbd>, follow prompts to save and overwrite existing file.
4. Reboot the Raspberry Pi

    ```bash
    sudo reboot
    ```

---

### Enable I2C Permissions

[How can I set device rw permissions permanently on Raspbian?](https://unix.stackexchange.com/questions/147494/how-can-i-set-device-rw-permissions-permanently-on-raspbian)

Works on Raspberry Pi OS and Ubuntu 20.04. Grants permissions to the I2C bus for all users.

```bash
cd /etc/udev/rules.d
```

create an i2c rules file.

```bash
sudo nano i2c.rules
```

add the following to the new rules file. Then save and reboot.

```text
ACTION=="add", KERNEL=="i2c-[0-1]*", MODE="0666"
```

<!-- 

You need to grant yourself permission to the /dev/i2c device.

```bash
sudo groupadd i2c
sudo usermod -aG i2c $USER
```

All the following lines to /etc/rc.local as these permissions do not seem to persist. See notes above to create rc.local autostart on Ubuntu.

```bash
# Allow all to have access to I2C
chown :i2c /dev/i2c-1
chmod g+rw /dev/i2c-1

```

Check permissions applied

```bash
ls i2c-1 -all
```

should look like he following

```text
crw-rw---- 1 root i2c 89, 1 Apr  1 17:23 i2c-1
``` -->

---

## Install Docker

```bash
sudo apt -y install docker.io && sudo usermod -aG docker $USER
```

---

## Install Azure SQL Edge with Docker

### Learn more about Azure SQL Edge

[Azure SQL Edge documentation](https://docs.microsoft.com/en-us/azure/azure-sql-edge/)

### Learn about Docker persistent storage volumes

[Docker Containers Tutorial – Persistent Storage Volumes and Stateful Containers](http://www.ethernetresearch.com/docker/docker-tutorial-persistent-storage-volumes-and-stateful-containers/)

### Create Docker Data Volume

Create a new persistent storage volume in the Host Machine.

```bash
docker volume create azure-sql-edge-data
```

Inspect the storage volume to get more detailed information.

```bash
docker volume inspect azure-sql-edge-data
```

Check the data in the storage volume

```bash
sudo ls /var/lib/docker/volumes/azure-sql-edge-data/_data
```

Remove a docker data volume

```bash
 docker volume rm azure-sql-edge-data
```

### Start Azure SQL Edge

```bash
docker run --cap-add SYS_PTRACE -e 'ACCEPT_EULA=1' -e 'MSSQL_SA_PASSWORD=<Your password>' --restart always -p 1433:1433 --name azuresqledge  -v azure-sql-edge-data:/var/opt/mssql -d mcr.microsoft.com/azure-sql-edge
```

### Azure SQL Management Tools

#### Windows only (Full featured)

[SQL Server Management Studio](https://docs.microsoft.com/en-us/sql/ssms/download-sql-server-management-studio-ssms?view=sql-server-ver15)

#### Cross platform Linux, macOS, Windows (Lighter weight)

[Azure Data Studio](https://docs.microsoft.com/en-us/sql/azure-data-studio/what-is?view=sql-server-ver15)

### SQL Server Samples Databases

Northwind is a great starting point

[Northwind and pubs sample databases for Microsoft SQL Server](https://github.com/microsoft/sql-server-samples/tree/master/samples/databases/northwind-pubs)

---

## Install MySql

[Docker Containers Tutorial – Persistent Storage Volumes and Stateful Containers](http://www.ethernetresearch.com/docker/docker-tutorial-persistent-storage-volumes-and-stateful-containers/)

Create a new persistent storage volume in the Host Machine.

```bash
docker volume create mysql-data
```

Inspect the storage volume to get more detailed information.

```bash
docker volume inspect mysql-data
```

Check the data in the storage volume

```bash
sudo ls /var/lib/docker/volumes/mysql-data/_data
```

```bash
docker run --name mysql1 -v mysql-data:/var/lib/mysql -e MYSQL_ROOT_HOST=% -e MYSQL_ROOT_PASSWORD="<Your Password>" --restart always -p 3306:3306 -d mysql/mysql-server
```
