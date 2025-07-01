# kirintool
Pocs and Exps about Kirin Soc.Tools for unlocking bootloader and flashing images.

#### QQ Group:972822667 
#### Yunhu Group 821815308
# DISCLAIMER
### Do not use this tool for any commercial behavior.

### Our tool is free forever.I do not accept any donation!!

### For anyone who wants to reuse this tool,please publish your source code and ensure that your tool is completely free!!

# Functions we want to support:
Bootrom exploit

Kirin 659-960 Using xloader exploit

Kirin 710 Bootrom exploit

Kirin 710A Bootrom exploit

Kirin 970 xloader exploit(Bootrom exploit need to bruteforce the address)

Kirin 980 Bootrom exploit

Kirin 810/820/990/990 5g: Xloader encrypted,still trying

Temporary unlock bootloader

All socs can achieve this followa bootrom exploit

Persistently Unlock bootloader

Hard to achieve now.If anyone have a persistent unlocked bootloader with EMUI10 or Kirin 990/710A devices,please contact me via hicode002@gmail.com

If you have compartner account,please contact me ,too.We are trying to find safe methods to use it.

If you have any fastboot/xloader persistent exploit,contact me ,too.(hicode002@gmail.com)


Firmware decryption

Fastboot decryption is OK!

Xloader decryption is still trying

Other firmware decryption can be achieved by patching fastboot,not tested.

Oeminfo is verified by hmac from Kirin 980.Offline editing is impossible

Nvme is checked by crc,so  we can offline edit it,not tested.

Partition Dump

Only for factory fastboot.Because retail fastboot delete these commands like fastboot oem emmc-dump,fastboot getvar ptable

And maybe we can patch fastboot commands to load partition to memory and dump it.(Not tested)

Image flash

Works for all socs.

USB DLOAD MODE

Works for all emui system.Harmony os use another usb update protocol(maybe upload zip directly),not tested(need a harmony os phone to test,you can contact me)

For EMUI9 and initial versions of EMUI10,there exists exploits to corrupt the xloader partition(and in fact we can change all partitions and base_verlist base_ver)


Recovery/Erecovery exploit

Unziploc needs vulunerable recovery_ramdisk.img.I have not found any one.So I am curious about what system version taszk lab used.

ReUnzip works ,but we need a phone/develop board.Because we need to pretend the device as a usb disk.The computer's usb can not do that.

Engineer recovery leaves an opened adb port.Need usb connection and flashing.But selinux is enabled.

Kernel/Trustzone exploit

Not tested.
