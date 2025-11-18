import serial, serial.tools.list_ports
import zlib
from crccheck.crc import CrcX25
import time
import os
direc=""
def crc16_x25(data):
    
    crc=CrcX25.calc(data)
    
    return (crc&0xffff).to_bytes(length=2,byteorder="little")
 
def build_connection():
    
    device=None
    ports=serial.tools.list_ports.comports(include_links=False)
    for port in ports:
        
        if port.vid==0x12D1 and "DBAdapter Reserved Interface" in port.description:
            device=port.device
            print(device," found.")
    if device==None:
        print("No devices found.")
        return None
       
    return serial.Serial(port=device,baudrate=9600,timeout=5)
def handshake_cmd():
    
    c=b"\x26\x00\x00\x25\xa7\x00\x06\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x01\x00"
    c=c+crc16_x25(c)+b"\x7e"
    return c
def convert(data):
    lin1=bytearray(data)
    i=0
    while i>=0 and i<len(lin1):
        if lin1[i]==0x7e:
            lin1[i]=0x7d
            lin1.insert(i+1,0x5e)
            i=i+1
        elif lin1[i]==0x7d:
            lin1.insert(i+1,0x5d)
            i=i+1
        i=i+1
    lin2=bytes(lin1)
    return lin1
def unlock_cmd():
    with open(direc+"\\unlockcode","rb") as file:
        lin1=file.read()
    lin1=b"\x0b"+lin1
    lin1=b"\x7e"+convert(lin1+crc16_x25(lin1))+b"\x7e"
    return lin1
def read_header(name):
    with open(direc+"\\"+name+".img.header","rb") as file:
        lin1=file.read()
    lin2=bytearray(lin1)
    lin2[92]=0
    lin2[93]=0
    lin1=bytes(lin2)
    lin1=lin1+b"\x00"
    return lin1
def head_cmd(data):
    c=b"\x41"+data
    c=b"\x7e"+convert(c+crc16_x25(c))+b"\x7e"
    return c
def tail_cmd(data):
    c=b"\x43"+data
    c=b"\x7e"+convert(c+crc16_x25(c))+b"\x7e"
    return c
def data_cmd(data,len,fileseq,addr):
    c=b"\x0f"+(int.from_bytes(fileseq,"big")+addr).to_bytes(length=4,byteorder="big")
    c=c+len.to_bytes(length=4,byteorder="big")
    c=c+data
    c=c+crc16_x25(c)
    c=convert(c)
    c=b"\x7e"+c+b"\x7e"
    return c
def reboot_cmd():
    c=b"\x0a"
    c=c+crc16_x25(c)
    c=b"\x7e"+c+b"\x7e"
    return c
def forcereboot_cmd():
    c=b"\x32"
    c=c+crc16_x25(c)
    c=b"\x7e"+c+b"\x7e"
    return c
def custom_cmd(data):
    c=data+crc16_x25(data)
    c=convert(c)
    c=b"\x7e"+c+b"\x7e"
    return c

myport=build_connection()
time_out=0.1
if myport==None:
    print("Make sure you have installed huawei hisuite driver and plugged usb into both computer and your device.Aborted.")
    exit()
def do_handshake():
    
    myport.write(handshake_cmd())
    
    time.sleep(0.1)
    lin1=myport.read(myport.in_waiting)
    if b"\x7e\x26\x00\x00\x25\xa7"in lin1:
        print("HandShake Successful!")
    else:
        print("HandShake Failed!",lin1,"\nAborted!")
        
        exit()
def cmdsend(data):
    lin3=bytearray(data)
    lin4=len(lin3)
    for i in range(0,lin4,0x4000):
        if i+0x4000>=lin4:
            lin5=lin3[i:lin4]
        else :
            lin5=lin3[i:i+0x4000]
        
        lin6=bytes(lin5)
        myport.write(lin6)
        
    
    time.sleep(time_out)
    lin1=myport.read(myport.in_waiting)
    if b"\x7e\x02\x6a\xd3\x7e"==lin1:
        
        time.sleep(0.1)
        return 1
    else:
        print("Failed!\n",lin1)
        time.sleep(0.1)
        return 0
def send_image(name,bsiz):
    len1=os.path.getsize(direc+"\\"+name+".img")
    addr=0
    fileseq1=bytearray(read_header(name))[20:24]
    fileseq1.reverse()
    fileseq=bytes(fileseq1)
    
    print("Send ",name," Size:",len1)
    with open(direc+"\\"+name+".img","rb") as file:
        while len1>0:
            if len1<bsiz:
                
                lin1=file.read(len1)
                
                lin2=zlib.compress(lin1,level=1)
                
                
                
                cmdsend(data_cmd(lin2,len1,fileseq,addr))
                len1=0
                break
            else:
                lin1=file.read(bsiz)
                len1=len1-bsiz
                lin2=zlib.compress(lin1,level=1)
                cmdsend(data_cmd(lin2,bsiz,fileseq,addr))
            addr=addr+bsiz
    print("OK!")
    
#try send head and tail only
do_handshake()
time.sleep(0.1)
direc="dload"
print("Unlock Cmd")
cmdsend(unlock_cmd())
print("Start to download images")
time.sleep(0.1)

with open(direc+"\\list.txt","r") as file1:
    arr1=file1.readlines()
for i in arr1:
    name=i.split(' ')[0]
    flag=int(i.split(' ')[1])
    #print(name)'''
    #print(flag)
    if flag!=1:
        print("Flash terminated!OK!")
        break
    print("Now Start to download image "+name)
    cmdsend(head_cmd(read_header(name)))
    send_image(name,0x200000)
    time_out=2
    cmdsend(tail_cmd(read_header(name)))
    time_out=0.1
    print("Download "+name+" OK!")
    time.sleep(0.1)
print("Success!Do you  want to reboot your phone now?Type 1 for yes,0 for no.Then press enter.")
i1=int(input())
if i1!=1:
    exit()

cmdsend(reboot_cmd())
time.sleep(0.1)
cmdsend(forcereboot_cmd())
print("USB Download Mode Exit!Done!")
#myport.write("\x26\x00\x00\x25\xa7\x00\x06\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x01\x00")
#handshake
#unlock_cmd
#head_cmd
#tail_cmd
#data_cmd
#reboot_cmd
#forcereboot_cmd
#response
