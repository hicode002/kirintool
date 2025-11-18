
from argparse import ArgumentParser
import binascii
import os
unlock_cmd=b""
BLOCK_MAGIC_NUM = b'\x55\xAA\x5A\xA5'
info_str=""
par=""
start_addr=0

def print_bytes(data):
    print(binascii.hexlify(data).decode('utf-8'),end='')
def get_unlock_cmd(path):
    global unlock_cmd
    global start_addr
    with open(path,"rb") as file:
        lin1=file.read(1)
        while lin1!=b"\x55":
            start_addr=start_addr+1
            lin1=file.read(1)
            
        lin1=file.read(3)
        if lin1!=b"\xAA\x5A\xA5":
            print("[Error] Invalid file header.Aborted.")
            exit()
        lin1=file.read(8)
        lin1=file.read(8)
        if b"hw" in lin1 or b"HW" in lin1 or b"hW" in lin1 or b"Hw" in lin1:
            unlock_cmd=lin1
            print("[Info] Found valid unlock code. ",end="")
            print_bytes(unlock_cmd)
            print("")
            return
        else:
            print("[Error] No valid unlock code found.Aborted.")
            exit()
def handle_header(file,mode):
    global info_str
    now=b""
    d_len=0
    name=""
    lin1=file.read(4)
    now=now+lin1
    if lin1!=BLOCK_MAGIC_NUM:
        print("[Info] Reach the end!Done!")
        return 0,""
    
    lin1=file.read(4)
    now=now+lin1
    head_len=int.from_bytes(lin1,byteorder="little")
    #print(head_len)
    lin1=file.read(4)
    now=now+lin1
    lin1=file.read(8)
    now=now+lin1
    lin1=file.read(4)
    now=now+lin1
    lin1=file.read(4)
    now=now+lin1
    d_len=int.from_bytes(lin1,byteorder="little")
    #print(d_len)
    lin1=file.read(32)
    now=now+lin1
    lin1=file.read(32)
    now=now+lin1
    name=(lin1+b"\x00").decode('utf-8')
    name=name.split("\0")[0]
    print(name)
    
    lin1=file.read(2)
    now=now+lin1
    lin1=file.read(2)
    now=now+lin1
    lin1=file.read(2)
    now=now+lin1
    rem_len=head_len-(4 + 4 + 4 + 8 + 4 + 4 + 16 + 16 + 32 + 2 + 2 + 2)
    if rem_len>0:
        lin1=file.read(rem_len)
    if mode==0 or mode==4:
        with open("dload\\"+name+".img.header","wb") as file2:
            file2.write(now)    
    if mode==0 or mode==1 or mode==4:
        with open("dload\\list.txt","ab") as file2:
            file2.write(bytes(name+" 1\n","utf-8"))
            
    info_str=info_str+name+" "+str(d_len)+"B\n"
    return d_len,name
def handle_data(file,mode,d_len,name):
    #print(par.upper())
    if d_len<=0:
        print("[Error] Image "+name+" data not found!Aborted.")
        exit()
    d_len_mod=d_len%(4*1048576)
    d_len_num=d_len//(4*1048576)
    if mode==0 or mode==1 or mode==4:
        file2=open("dload\\"+name+".img","wb")
    if mode==3 and par.upper()==name.upper():
        file2=open(name+".img","wb")
    
    for i in range(0,d_len_num):    
        lin1=file.read(4*1048576)
        if mode==0 or mode==1:
            file2.write(lin1)
        if mode==3 and par.upper()==name.upper():
            file2.write(lin1)
    if d_len_mod>0:
        lin1=file.read(d_len_mod)
        if mode==0 or mode==1:
            file2.write(lin1)
        if mode==3 and par.upper()==name.upper():
            file2.write(lin1)
    if mode==0 or mode==1 or mode==4:
        file2.close()
    if mode==3 and par.upper()==name.upper():
        file2.close()
    
    ali_pad=file.tell()%4
    if ali_pad!=0:
        ali_pad=4-ali_pad
        file.read(ali_pad)
    if mode==4 and name.upper()=="XLOADER":
        print("Mode 4. OK！Exit!")
        return 0
    if mode==3 and par.upper()==name.upper():
        print("[Info] In mode 3,image "+name+" dumped successfully.Done.")
    return 1
        
def parse_image(path,mode):
    print("[Info] Parsing images...")
    with open(path,"rb") as file:
        file.seek(start_addr)
        print("[Info] Start Addr: ",hex(start_addr))
        d_len,name=handle_header(file,mode)
        #print(d_len)
        while d_len!=0:
            if handle_data(file,mode,d_len,name)==0:
                return
            d_len,name=handle_header(file,mode)
        
        return
if __name__ == '__main__':
    #global par
    argparser = ArgumentParser(description='Unpack Huawei update.app package.This tool can generate both unpacked images and image headers.It can also generate a image list file and unlockcode file.')
    argparser.add_argument('-f', '--update_file', help='Should be an UPDATE.APP file.',required=True)
    argparser.add_argument('mode',choices=['0','1','2','3','4'],help='Input work mode.For mode 0 and 1,the tool will create a folder called dload in this directory.0 is  for unpacking update.app to generate unlockcode,image list,image header and image files.1  for just unpacking to get image files and  list.2 for only showing the image info on the console.3 for dumping specified partition with header,using -p',default='0')
    argparser.add_argument('-p','--partition',required=False,help='Only work in mode 3.Dump a single partition and its header.In other mode,it will be omitted.')
    args = argparser.parse_args()
    
    update_file_path = args.update_file
    mode=int(args.mode)
    par=args.partition
    
    if par!=None and mode!=3:
        print("[Warning] You are not in mode 3.-p argument omitted")
    elif par==None and mode==3:
        print("[Error] You are in mode 3.You do not use -p argument.Aborted.")
        exit()
    if update_file_path.endswith('.APP')==False:
        print("[Error] Invalid file.Aborted")
        exit()
    print("{Info] In mode "+str(mode)+".")
        
    
    get_unlock_cmd(update_file_path)
    if mode==0 or mode==1 or mode==4:
        os.makedirs("dload",exist_ok=True)
        with open("dload\\list.txt","w") as file:
            file.write('')
    parse_image(update_file_path,mode)
    if mode==0 or mode==4:
        with open("dload\\unlockcode","wb") as file:
            file.write(unlock_cmd)
        print("[Info] In mode 0,unlockcode generated.")
    print("Images List:")
    print("-----"*20)
    print(info_str)
    print("-----"*20)
    print("Image Parse Finished!")
    if mode==0 or mode ==1 or mode==4:
        print("Stored in dload\\ directory.")
    print("Program exit now")
#partition name partition header start_addr,end_addr partition image start_addr end_addr.unlock_cmd
