import os
import time
from crccheck.crc import CrcX25
def crc16_x25(data):
    
    crc=CrcX25.calc(data)
    
    return (crc&0xffff).to_bytes(length=2,byteorder="little")
print("This tool is for kicking your device into BOOTROM Mode from USB Update Mode(5%)")
print("Please Make sure you have use appkupk.py to unpack your update.app.And make sure you have made python.exe in global variable PATH.")
time.sleep(0.5)
if os.path.exists("dload\\XLOADER.img")==False:
    print("No VALID DIRECTORY.Please unpack first.Aborted.")
    exit(0)
if os.path.exists("crc_hacked.bin")==False or  os.path.exists("get_hacked_image.py")==False:
    print("Missing Necessary Componets!Aborted.")
    exit(0)
with open("dload\\PACKAGE_TYPE.img","rb") as file1:
    ss1=file1.read()
    s1=ss1.decode('utf-8',errors='ignore')
    file1.close()
#print(s1)
if "OFFLINE_UPDATE" in s1:
    print("PACKAGE_TYPE correct.Continue")
else :
    print("Online update package found.Replacing it with a hacked one(may not work)...")
    if os.path.exists("pk_cracked.bin")==False:
        print("Missing necessary componets.Skip patching.")
    else:
        with open("pk_cracked.bin","rb") as file1:
            lin3=file1.read()
            file1.close()
        with open("dload\\PACKAGE_TYPE.img","wb") as file1:
            file1.write(lin3)
            file1.close()
        print("PACKAGE_TYPE PATCH SUCCESS.")

print("Backing Up origin xloader.img...")
os.system("copy dload\\xloader.img dload\\xloader_bak.img")
print("Start patching...")
os.system("python get_hacked_image.py")
time.sleep(1)
print("Now checking if patching is valid...")
with open("dload\\XLOADER_BAK.img","rb") as file:
    a=file.read(0x8000)
with open("dload\\XLOADER.img","rb") as file:
    b=file.read(0x8000)
if a==b:
    print("Patch Failed.")
    os.system("del dload\\XLOADER.img")
    os.system("rename dload\\XLOADER_BAK.img XLOADER.img")
    exit(0)
c=crc16_x25(a)
d=crc16_x25(b)
if c==d:
    print("Equal CRC.Patch OK!")
    print("Now change list.txt!")
    with open("dload\\list.txt","r") as file:
        arr1=file.readlines()
    fl=0
    for i in arr1:
        name=i.split(' ')[0]
        if name=="XLOADER":
            fl=1
            continue
        if fl==1:
            i=name+" 0"
            break
    with open("dload\\list.txt","w") as file:
        file.writelines(arr1)
    file.close()
    print("Now We will start downloading patched image!")
    
    time.sleep(0.5)
    os.system("python usbdload.py")
    print("Restore all changes.")
    with open("dload\\list.txt","r") as file:
        arr1=file.readlines()
    fl=0
    for i in arr1:
        name=i.split(' ')[0]
        if "XLOADER" in name:
            fl=1
            continue
        if fl==1:
            i=name+" 1"
            break
    with open("dload\\list.txt","w") as file:
        file.writelines(arr1)
    os.system("del dload\\XLOADER.img")
    os.system("rename dload\\XLOADER_BAK.img XLOADER.img")
    print("Now you are in BOOTROM MODE.Exit!")
    exit(0)
else:
    print("Not Equal CRC.Patch Failed!")
    os.system("del dload\\XLOADER.img")
    os.system("rename dload\\XLOADER_BAK.img XLOADER.img")
    exit(0)


print(c)
print(d)

