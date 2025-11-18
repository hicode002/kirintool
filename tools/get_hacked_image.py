from crccheck.crc import CrcX25
def crc16_x25(data):
    
    crc=CrcX25.calc(data)
    
    return (crc&0xffff).to_bytes(length=2,byteorder="little")

with open("dload\\XLOADER.img","rb") as file:
    a1=file.read(0x8000)
    a2=file.read()
file.close()
with open("crc_hacked.bin","rb") as file:
    b=file.read()
file.close()
b2=bytearray(b)
a3=bytearray(a1)
c=bytearray([])
for i in range(0,0x8000):
    c.append(b2[i]^a3[i])
c1=bytes(c)
with open("dload\\XLOADER.img","wb") as file:
    file.write(c1)
    file.write(a2)
file.close()
print("Generated Patched XLOADER OK!")
    
