import os

os.system('for d in $(find /mnt/c/dev/Dreamgarage/ -maxdepth 15 -type d); do echo $d; done > test.txt')

folders = open('test.txt').read().split('\n')

for folder in folders:
    folder = folder.replace('/mnt/c','C:')
    fixCMD = 'fsutil.exe file setCaseSensitiveInfo ' + folder + ' disable'
    print fixCMD
    os.system(fixCMD)
