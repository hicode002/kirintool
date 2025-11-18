
def reverse_byte(byte):
    
    return int('{:08b}'.format(byte)[::-1], 2)

def reverse_32bits(x):
    
    return int('{:032b}'.format(x)[::-1], 2)

def crc32(data: bytes) -> int:
    crc = 0xFFFFFFFF  
    poly = 0x1EDC6F41  

    
    reversed_data = bytes([reverse_byte(b) for b in data])

    for byte in reversed_data:
        crc ^= (byte << 24)  
        for _ in range(8):
            if crc & 0x80000000:
                crc = (crc << 1) ^ poly
            else:
                crc <<= 1
            crc &= 0xFFFFFFFF  

    
    crc = reverse_32bits(crc)
    
    
    return crc ^ 0xFFFFFFFF
def is_alpha(item):
    for i in item:
        if 32<=i<=126 or i==0:
            continue
        return False
    return True
def nvme_edit_interactive(orig):
    orig_a=bytearray(orig)
    item_strs=[]
    item_data=[]
    item_offset=[]
    for i in range(0x0,len(orig)-16,16):
        item=orig_a[i+4:i+12]
        if is_alpha(item)==False:
        
            continue
        item_str=bytes(item).decode('utf-8').rstrip('\0')
        item_strs.append(item_str)
        item_data.append(bytes(orig_a[i:i+128]))
        item_offset.append(i)
    print("NVME EDITOR START.INTERACTIVE MODE.")
    
    while True:
        print("Input the name of the item you want to edit.Input 0 to finish editing and save.")
        a=input()
        if a=="0":
            print("EXIT EDITOR.SAVING...")
            break
        fl=1
    
        for ii in range(0,len(item_strs)):
            if a==item_strs[ii]:
                fl=0
                print(item_strs[ii])
                print(item_data[ii][24:128])
        if fl==1:
            print("INVALID ITEM")
            continue
        print("Skip?1 for continue with string edit.2 for continue with hex edit.other for skip")
        linn=input()
        if linn!='1' and linn!='2':
            print("SKIP!")
            continue
        print("Input new value:")
        lin=input()
        if linn=='1':
            lin2=lin.encode('utf-8','ignore')
        else :
            lin2=bytes.fromhex(lin)
        len1=len(lin2)
        for ii in range(0,104-len1):
            lin2=lin2+b"\x00"
        for ii in range(0,len(item_strs)):
            if a==item_strs[ii]:
                len1_lin=len1.to_bytes(length=4,byteorder="little")
                len1_orig=int.from_bytes(bytes(item_data[ii][16:20]),byteorder="little")
                if len1<len1_orig:
                    len1_lin=bytes(item_data[ii][16:20])
            
                new1=bytes(item_data[ii][0:16])+len1_lin+lin2
                crcc=(crc32(new1)).to_bytes(4,byteorder='little')
                new2=bytes(item_data[ii][0:16])+len1_lin+crcc+lin2
                item_data[ii]=new2
                for j in range(0,128):
                    orig_a[item_offset[ii]+j]=new2[j]
        print("THIS ITEM EDIT OK!")
    return bytes(orig_a)
        
def nvme_edit_single(orig,src,mode,newstr):
    orig_a=bytearray(orig)
    item_strs=[]
    item_data=[]
    item_offset=[]
    for i in range(0x0,len(orig)-16,16):
        item=orig_a[i+4:i+12]
        if is_alpha(item)==False:
        
            continue
        item_str=bytes(item).decode('utf-8').rstrip('\0')
        item_strs.append(item_str)
        item_data.append(bytes(orig_a[i:i+128]))
        item_offset.append(i)
    print("NVME EDITOR START.TARGET ITEM IS ",src)
    a=src
    while True:
        
        if a=="0":
            print("EXIT EDITOR.SAVING...")
            break
        fl=1
        
        for ii in range(0,len(item_strs)):
            if a==item_strs[ii]:
                fl=0
                print(item_strs[ii])
                print(item_data[ii][24:128])
        if fl==1:
            print("INVALID ITEM.EDIT FAILED.")
            a="0"
            continue
        
        linn=str(mode)
        if linn!='1' and linn!='2':
            print("SKIP!EDIT FAILED")
            a="0"
            continue
        
        lin=newstr
        if linn=='1':
            lin2=lin.encode('utf-8','ignore')
        else :
            lin2=bytes.fromhex(lin)
        len1=len(lin2)
        for ii in range(0,104-len1):
            lin2=lin2+b"\x00"
        for ii in range(0,len(item_strs)):
            if a==item_strs[ii]:
                len1_lin=len1.to_bytes(length=4,byteorder="little")
                len1_orig=int.from_bytes(bytes(item_data[ii][16:20]),byteorder="little")
                if len1<len1_orig:
                    len1_lin=bytes(item_data[ii][16:20])
            
                new1=bytes(item_data[ii][0:16])+len1_lin+lin2
                crcc=(crc32(new1)).to_bytes(4,byteorder='little')
                new2=bytes(item_data[ii][0:16])+len1_lin+crcc+lin2
                item_data[ii]=new2
                for j in range(0,128):
                    orig_a[item_offset[ii]+j]=new2[j]
        print("THIS ITEM EDIT OK!")
        a="0"
    return bytes(orig_a)    
def fblock_patch(path):
    print("Current file is "+path)
    print("Start FBLOCK PATCH")
    with open(path,"rb") as file1:
        orig=file1.read()
        file1.close()
    orig_a=nvme_edit_single(orig,"FBLOCK",2,"00")
    with open(path,"wb") as file1:
        file1.write(orig_a)
        file1.close()
    print("FBLOCK PATCH OK!")
if __name__ == '__main__':
    path=input("Input NVME file path\n")
    with open(path,"rb") as file1:
        orig=file1.read()
        file1.close()

    orig_a=nvme_edit_interactive(orig)
    with open(path,"wb") as file1:
        file1.write(bytes(orig_a))
        file1.close()            
    print("OK!")
            
            
#test_str=b"\x01\x00\x00\x00\x42\x4F\x41\x52\x44\x49\x44\x00\x01\x00\x00\x00\x20\x00\x00\x00\x59\x44\x32\x36\x35\x32\x32\x31\x35\x36\x32\x30\x37\x38\x32\x30\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00"
#print(len(test_str))

'''
struct NVE_partition_header {
	char nve_partition_name[32];
	unsigned int nve_version;
	unsigned int nve_block_id;
	unsigned int nve_block_count;
	unsigned int valid_items;
	unsigned int nv_checksum;
	unsigned int nve_crc_support;
	unsigned char reserved[68];
	unsigned int nve_age;
};

struct NV_items_struct {
	unsigned int nv_number;
	char nv_name[NV_NAME_LENGTH];
	unsigned int nv_property;
	unsigned int valid_size;
	unsigned int crc;
	char nv_data[NVE_NV_DATA_SIZE];
};

struct NVE_partittion {
	struct NV_items_struct NV_items[NV_ITEMS_MAX_NUMBER];
	struct NVE_partition_header header;
};

struct NVE_struct {
	int nve_major_number;
	int initialized;
	unsigned int nve_partition_count;
	unsigned int nve_current_id;
	struct NVE_partittion *nve_current_ramdisk;
	struct NVE_partittion *nve_update_ramdisk;
	struct NVE_partittion *nve_store_ramdisk;
};
    
'''
#print(hex(crc32(test_str)))
